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
using System.Numerics;
using System.Text.Json.Serialization;

namespace AgarioModels
{
    public class GameObject
    {
        public long ID { get; private set; }

        public Vector2 Location { get; private set; }

        public float X { get; set; }

        public float Y { get; set; }

        public int ARGBColor { get; private set; }

        public float Mass { get; private set; }

        /// <summary>
        /// Radius depends on mass
        /// </summary>
        public float Radius
        {
            get { return (float)Math.Sqrt(Mass / Math.PI); }
        }
        [JsonConstructor]
        public GameObject(float X, float Y, int ARGBColor, long ID, float Mass)
        {
            Location = new Vector2(X, Y);
            this.X = Location.X;
            this.Y = Location.Y;
            this.ARGBColor = ARGBColor;
            this.ID = ID;
            this.Mass = Mass;
        }
    }
}