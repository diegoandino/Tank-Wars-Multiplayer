using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// This Class represents our Tank Object.  When updating a tank we ensure the
    /// correct one is updated by checking it's ID.
    /// @ Authors Diego Andino, Tarik Vu.
    /// CS3500 Fall 2020 Semester
    /// </summary>
    public class Tank
    {
        [JsonProperty(PropertyName = "tank", Order = 1)]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "loc", Order = 2)]
        public Vector2D location { get; set; }

        [JsonProperty(PropertyName = "bdir", Order = 3)]
        public Vector2D orientation { get; set; }

        [JsonProperty(PropertyName = "tdir", Order = 4)]
        private Vector2D aiming = new Vector2D(0, -1);

        [JsonProperty(PropertyName = "name", Order = 5)]
        public string name { get; private set; }

        [JsonProperty(PropertyName = "hp", Order = 6)]
        public int hitPoints { get; set; }

        [JsonProperty(PropertyName = "score", Order = 7)]
        public int score { get; private set; }

        [JsonProperty(PropertyName = "died", Order = 8)]
        public bool died { get; set; }

        [JsonProperty(PropertyName = "dc", Order = 9)]
        public bool disconnected { get; set; }

        [JsonProperty(PropertyName = "join", Order = 10)]
        private bool joined = false;

        /// <summary>
        /// Default constructor for the tanks
        /// </summary>
        public Tank(int id, int hp, int score, string name, Vector2D location, Vector2D tdir, Vector2D bdir, bool died, bool disconnected, bool joined)
        {
            ID = id;
            hitPoints = hp;
            aiming = tdir;
            orientation = bdir;

            this.score = score;
            this.location = location;
            this.name = name;
            this.died = died;
            this.disconnected = disconnected;
            this.joined = joined;

            // SERVER SIDE: Setting the default movement requests to false
            reqNone = false;
            reqUp = false;
            reqDown = false;
            reqLeft = false;
            reqRight = false;

            fireCD = new Stopwatch();
            fireCD.Start();
        }


        /// <summary>
        /// Takes in a tank t with the same id with updated information after being deserialized from server
        /// Throws Argument Exception if ID's dont match.
        /// </summary>
        /// <param name="t">Deserialized tank with updated information</param>
        /// <returns>returns this tank with updated information from tank t.</returns>
        internal Tank Update(Tank t)
        {
            // Id's MUST match.
            if (ID != t.ID)
                throw new ArgumentException("Cannot update tank info.\n Mismatched ID's");

            // Updating everything but ID and Name.
            location = t.location;
            orientation = t.orientation;
            aiming = t.aiming;
            hitPoints = t.hitPoints;
            score = t.score;
            died = t.died;
            disconnected = t.disconnected;
            joined = t.joined;

            // Return this tank to be updated in our world's dictionary
            return this;
        }

        /// <summary>
        /// Public method to return the aiming 2D Vector.
        /// </summary>
        /// <returns></returns>
		public Vector2D GetAiming()
        {
            return aiming;
        }

        /// <summary>
        /// Public method to set our turret's angle
        /// </summary>
        /// <param name="angle"></param>
        public void SetAiming(Vector2D angle)
        {
            aiming = angle;
        }

        ///--------------------------------------------///
        ///--------------------------------------------///
        ///-------------SERVER SIDE CODE---------------///
        ///--------------------------------------------///
        ///--------------------------------------------///
        
        // Request Booleans for Firing
        [JsonIgnore]
        public bool reqNone { get; set; }
        [JsonIgnore]
        public bool reqUp { get; set; }
        [JsonIgnore]
        public bool reqDown { get; set; }
        [JsonIgnore]
        public bool reqLeft { get; set; }
        [JsonIgnore]
        public bool reqRight { get; set; }

        // Cooldowns for tanks
        [JsonIgnore]
        public Stopwatch fireCD { get; set; }
        
        // Tank's powerup count
        [JsonIgnore]
        public int PowerupCount { get; set; }
        
        // Tank boolean to check if it collided 
        [JsonIgnore]
        public bool Collided { get; set; }
        
        // Tank's temporary location for collision checking
        [JsonIgnore]
        public Vector2D tempLocation { get; set; }


		/// <summary>
		/// setting this tank's movement request for our server to read and to approve / ignore
		/// </summary>
		/// <param name="req">This tank's movement request to the server</param>
		public void SetMoveRequest(string req)
        {
            reqNone = false;
            reqUp = false;
            reqDown = false;
            reqLeft = false;
            reqRight = false;

            if (req.Equals("none"))
                reqNone = true;

            if (req.Equals("up"))
                reqUp = true;

            if (req.Equals("down"))
                reqDown = true;

            if (req.Equals("left"))
                reqLeft = true;

            if (req.Equals("right"))
                reqRight = true;
        }

        
        /// <summary>
        /// Toggles whether or not this Tank is dead
        /// </summary>
        public void ToggleDead()
        {
            if (this.died)         
                died = false;
            else          
                died = true;
          
        }


        /// <summary>
        /// Decreases the Tank's Hp
        /// </summary>
        public void DecreaseHP()
		{
            hitPoints -= 1;
		}


        /// <summary>
        /// Kills tank immediately. 
        /// </summary>
        public void InstaKill()
		{
            hitPoints = 0;
		}


        /// <summary>
        /// Reset this tank's Hp
        /// </summary>
        public void ResetHP()
        {
            hitPoints = 3;
        }

        /// <summary>
        /// Adds to this player's score.
        /// </summary>
		public void AddToScore()
		{
            score += 1;
		}

	}
}
