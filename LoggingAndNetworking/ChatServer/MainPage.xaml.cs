using Microsoft.Extensions.Logging;
using NetworkingLibrary;
using System.Net;
using System.Data;
using System.Text;

/// <summary> 
/// Author:    Ha Thu
/// Author:    Amber Tran
/// Date:      3/31/2024 
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Ha Thu, Amber Tran - This work may not be copied for use in Academic Coursework. 
/// 
/// I, Ha Thu, Amber Tran, certify that I wrote this code from scratch and did not copy it in 
/// part or whole from another source.  All references used in the completion of the assignment are 
/// cited in my README file. 
/// 
/// Class Contents 
/// This GUI class provides access to a chatting program, which allows multiple clients to join a server with other 
/// clients and communicate with each other.This class is responsible for tracking clients, creating / losing pathways
/// for communication between clients, store clients, and logging server activities. 
/// 
/// </summary>
/// 
namespace ChatServer
{
    /// <summary>
    /// The main class for the chat server application. This class is responsible for managing client connections,
    /// processing incoming messages, and broadcasting messages to all connected clients.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly ILogger<MainPage> _logger;
        private Networking serverNetworkConnection;
        // Thread-safe list to keep track of all connected clients.
        private List<Networking> clients = new List<Networking>();
        private const int PortNumber = 11000;
        private bool isServerRunning = false;
        // Object used for thread synchronization to ensure thread-safe access to the clients list.
        private readonly object clientsLock = new object();

        /// <summary>
        /// Constructor for the MainPage class. Initializes the server and logging mechanism.
        /// </summary>
        /// <param name="logger">The logger instance for logging server activities.</param>
        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
            _logger.LogInformation("Server initialized");
        }

        /// <summary>
        /// Toggles the server's operational state between running and stopped.
        /// </summary>
        private void ConnectToServer(object sender, EventArgs e)
        {
            if (!isServerRunning)
            {
                StartServer();
            }
            else
            {
                StopServer();
            }
        }

        /// <summary>
        /// Starts the server, making it listen for incoming client connections on a specified port.
        /// </summary>
        private void StartServer()
        {
            serverNetworkConnection = new Networking(_logger, OnClientConnect, OnClientDisconnect, OnMessageReceived);
            Task.Run(async () =>
            {
                isServerRunning = true;
                await serverNetworkConnection.WaitForClientsAsync(PortNumber, true);
            });

            Dispatcher.Dispatch(() =>
            {
                UpdateConnectionStatus(true);
                var localIP = FetchLocalIPAddress();
                ChatLog.Text += $"Server started on port {PortNumber}, host name: {ServerName.Text}.\nListening on IP: {localIP}\n";
            });

            _logger.LogInformation($"Server started on port {PortNumber}.");
        }

        /// <summary>
        /// Stops the server and disconnects all currently connected clients.
        /// </summary>
        private void StopServer()
        {
            if (clients.Count > 0)
            {
                Task.Run(() =>
                {
                    lock (clientsLock)
                    {
                        List<Networking> temp = clients.ToList();
                        foreach (var client in temp)
                        {
                            client.Disconnect();
                        }
                        clients.Clear();
                    }

                    serverNetworkConnection.StopWaitingForClients();
                    isServerRunning = false;
                });
            }

            Dispatcher.Dispatch(() =>
            {
                UpdateConnectionStatus(false);
                UserList.Text = "";
                ChatLog.Text = "";
                ChatLog.Text += "Server stopped.\n";
            });

            _logger.LogInformation("Server stopped.");
        }

        /// <summary>
        /// Updates the UI to reflect the current connection status of the server.
        /// </summary>
        /// <param name="isConnected">True if the server is running, false otherwise.</param>
        private void UpdateConnectionStatus(bool isConnected)
        {
            Dispatcher.Dispatch(() =>
            {
                ConnectBtn.Text = isConnected ? "Stop Server" : "Start Server";
                ConnectionStatus.Text = isConnected ? "Server: Running" : "Server: Stopped";
                ConnectBtn.BackgroundColor = isConnected ? Colors.Gray : Colors.Blue;
            });
        }

        /// <summary>
        /// Handles new client connections by adding them to the client list and broadcasting the updated list to all clients.
        /// </summary>
        /// <param name="client">The networking instance representing the newly connected client.</param>
        public void OnClientConnect(Networking client)
        {
            client.ID = client.TcpClient.Client.RemoteEndPoint.ToString();
            lock (clientsLock)
            {
                clients.Add(client);
            }

            _logger.LogInformation($"{client.ID} connected.");
            Dispatcher.Dispatch(() =>
            {
                ChatLog.Text += $"{client.ID} joined the chat\n";
                UpdateUserList();
            });
        }

        /// <summary>
        /// Handles client disconnections by removing them from the client list and broadcasting the updated list to all remaining clients.
        /// </summary>
        /// <param name="client">The networking instance representing the disconnected client.</param>
        public void OnClientDisconnect(Networking client)
        {
            lock (clientsLock)
            {
                clients.Remove(client);
            }

            _logger.LogInformation($"{client.ID} disconnected.");
            Dispatcher.Dispatch(() =>
            {
                ChatLog.Text += $"{client.ID} left the chat.\n";
            });
            UpdateUserList();
        }

        /// <summary>
        /// Processes incoming messages from clients. This includes handling special commands such as name changes and requests for participant lists.
        /// </summary>
        /// <param name="client">The networking instance sending the message.</param>
        /// <param name="message">The message content.</param>
        public async void OnMessageReceived(Networking client, string message)
        {
            if (message.StartsWith("Command Name "))
            {
                string newName = ExtractNameFromCommand(message);
                // Check for duplicate names only if there's more than one client, 
                // since the client might be trying to set its initial name.
                if (!IsNameDuplicate(newName)) // Simplified the check condition.
                {
                    client.ID = newName;
                    _logger.LogInformation($"{client.TcpClient.Client.RemoteEndPoint} has set their name to {newName}.");
                    // Confirm name set
                    client.SendAsync(message);
                    Dispatcher.Dispatch(() => UpdateUserList());

                }
                else
                {
                    client.SendAsync("NAME REJECTED \n");
                }
            }
            else if (message.StartsWith("Command Participants"))
            {
                await SendParticipantsListToClient(client);
                Dispatcher.Dispatch(() => ChatLog.Text += $"System: Command Participants received. User list sent.\n");
                _logger.LogInformation("Participants list sent.");
            }
            else
            {
                // Handle other messages.
                Dispatcher.Dispatch(() => { ChatLog.Text += $"{client.ID}: {message}\n"; });
                BroadcastMessage($"{client.ID}- {message}");
                _logger?.LogDebug("Server onMessage called.");
            }
        }

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        /// <param name="message">The message to be broadcasted.</param>
        private void BroadcastMessage(string message)
        {
            foreach (var client in clients)
            {
                client.SendAsync(message);
                _logger?.LogDebug("Message sent to: " + client.ID);
            }
        }

        /// <summary>
        /// Broadcasts the updated list of participants to all connected clients.
        /// </summary>
        private void UpdateUserList()
        {
            Dispatcher.Dispatch(() => UserList.Text = "");
            foreach (var client in clients)
            {
                Dispatcher.Dispatch(() =>
                {
                    UserList.Text += $"{client.ID}: {client.TcpClient.Client.LocalEndPoint}\n";
                   _logger.LogInformation("Participants list updated.");
                });
            }
        }

        /// <summary>
        /// Sends the current list of participants to a specific client. Used primarily in response to a "Command Participants" request.
        /// </summary>
        /// <param name="client">The client requesting the participant list.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task SendParticipantsListToClient(Networking client)
        {
            string participantsList = $"Command Participants,{string.Join(",", clients.Select(c => $"[{c.ID}]"))}";
            await client.SendAsync(participantsList);
        }

        /// <summary>
        /// Checks if a specified name is already in use by another connected client.
        /// </summary>
        /// <param name="name">The name to check for duplicates.</param>
        /// <returns>True if the name is already in use; otherwise, false.</returns>
        private bool IsNameDuplicate(string name)
        {
            lock (clientsLock) // Ensuring thread-safe access to the clients list.
            {
                foreach (var client in clients)
                {
                    if (client.ID.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true; // Name is found to be duplicate.
                    }
                }
            }
            return false; // No duplicates found.
        }

        /// <summary>
        /// Extracts a client's new name from a name change command.
        /// </summary>
        /// <param name="command">The command containing the new name.</param>
        /// <returns>The extracted name.</returns>
        private string ExtractNameFromCommand(string command)
        {
            string name = command.Substring("Command Name ".Length);
            return name.Trim('[', ']').Trim();
        }

        /// <summary>
        /// Retrieves the server's local IP address to be displayed in the server UI.
        /// </summary>
        /// <returns>The local IP address of the server.</returns>
        private string FetchLocalIPAddress()
        {
            ServerName.Text = Dns.GetHostName();
            var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            var ip = hostAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            IPAddress.Text = ip.ToString();
            return ip?.ToString() ?? "IP not found";
        }

        /// <summary>
        /// Helper method for server name changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerNameCompleted(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() => ChatLog.Text += $"System: Host name changed to {ServerName.Text}");
            _logger.LogDebug("Host name changed \n");
        }
        /// <summary>
        /// Server name change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerNameChanged(object sender, TextChangedEventArgs e)
        {
            ServerName.Text = e.NewTextValue;
            _logger.LogDebug($"Server Name was changed.\n");
        }
    }
}
