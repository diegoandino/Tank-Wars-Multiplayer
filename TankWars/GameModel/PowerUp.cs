using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Class representing Powerups.
    /// @Authors Tarik Vu, Diego Andino.
    /// CS3500 Fall 2020 Semester
    /// </summary>
    public class PowerUp
    {
        [JsonProperty(PropertyName = "power")]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; set; }

        [JsonProperty(PropertyName = "died")]
        public bool died { get; private set; }

        /// <summary>
        /// Default constructor for the Powerups
        /// </summary>
        public PowerUp(int id, Vector2D location, bool died)
        {
            this.ID = id;
            this.location = location;
            this.died = died;
        }


        /// <summary>
        /// Takes in a PowerUp pw with the same id with updated information after being deserialized from server
        /// Throws Argument Exception if ID's dont match.
        /// </summary>
        /// <param name="pw">Deserialized Power Up with updated information</param>
        /// <returns>returns this Power Up with updated information from Power Up pw.</returns>
        internal PowerUp Update(PowerUp pw)
        {
            if (this.ID != pw.ID)
                throw new ArgumentException("Cannot update powerup info.\n Mismatched ID's.");

            // Update the Power Up except its ID
            location = pw.location;
            died = pw.died;

            return this;
        }

        ///--------------------------------------------///
        ///--------------------------------------------///
        ///-------------SERVER SIDE CODE---------------///
        ///--------------------------------------------///
        ///--------------------------------------------///

        /// <summary>
        /// Toggles whether or not this powup is dead
        /// </summary>
        public void ToggleDead()
        {
            if (this.died)
                died = false;
            else
                died = true;
        }

    }
}
