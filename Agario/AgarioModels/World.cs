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
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Text.Json;

namespace AgarioModels;

/// <summary>
/// A class represents the wolrd of the game
/// </summary>
public class World
{
    ILogger _logger;
    public readonly int Width = 5000; // Example default width
    public readonly int Height = 5000; // Example default height
    public float screenWidth {  get; set; }
    public float screenHeight { get; set; }

    public Dictionary<long, Player> Players;
    public Dictionary<long, Food> FoodList;
    public long UserID;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="logger"></param>
    public World(ILogger logger)
    {
        Players = new Dictionary<long, Player>();
        FoodList = new Dictionary<long, Food>();
        _logger = logger;
    }
}