using Microsoft.Extensions.Logging;
using NetworkingLibrary;
using System.Net;
using Windows.Media.Protection.PlayReady;

/// <summary> 
/// Author:    Ha Thu 
/// Author:    Amber Tran
/// Date:      31-March-2024
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Ha Thu and Amber Tran d - This work may not be copied for use in Academic Coursework. 
/// 
/// I, Ha Thu, Amber Tran, certify that I wrote this code from scratch and did not copy it in 
/// part or whole from another source.  All references used in the completion of the assignment are 
/// cited in my README file. 
/// 
/// Class Contents 
/// This class is responsible for allowing clients to join an established server. The client has
/// the ability to send unique messages which will allow a changing of the clients displayed
/// name in the server, see a list of participants, and pass a list of participants into the
/// server.Clients can communicate to each other while the server is established.
/// </summary> 
namespace ChatClient
{
    /// <summary>
    /// Main class for the chat client application, handling user interactions, networking, and UI updates.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly ILogger<MainPage> _logger;
        private Networking networkConnection;
        private bool isConnected = false;
        private const int DefaultPort = 11000;
        private string ipAddress;

        /// <summary>
        /// Initializes the chat client, setting up the network connection and UI components.
        /// </summary>
        /// <param name="logger">Logger for capturing application activity.</param>
        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
            _logger.LogInformation("Client initialized");
            networkConnection = new Networking(_logger, OnConnect, HandleDisconnect, OnMessage);
            isConnected = false;
            FetchLocalIPAddress();
        }

        /// <summary>
        /// Retrieves the device's local IP address and updates the UI accordingly.
        /// </summary>
        private void FetchLocalIPAddress()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
            var ip = hostAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            ipAddress = ip?.ToString();
            localHostLabel.Text = ipAddress; // Updates UI with the IP address
        }

        /// <summary>
        /// Initiates a connection to the server using the provided IP address and port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectToServer(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                try
                {
                    await networkConnection.ConnectAsync(ipAddress, DefaultPort);
                    UpdateUIConnectionStatus(true); // Updates UI to reflect connection status
                    _logger.LogInformation($"Client successfully connected to server: {ipAddress}");
                    chatLog.Text += $"Client successfully connected to server: {ipAddress}\n";
                }
                catch (Exception ex)
                {
                    UpdateUIConnectionStatus(false);
                    _logger.LogError($"Failed to connect: {ex.Message}");
                    chatLog.Text += "Couldn't connect to server.\n";
                }
            }
            else
            {
                networkConnection.Disconnect();
                UpdateUIConnectionStatus(false);
            }
        }

        /// <summary>
        /// Callback for successful connection to the server.
        /// </summary>
        /// <param name="client"></param>
        public void OnConnect(Networking client)
        {
            networkConnection.HandleIncomingDataAsync();
            networkConnection.SendAsync($"Command Name [{userNameEntry.Text}]");
            UpdateUIConnectionStatus(client.TcpClient.Client.Connected);
        }

        /// <summary>
        /// Callback for disconnection from the server.
        /// </summary>
        private void HandleDisconnect(Networking client)
        {
            _logger.LogInformation($"{client.ID} disconnected");
            UpdateUIConnectionStatus(false);
        }

        /// <summary>
        /// Processes incoming messages and updates the UI or internal state as necessary.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        private void OnMessage(Networking client, string message)
        {
            _logger.LogInformation($"Message received: {message}");
            Dispatcher.Dispatch(() => UpdateChatLogWithMessage(client, message));
        }

        /// <summary>
        /// Processes a received message, updating the chat log or participants list as necessary.
        /// </summary>
        private void UpdateChatLogWithMessage(Networking client, string message)
        {
            if (message.StartsWith("Command Participants,"))
            {
                UpdateParticipantsList(message);
            }
            else if (message.StartsWith("NAME REJECTED"))
            {
                DisplayAlert("Name Rejected", "The chosen username is already in use. Please select a different username.", "OK");
            }
            else if (message.StartsWith("Command Name "))
            {
                client.ID = message.Substring("Command Name ".Length);
                Dispatcher.Dispatch(() => chatLog.Text += $"Command Name called: {client.ID}\n");
                _logger?.LogDebug("Command Name Called: " + client.ID);
                return;
            }
            else
            {
                chatLog.Text += $"{message}\n";
                _logger?.LogDebug("OnMessage called.");
            }
        }

        /// <summary>
        /// Updates the participants list in the UI based on a message from the server.
        /// </summary>
        /// <param name="message">The message containing the participants list.</param>
        private void UpdateParticipantsList(string message)
        {
            participantsList.Text = "";
            var participants = message.Substring("Command Participants,".Length).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var participant in participants)
            {
                participantsList.Text += $"{participant.Trim('[', ']')}\n";
            }
            _logger.LogInformation("Participants list updated.");
        }

        /// <summary>
        /// Sends a message entered by the user to the server.
        /// </summary>
        private async void SendMessage_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(chatEntry.Text))
            {
                try
                {
                    await networkConnection.SendAsync(chatEntry.Text); // Ensures message ends with newline
                    chatEntry.Text = ""; // Clears input field after sending
                }
                catch (Exception ex)
                {
                    Dispatcher.Dispatch(() =>
                    {
                        chatLog.Text += $"Error sending message: {ex.Message}\n";
                    });
                    _logger.LogError($"Error sending message: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Requests the list of current participants from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RetrieveUsers(object sender, EventArgs e)
        {
            participantsList.Text = ""; // Clears current participants list
            await networkConnection.SendAsync("Command Participants");
        }

        /// <summary>
        /// Requests the server to change name
        /// </summary>
        private void ServerNameCompleted(object sender, EventArgs e)
        {
            networkConnection.ID = userNameEntry.Text;
            if (networkConnection != null && networkConnection.TcpClient != null && networkConnection.TcpClient.Connected)
            {
                networkConnection.SendAsync($"Command Name [{networkConnection.ID}]");
                chatLog.Text += $"User name changed to {networkConnection.ID}\n";
            }
            _logger?.LogInformation($"User name changed to {networkConnection.ID}");

        }

        /// <summary>
        /// Helper method for server name change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerNameChanged(object sender, TextChangedEventArgs e)
        {
            userNameEntry.Text = e.NewTextValue;
        }
        /// <summary>
        /// Helper method handling when changinf ip address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void IPAddressCompleted(object sender, EventArgs e)
        {
            ipAddress = localHostLabel.Text;
            chatLog.Text += $"IP address changed. New host: {ipAddress}\n";
            _logger.LogDebug($"IP address changed. New host: {ipAddress}\n");

        }

        /// <summary>
        /// Helper method for localHost changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IPAddressChanged(object sender, TextChangedEventArgs e)
        {
            localHostLabel.Text = e.NewTextValue;
        }

        /// <summary>
        /// Updates the UI with the new connection status and alters controls accordingly.
        /// </summary>
        /// <param name="isConnected">Whether the client is currently connected to the server.</param>
        private void UpdateUIConnectionStatus(bool isConnected)
        {
            this.isConnected = isConnected;
            ConnectBtn.Text = isConnected ? "Disconnect" : "Connect";
            connectionStatusLabel.Text = isConnected ? "Status: Connected to server" : "Status: Disconnected";
            ConnectBtn.BackgroundColor = isConnected ? Colors.Gray : Colors.Blue;
        }

        /// <summary>
        /// Additional features like settings and about dialogs to enhance user interaction.
        /// </summary>
        private void Settings_Clicked(object sender, EventArgs e)
        {
            // Implement settings feature enhancement here
            _logger.LogInformation("Settings clicked.");
        }

        private void About_Clicked(object sender, EventArgs e)
        {
            _logger.LogInformation("About menu item clicked.");
            DisplayAlert("About", "ChatClient v1.0. Developed with care by Mia and Amber", "OK");
        }
    }
}
