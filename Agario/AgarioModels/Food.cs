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
using System.Text.Json.Serialization;

namespace AgarioModels
{
    public class Food : GameObject
    {
        [JsonConstructor]
        public Food(float X, float Y, int ARGBColor, long ID, float Mass) : base(X, Y, ARGBColor, ID, Mass)
        { }
    }
}