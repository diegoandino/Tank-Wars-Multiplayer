using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TankWars
{
	/// <summary>
	/// Class represening Beams.
	/// @Authors Tarik Vu, Diego Andino.
	/// CS3500 Fall 2020 Semester
	/// </summary>
	public class Beam
	{
		[JsonProperty(PropertyName = "beam")]
		public int ID { get; private set; }

		[JsonProperty(PropertyName = "org")]
		public Vector2D origin { get; private set; }

		[JsonProperty(PropertyName = "dir")]
		public Vector2D direction { get; private set; }

		[JsonProperty(PropertyName = "owner")]
		public int ownerID { get; private set; }

		/// <summary>
		/// Default constructor for the Beams
		/// </summary>
		public Beam(int id, int ownerID, Vector2D origin, Vector2D direction)
		{

			this.ID = id;
			this.ownerID = ownerID;
			this.origin = origin;
			this.direction = direction;

			//  Server side code
			duration = new Stopwatch();
			duration.Start();
			
		}

		/// <summary>
		/// Takes in a Beam proj with the same id with updated information after being deserialized from server
		/// Throws Argument Exception if ID's dont match.
		/// </summary>
		/// <param name="b">Deserialized Beam with updated information</param>
		/// <returns>returns this Beam with updated information from Beam b.</returns>
		internal Beam Update(Beam b)
		{
			if (this.ID != b.ID)
				throw new ArgumentException("Cannot update Beam info.\n Mismatched ID's.");

			// Update the Beam except its ID
			this.origin = b.origin;
			this.direction =b.direction;

			return this;
		}

		///--------------------------------------------///
		///--------------------------------------------///
		///-------------SERVER SIDE CODE---------------///
		///--------------------------------------------///
		///--------------------------------------------///
		

		// Duration for Beam lifespan.
		[JsonIgnore]
		public Stopwatch duration { get; set; }
	}
}
