using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace GameModel
{
    /// <summary>
    /// This class represents our ControlCommands per specifications of PS8.
    /// When processing inputs from our client, this class is used to send JSONS with the following
    /// information: Moving, Fire, and tdir.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        [JsonProperty(PropertyName = "moving")]
        public string moving { get; private set; }

        [JsonProperty(PropertyName = "fire")]
        public string fire { get; private set; }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D tdir { get; private set; }

        /// <summary>
        /// Creates a ControlCommand Object to be sent to our server.
        /// </summary>
        public ControlCommand()
        {
            moving = "none";
            fire = "none";
            tdir = new Vector2D();
              
        }

        /// <summary>
        /// moving can only be one of the following strings: "none", "up", "left", "down", or "right".
        /// Throws ArguementException otherwise.
        /// </summary>
        /// <param name="request">movement request</param>
        public void SetMoving(string request)
        {
            // Only 5 possible inputs
            if (request.Equals("none") || request.Equals("up") ||
                request.Equals("left") || request.Equals("down") || request.Equals("right"))
                this.moving = request;
            else
                throw new ArgumentException("Invalid Movement request: " + request);
        }

        /// <summary>
        /// Fire can only be one of the following strings: "none", "main", or "alt".
        /// Throws ArguementException otherwise.
        /// </summary>
        /// <param name="request">firing Request</param>
        public void SetFire(string request)
        {
            // Only 3 possible inputs
            if (request.Equals("none") || request.Equals("main") || request.Equals("alt"))
                this.fire = request;
            else
                throw new ArgumentException("Invalid Fire request: " + request);
        }

        /// <summary>
        /// set's our turret direction
        /// </summary>
        /// <param name="tdir">Vector2d of our turret.</param>
        public void SetTDIR(Vector2D tdir)
        {
            this.tdir = tdir;
        }
    }
}
