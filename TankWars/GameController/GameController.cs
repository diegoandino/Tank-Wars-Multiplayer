using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using GameModel;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// The GameController.  
    /// Here we keep track of 
    /// - The server's socket state,
    /// - Handle the handshake / Sending
    /// - Events
    /// - Movement commands.
    /// - World Size
    /// @Authors Tarik Vu, Diego Andino
    /// </summary>
    public class GameController
    {
        // Controller events that the view can subscribe to
        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        public delegate void ServerUpdateHandler();
        public event ServerUpdateHandler UpdateArrived;

        /// <summary>
        /// State representing the connection with the server
        /// </summary>
        SocketState theServer = null;

        /// <summary>
        /// Our player name
        /// </summary>
        private string PlayerName;

        /// <summary>
        /// Private world object
        /// </summary>
        private World theWorld;

        /// <summary>
        /// Private Player ID
        /// </summary>
        private int? PlayerID;

        /// <summary>
        /// Allows our DrawingPanel to know to show FPS
        /// </summary>
        public bool showFPS { get; private set; }

        /// <summary>
        /// Our ControlCommand Object to send to our Server
        /// </summary>
        private readonly ControlCommand commands;

        // Boolean logic for smoothing movement.
        private bool up;
        private bool down;
        private bool left;
        private bool right;

        /// <summary>
        /// Creates our GameController.  World size is to be set later upon connection.
        /// We also initalize our movement commands here.
        /// </summary>
        public GameController()
        {
            // World Size to be set upon connection.
            theWorld = new World();
            showFPS = false;

            // Making our ControlCommand, Initialzing movement bools. 
            up = false;
            down = false;
            left = false;
            right = false;
            commands = new ControlCommand();
        }

        /// <summary>
        /// Returns the world 
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return theWorld;
        }

        /// <summary>
        /// Returns our client (player) ID
        /// </summary>
        /// <returns></returns>
        public int GetPlayerID()
        {
            return (int)PlayerID;
        }

        /// <summary>
        /// Begins the process of connecting to the server
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string address, string name)
        {
            PlayerName = name;
            Networking.ConnectToServer(OnConnect, address, 11000);
        }

        /// <summary>
        /// OnConnect callback for Connect
        /// </summary>
        /// <param name="obj"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Error while connecting to server");
                return;
            }

            // Grab our current state
            theServer = state;

            lock (state)
            {
                // Send the player name to the server          
                Networking.Send(state.TheSocket, PlayerName + "\n");

                // Start an event loop to receive messages from the server
                state.OnNetworkAction = ReceiveMessage;
                Networking.GetData(state);
            }
        }


        /// <summary>
        /// Method to be invoked by the networking library when 
        /// data is available,  Here we also send our commands to the server via
        /// Serializing our CommandControls object.
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            if (state.ErrorOccured)
            {
                // Inform the view 
                Error("Lost connection to server\n" +
                    "Please reconnect or restart your client.");

                return;
            }

            // Setup current world
            ProcessMessages(state);

            // Sending our commands to the server
            string commandJson = JsonConvert.SerializeObject(commands);
            Networking.Send(theServer.TheSocket, commandJson + "\n");

            // Continue the event loop
            Networking.GetData(state);

            // Our update came 
            UpdateArrived();
        }


        /// <summary>
        /// Private helper method to setup player ID, world size and JSON walls
        /// that are only sent once. 
        /// </summary>
        /// <param name="state"></param>
		private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.
            foreach (string p in parts)
            {

                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                // First two messages are of type integer; Setup ID and world size
                int n;
                bool isInt = int.TryParse(p, out n);
                if (isInt)
                {
                    if (PlayerID == null)
                        PlayerID = n;
                    else
                    {
                        if (theWorld.size == 0)
                        {
                            theWorld.size = n;
                        }

                        // Call our connected callback and use size to load in background
                        Connected();
                    }
                    continue;
                }

                // Skipping incomplete JSONS
                if (p[0] != '{' || !p.EndsWith("\n"))
                {
                    continue;
                }

                // Load and parse the incoming JSON
                LoadObject(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }
        }

        /// <summary>
        /// Private helper method to load and add JSON objects to 
        /// our world.
        /// </summary>
        /// <param name="p"></param>
        private void LoadObject(string p)
        {
            JObject obj = JObject.Parse(p);

            if (obj.ContainsKey("wall"))
                theWorld.AddWall(obj, p);

            else if (obj.ContainsKey("tank"))
                theWorld.AddTank(obj, p);

            else if (obj.ContainsKey("power"))
                theWorld.AddPowerUp(obj, p);

            else if (obj.ContainsKey("proj"))
                theWorld.AddProjectile(obj, p);

            else if (obj.ContainsKey("beam"))
                theWorld.AddBeam(obj, p);
        }

        /// <summary>
        /// Handling movement request using boolean logic. 
        /// </summary>
        public void HandleMoveRequest(object sender, string key)
        {
            // Set current key press
            if (key.Equals("up"))
                up = true;
            if (key.Equals("down"))
                down = true;
            if (key.Equals("left"))
                left = true;
            if (key.Equals("right"))
                right = true;

            // Set our command object accordingly
            commands.SetMoving(key);
        }

        /// <summary>
        /// Canceling a movement request using boolean logic.
        /// </summary>
        public void CancelMoveRequest(object sender, string key)
        {
            // Undoing current key press
            if (key.Equals("up"))
                up = false;
            if (key.Equals("down"))
                down = false;
            if (key.Equals("left"))
                left = false;
            if (key.Equals("right"))
                right = false;

            // If no other keys are being held down, we stop moving
            if (!up && !down && !left & !right)
                commands.SetMoving("none");

            // If there is still a key being held down, we set our movement to it
            if (up)
                commands.SetMoving("up");

            else if (down)
                commands.SetMoving("down");

            else if (left)
                commands.SetMoving("left");

            else if (right)
                commands.SetMoving("right");

            else
                commands.SetMoving("none");
        }

        /// <summary>
        /// Handling mouse request
        /// </summary>
        public void HandleMouseRequest(object sender, string request)
        {
            commands.SetFire(request);
        }

        /// <summary>
        /// Handle the mouse movement via centering x and y at our tank center.
        /// We then normalize and set the Turret direction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mousePoint"></param>
        public void HandleMouseMovement(object sender, Vector2D mousePoint)
        {
            // x,y to be centered at our tank
            double x = mousePoint.GetX() - 450;
            double y = mousePoint.GetY() - 450;

            mousePoint = new Vector2D(x, y);
            mousePoint.Normalize();

            commands.SetTDIR(mousePoint);
        }

        /// <summary>
        /// Toggles on / off our Drawing panel to draw our FPS 
        /// </summary>
        public void ToggleFPS()
        {
            if (showFPS)
                showFPS = false;
            else
                showFPS = true;
        }


        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close()
        {
            theServer?.TheSocket.Close();
        }

    }
}
