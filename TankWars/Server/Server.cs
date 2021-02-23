using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GameModel;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TankWars;

namespace Server
{
	/// <summary>
	/// This class acts as our Server (controller) that communicates to our ServerWorld
	/// when to update.  This class parses requests from clients connected and sends them to the world.
	/// Here we set our constants for Tank wars, and pass them upon Construction of ServerWorld.
	/// @Authors Tarik Vu, Diego Andino.
	/// CS3500 Fall 2020 Semester
	/// </summary>
	class Server
	{
		/// <summary> Clients that are connected, each with an ID. </summary>
		private Dictionary<long, SocketState> clients;

		/// <summary> Our Sever's private world. </summary>
		private ServerWorld serverWorld;

		/// <summary> Default world size. </summary>
		private int WORLD_SIZE = 2000;

		/// <summary> Default Milliseconds per frame to send. </summary>
		private long FRAME_TICKS = 17;

		/// <summary> Default Projectile firing cooldown. </summary>
		private double FIRE_COOLDOWN = 250;

		/// <summary> Default amount of milleseconds bewteen an object dying and respawning </summary>
		private double RESPAWN_RATE = 3000;

		
		/// <summary> Constant tank speed. </summary>
		private const double TANK_SPEED = 3;

		/// <summary> Constant Projectile speed. </summary>
		private const double PROJECTILE_SPEED = 30;

		/// <summary> Constant amount of powerups at any given time in our world. </summary>
		private const int MAX_PWRUPS = 5;

		
		/// <summary> Stopwatch used inside busyloop method for updating World. </summary>
		private Stopwatch sw;


		/// <summary>
		/// Starting up our Server and holding our console open.
		/// Server controls our constants and passes them into our world to use.
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Server server = new Server();			
			server.Start();

			Console.Read();
		}


		/// <summary>
		/// Constructor for the Server object.
		/// Here we also start another thread that is in charge of updating our world.
		/// </summary>
		public Server()
		{
			// Create our serverWorld with our constants.
			LoadConstants();
			serverWorld = new ServerWorld(PROJECTILE_SPEED, TANK_SPEED, MAX_PWRUPS, RESPAWN_RATE, FIRE_COOLDOWN, WORLD_SIZE);
			clients = new Dictionary<long, SocketState>();

			// Using a seperate thread to update the world.
			Thread t = new Thread(BusyLoop);
			t.Start();

			// Stopwatch for busy loop
			sw = new Stopwatch();
			sw.Start();
		}


		/// <summary>
		/// Loads the constants only if given in the settings.xml file.
		/// Else, load defaults.
		/// </summary>
		private void LoadConstants()
		{
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
					if (name == "UniverseSize")
					{
						Int32.TryParse(node.InnerText, out int res);
						WORLD_SIZE = res; 
					}

					else if (name == "MSPerFrame")
					{
						Int32.TryParse(node.InnerText, out int res);
						FRAME_TICKS = res;
					}

					else if (name == "FramesPerShot")
					{
						Int32.TryParse(node.InnerText, out int res);
						FIRE_COOLDOWN = res;
					}

					else if (name == "RespawnRate")
					{
						Int32.TryParse(node.InnerText, out int res);
						RESPAWN_RATE = res;
					}
				}
			}

			catch
			{
				throw new ArgumentException("Error Caching Walls");
			}
		}


		/// <summary>
		/// Starts the server with Networking class. 
		/// </summary>
		private void Start()
		{
			Networking.StartServer(HandleNewClient, 11000);
			Console.WriteLine("Server Is Running. Now Accepting New Clients.");
		}


		/// <summary>
		/// Callback for StartServer when a client connects.
		/// </summary>
		private void HandleNewClient(SocketState state)
		{
			if (state.ErrorOccured)
				return;

			state.OnNetworkAction = ReceivePlayerName;

			// Continues Receive Loop
			Networking.GetData(state);
		}


		/// <summary>
		/// Callback to receive player name.
		/// </summary>
		/// <param name="state"></param>
		private void ReceivePlayerName(SocketState state)
		{
			// If a Socket disconnects, return.
			if (state.ErrorOccured)
				return;

			// Add tank to the world if it doesnt exist yet
			if (!clients.ContainsKey(state.ID))
			{
				lock (serverWorld)
				{
					serverWorld.AddTank(state.ID, state.GetData());
				}
			}

			state.OnNetworkAction = HandleRequests;

			// Send start-up info; Lock the socket to avoid race conditions
			lock (state.TheSocket)
			{
				if (!clients.ContainsKey(state.ID))
				{
					Networking.Send(state.TheSocket, $"{ state.ID }\n{ WORLD_SIZE }\n");
					Networking.Send(state.TheSocket, serverWorld.GetWalls() + "\n");
				}
			}

			// Add client to clients list 
			lock (clients)
			{
				if (!clients.ContainsKey(state.ID))
				{
					clients.Add(state.ID, state);
					Console.WriteLine($"Player({ state.ID }): { state.GetData().TrimEnd('\n') } Has Joined The Game!");
				}
			}

			Networking.GetData(state);
		}


		/// <summary>
		/// Callback for ReceivePlayerName to handle incoming requests.
		/// Parses the JSON requests.  This method uses the same logic as ClientSide handshake
		/// of Processing Messages, but instead of loading in objects we use the given information to 
		/// update our tank with the ControlCommand. For incorrect commands that did not parse, we
		/// ignore the request. 
		/// </summary>
		/// <param name="state"></param>
		private void HandleRequests(SocketState state)
		{
			if (state.ErrorOccured)
			{
				lock (serverWorld)
				{
					RemoveClient(state.ID);
					serverWorld.GetTank(state.ID).disconnected = true;

					return;
				}
			}

			lock (state)
			{
				string totalData = state.GetData();
				string[] parts = Regex.Split(totalData, @"(?<=[\n])");

				// Loop until we have processed all messages.
				foreach (string p in parts)
				{
					// Ignore empty strings added by the regex splitter
					if (p.Length == 0)
						continue;

					// Ignoring strings with no newline
					if (p[p.Length - 1] != '\n')
						break;

					// Skipping incomplete JSONS
					if (p[0] != '{' || !p.EndsWith("\n"))
						continue;

					// Process the command, update the tank w/ this client ID
					try
					{
						ControlCommand command = JsonConvert.DeserializeObject<ControlCommand>(p);

						serverWorld.SetMoveTank(command, state.ID);
						serverWorld.RotateTurret(command, state.ID);
						serverWorld.FireTurret(command, state.ID);
					}

					catch
					{ /*command did not parse correctly; ignore*/ }

					//Remove data from the SocketState's growable buffer
					state.RemoveData(0, p.Length);
				}

				Networking.GetData(state);
			}
		}


		/// <summary>
		/// Private method to infinitely loop to update after connecting.
		/// </summary>
		private void BusyLoop()
		{
			while (true)
			{
				while (sw.ElapsedMilliseconds < FRAME_TICKS) { /* Do Nothing */ }

				sw.Restart();
				Update();
			}
		}


		/// <summary>
		/// Updates the world at the current FPS.
		/// </summary>
		private void Update()
		{
			lock (serverWorld)
			{
				serverWorld.UpdateWorld();
				serverWorld.CheckCollision();

				foreach (SocketState state in clients.Values.ToList())
				{
					Networking.Send(state.TheSocket, serverWorld.GetWorld() + "\n");
				}
			}
		}


		/// <summary>
		/// Removes a client from the clients dictionary
		/// </summary>
		/// <param name="id">The ID of the client</param>
		private void RemoveClient(long id)
		{
			Console.WriteLine("Client " + id + " disconnected");
			lock (clients)
			{
				if (clients.ContainsKey(id))
					clients.Remove(id);
			}
		}

	}
}
