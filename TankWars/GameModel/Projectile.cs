using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
	/// <summary>
	/// Class representing Projectiles.
	/// @Authors Tarik Vu, Diego Andino.
	/// CS3500 Fall 2020 Semester
	/// </summary>
	public class Projectile
	{
		[JsonProperty(PropertyName = "proj")]
		public int ID { get; private set; }


		[JsonProperty(PropertyName = "loc")]
		public Vector2D location { get; set; }
		
		
		[JsonProperty(PropertyName = "dir")]
		public Vector2D orientation { get; set; }
		
		
		[JsonProperty(PropertyName = "died")]
		public bool died { get; set; }
		
		
		[JsonProperty(PropertyName = "owner")]
		public int ownerID { get; private set; }


		/// <summary>
		/// Default constructor for our projectiles
		/// </summary>
		public Projectile(int id, int ownerID, bool died, Vector2D location, Vector2D orientation)
		{
			this.ID = id;
			this.ownerID = ownerID;
			this.died = died;
			this.location = location;
			this.orientation = orientation;
		}

		/// <summary>
		/// Takes in a Projectile proj with the same id with updated information after being deserialized from server
		/// Throws Argument Exception if ID's dont match.
		/// </summary>
		/// <param name="proj">Deserialized Projectile with updated information</param>
		/// <returns>returns this Projectile with updated information from Projectile proj.</returns>
		internal Projectile Update(Projectile proj)
		{
			if (this.ID != proj.ID)
				throw new ArgumentException("Cannot update projectile info.\n Mismatched ID's.");

			// Update the Projectile except its ID
			orientation = proj.orientation;
			location = proj.location;
			died = proj.died;

			return this;
		}
	}
}
