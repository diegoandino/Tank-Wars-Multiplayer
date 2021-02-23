TankWars PS8 11/20/2020 Fall 2020 Semester
Authors: Tarik Vu (u0759288) and Diego Andino (u1075562)

Design Decisions: 

	Workflow
	- To help organize the workflow, we created seperate .cs Files for our World, Commands, as well as all the objects that resides inside of the world. 
	- Followed the MVC pattern by Seperating our GameView, GameController, and GameModel.  Where view is utilizing controller and world to update itself via the drawing panel. 
	- For optimal performance, we utilized dictionaries where possible with things such as sprite names(in drawing panel) and object Id's (in world).  This allowed us to grab that object whenever we needed.
	  as well as not needing to reload an image perframe.  

	PS8 Requirements
	- Per assignment specifications, The first 8 players will have a unique color assigned to them, afterwards each subsequent player who joins will have a random color. 
	- When sending commands to our server, we utilize the ControlCommand class and deserialize to send it within our recieve loop. 
	- Boolean logic helped with smoothing out our movement. 
	- The NetworkController we are using is the defaulted one that was provided by professor Kopta (We clarified using his version with him on piazza).

	Sprites
	- Our tanks will be firing Hadoken blasts. 
	- Our powerups are sonic coins.
	- Special death animation (Among Us).
	- These sprites have been added to our resources folder and referenced accordingly 

Challenges of PS6:
	- Handshake: At first we were having issues with things such as sending the player name, we were unsure on how to have a copy of the name to send to the server because we would be sending withing a callback
	  method. Eventually we decided with some help and approval from a TA that saving the player name as a instance variable inside the controller would work.
	- Drawing: There were plenty of challenges here, as we spent a majority of the time working on drawing objects correctly.  From getting the correct positioning of our turret, to loading in the walls.  We overcame
	  these obsticales by utilizing TA hours, Piazza, lecture videos as well as looking at the Canvas pages provided.  Beams were also difficult due to them not drawing at our tank's desired location.
	- FPS: Just figuring out how to measure it accurately and correctly.  Figuring out where to measure it from (which method to use it inside of Diego figured out to use a stopwatch inside of OnPaint). 
	- TankOrientation: This caused bugs having to do with our stringDrawer and HPbar Drawer.  The orientation caused these objects to either be inverted, slanted, disproportioned, etc.  We managed to work around this 
	  issue by checking the tank's orientation and drawing appropriately using transformations. 

Resources: 
	- General Help: 
		- Directory Navigation: https://stackoverflow.com/questions/3163495/better-way-to-get-the-base-directory
		- Random Beam Colors: https://stackoverflow.com/questions/8465675/creating-random-colors-system-drawing-color
	- Sprites
		- Sonic Ring:	https://toppng.com/show_download/167435/vintagesonic1-sonic-ring-gif/large
		- HaDoken:		https://www.pngwing.com/en/free-png-ifrlh
		- Among Us Death Image: https://www.reddit.com/r/AmongUs/comments/iv6w4g/i_think_you_all_know_the_drill_dead_body_pngs/

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////// PS9 README ///////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

TankWars Server PS9  12/3/2020 Fall 2020 Semester  
Authors: Tarik Vu (u0759288) and Diego Andino (u1075562)

Design Decisions: 

	Workflow:
	- We made a seperate class for our Server's world to keep track of collision logic, current players, objects, as well as using this class to parse them into Jsons for
	  our server to send to each client. This is to minimize the length of our original world class, and for the server to have it's own world to reference. This makes it 
	  so our Server.cs only has to focus on managing connection logic, as well as telling when our world has to update.  

	PS9 Requirements:
    - Settings: All walls are stored inside the XML as before. However now, inside of Server.cs we hold all our constants for the world such as, our frame tick rate,
	  tank / projectile speed, as defaults. If the settings.xml provides different settings we overwrite our defaults. 

	- Zones / Wraparound:  When spawning our tanks we have chosen 5 specific zones to spawn. We have the top and bottom most parts of the map, the center, as well
	  as the area in between the bunkers and the "L Walls". The Zones struct is also being utilized to dictate the playable area of our world. If a tank spawns,
	  outside of the world due to a differing world size, we spawn the tank in the center of the world.

	- Cooldowns: To handle the logic of respawning we utilize two dictionaries that keep track of Tank ID's and Powerup ID's as keys that hold Stopwatches as Objects.
	  Per frame, we check the elapsed milliseconds and respawn if the object has been dead longer than the respawn time. Also, Tank's also have their own Stopwatch for
	  firing.

Challenges of PS9:
	- Handshake: We were unable to have our clients correctly recieve any JSONS at first. However, through debugging we found that Jsons were not being sent with new lines, 
	  and that our Tanks were being deserialized with TDIR with its first parameter, and not it's ID, this was fixed by specifying our ordering for JSonProperties inside our Tank class.

    - Object ID's: We used a HashSet to contain non - duplicate ID's while using a GetID method that auto-generated ID's not being used by another object.
      The idea here is when an object is created, it "Checks out" an ID and when an object is removed from our world ID "turns in" its ID for a future object to use. 
	  There can be 10,000 given ID's at any given time.

    - Collisions with Walls: This took the longest for us to push through, however with TA help and some googling we came across different methods to detect when two squares overlap / collide.
	  This was made even more difficult / confusing due to our inverted Y - Axis and because we couldn't tell in which order Point 1 and Point 2 where being sent at. This was fixed with
	  some simple value checks for the bounds.

Resources: 
    - JSON ordering: https://www.newtonsoft.com/json/help/html/JsonPropertyOrder.htm
	- Collisions: https://stackoverflow.com/questions/306316/determine-if-two-rectangles-overlap-each-other
	- Struct: https://www.tutorialsteacher.com/csharp/csharp-struct