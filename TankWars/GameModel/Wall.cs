using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
	/// <summary>
	/// Class representing our walls
	/// @Authors Diego Andino, Tarik Vu.
	/// CS3500 Fall 2020 Semester
	/// </summary>
	public class Wall
	{
		[JsonProperty(PropertyName = "wall")]
		private int id;

		[JsonProperty(PropertyName = "p1")]
		private Vector2D p1; 

		[JsonProperty(PropertyName = "p2")]
		private Vector2D p2; 

		/// <summary>
		/// Default constructor for the walls
		/// </summary>
		public Wall(int id, Vector2D p1, Vector2D p2)
		{
			this.id = id;
			this.p1 = p1;
			this.p2 = p2;
		}

		/// <summary>
		/// Return the first point
		/// </summary>
		/// <returns></returns>
		public Vector2D GetP1()
        {
			return p1;
        }

		/// <summary>
		/// Return the second point
		/// </summary>
		/// <returns></returns>
		public Vector2D GetP2()
        {
			return p2;
        }
	}
}
