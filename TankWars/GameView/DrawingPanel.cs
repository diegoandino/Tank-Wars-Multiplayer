using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.Generic;

namespace TankWars
{
    /// <summary>
    /// This class is attatched to our form and is to be used as our primary drawer.
    /// Updates and draws objects perframe with Onpaint when invoked.
    /// @ Authors Tarik Vu, Diego Andino.
    /// CS3500 Fall 2020 Semester
    /// </summary>
    public class DrawingPanel : Panel
    {
        /// <summary>
        /// Our world containing our objects to draw.  
        /// </summary>
        private World theWorld;

        /// <summary>
        /// Our game controller.
        /// </summary>
        private GameController theController;

        /// <summary>
        /// Private dictionary to store sprites.
        /// </summary>
        private Dictionary<string, Image> sprites;


        /// <summary>
        /// Private StopWatch to count the FPS.
        /// </summary>
        private Stopwatch sw = new Stopwatch();

        /// <summary>
        /// Private double to store Total FPS.
        /// </summary>
        private double FPS;

        /// <summary>
        /// Private double to store single frame count.
        /// </summary>
        private double FPSCount;

        /// <summary>
        /// The constructor for our Drawing panel.  Takes in a World w.
        /// This class was constructed using Lab 12 as a direct reference.
        /// </summary>
        /// <param name="w">World to draw.</param>
        public DrawingPanel(World w, GameController controller)
        {
            FPSCount = 0;

            DoubleBuffered = true;
            theWorld = w;
            theController = controller;
            sprites = new Dictionary<string, Image>();
        }


        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }


        /// <summary>
        /// Returns the FPS count in seconds.
        /// </summary>
        public double GetFPS()
        {
            return FPS;
        }


        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }


        /// <summary>
        /// Draws our tanks
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            // Setup
            int width = 60;
            int height = 60;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

            // Draw tank with appropriate color
            string color = theWorld.TankColors[t.ID];
            e.Graphics.DrawImage(GetSprite(color + "Tank.png"), r);
        }
        
        
        /// <summary>
        /// Draws our dead tanks
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void DeathDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            // Setup
            int width = 60;
            int height = 60;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

			// Draw death img with appropriate color
			string color = theWorld.TankColors[t.ID];
			e.Graphics.DrawImage(GetSprite(color + "Death.png"), r);
		}


        /// <summary>
        /// Draws the HP bar for the tank using tank's orientation to draw appropriately.
        /// </summary>
        /// <param name="o"> Object o as tank (which tank to draw)</param>
        /// <param name="e"></param>
        private void HPDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            int width = 60;
            int height = 5;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //Differnt colors for differnt amounts of hp
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            {
                // Using Tank Orientation to position HP Bar. 
                // We position the HP bar Above the tank.
                Point barLocation;
                Size barSize;
                Rectangle threeHp;
                Rectangle twoHp;
                Rectangle oneHp;

                // Vertical
                if (t.orientation.GetX() == 0)
                {
                    barSize = new Size(width, height);

                    if (t.orientation.GetY() == -1)
                        barLocation = new Point(-30, -50); // North

                    else
                    {
                        e.Graphics.RotateTransform(180);
                        barLocation = new Point(-30, -50); // South
                    }

                    // Different Rectangle lengths for hp amount
                    threeHp = new Rectangle(barLocation, barSize);

                    twoHp = new Rectangle(barLocation, barSize);
                    twoHp.Width = twoHp.Width - 20;

                    oneHp = new Rectangle(barLocation, barSize);
                    oneHp.Width = oneHp.Width - 40;
                }

                // Horizontal requires different adjustments to height and width.
                else
                {
                    barSize = new Size(height, width);

                    if (t.orientation.GetX() == -1 && t.orientation.GetY() == 0)
                        barLocation = new Point(35, -30); // West
                    else
                    {
                        e.Graphics.RotateTransform(180);
                        barLocation = new Point(35, -30); // East
                    }

                    // For horizontal orientaion we adjust the rectangle's height
                    threeHp = new Rectangle(barLocation, barSize);

                    twoHp = new Rectangle(barLocation, barSize);
                    twoHp.Height = twoHp.Height - 20;

                    oneHp = new Rectangle(barLocation, barSize);
                    oneHp.Height = oneHp.Height - 40;

                }

                // Fill accordingly
                if (t.hitPoints == 3)
                    e.Graphics.FillRectangle(greenBrush, threeHp);

                if (t.hitPoints == 2)
                    e.Graphics.FillRectangle(yellowBrush, twoHp);

                if (t.hitPoints == 1)
                    e.Graphics.FillRectangle(redBrush, oneHp);
            }
        }


        /// <summary>
        /// Draws our Strings and uses our Tank's orientation to draw the string
        /// in the appropriate location.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void StringDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            Font f = new Font("Times New Roman", 14);

            string name = t.name;
            int score = t.score;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {

                Point textLocation = new Point((int)t.orientation.GetX() - 30, ((int)t.orientation.GetY()) + 30);

                // Vertical
                if (t.orientation.GetX() == 0)
                {
                    // Facing South
                    if (t.orientation.GetY() == 1)
                        e.Graphics.RotateTransform(180);
                }

                // Horizontal
                else
                {
                    if (t.orientation.GetX() == -1 && t.orientation.GetY() == 0)
                        e.Graphics.RotateTransform(90);

                    else
                        e.Graphics.RotateTransform(270);
                }

                e.Graphics.DrawString(name + ": " + score, f, whiteBrush, textLocation);
            }
        }

        /// <summary>
        /// Draws the Turret
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw tank with appropriate color and size
            string color = theWorld.TankColors[t.ID];
            e.Graphics.ScaleTransform(0.75f, 0.75f);
            e.Graphics.DrawImage(GetSprite(color + "Turret.png"), new Point(-35, -30));
        }

        /// <summary>
        /// Draws our Projectiles
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {

            Projectile p = o as Projectile;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.ScaleTransform(0.5f, 0.5f);
            e.Graphics.DrawImage(GetSprite("Hadouken.png"), new Point((int)p.orientation.GetX() - 30, (int)p.orientation.GetY() - 30));
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Beams
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Random Random = new Random();

            // Beam color
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb
                (Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255))))

            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.WhiteSmoke))
            {
                e.Graphics.DrawLine(new System.Drawing.Pen(redBrush, 8), new Point(0, 0), new Point(3000, -3000));
                e.Graphics.DrawLine(new System.Drawing.Pen(whiteBrush, 3), new Point(0, 0), new Point(3000, -3000));

            }
        }

        /// <summary>
        /// Draws our Walls
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall w = o as Wall;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // scale and adjust
            e.Graphics.ScaleTransform(0.79f, 0.79f);
            e.Graphics.DrawImage(GetSprite("WallSprite.png"), new Point(-32, -32));
        }


        /// <summary>
        /// Draws Powerups.  Here we draw a sonic coin as our powerup.
        /// Reference in our README
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.ScaleTransform(0.4f, 0.4f);
            e.Graphics.DrawImage(GetSprite("sonic_coin.png"), new Point(-30, -30));
        }


        /// <summary>
        /// Draws the FPS for our tank
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void FPSDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {
                Font font = new Font("Times New Roman", 28);
                e.Graphics.DrawString("FPS: " + GetFPS().ToString(), font, whiteBrush, new Point(-30, 0));
            }
        }

        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn
        /// When first centering our view, a try catch is used to skip the first time Onpaint is invoked.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Stopwatch for calculating FPS
            sw.Start();

            lock (theWorld)
            {
                // Centering View (provided code)
                try
                {
                    double playerX = theWorld.Tanks[theController.GetPlayerID()].location.GetX();
                    double playerY = theWorld.Tanks[theController.GetPlayerID()].location.GetY();

                    // calculate view/world size ratio
                    double ratio = (double)900 / (double)theWorld.size;
                    int halfSizeScaled = (int)(theWorld.size / 2.0 * ratio);

                    double inverseTranslateX = -WorldSpaceToImageSpace(theWorld.size, playerX) + halfSizeScaled;
                    double inverseTranslateY = -WorldSpaceToImageSpace(theWorld.size, playerY) + halfSizeScaled;

                    e.Graphics.TranslateTransform((float)inverseTranslateX, (float)inverseTranslateY);
                }

                catch { }  // Do nothing

                // Draw background
                Image backGround = GetSprite("Background.png");
                Rectangle size = new Rectangle(0, 0, theWorld.size, theWorld.size);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawImage(backGround, size);
            }

            // Drawing the walls  
            lock (theWorld.Walls)
            {
                foreach (Wall wall in theWorld.Walls.Values)
                {
                    // The points of the walls
                    double p1x = wall.GetP1().GetX();
                    double p1y = wall.GetP1().GetY();

                    double p2x = wall.GetP2().GetX();
                    double p2y = wall.GetP2().GetY();

                    DrawUntilEndPoint(e, wall, p1x, p1y, p2x, p2y);
                }
            }

            // Tanks
            lock (theWorld.Tanks)
            {
                // Draw the players
                foreach (Tank tank in theWorld.Tanks.Values.ToList())
                {
                    // Draw only if alive, Server will send the tank JSON with full health when it respawns.
                    if (tank.hitPoints != 0)
                    {
                        // Tank body
                        DrawObjectWithTransform(e, tank, theWorld.size, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), TankDrawer);

                        // Turret 
                        DrawObjectWithTransform(e, tank, theWorld.size, tank.location.GetX(), tank.location.GetY(), tank.GetAiming().ToAngle(), TurretDrawer);

                        // Name and Score
                        DrawObjectWithTransform(e, tank, theWorld.size, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), StringDrawer);

                        // HP bar
                        DrawObjectWithTransform(e, tank, theWorld.size, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), HPDrawer);

                        // Draw FPS
                        if (tank.ID == theController.GetPlayerID() && theController.showFPS)
                            DrawObjectWithTransform(e, tank, theWorld.size, tank.location.GetX() - 420, tank.location.GetY() - 410, 0, FPSDrawer);
                    }

                    else if (tank.hitPoints == 0) 
                    {
                        DrawObjectWithTransform(e, tank, theWorld.size, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), DeathDrawer);
                    }
                }
            }

            // Load the power-ups
            lock (theWorld.PowerUps)
            {
                // Draw the powerups
                foreach (PowerUp pow in theWorld.PowerUps.Values.ToList())
                {
                    if (!pow.died)
                        DrawObjectWithTransform(e, pow, theWorld.size, pow.location.GetX(), pow.location.GetY(), 0, PowerupDrawer);
                }
            }

            // Loading the Projectiles
            lock (theWorld.Projectiles)
            {
                foreach (Projectile proj in theWorld.Projectiles.Values.ToList())
                {
                    if (!proj.died)
                        DrawObjectWithTransform(e, proj, theWorld.size, proj.location.GetX(), proj.location.GetY(), proj.orientation.ToAngle() - 90, ProjectileDrawer);
                }
            }

            // Load Beams
            lock (theWorld.Beams)
            {
                foreach (Beam b in theWorld.Beams.Values.ToList())
                {
                    DrawObjectWithTransform(e, b, theWorld.size, b.origin.GetX(), b.origin.GetY(), b.direction.ToAngle() - 45, BeamDrawer);
                    FPSCount += FPS;
                }

                // Clearing the Beams after a certain amount of frames
				if (FPSCount >= 1200)
				{
                    theWorld.Beams.Clear();
                    FPSCount = 0; 
				}
            }


            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);

            // Calculate FPS with the send rate
            double sendRate = sw.ElapsedMilliseconds;
            FPS = Math.Ceiling(1000 / sendRate);

            sw.Restart();
        }

        /// <summary>
        /// Private helper method used to get our sprites to be drawn.
        /// Note the name MUST specify what image type. EX .png, .img, .jpg etc.
        /// Throws ArguementException if sprite was not found.
        /// 
        /// If we are already using a sprite, cache it to the dictionary.
        /// </summary>
        /// <param name="spriteName">Name of sprite to be drawn.</param>
        /// <returns></returns>
        private Image GetSprite(string spriteName)
        {
            try
            {
                if (sprites.ContainsKey(spriteName))
                    return sprites[spriteName];

                // Navigating directories
                DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
                DirectoryInfo gameDir = di.Parent.Parent.Parent;
                string finalPath = gameDir.FullName + "\\Resources\\Sprites\\" + spriteName;

                sprites.Add(spriteName, Image.FromFile(finalPath));
                return sprites[spriteName];
            }

            // Couldnt find sprite.
            catch
            {
                throw new ArgumentException("Image: " + spriteName + " not found.  Did you specify the image type?");
            }
        }

        /// <summary>
        /// This helper method uses are 2 Vector2D point locations to help call DrawObjectWithTransform
        /// in able to draw our walls at the appropriate locations. Due to this logic taking up so many lines.
        /// Walls are to be drawn using a "start point" and an "end point".  Starting at the
        /// lower x / y value, and incrementing up by 50 until we have reached the other endpoint of the
        /// opposite wall's x / y value.  Walls with the same x value are on the vertical axis, while walls
        /// sharing the same y value are on the horizontal axis.
        /// </summary>
        /// <param name="e">Paintevent object</param>
        /// <param name="wall">Wall to be drawn</param>
        /// <param name="p1x">x Cordinate of point 1</param>
        /// <param name="p1y">y cordinate of point 1</param>
        /// <param name="p2x">x cordinate of point 2</param>
        /// <param name="p2y">y cordinate of point 2</param>
        private void DrawUntilEndPoint(PaintEventArgs e, Wall wall, double p1x, double p1y, double p2x, double p2y)
        {
            // Same x value , Wall is Vertical
            if (p1x == p2x)
            {
                // First x coordinate for our wall,
                double x = p1x; double start; double end;

                if (p1y < p2y)
                {
                    start = p1y;
                    end = p2y;
                }
                else
                {
                    start = p2y;
                    end = p1y;
                }

                // Draw until last x cordinate
                while (start <= end)
                {
                    DrawObjectWithTransform(e, wall, theWorld.size, x, start, 0, WallDrawer);
                    start += 50;
                }
            }

            // Same y value , Wall is Horizontal
            if (p1y == p2y)
            {
                // First y coordinate for our wall
                double y = p1y; double start; double end;

                if (p1x < p2x)
                {
                    start = p1x;
                    end = p2x;
                }
                else
                {
                    start = p2x;
                    end = p1x;
                }

                // Draw until last y cordinate
                while (start <= end)
                {
                    DrawObjectWithTransform(e, wall, theWorld.size, start, y, 0, WallDrawer);
                    start += 50;
                }
            }
        }
    }
}