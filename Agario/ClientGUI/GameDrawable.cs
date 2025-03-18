/// <summary>
/// Author:    Thu Ha
/// Partner:   None
/// Date:      04/20/2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Thu Ha - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, Thu Ha, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source. All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// This file contains the GameDrawable class, which is a part of the ClientGUI namespace.
/// The GameDrawable class extends ScrollView and implements the IDrawable interface, functioning
/// as a dynamic drawing surface for the Agar.io-like game. It handles all graphical output related to 
/// the game world, including the rendering of players, food, and the game grid based on the current 
/// game state and player interactions. This class dynamically adjusts what is shown on screen to 
/// provide an optimized and smooth gaming experience.
/// </summary>
#if MACCATALYST || ANDROID || IOS
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using System.Reflection;
using IImage = Microsoft.Maui.Graphics.IImage;
using AgarioModels;

namespace ClientGUI;
public class GameDrawable : ScrollView, IDrawable
{
    private IImage welcomeScreen;

    public World World;
    public delegate void ObjectDrawer(GameObject o, ICanvas canvas);
    public int PlayerMass { get; private set; } = 150;
    public bool DefaultTheme { get; set; }
    private GraphicsView PlaySurface = new GraphicsView();
    public int ViewSize = 800;
    private float left;
    private float top;
    private float right;
    private float bottom;
    /// <summary>
    /// Initializes a new instance of the GameDrawable class, setting up the default configurations for the game's drawing surface.
    /// </summary>
    public GameDrawable()
    {
        BackgroundColor = Colors.Black;
        PlaySurface.Drawable = this;
        Content = PlaySurface;
        DefaultTheme = true;
        PlaySurface.WidthRequest = ViewSize;
        PlaySurface.HeightRequest = ViewSize;
    }
    /// <summary>
    /// Sets the world model to be drawn.
    /// </summary>
    /// <param name="world">The world object containing all game data to be rendered.</param>
    public void DrawWorld(World world)
    {
        World = world;
    }

    /// <summary>
    /// Main drawing method for the game interface, updates the canvas with the current game state including players and food.
    /// </summary>
    /// <param name="canvas">Canvas used for drawing.</param>
    /// <param name="dirtyRect">The rectangle area that needs to be updated (not used in this implementation).</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        defaultThemeBackground(canvas, dirtyRect);// Draw the background 
        if (!DefaultTheme)
        {
            darkModeOn(canvas, dirtyRect);
        }
        canvas.ResetState();// Reset any previous drawing states
        if (World != null && World.UserID > -1)
        {
            // center viewsize
            Player currPlayer = World.Players[World.UserID];
            float playerX = (float)currPlayer.X;
            float playerY = (float)currPlayer.Y;
            float zoomSize = currPlayer.Mass / 100 + 700;
            left = playerX - zoomSize;
            right = playerX + zoomSize;
            bottom = playerY - zoomSize;
            top = playerY + zoomSize;

            lock (World.Players)
            {
                foreach (Player player in World.Players.Values)
                {
                    if (IsInViewport(player, left, top, right, bottom))
                    {
                        DrawPlayer(canvas, player, left, bottom, zoomSize);
                    }
                }
            }

            lock (World.FoodList)
            {
                foreach (Food food in World.FoodList.Values)
                {
                    if (IsInViewport(food, left, top, right, bottom))
                    {
                        DrawFood(canvas, food, left, bottom, zoomSize);
                    }
                }
            }
        }
        else
        {
            welcomeScreen = processingBackground("welcomescreen.png");
            canvas.DrawImage(welcomeScreen, 0, 0, ViewSize, ViewSize);
        }
    }
    /// <summary>
    /// Draws a player on the canvas.
    /// </summary>
    /// <param name="canvas">Canvas used for drawing.</param>
    /// <param name="player">Player object to draw.</param>
    /// <param name="left">Left boundary of the drawing area.</param>
    /// <param name="top">Top boundary of the drawing area.</param>
    /// <param name="zoomView">Scaling factor for the view.</param>
    private void DrawPlayer(ICanvas canvas, Player player, float left, float top, float zoomView)
    {
        ConvertFromWorldToScreen(player, zoomView, left, top, out float ratioX, out float ratioY);
        canvas.FillColor = Color.FromInt(player.ARGBColor);
        canvas.FillCircle(ratioX * ViewSize, ratioY * ViewSize, player.Radius);
        canvas.StrokeColor = Colors.Black;
        canvas.DrawString(player.Name, ratioX * ViewSize, (ratioY * ViewSize) - player.Radius - 10, HorizontalAlignment.Center);
    }
    /// <summary>
    /// Draws food on the canvas.
    /// </summary>
    /// <param name="canvas">Canvas used for drawing.</param>
    /// <param name="food">Food object to draw.</param>
    /// <param name="left">Left boundary of the drawing area.</param>
    /// <param name="top">Top boundary of the drawing area.</param>
    /// <param name="zoomView">Scaling factor for the view.</param>
    private void DrawFood(ICanvas canvas, Food food, float left, float top, float zoomView)
    {
        ConvertFromWorldToScreen(food, zoomView, left, top, out float ratioX, out float ratioY);
        canvas.FillColor = Color.FromInt(food.ARGBColor);
        canvas.FillCircle(ratioX * ViewSize, ratioY * ViewSize, food.Radius);
    }
    /// <summary>
    /// Checks if a game object is within the current viewport.
    /// </summary>
    /// <param name="obj">Game object to check.</param>
    /// <param name="left">Left boundary of the viewport.</param>
    /// <param name="top">Top boundary of the viewport.</param>
    /// <param name="right">Right boundary of the viewport.</param>
    /// <param name="bottom">Bottom boundary of the viewport.</param>
    /// <returns>true if the object is within the viewport; otherwise, false.</returns>
    private bool IsInViewport(GameObject obj, float left, float top, float right, float bottom)
    {
        return (obj.X + obj.Radius) > left && (obj.Y - obj.Radius) < top && (obj.X - obj.Radius) < right && (obj.Y + obj.Radius) > bottom;
    }

    /// <summary>
    /// Converts world coordinates to screen coordinates.
    /// </summary>
    /// <param name="obj">The game object to convert.</param>
    /// <param name="zoomView">Zoom level of the view.</param>
    /// <param name="leftBound">Left boundary of the screen.</param>
    /// <param name="bottom">Bottom boundary of the screen.</param>
    /// <param name="ratioX">Output parameter for the X coordinate on the screen.</param>
    /// <param name="ratioY">Output parameter for the Y coordinate on the screen.</param>
    private void ConvertFromWorldToScreen(
                  in GameObject obj, in float zoomView, in float leftBound, in float bottom,
                  out float ratioX, out float ratioY)
    {
        float offsetX = obj.X - leftBound;
        float offsetY = obj.Y - bottom;
        ratioX = offsetX / (zoomView * 2);
        ratioY = offsetY / (zoomView * 2);
    }

#if MACCATALYST || ANDROID || IOS
    private IImage processingBackground(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "ClientGUI.Resources.Images";
        return PlatformImage.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#else
    private IImage processingBackground(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "ClientGUI.Resources.Images";
        var processing = new W2DImageLoadingService();
        return processing.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#endif
    private void defaultThemeBackground(ICanvas canvas, RectF rectF)
    {
        float gridSize = 50; // Size of each grid cell
        canvas.FillColor = Colors.LightGray;
        canvas.FillRectangle(rectF);
        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.Black;

        for (float x = 0; x <= ViewSize; x += gridSize)
        {
            canvas.DrawLine(x, 0, x, ViewSize);
        }

        for (float y = 0; y <= ViewSize; y += gridSize)
        {
            canvas.DrawLine(0, y, ViewSize, y);
        }
    }
    private void darkModeOn(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(dirtyRect);
    }
    /// <summary>
    /// Calculates the player's score based on their mass.
    /// </summary>
    /// <param name="player">Player whose score is to be calculated.</param>
    /// <returns>The calculated score.</returns>
    public float getScore(Player player)
    {
        float finalMass = player.Mass - 150;
        if (finalMass < 0)
            return -1;
        else if (finalMass < 100 && finalMass > 0)
        {
            return player.Mass;
        }
        else if (finalMass >= 100 && finalMass < 200)
        {
            return (float)(100 + 1.5 * (player.Mass - 100));
        }
        else
        {
            return (float)(250 + 2 * (player.Mass - 200));
        }
    }
}