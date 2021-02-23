using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    /// <summary>
    /// This is the "View" of Tank wars. Holding a world object and controller object in order
    /// to update itself accordingly on it's drawing panel.  Here we also handle calculation of FPS
    /// @ Authors Tarik Vu, Diego Andino.
    /// CS3500 Fall 2020 Semester
    /// </summary>
    public partial class GameView : Form
    {
        /// <summary>
        /// Controller that handles updates from the server
        /// </summary>
        private GameController theController;

        /// <summary>
        /// The "World" that contains our players, walls, projectiles, and powerups.
        /// Our controller owns the world, but we have a reference to it.
        /// </summary>
        private World theWorld;

        /// <summary>
        /// Our Drawing panel that will update per frame.
        /// </summary>
        private DrawingPanel drawingPanel;

        /// <summary>
        /// Default Clientsize as specified in WorldSpace vs. Image space
        /// </summary>
        private const int defClientSize = 900;

        /// <summary>
        /// Constructor for the View.
        /// </summary>
        /// <param name="ctl"></param>
        public GameView(GameController ctl)
        {
            // Initalize
            InitializeComponent();
            FormClosed += OnExit;
            theController = ctl;
            theWorld = theController.GetWorld();
            HelpPanel.Hide();


            // Register handlers for the controller's events
            theController.Error += ShowError;
            theController.UpdateArrived += OnFrame;
            theController.Connected += HandleConnected;


            // Set up key handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
        }

        /// <summary>
        /// Clicking handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                theController.HandleMouseRequest(sender, "main");

            if (e.Button == MouseButtons.Right)
                theController.HandleMouseRequest(sender, "alt");
        }


        /// <summary>
        /// Clicking handler when we "unclick"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            theController.HandleMouseRequest(sender, "none");
        }


        /// <summary>
        /// Key down handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            // Close
            if (e.KeyCode == Keys.Escape)
                Application.Exit();

            // Movement keys
            if (e.KeyCode == Keys.W)
                theController.HandleMoveRequest(sender, "up");

            else if (e.KeyCode == Keys.A)
                theController.HandleMoveRequest(sender, "left");

            else if (e.KeyCode == Keys.S)
                theController.HandleMoveRequest(sender, "down");

            else if (e.KeyCode == Keys.D)
                theController.HandleMoveRequest(sender, "right");

            // FPS Toggle
            else if (e.KeyCode == Keys.F)
                theController.ToggleFPS();

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }


        /// <summary>
        /// Key up handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                theController.CancelMoveRequest(sender, "up");

            else if (e.KeyCode == Keys.A)
                theController.CancelMoveRequest(sender, "left");

            else if (e.KeyCode == Keys.S)
                theController.CancelMoveRequest(sender, "down");

            else if (e.KeyCode == Keys.D)
                theController.CancelMoveRequest(sender, "right");
        }


        /// <summary>
        /// When we try to connect, we first check our player name,
        /// Then disable buttons to try and connect.  If the connection fails
        /// we enable the buttons to try and reconnect. Player name
        /// must be nonempty and at least 16 characters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {

            if (NameTextBox.Text.Length == 0 || NameTextBox.Text.Length > 16)
            {
                MessageBox.Show("Player Name Cannot Exceed 16 Characters Or Be Empty");
                return;
            }

            // Disable controls to try to connect to Server
            ConnectButton.Enabled = false;
            NameTextBox.Enabled = false;
            ServerTextBox.Enabled = false;

            // Try to connect to Server
            try
            {
                theController.Connect(ServerTextBox.Text, NameTextBox.Text);
                ConnectButton.Text = "Connected!";
            }

            // Couldn't connect
            catch
            {
                MessageBox.Show("Connecting to server failed.");
                ConnectButton.Enabled = true;
                NameTextBox.Enabled = true;
                ServerTextBox.Enabled = true;

                return;
            }
        }


        /// <summary>
        /// Handler for the controller's UpdateArrived event
        /// </summary>
        private void OnFrame()
        {
            // Invalidate this form and all its children
            // This will cause the form to redraw as soon as it can
            try
            {
                MethodInvoker m = new MethodInvoker(() => this.Invalidate(true));
                Invoke(m);
            }

            catch
            {
                // Do nothing.
            }
        }


        /// <summary>
        /// Upon Connection, we Setup our panel. Through invoking.
        /// </summary>
        private void HandleConnected()
        {
            // Setup Drawing panel with our world size which we now have
            this.Invoke(new MethodInvoker(
                () => SetupPanel()));
        }


        /// <summary>
        /// This method sets up our clientsize via defClientsize, as well as setting up our
        /// drawingpanel size with our now provided worldsize.
        /// We also handle mouse tracking within our drawing panel.
        /// </summary>
        private void SetupPanel()
        {
            // Basic Settings
            ClientSize = new Size(defClientSize, defClientSize);
            drawingPanel = new DrawingPanel(theWorld, theController);
            drawingPanel.Location = new Point(0, 0);
            drawingPanel.Size = new Size(theWorld.size, theWorld.size);
            drawingPanel.BackColor = Color.Black;

            // Handling mouse location & clicking
            drawingPanel.MouseMove += DetectMouseLocation;
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;

            this.Controls.Add(drawingPanel);
        }


        /// <summary>
        /// Detecting our mouse location on our drawing panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetectMouseLocation(object sender, MouseEventArgs e)
        {
            Vector2D mousePoint = new Vector2D(e.X, e.Y);
            theController.HandleMouseMovement(sender, mousePoint);
        }


        /// <summary>
        /// Method used to report any errors that occured.
        /// </summary>
        /// <param name="err"></param>
        private void ShowError(string err)
        {
            MessageBox.Show(err);

            // Re-enable the controlls so the user can reconnect
            this.Invoke(new MethodInvoker(
                () =>
                {
                    ConnectButton.Text = "Connect";
                    ConnectButton.Enabled = true;
                    ServerTextBox.Enabled = true;
                    NameTextBox.Enabled = true;
                }));
        }


        /// <summary>
        /// When our form closes, close our controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExit(object sender, FormClosedEventArgs e)
        {
            theController.Close();
        }


        /// <summary>
        /// Help Button On Click handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpButton_Click(object sender, EventArgs e)
        {
            // Keep focus on game
            this.ActiveControl = null;
            HelpButton.TabStop = false;

            // Enable or disable panel
            if (!HelpPanel.Visible)
                HelpPanel.Show();
            else
                HelpPanel.Hide();
        }


        /// <summary>
        /// Show Controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlButton_Click(object sender, EventArgs e)
        {
            // Stop the tank, our popup for movement is here
            theController.HandleMoveRequest(sender, "none");

            MessageBox.Show("Movement:\tW,A,S,D\n" +
                            "Default Fire:\tLeft Click\n" +
                            "Special Beam:\tRight Click\n" +
                            "Aiming:\t\tMouse\n" +
                            "Show FPS:\tF\n" +
                            "Quit:\t\tEscape",
                            "Controls", MessageBoxButtons.OK);

            // Focus on game
            this.ActiveControl = null;
            ControlButton.TabStop = false;
        }


        /// <summary>
        /// Show more about our game.  Here we link our README to represent the "About" of the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutButton_Click(object sender, EventArgs e)
        {
            // Stop the tank our about window is open
            theController.HandleMoveRequest(sender, "none");

            // Getting our readme
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo gameDir = di.Parent.Parent.Parent;
            string finalPath = gameDir.FullName + "\\Resources\\README.txt";

            // Label to put into form
            Label label = new Label();
            label.Text = File.ReadAllText(finalPath);
            label.Size = new Size(700, 700)
;
            // Form displaying our "about" information
            Form form = new Form();
            form.Size = new Size(750, 750);
            form.Controls.Add(label);
            form.Text = "About (README)";
            form.ShowDialog();

            // Keep focus on game
            this.ActiveControl = null;
            AboutButton.TabStop = false;
        }

	}
}
