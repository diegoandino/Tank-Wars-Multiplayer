using GameModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TankWars;

namespace Server
{
    /// <summary>
    /// This class represents our world from the server side. This class takes care of keeping track of our
    /// objects, their locations, as well as the given state of our objects. Some objects utilize a Stopwatch for 
    /// respawning / duration purposes.
    /// @Authors Tarik Vu, Diego Andino.
    /// CS3500 Fall 2020 Semester
    /// </summary>
    class ServerWorld
    {
        /// <summary> Dictionary to store the Tanks in. </summary>
        private Dictionary<long, Tank> tanks;

        /// <summary>Dictionary to store the walls in.</summary>
        private Dictionary<long, Wall> walls;

        /// <summary>Dictionary to store the Projectiles.</summary>
        private Dictionary<long, Projectile> projectiles;

        /// <summary> Dictionary to store the Powerups.</summary>
        private Dictionary<long, PowerUp> powerups;

        /// <summary>Our dead powerups. </summary>
        private Dictionary<long, Stopwatch> deadPowerups;

        /// <summary>Our Dead Tanks</summary>
        private Dictionary<long, Stopwatch> deadTanks;

        /// <summary>Dictionary for Beams <summary>
        private Dictionary<long, Beam> beams;

        /// <summary>This HashSet will hold all of the unique keys of the objects in our world. </summary>
        private HashSet<long> KeyLib;

        /// <summary>
        /// This random object helps generate things such as ID's for objects
        /// as well as is used in calculating locations
        /// </summary>
        private Random r;

        /// <summary>
        /// Private dictionary holding a tuple that dictates the
        /// max and min x,y values of our spawn zones. 
        /// Each zone has their own (int) ID.
        /// </summary>
        private Dictionary<int, Zone> spawnZones;

        /// <summary>
        /// Playable world area determined by the server's world size.
        /// </summary>
        private Zone worldZone;

        // Constants for our world
        private int WORLD_SIZE;
        private int MAX_PWRUPS;
        private double PROJECTILE_SPEED;
        private double TANK_SPEED;
        private double RESPAWN_RATE;
        private double FIRE_COOLDOWN;


        /// <summary>
        /// A stuct to dictate our spawning / playable world zones.
        /// </summary>
        struct Zone
        {
            // Boundaries for this given zone
            public double X_Min; public double X_Max;
            public double Y_Min; public double Y_Max;

            /// <summary>
            /// Constructs a Zone with cordinates to use for spawning
            /// </summary>
            /// <param name="X_Min">Minimum x cordinate of this zone</param>
            /// <param name="X_Max">Maximum x cordinate of this zone</param>
            /// <param name="Y_Min">Minimum y cordinate of this zone</param>
            /// <param name="Y_Max">Maximum y cordinate of this zone</param>
            public Zone(double X_Min, double X_Max, double Y_Min, double Y_Max)
            {
                this.X_Min = X_Min;
                this.X_Max = X_Max;

                this.Y_Min = Y_Min;
                this.Y_Max = Y_Max;
            }
        }


        /// <summary>
        /// Constructor for the Server World.
        /// We set things such as our fire rate, tank speed, proj speed, etc. 
        /// These values are based on what the constants from our server allows. 
        /// </summary>
        public ServerWorld(double PROJECTILE_SPEED, double TANK_SPEED, int MAX_PWRUPS, double RESPAWN_RATE, double FIRE_COOLDOWN, int WORLD_SIZE)
        {
            // Dictionaries for our objects
            tanks = new Dictionary<long, Tank>();
            walls = new Dictionary<long, Wall>();
            beams = new Dictionary<long, Beam>();
            powerups = new Dictionary<long, PowerUp>();
            projectiles = new Dictionary<long, Projectile>();
            spawnZones = new Dictionary<int, Zone>();

            // Dictionaries for our DEAD objects to be respawned
            deadTanks = new Dictionary<long, Stopwatch>();
            deadPowerups = new Dictionary<long, Stopwatch>();

            // Constants for this world, provided by server.
            this.WORLD_SIZE = WORLD_SIZE;
            this.TANK_SPEED = TANK_SPEED;
            this.MAX_PWRUPS = MAX_PWRUPS;
            this.RESPAWN_RATE = RESPAWN_RATE;
            this.FIRE_COOLDOWN = FIRE_COOLDOWN;
            this.PROJECTILE_SPEED = PROJECTILE_SPEED;

            // Random object and Key libary.
            r = new Random();
            KeyLib = new HashSet<long>();

            // Game world Zone
            worldZone = new Zone(WORLD_SIZE / 2 * -1, WORLD_SIZE / 2, WORLD_SIZE / 2 * -1, WORLD_SIZE / 2);

            PrepareSpawn();
            CacheWalls();
            LoadPowerups();
        }


        /// <summary>
        /// Prepares our spawn locations for tanks, and objects.
        /// All spawn locations are located inside our world and outside
        /// of tanks.
        /// </summary>
        private void PrepareSpawn()
        {
            Zone top = new Zone(-900, 900, -900, -500);
            Zone bot = new Zone(-900, 900, 500, 900);
            Zone left = new Zone(-700, -500, -500, 500);
            Zone right = new Zone(500, 700, -500, 700);
            Zone center = new Zone(-160, 160, -200, 200);

            spawnZones.Add(1, top);
            spawnZones.Add(2, bot);
            spawnZones.Add(3, left);
            spawnZones.Add(4, right);
            spawnZones.Add(5, center);
        }


        /// <summary>
        /// Loads in our powerups.
        /// </summary>
        private void LoadPowerups()
        {
            for (int i = 0; i < MAX_PWRUPS; i++)
            {
                PowerUp p = new PowerUp(GetRandomID(), GetRandomLoc(), false);
                powerups.Add(p.ID, p);
            }
        }


        /// <summary>
        /// Caches the walls in the World (server) for later use by opening our Settings.XML file.
        /// Throws Arguement exception if we have an issue caching the walls.
        /// </summary>
        /// <returns></returns>
        private void CacheWalls()
        {
            // Get Path
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo gameDir = di.Parent.Parent.Parent;
            string path = gameDir.FullName + "\\Resources\\settings.xml";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                foreach (XmlNode node in doc.DocumentElement)
                {
                    string name = node.Name;
                    if (name == "Wall")
                    {
                        //Checks for combination <p1> then <p2>
                        Vector2D vector1 = new Vector2D();
                        Vector2D vector2 = new Vector2D();

                        if (node.FirstChild.Name == "p1" || node.FirstChild.Name == "p2")
                        {
                            string[] points = { node.FirstChild.FirstChild.InnerText, node.FirstChild.LastChild.InnerText };

                            Int32.TryParse(points[0], out int x);
                            Int32.TryParse(points[1], out int y);

                            vector1 = new Vector2D(x, y);
                        }

                        //Checks for combination <p2> then <p1>
                        if (node.LastChild.Name == "p2" || node.LastChild.Name == "p1")
                        {
                            string[] points = { node.LastChild.FirstChild.InnerText, node.LastChild.LastChild.InnerText };

                            Int32.TryParse(points[0], out int x);
                            Int32.TryParse(points[1], out int y);

                            vector2 = new Vector2D(x, y);
                        }

                        AddWall(vector1, vector2);
                    }
                }
            }

            catch
            {
                throw new ArgumentException("Error Caching Walls");
            }
        }


        /// <summary>
        /// Generates a random ID for world objects.
        /// HashSet needs to be cleared after object dies.
        /// </summary>
        /// <returns></returns>
        private int GetRandomID()
        {
            int n = r.Next(1, 10000);

            // Re-roll until it finds a unique ID
            while (KeyLib.Contains(n))
            {
                n = r.Next(1, 10000);
            }

            KeyLib.Add(n);
            return n;
        }


        /// <summary>
        /// Adds a tank to the world (server).    
        /// </summary>
        public void AddTank(long id, string name)
        {
            Vector2D location = GetRandomLoc();
            Vector2D tdir = new Vector2D(1, 0); tdir.Normalize();
            Vector2D bdir = new Vector2D(0, -1); bdir.Normalize();
            name = name.TrimEnd('\n');

            Tank t = new Tank((int)id, 3, 0, name, location, tdir, bdir, false, false, true);

            lock (tanks)
            {
                if (!tanks.ContainsKey(id))
                    tanks.Add(id, t);
            }
        }


        /// <summary>
        /// Get individual tank by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tank GetTank(long id)
        {
            if (tanks.ContainsKey(id))
                return tanks[id];
            throw new ArgumentNullException("Tank does not exist");
        }


        /// <summary>
        /// Adds walls to the world (server).
        /// </summary>
        public void AddWall(Vector2D p1, Vector2D p2)
        {
            Wall w = new Wall(walls.Count, p1, p2);

            lock (walls)
            {
                walls.Add(walls.Count, w);
            }
        }


        /// <summary>
        /// Public method to get all walls when needed by the client.
        /// </summary>
        /// <returns></returns>
        public string GetWalls()
        {
            if (walls.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            foreach (Wall wall in walls.Values)
            {
                sb.Append(JsonConvert.SerializeObject(wall) + "\n");
            }

            string json = sb.ToString().Substring(0, sb.Length - 1);
            return json;
        }


        /// <summary>
        /// Returns the state of the world as a JSON string to be sent to each client.
        /// When a tank disconnects, we send it as died with 0 HP left with disconnected set to true.
        /// </summary>
        /// <returns></returns>
        internal string GetWorld()
        {
            StringBuilder sb = new StringBuilder();

            lock (tanks)
            {
                foreach (Tank tank in tanks.Values.ToList())
                {
                    if (tank.disconnected == true)
					{
                        tank.died = true;
                        tank.hitPoints = 0;
                        tanks.Remove(tank.ID);

                        sb.Append(JsonConvert.SerializeObject(tank) + "\n");
                    }

                    else
                        sb.Append(JsonConvert.SerializeObject(tank) + "\n");
                }
            }

            lock (projectiles)
            {
                foreach (Projectile proj in projectiles.Values.ToList())
                {
                    sb.Append(JsonConvert.SerializeObject(proj) + "\n");
                }
            }

            lock (powerups)
            {
                foreach (PowerUp pow in powerups.Values.ToList())
                {
                    sb.Append(JsonConvert.SerializeObject(pow) + "\n");
                }
            }

            lock (beams)
            {
                foreach (Beam beam in beams.Values.ToList())
                {
                    sb.Append(JsonConvert.SerializeObject(beam) + "\n");

                }
            }

            string json = sb.ToString().Substring(0, sb.Length - 1);
            return json;
        }


        /// <summary>
        /// Private helper method to update the state of the objects inside our world.
        /// Tanks well have a "Request" to move in a certain direction.  
        /// </summary>
        internal void UpdateWorld()
        {
            // Updating Tanks position (per frame)
            lock (tanks)
            {
                foreach (Tank t in tanks.Values.ToList())
                {
                    // Try to respawn, next frame if alive, we can start moving again.
                    if (t.died)
                    {
                        Respawn(t);
                        continue;
                    }

                    Vector2D position = new Vector2D();
                    Vector2D orientation = new Vector2D();

                    if (t.reqNone)
                    {
                        position = new Vector2D(0, 0);
                        orientation = tanks[t.ID].orientation;
                    }

                    if (t.reqUp)
                    {
                        position = new Vector2D(0, -1) * TANK_SPEED;
                        orientation = new Vector2D(0, -1);
                    }

                    if (t.reqDown)
                    {
                        position = new Vector2D(0, 1) * TANK_SPEED;
                        orientation = new Vector2D(0, 1);
                    }

                    if (t.reqLeft)
                    {
                        position = new Vector2D(-1, 0) * TANK_SPEED;
                        orientation = new Vector2D(-1, 0);
                    }

                    if (t.reqRight)
                    {
                        position = new Vector2D(1, 0) * TANK_SPEED;
                        orientation = new Vector2D(1, 0);
                    }

                    tanks[t.ID].location += position;
                    tanks[t.ID].orientation = orientation;
                }
            }


            // Update projectiles location
            lock (projectiles)
			{
                foreach (Projectile p in projectiles.Values.ToList())
                {
                    // Remove dead projectiles.
                    if (p.died)
                    {
                        projectiles.Remove(p.ID);
                        KeyLib.Remove(p.ID);
                    }

                    Vector2D direction = p.orientation;
                    Vector2D velocity = (direction *= PROJECTILE_SPEED);
                    p.location += velocity;
                }
			}


            // Respawning powerups
            lock (powerups)
            {
                foreach (PowerUp pow in powerups.Values.ToList())
                {
                    if (pow.died)
                        Respawn(pow);
                }
            }


            // Beams
            lock (beams)
            {
                foreach (Beam b in beams.Values.ToList())
                {
                    if (b.duration.ElapsedMilliseconds > 200)
                    {
                        beams.Remove(b.ID);
                        KeyLib.Remove(b.ID);
                    }
                }
            }
        }


        /// <summary>
        /// Respawns our tanks and powerups based on their stopWatch and our RESPAWN_RATE.
        /// If obj is not a tank or powerup, this method does nothing.
        /// </summary>
        /// <param name="obj">Either a Tank or a powerup.</param>
        private void Respawn(object obj)
        {
            lock (deadPowerups)
            {
                if (obj is PowerUp)
                {
                    PowerUp p = obj as PowerUp;

                    // If the stop watch has been going for longer than allowed, 
                    // Remove pow from dead pows and place it back into the world.
                    if (deadPowerups[p.ID].ElapsedMilliseconds > RESPAWN_RATE)
                    {
                        p.ToggleDead();
                        p.location = GetRandomLoc();
                        deadPowerups.Remove(p.ID);
                    }
                }
            }

            // Same logic for our tank
            lock (deadTanks)
            {
                if (obj is Tank)
                {
                    Tank t = obj as Tank;
                    if (deadTanks[t.ID].ElapsedMilliseconds > RESPAWN_RATE)
                    {
                        t.ToggleDead();
                        t.ResetHP();
                        t.location = GetRandomLoc();
                        deadTanks.Remove(t.ID);
                    }
                }
            }
        }


        /// <summary>
        /// Private helper method that returns a random Vector2D location inside the world
        /// boundaries, and outside of walls. Uses our Zone struct that had been loaded.
        /// </summary>
        /// <returns></returns>
        private Vector2D GetRandomLoc()
        {
            int zoneNum = r.Next(1, spawnZones.Count + 1);

            Zone z = spawnZones[zoneNum];
            double x = r.Next((int)z.X_Min, (int)z.X_Max);
            double y = r.Next((int)z.Y_Min, (int)z.Y_Max);

            return new Vector2D(x, y);
        }


        /// <summary>
        /// This method Set the given player's tank movement request
        /// This request is later processed when the seperate thread to update the
        /// world inside of server is called. 
        /// </summary>
        /// <param name="command"></param>
        internal void SetMoveTank(ControlCommand command, long id)
        {
            // Dead tanks cant move
            lock (deadTanks)
			{
                if (deadTanks.ContainsKey(id)) { return; }
			}

            lock (tanks)
			{
                if (tanks[id].Collided == true)
                    tanks[id].tempLocation = tanks[id].location;

                else
                {
                    // Keep the temp. location updated instead of at (0, 0)
                    tanks[id].tempLocation = tanks[id].location;
                    tanks[id].Collided = false;
                }

                tanks[id].SetMoveRequest(command.moving);
			}
        }


        /// <summary>
        /// Handles the turret rotation through JSON commands.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="iD"></param>
        internal void RotateTurret(ControlCommand command, long iD)
        {
            tanks[iD].SetAiming(command.tdir);
        }


        /// <summary>
        /// Handles the turret fire through JSON commands.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="id">The ID of the OWNDER tank firing, not the projectile ID.</param>
        /// <param name="speed"></param>
        internal void FireTurret(ControlCommand command, long id)
        {
            // Only Fire if the Tank's cooldown is reset or if alive.
            if (command.fire == "none" || deadTanks.ContainsKey(id))
                return;

            if (command.fire == "main")
            {
                if (tanks[id].fireCD.ElapsedMilliseconds < FIRE_COOLDOWN)
                    return;
                else
                    tanks[id].fireCD.Restart();

                Projectile p = new Projectile(GetRandomID(), (int)id, false, tanks[id].location, tanks[id].GetAiming());
                projectiles.Add(p.ID, p);
            }

            if (command.fire == "alt")
            {
                if (tanks[id].PowerupCount >= 1)
                {
                    tanks[id].PowerupCount -= 1;
                    Beam b = new Beam(GetRandomID(), (int)id, tanks[id].location, tanks[id].GetAiming());

                    beams.Add(b.ID, b);
                    b.duration.Start();
                }
            }
        }


        /// <summary>
        /// Method that checks for object collision detection.
        /// </summary>
        internal void CheckCollision()
        {
            // Objects must stay in our world.
            WrapAround();

            double tankWidth = 60;
            double wallWidth = 50;
            double tankPadding = tankWidth / 2;
            double wallPadding = wallWidth / 2;

            // Collisions with our walls
            foreach (Wall wall in walls.Values.ToList())
            {
                // Setting the Left and right cordinates of the wall
                // Right sides Always have a greater X value.
                double wallLeft; double wallRight;
                if (wall.GetP1().GetX() < wall.GetP2().GetX())
                {
                    wallLeft = wall.GetP1().GetX() - wallPadding;
                    wallRight = wall.GetP2().GetX() + wallPadding;
                }

                else
                {
                    wallLeft = wall.GetP2().GetX() - wallPadding;
                    wallRight = wall.GetP1().GetX() + wallPadding;
                }

                // Setting Top and bottom cordinates of a wall
                // Bottom Sides always have a greater Y value.
                double wallTop; double wallBot;
                if (wall.GetP2().GetY() < wall.GetP1().GetY())
                {
                    wallTop = wall.GetP2().GetY() - wallPadding;
                    wallBot = wall.GetP1().GetY() + wallPadding;
                }

                else
                {
                    wallTop = wall.GetP1().GetY() - wallPadding;
                    wallBot = wall.GetP2().GetY() + wallPadding;
                }

                // Tank vs. Wall Collision Detection
                foreach (Tank tank in tanks.Values.ToList())
                {
                    // Setting Tank boundaries
                    double tankLeft = tank.location.GetX() - tankPadding;
                    double tankRight = tank.location.GetX() + tankPadding;
                    double tankTop = tank.location.GetY() - tankPadding;
                    double tankBot = tank.location.GetY() + tankPadding;

                    // Normal collision check
                    if (tankLeft < wallRight && tankRight > wallLeft
                        && tankTop < wallBot && tankBot > wallTop)
                    {
                        tank.Collided = true;
                        tank.location = tank.location = tank.tempLocation;
                    }
                }

                lock (projectiles)
				{
                    // Projectile vs Wall Collision
                    foreach (Projectile proj in projectiles.Values.ToList())
                    {
                        if (proj.location.GetX() >= wallLeft && proj.location.GetX() <= wallRight &&
                            proj.location.GetY() >= wallTop && proj.location.GetY() <= wallBot)

                            proj.died = true;
                    }
				}
            }

            // Tank Collision Detection
            foreach (Tank tank in tanks.Values.ToList())
            {
                lock (projectiles)
				{
                    // Tank vs proj.
                    foreach (Projectile proj in projectiles.Values.ToList())
                    {
                        // Dead tanks cant be hit.
                        if (tank.died)
                            continue;

                        // Calculating point on the circle of our tank using radius and angle from projectile                
                        Vector2D distanceFromTank = proj.location - tank.location;
                        Vector2D tankRadius = GetTankRadius(tank.location, proj.location, tankWidth);

                        // Tank gets hit by a projectile
                        if (distanceFromTank.Length() < tankRadius.Length() && (proj.ownerID != tank.ID))
                        {
                            proj.died = true;

                            // TankHit Toggles Dead if its kills tank
                            TankHit(tank.ID, proj.ownerID, proj);

                            // Our tank died on impact, add to deadTanks
                            if (tank.died)
                            {
                                Stopwatch timeToRespawn = new Stopwatch();
                                deadTanks.Add(tank.ID, timeToRespawn);

                                timeToRespawn.Start();
                            }
                        }
                    }
				}

                // Tank vs power up
                foreach (PowerUp pow in powerups.Values.ToList())
                {
                    // Calculating point on the circle of our tank using radius and angle from Powerup
                    Vector2D distanceFromTank = pow.location - tank.location;
                    Vector2D tankRadius = GetTankRadius(tank.location, pow.location, tankWidth);

                    // Tank picks up powerup
                    if (distanceFromTank.Length() < tankRadius.Length() && pow.died == false)
                    {
                        pow.ToggleDead();

                        // Tank's can only have 2 ammo
                        if (tank.PowerupCount < 2)
                            tank.PowerupCount++;

                        Stopwatch timeToRespawn = new Stopwatch();
                        deadPowerups.Add(pow.ID, timeToRespawn);
                        timeToRespawn.Start();
                    }
                }

                // Tanks vs Beams
                foreach (Beam b in beams.Values.ToList())
                {
                    Tank owner = tanks[b.ownerID];

                    if (tank.died)
                        continue;

                    if (Intersects(owner.location, owner.GetAiming(), tank.location, tankWidth / 2))
                    {
                        TankHit(tank.ID, owner.ID, b);

                        // Our tank died on impact, add to deadTanks
                        if (tank.died)
                        {
                            Stopwatch timeToRespawn = new Stopwatch();
                            deadTanks.Add(tank.ID, timeToRespawn);
                            timeToRespawn.Start();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This private method is used to ensure that Any tanks that leave the boundaries
        /// of our world "Wraps Around" to the other side.  Any projectile that leaves these bounds
        /// are instantly killed on that frame. This logic does not apply to beams.  We check the bounds by using
        /// worldZone which was set by our server upon WorldServer's construction, and World Size.
        /// 
        /// If a tank is ever outside of the world's bounds, we spawn it dead center.
        /// </summary>
        private void WrapAround()
        {
            // Checking Tank's to ensure they stay within bounds
            lock (tanks)
            {
                foreach (Tank t in tanks.Values.ToList())
                {
                    // If tank spawns outside of the world, spawn dead center
                    if (!(t.location.GetX() > worldZone.X_Min) && !(t.location.GetX() < worldZone.X_Max))
                        t.location = new Vector2D(0 ,0);

                    if (!(t.location.GetY() > worldZone.Y_Min) && !(t.location.GetY() < worldZone.Y_Max))
                        t.location = new Vector2D(0, 0);


                    // Tank is leaving world on the left side
                    if (t.location.GetX() <= worldZone.X_Min)
                        t.location = new Vector2D(t.location.GetX() + WORLD_SIZE, t.location.GetY());
                    
                    // Leaving Right Side
                    if (t.location.GetX() >= worldZone.X_Max)
                        t.location = new Vector2D(t.location.GetX() - WORLD_SIZE, t.location.GetY());
                    
                    // Leaving Top Side
                    if (t.location.GetY() <= worldZone.Y_Min)
                        t.location = new Vector2D(t.location.GetX(), t.location.GetY() + WORLD_SIZE);
                    
                    // Leaving Bot Side
                    if (t.location.GetY() >= worldZone.Y_Max)
                        t.location = new Vector2D(t.location.GetX(), t.location.GetY() - WORLD_SIZE);
                }
            }

            // Projectiles die when leaving world bounds.
            lock (projectiles)
            {
                foreach (Projectile p in projectiles.Values.ToList())
                {
                    if (p.location.GetX() <= worldZone.X_Min || p.location.GetX() >= worldZone.X_Max ||
                        p.location.GetY() <= worldZone.Y_Min || p.location.GetY() >= worldZone.Y_Max)
                        
                        p.died = true;
                }
            }
        }


        /// <summary>
        /// Determines if a ray intersects a circle.
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        private static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }


        /// <summary>
        /// Returns a Vector2D dictating a point on the tank's radius relative to the angle of the
        /// object (projectile / powerup). Used for collision detection for Tanks calulated as a circle
        /// and another objects locatiton Vector2D.
        /// </summary>
        /// <param name="tank">Vector2D location of the tank</param>
        /// <param name="obj">Vector2D location of the object</param>
        /// <param name="tankWidth">Width of the tank</param>
        /// <returns></returns>
        private Vector2D GetTankRadius(Vector2D tank, Vector2D obj, double tankWidth)
        {
            double radius = tankWidth / 2;
            float angle = Vector2D.AngleBetweenPoints(tank, obj);

            float x = (float)(Math.Cos(angle) * radius);
            float y = (float)(Math.Sin(angle) * radius);

            return new Vector2D(x, y);
        }


        /// <summary>
        /// Private helper method to decrease a tank's HP.  When the Tank's hp is 
        /// zero, we kill that tank. NOTE that this method WILL toggle a tank's dead status.
        /// If hit by a projectile, decrement hp by 1, if by a Beam, kill instantly.
        /// </summary>
        /// <param name="tankDamagedID">Tank being hurt</param>
        /// <param name="ownerOfProjID">Owner id of tank that shot</param>
        /// <param name="obj">what the tank was hit by (projectile or beam).</param>
        private void TankHit(int tankDamagedID, int ownerOfProjID, object obj)
        {
            Tank dealer = tanks[ownerOfProjID];
            Tank receiver = tanks[tankDamagedID];

            if (obj is Projectile)
            {
                if (receiver.hitPoints == 1)
                {
                    receiver.DecreaseHP();
                    receiver.ToggleDead();
                    dealer.AddToScore();
                }

                else
                    receiver.DecreaseHP();
            }

            // Instant kill
            if (obj is Beam)
            {
                receiver.InstaKill();
                receiver.ToggleDead();
                dealer.AddToScore();
            }
        }

    }
}
