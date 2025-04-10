﻿using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary> 
/// Author:    Ha Thu 
/// Author:    Amber Tran
/// Date:      3/29/2024 
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Amber T and Ha Thu - This work may not be copied for use in Academic Coursework. 
/// 
/// I, Amber Tran, Ha Thu, certify that I wrote this code from scratch and did not copy it in 
/// part or whole from another source.  All references used in the completion of the assignment are 
/// cited in my README file. 
/// 
/// Class Contents 
/// This class represents an abstract, reusable network communication channel, which provides the 
/// ability for network communication between servers and channels.Aswell, data can be transferred 
/// between multiple connections asynchronously,and have actions taken in the server recorded through 
/// a logging mechanism.
/// 
/// 
/// </summary>
/// 
namespace NetworkingLibrary;

public class Networking : INetworking
{
    private readonly ILogger? _logger;
    ReportMessageArrived _onMessage;
    ReportConnectionEstablished _onConnect;
    ReportDisconnect _onDisconnect;
    private CancellationTokenSource _cancelToken = new();
    public TcpClient? TcpClient { get; private set; }
    private readonly char _stopChar = '\n';

    public Networking(ILogger logger,
            ReportConnectionEstablished onConnect,
            ReportDisconnect onDisconnect,
            ReportMessageArrived onMessage)
    {
        _logger = logger;
        _onMessage = onMessage;
        _onConnect = onConnect;
        _onDisconnect = onDisconnect;
        _cancelToken = new CancellationTokenSource();
    }

    /// <summary>
    ///   <para>
    ///     A Unique identifier for the entity on the "other end" of the wire.
    ///   </para>
    ///   <para>
    ///     The default ID is the tcp client's remote end point, but you can change it
    ///     if desired, to something like: "Jim"  (for a servers connection to the Jim client)
    ///   </para>
    /// </summary>
    public string ID { get; set; }

    bool INetworking.IsConnected => TcpClient?.Connected ?? false;

    bool INetworking.IsWaitingForClients => throw new NotImplementedException();

    string INetworking.RemoteAddressPort => TcpClient?.Client.RemoteEndPoint?.ToString() ?? "";

    string INetworking.LocalAddressPort => TcpClient?.Client.LocalEndPoint?.ToString() ?? "";

    /// <summary>
    ///   <para>
    ///     Precondition: Networking socket has already been connected.
    ///   </para>
    ///   <para>
    ///     Used when one side of the connection waits for a network messages 
    ///     from a the other (e.g., client -> server, or server -> client).
    ///     Usually repeated (see infinite).
    ///   </para>
    ///   <para>
    ///     Upon a complete message (based on terminating character, '\n') being received, the message
    ///     is "transmitted" to the _handleMessage function.  Upon successfully handling one message,
    ///     if multiple messages are "queued up", continue to send them (one after another)until no 
    ///     messages are left in the stored buffer.
    ///   </para>
    ///   <para>
    ///     Once all data/messages are processed, continue to wait for more data (and repeat).
    ///   </para>
    ///   <para>
    ///     If the TcpClient stream's ReadAsync is "interrupted" (by the connection being closed),
    ///     the stored handle disconnect delegate will be called and this function will end.  
    ///   </para>        
    ///   <para>
    ///     Note: This code will "await" network activity and thus the _handleMessage (and 
    ///     _handleDisconnect) methods are never guaranteed to be run on the same thread, nor are
    ///     they guaranteed to use the same thread for subsequent executions.
    ///   </para>
    /// </summary>
    /// 
    /// <param name="infinite">
    ///    if true, then continually await new messages. If false, stop after first complete message received.
    ///    Thus the "infinite" handling will never return (until the connection is severed).
    /// </param>
    public async Task HandleIncomingDataAsync(bool infinite = true)
    {
        try
        {
            var stream = TcpClient.GetStream();
            var buffer = new byte[4096];
            var sb = new StringBuilder();

            while (infinite)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancelToken.Token);
                if (bytesRead == 0)
                {
                    _logger.LogDebug("Connection closed");
                    break;
                }

                var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                sb.Append(receivedData);

                if (receivedData.Contains(_stopChar))
                {
                    var messages = sb.ToString().Split(_stopChar);
                    foreach (var message in messages)
                    {
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            _onMessage(this, message);
                        }
                    }
                    sb.Clear();
                }
            }
            _onDisconnect.Invoke(this);
        }
        catch (Exception ex)
        {
            _logger.LogError($"HandleIncomingDataAsync exception: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    /// <summary>
    ///   <para>
    ///     Open a connection to the given host/port.  Returns when the connection is established,
    ///     or when an exception is thrown.
    ///   </para>
    ///   <para>
    ///     Note: Servers will not call this method.  It is used by clients connecting to
    ///     a program that is waiting for connections.
    ///   </para>
    ///   <para>
    ///     If the connection happens to already be established, this is a NOP (i.e., nothing happens).
    ///   </para>
    ///   <para>
    ///     For the implementing class, the signature of this method should use async.
    ///   </para>
    ///   <remark>
    ///     This method will have to create and use the low level C# TcpClient class.
    ///   </remark>
    /// </summary>
    /// <param name="host">e.g., 127.0.0.1, or "localhost", or "thebes.cs.utah.edu"</param>
    /// <param name="port">e.g., 11000</param>
    /// <exception cref="Exception"> 
    ///     Any exception caused by the underlying TcpClient object should be handled (logged)
    ///     and then propagated (re-thrown).   For example, failure to connect will result in an exception
    ///     (i.e., when the server is down or unreachable).
    ///     
    ///     See TcpClient documentation for examples of exceptions.
    ///     https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.-ctor?view=net-7.0#system-net-sockets-tcpclient-ctor
    /// </exception>
    public async Task ConnectAsync(string host, int port)
    {
        try
        {
            {
                TcpClient = new TcpClient();
                await TcpClient.ConnectAsync(host, port);
                ID = TcpClient.Client.RemoteEndPoint.ToString();
                _onConnect?.Invoke(this);
                _logger.LogInformation($"Connected to {host}:{port}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ConnectAsync failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///   <para>
    ///     Close the TcpClient connection between us and them.
    ///   </para>
    ///   <para>
    ///     Important: the reportDisconnect handler will _not_ be called (because if your code
    ///     is calling this method, you already know that the disconnect is supposed to happen).
    ///   </para>
    ///   <para>
    ///     Note: on the SERVER, this does not stop "waiting for connects" which should be stopped first with: StopWaitingForClients
    ///   </para>
    /// </summary>
    public void Disconnect()
    {
        TcpClient.Close();
        _logger.LogInformation("Disconnected.");
        _cancelToken.Cancel();
        _cancelToken.Dispose();
        _cancelToken = new();
    }

    /// <summary>
    ///   <para>
    ///     Send a message across the channel (i.e., the TCP Client Stream).  This method
    ///     uses WriteAsync and the await keyword.
    ///   </para>
    ///   <para>
    ///     Important: If the message contains the termination character (TC) (e.g., '\n') it is
    ///     considered part of a **single** message.  All instances of the TC will be replaced with the 
    ///     characters "\\n".
    ///   </para>
    ///   <para>
    ///     If an exception is raised upon writing a message to the client stream (e.g., trying to
    ///     send to a "disconnected" recipient) it must be caught, and then 
    ///     the _reportDisconnect method must be invoked letting the user of this object know 
    ///     that the connection is gone. No exception is thrown by this function.
    ///   </para>
    ///   <para>
    ///     If the connection has been closed already, the send will simply return without
    ///     doing anything.
    ///   </para>
    ///   <para>
    ///     Note: messages are encoded using UTF8 before being sent across the network.
    ///   </para>
    ///   <para>
    ///     For the implementing class, the signature of this method should use "async Task SendAsync(string text)".
    ///   </para>
    ///   <remark>
    ///     Will use the stored tcp object's stream's writeasync method.
    ///   </remark>
    /// </summary>
    /// <param name="text"> 
    ///   The entire message to send. Note: this string may contain the Termination Character '\n', 
    ///   but they will be replaced by "\\n".  Upon receipt, the "\\n" will be replaced with '\n'.
    ///   Regardless, it is a _single_ message from the Networking libraries point of view.
    /// </param>
    public async Task SendAsync(string text)
    {
        if (TcpClient != null && TcpClient.Connected)
        {
            // Replace termination char "\n" to "\\n"
            string newText = text.Replace("\n", "\\n") + _stopChar;
            // Encoding messages before use
            var buffer = new byte[4096];
            // Get the stream that associated with tcpClient
            NetworkStream stream = TcpClient.GetStream();
            buffer = Encoding.UTF8.GetBytes(newText); // Ensure message ends with stopChar
            try
            {
                await stream.WriteAsync(buffer, 0, buffer.Length, _cancelToken.Token);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Cannot send messages, an error occurred: {ex.Message}");
                Disconnect();
            }
        }
    }

    /// <summary>
    ///   <para>
    ///     This method is only used by Server applications.
    ///   </para>
    ///   <para>
    ///     Handle client connections;  wait for network connections using the low level
    ///     TcpListener object.  When a new connection is found:
    ///   </para>
    ///   <para> 
    ///     IMPORTANT: create a new thread to handle communications from the new client.  
    ///   </para>
    ///   <para>
    ///     This routine runs indefinitely until stopped (could accept many clients).
    ///     Important: The TcpListener should have a cancellationTokenSource attached to it in order
    ///     to allow for it to be shutdown.
    ///   </para>
    ///   <para>
    ///     Important: you will create a new Networking object for each client.  This
    ///     object should use the original call back methods instantiated in the servers Networking object. 
    ///     The new networking object will need to store the new tcp client object returned from the tcp listener.
    ///     Finally, the new networking object (on its new thread) should HandleIncomingDataAsync
    ///   </para>
    ///   <para>
    ///     Again: All connected clients will "share" the same onMessage and 
    ///     onDisconnect delegates, so those methods had better handle this Race Condition.  (IMPORTANT: 
    ///     the locking does _not_ occur in the networking code.)
    ///   </para>
    ///   <para>
    ///     For the implementing class, the signature of this method should use async.
    ///   </para>
    /// </summary>
    /// <param name="port"> Port to listen on </param>
    /// <param name="infinite"> If true, then each client gets a thread that read an infinite number of messages</param>
    public async Task WaitForClientsAsync(int port, bool infinite)
    { 
        TcpListener tcpListener = new TcpListener(IPAddress.Any, port);

        tcpListener.Start();

        try
        {
            _cancelToken = new CancellationTokenSource();
            while (infinite)
            {
                var newNetworkingClient = new Networking(_logger, _onConnect, _onDisconnect, _onMessage);
                newNetworkingClient.TcpClient = await tcpListener.AcceptTcpClientAsync();
                newNetworkingClient._onConnect(newNetworkingClient);
                new Thread(async () => { await newNetworkingClient.HandleIncomingDataAsync(); }).Start();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Waiting for client failed: {ex.Message}");
        } finally{tcpListener.Stop(); }
    }

    /// <summary>
    ///   <para>
    ///     Stop listening for connections.  This is achieved using the Cancellation Token Source that
    ///     was attached to the tcplistner back in the wait for clients method.
    ///   </para>
    ///   <para>
    ///     This code allows for graceful termination of the program, such as if a disconnect button
    ///     is pressed on a GUI.
    ///   </para>
    ///   <para>
    ///     This code should be a very simple call to the Cancel method of the appropriate cancellation token
    ///   </para>
    /// </summary>
    public void StopWaitingForClients()
    {
        _cancelToken.Cancel();
        _logger?.LogDebug("Stopped waiting for clients.");
    }

    /// <summary>
    ///   Stop listening for messages.  This is achieved using the Cancellation Token Source.
    ///   This allows for graceful termination of the program. This method should also be very simple
    ///   utilizing the cancellation token associated with the ReadAsync method used in the
    ///   HandleIncomingData method
    /// </summary>
    public void StopWaitingForMessages()
    {
        _cancelToken.Cancel();
        _logger?.LogDebug("Stopped waiting for messages.");
    }


    /// <summary>
    ///   A method that will be called by the networking code when a complete message comes across the channel.
    /// </summary>
    /// <param name="channel">The Networking Object that received the message</param>
    /// <param name="message">The message itself (of course without the terminating protocol character).</param>
    public delegate void ReportMessageArrived(Networking channel, string message);

    /// <summary>
    ///   <para>
    ///     A method that will be called by the networking object when the channel is disconnected.
    ///   </para>
    ///   <para>
    ///     Usage: an outside code base (e.g., a web browser) will be using a Networking object
    ///     to communicate (e.g., with a web server).  If the web server "goes down" (or isn't up
    ///     in the first place) the networking object will call this function so the outside program can take
    ///     the appropriate action (e.g., put up a "504" web page).
    ///   </para>
    /// </summary>
    /// <param name="channel"> The networking object. </param>
    public delegate void ReportDisconnect(Networking channel);

    /// <summary>
    ///  <para>
    ///    The Networking object will call this method when the program is connected to.
    ///  </para>
    ///  <para>
    ///    If the Networking object represents a client asking for a connection to a server
    ///    this method will be called when the connection is established (after connectasync is successful).
    ///  </para>
    ///  <para>
    ///    If the Networking object is being used by a SERVER waiting for client connects, then this
    ///    method will also be called (once for each client connect).
    ///  </para>
    /// </summary>
    /// <param name="channel">The Networking Object itself</param>
    public delegate void ReportConnectionEstablished(Networking channel);
}