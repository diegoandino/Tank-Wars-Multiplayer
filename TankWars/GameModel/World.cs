using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace TankWars
{
    ///--------------------------------------------///
    ///--------------------------------------------///
    ///-------------CLIENT SIDE CODE---------------///
    ///--------------------------------------------///
    ///--------------------------------------------///


    /// <summary>
    /// This Class represents our world.  And holds multiple dictionaries 
    /// that hold different objects.  We can then use the state of those objects
    /// to update our world to be drawn.  If an object already exists, it is updated.
    /// If a player (tank) is disconnected, it is removed from it's dictionary.
    /// @ Authors Tarik Vu, Diego Andino.
    /// CS3500 Fall 2020 Semester
    /// </summary>
    public class World
    {
        /// <summary>
        /// Dictionary to store the current players in the world.
        /// </summary>
        public Dictionary<int, Tank> Tanks { get; private set; }


        /// <summary>
        /// Dictionary to store Tank colors with their ID's
        /// </summary>
        public Dictionary<int, string> TankColors { get; private set; }


        /// <summary>
        /// Dictionary to store the current powerups in the world.
        /// </summary>
        public Dictionary<int, PowerUp> PowerUps { get; private set; }


        /// <summary>
        /// Dictionary to store the current walls in the world.
        /// </summary>
        public Dictionary<int, Wall> Walls { get; private set; }


        /// <summary>
        /// Dictionary to store the current beams in the world.
        /// </summary>
        public Dictionary<int, Beam> Beams { get; private set; }


        /// <summary>
        /// Dictionary to store the current projectiles in the world.
        /// </summary>
        public Dictionary<int, Projectile> Projectiles { get; private set; }


        /// <summary>
        /// World size property.
        /// </summary>
        [JsonProperty(PropertyName = "WorldSize")]
        public int size { get; set; }


        /// <summary>
        /// Default constructor for our world
        /// </summary>
        public World()
        {
            Tanks = new Dictionary<int, Tank>();
            TankColors = new Dictionary<int, string>();
            PowerUps = new Dictionary<int, PowerUp>();
            Walls = new Dictionary<int, Wall>();
            Projectiles = new Dictionary<int, Projectile>();
            Beams = new Dictionary<int, Beam>();
        }

        /// <summary>
        /// This method assigns tanks their colors based on their ID number.
        /// First 8 players will have distinguished colors, whereas any 
        /// subsequent player that joins will be assigned a random color.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTankColor(int id)
        {
            if (id == 0)
                return "Blue";

            else if (id == 1)
                return "Red";

            else if (id == 2)
                return "Purple";

            else if (id == 3)
                return "Dark";

            else if (id == 4)
                return "Green";

            else if (id == 5)
                return "Orange";

            else if (id == 6)
                return "LightGreen";

            else if (id == 7)
                return "Yellow";

            // More than 8 players, we give the tank a random color
            else
            {
                Random r = new Random();
                return GetTankColor(r.Next(0, 8));
            }
        }

        /// <summary>
        /// Adding / updating Powerups
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public void AddPowerUp(JObject obj, string message)
        {
            int id = (int)obj.First;
            PowerUp pw = JsonConvert.DeserializeObject<PowerUp>(message);

            if (!PowerUps.ContainsKey(id))
                PowerUps.Add(id, pw);

            else
                PowerUps[id] = PowerUps[id].Update(pw);
        }

        /// <summary>
        /// Adding / updating projectiles
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public void AddProjectile(JObject obj, string message)
        {
            int id = (int)obj.First;
            Projectile proj = JsonConvert.DeserializeObject<Projectile>(message);

            if (!Projectiles.ContainsKey(id))
                Projectiles.Add(id, proj);

            else
                Projectiles[id] = Projectiles[id].Update(proj);
        }

        /// <summary>
        /// Adding / updating beams
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public void AddBeam(JObject obj, string message)
        {
            int id = (int)obj.First;
            Beam beam = JsonConvert.DeserializeObject<Beam>(message);

            if (!Beams.ContainsKey(id))
                Beams.Add(id, beam);
            else
                Beams[id] = Beams[id].Update(beam);
        }

        /// <summary>
        /// Adding Walls to the world,  They are never updated. Only added once.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public void AddWall(JObject obj, string message)
        {
            // Get the object
            int id = (int)obj.First;
            Wall wall = JsonConvert.DeserializeObject<Wall>(message);

            // Add if object does not exist yet.
            if (!Walls.ContainsKey(id))
                Walls.Add(id, wall);
        }

        /// <summary>
        /// Add Tank has slightly different logic where it is the only object that can be 
        /// "Disconnected".  And if so, we remove it from our world.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public void AddTank(JObject obj, string message)
        {
            int id = (int)obj.First;
            Tank tank = JsonConvert.DeserializeObject<Tank>(message);

            // Adding new tanks
            if (!Tanks.ContainsKey(id))
            {
                Tanks.Add(id, tank);
                TankColors.Add(id, GetTankColor(id));
            }

            // If this tank is disconnected, remove it.
            else if (tank.disconnected)
                Tanks.Remove(id);

            // Update the tank
            else
                Tanks[id] = Tanks[id].Update(tank);

        }
    }

}
