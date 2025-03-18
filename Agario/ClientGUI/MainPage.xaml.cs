/// <summary>
/// Author:    [Thu Ha]
/// Partner:   None
/// Date:      [04/20/2024]
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and [Thu Ha] - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, [Thu Ha], certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents (README file)
/// </summary>
using AgarioModels;
using Communications;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Timers;
/// <summary>
/// Contains the client-side GUI components and logic for an Agar.io-like game, handling user interactions,
/// game state updates, and network communication.
/// </summary>
namespace ClientGUI;
/// <summary>
/// The main page of the game, handling user inputs, rendering, and game mechanics.
/// </summary>
public partial class MainPage : ContentPage
{
    private ILogger<MainPage> _logger;
    private World world;
    private Networking client;
    private List<Player> playerList = null;
    private List<Food> foods = null;

    private int posX;
    private int posY;
    private string playerName;
    private bool isAlive;
    // Extra feature 
    private bool isHackModeOn = false;

    private DateTime gameTimer;
    private System.Timers.Timer timer;

    /// <summary>
    /// Constructor for MainPage that initializes components and sets up the game world.
    /// </summary>
    /// <param name="logger">Logger instance to output debug and runtime information.</param>
    public MainPage(ILogger<MainPage> logger)
    {
        _logger = logger;
        InitializeComponent();
        world = new World(logger);
        isAlive = true;
        _logger.LogInformation("Initial Component done!");
    }

    /// <summary>
    ///   <para>
    ///     When the connect button is clicked, connect to the server program
    ///     and start the sequence of communications. 
    ///   </para>
    /// </summary>
    /// <param name="sender"> ignored </param>
    /// <param name="e"> ignored </param>
    private async void ConnectBtnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(localHost.Text) || string.IsNullOrEmpty(userNameEntry.Text))
        {
            Dispatcher.Dispatch(() => ErrorBox.Text = "Incomplete input. Please enter both name and ip address before starting.");
        }
        else
        {
            playerName = userNameEntry.Text;
            if (client == null)
            {
                client = new Networking(_logger, OnConnect, OnDisconnect, OnMessage);
                try
                {
                    await client.ConnectAsync(localHost.Text, 11000);
                    client.HandleIncomingDataAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Connection failed: {ex}");
                    Dispatcher.Dispatch(() => ErrorBox.Text = $"Failed to connect. {ex.Message}");
                    return;
                }
            }
            client.SendAsync(string.Format(Protocols.CMD_Start_Game, playerName));
            world.screenWidth = (float)PlaySurface.Width;
            world.screenHeight = (float)PlaySurface.Height;
            worldPanel.DrawWorld(world);
            _logger.LogInformation($"Game started - player name: {playerName}");
            Dispatcher.Dispatch(() =>
            {
                timer = new System.Timers.Timer(30);
                timer.Elapsed += GameStep;
                timer.Start();
            });
        }
    }
    /// <summary>
    /// Periodically updates the game state, sends move commands, and refreshes the UI based on the timer ticks.
    /// </summary>
    /// <param name="state">State object passed to the Elapsed event.</param>
    /// <param name="e">Event data containing the elapsed event information.</param>
    private void GameStep(object state, ElapsedEventArgs e)
    {
        if (!isAlive)
        {
            HandleGameOver();
            return;
        }
        else
        {
            if (isHackModeOn)
            {
                HackMode();
            }
            Dispatcher.Dispatch(() =>
            {
                client.SendAsync(String.Format(Protocols.CMD_Move, posX, posY));
                PlaySurface.Invalidate();
                Mass.Text = $"Mass: {world.Players[world.UserID].Mass}";
                Location.Text = $"Location: {posX},{posY}";
                Time.Text = $"Time: {(-1) * (gameTimer - DateTime.Now).TotalSeconds}s";
            });
        }
    }
    /// <summary>
    /// Handles the game over scenario, stops the game timer, and prompts the user to restart or exit.
    /// </summary>
    private void HandleGameOver()
    {
        Dispatcher.Dispatch(() =>
        {
            timer.Stop();  // Stop the game timer
            DisplayAlert("Game Over", "You have died. Restart the game?", "OK", "Exit");
            isAlive = true;
            isHackModeOn= false;
        });
    }
    /// <summary>
    /// Activates a "hack" mode where the player can automatically move towards the nearest food or vulnerable player.
    /// </summary>
    private void HackMode()
    {
        var currPlayer = world.Players[world.UserID];
        System.Numerics.Vector2 pos = currPlayer.Location;
        // Go to the closest food
        Food? targetFood = null;
        float foodDistance = 9999f;
        lock (world.FoodList)
        {
            foreach (Food f in world.FoodList.Values)
            {
                var dis = (currPlayer.Location - f.Location).Length();

                if (dis < foodDistance)
                {
                    foodDistance = dis;
                    targetFood = f;
                }
            }

            if (targetFood != null)
            {
                pos = targetFood.Location;
            }
        }

        // If there's a player nearby, chase it down instead
        lock (world.Players)
        {
            Player? nearbyPlayer = null;
            float closestDist = 9999f;
            foreach (Player p in world.Players.Values)
            {
                // if it is us or one of our offspring
                if (p.ID == currPlayer.ID || p.Name == currPlayer.Name) continue;
                float distance = (currPlayer.Location - p.Location).Length();
                if (distance < closestDist)
                {
                    closestDist = distance;
                    nearbyPlayer = p;
                }
            }

            if (nearbyPlayer != null && closestDist < currPlayer.Mass / 2)
            {
                if (nearbyPlayer.Mass < currPlayer.Mass * 0.65)
                {
                    pos = nearbyPlayer.Location;
                }
            }
        }
        // Calculate the actual target position
        System.Numerics.Vector2 distanceVector = pos - currPlayer.Location;
        System.Numerics.Vector2 normalizedDistanceVector = distanceVector / distanceVector.Length();
        System.Numerics.Vector2 movementVector = normalizedDistanceVector * (currPlayer.Mass > 300 ? 1000 : 1000 / 10f);

        System.Numerics.Vector2 adjustedTargetPosition = currPlayer.Location + movementVector;
        posX = (int)adjustedTargetPosition.X;
        posY = (int)adjustedTargetPosition.Y;
    }

    private void OnConnect(Networking channel)
    {
        _logger.LogInformation("Server Connection Established.");
    }
    private void OnDisconnect(Networking channel)
    {
        _logger.LogInformation("Disconnected from server.");
    }
    /// <summary>
    /// Updates player position based on mouse pointer changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Pointer event data.</param>
    private void PointerChanged(object sender, PointerEventArgs e)
    {
        if (world.Players.ContainsKey(world.UserID))
        {
            Point mousePos = (Point)e.GetPosition((View)sender);
            posX = ((int)mousePos.X - (int)world.screenWidth / 2) + (int)world.Players[world.UserID].X;
            posY = ((int)mousePos.Y - (int)world.screenHeight / 2) + (int)world.Players[world.UserID].Y;
        }
    }
    private void PanUpdated(object sender, PanUpdatedEventArgs e) { }
    /// <summary>
    /// Handles tap events by sending a "split" command to the server.
    /// </summary>
    /// <param name="sender">The source of the tap event.</param>
    /// <param name="e">Tapped event data.</param>
    private void OnTap(object sender, TappedEventArgs e)
    {
        if (world.Players.ContainsKey(world.UserID))
        {
            client.SendAsync(string.Format(Protocols.CMD_Split, posX, posY));
            _logger.LogDebug($"Player has split to {posX}, {posY}");
        }
    }
    /// <summary>
    /// Processes messages from the server, updating game state accordingly.
    /// </summary>
    /// <param name="channel">The networking channel receiving the message.</param>
    /// <param name="message">The message content.</param>
    private void OnMessage(Networking channel, string message)
    {
        try
        {
            if (message.Contains(Protocols.CMD_Player_Object))
            {
                world.UserID = long.Parse(message[Protocols.CMD_Player_Object.Length..]);
                PlayerName.Text = $"ID: {playerName} - {world.UserID}";
                _logger.LogInformation($"Client id sent");
            }

            else if (message.Contains(Protocols.CMD_Food))
            {
                Console.WriteLine(message);
                foods = JsonSerializer.Deserialize<List<Food>>(message[Protocols.CMD_Food.Length..]);
                if (foods == null) { return; }
                _logger.LogTrace("Update Food protocol deserialized");
                lock (world.FoodList)
                {
                    foreach (Food food in foods)
                    {
                        world.FoodList.TryAdd(food.ID, food);
                    }
                }
            }

            else if (message.Contains(Protocols.CMD_Update_Players))
            {
                playerList = JsonSerializer.Deserialize<List<Player>>(message[Protocols.CMD_Update_Players.Length..]);
                _logger.LogTrace("Update Players protocol deserialized");
                if (playerList == null) { return; }

                lock (world.Players)
                {
                    foreach (Player player in playerList)
                    {
                        world.Players[player.ID] = player;
                    }
                }
            }
            else if (message.Contains(Protocols.CMD_Dead_Players))
            {
                string msg = message[(Protocols.CMD_Dead_Players.Length + 1)..(message.Length - 1)];
                string[] idList = msg.Split(',');

                _logger.LogInformation($"{idList.Length} players have died");

                if (idList[0] != "")
                {
                    List<long> foodID = new List<long>(idList.Select(long.Parse).ToList());
                    if (foodID.Contains(world.UserID))
                    {
                        isAlive = false;
                        _logger.LogTrace("Dead players command recived.");
                    }
                }
                for (int i = 0; i < idList.Length; i++)
                {
                    if (long.TryParse(idList[i], out long id))
                    {
                        lock (world.Players)
                        {
                            world.Players.Remove(id);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else if (message.Contains(Protocols.CMD_Eaten_Food))
            {
                string msg = message[(Protocols.CMD_Eaten_Food.Length + 1)..(message.Length - 1)];
                string[] idList = msg.Split(',');
                if (idList.Length == 0)
                    return;
                for (int i = 0; i < idList.Length; i++)
                    if (long.TryParse(idList[i], out long id))
                    {
                        lock (world.FoodList)
                        {
                            world.FoodList.Remove(id);
                        }
                    }
                    else
                    {
                        return;
                    }
            }
            else if (message.Contains(Protocols.CMD_HeartBeat))
            {
                _logger.LogTrace("Heart beat command recived.");
            }
        }
        catch (Exception e)
        {
            _logger.LogDebug($"Cannot process message, {e.Message}");
        }
    }
    private void OnSizeAllocated(int size)
    {
        PlaySurface.WidthRequest = size;
        PlaySurface.HeightRequest = size;
        worldPanel.ViewSize = size;
    }
    /// <summary>
    /// Sets the game play area to a small size.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void SetSmallSize(object sender, EventArgs e)
    {
        OnSizeAllocated(600);  // Smaller size dimensions are set here
    }
    /// <summary>
    /// Sets the game play area to a large size.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void SetLargeSize(object sender, EventArgs e)
    {
        OnSizeAllocated(1200);  // Larger size dimensions
    }
    /// <summary>
    /// Resets the game play area to the default size.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void SetDefaultSize(object sender, EventArgs e)
    {
        OnSizeAllocated(800);
    }
    /// <summary>
    /// Resets the visual theme of the game to the default settings.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void SetDefaultTheme(object sender, EventArgs e)
    {
        worldPanel.DefaultTheme = true;
        Dispatcher.Dispatch(() => PlaySurface.Invalidate());
    }
    /// <summary>
    /// Changes the visual theme of the game to a blue color scheme.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void SetBlueTheme(object sender, EventArgs e)
    {
        worldPanel.DefaultTheme = false;
        Dispatcher.Dispatch(() => PlaySurface.Invalidate());
    }
    /// <summary>
    /// Enables the hack mode in the game, allowing automatic movement towards targets.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void HackModeOn(object sender, EventArgs e)
    {
        isHackModeOn = true;
        Dispatcher.Dispatch(() => PlaySurface.Invalidate());
    }

    /// <summary>
    /// Displays About menu content
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AboutClicked(object sender, EventArgs e)
    {
        DisplayAlert("About", "Agar.io Game Project\nImplementation by Mia\nCS 3500, University of Utah", "OK");
    }

    /// <summary>
    /// Provides help information on how to play the game.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data.</param>
    private void HelpClicked(object sender, EventArgs e)
    {
        DisplayAlert("How to Play Agar.io: A Quick Guide",
     "Welcome to Agar.io! Made by Mia :> Control your cell, eat food, and consume smaller cells to grow while avoiding larger cells. Here's how:\n\n" +
     "Basic Controls:\n" +
     "- **Move your Mouse**: The cell follows your cursor. Direct it where you want to go.\n" +
     "- **MouseClicked**: Left-click to split your cell into two for capturing targets or escaping threats.\n" +
     "- **Screen Size**: Choose the World Size you like in Setting-ScreenSize.\n" +
     "- **Dark Mode**: Choose theme in Setting - Dark Mode.\n\n" +
     "Gameplay Tips:\n" +
     "- **Eat to Grow**: Collect food to increase your size. Avoid larger cells which can eat you!\n" +
     "- **Splitting Strategy**: Use splitting to catch smaller cells or escape dangers, but be cautious as it makes you vulnerable.\n" +
     "- **Combining**: Your cells will merge back after splitting. Use this strategically to your advantage.\n" +
     "- **Watch Out for Corners**: Avoid being cornered by larger cells or map edges.\n\n" +
     "Advanced Strategies:\n" +
     "- **Hack Mode**: Turn on hackmode and watch the magic.\n\n" +
     "FAQ:\n" +
     "- **How do I get bigger quickly?** Eat smaller cells and food, and split wisely.\n" +
     "Explore different tactics to find what works best for you. Good luck!",
     "OK");
    }
}