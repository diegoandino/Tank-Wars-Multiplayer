using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    /// <summary>
	/// Tank Wars Starting point
	/// Controller and view are created here
	/// @Authors Diego Andino, Tarik Vu
	/// </summary>
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			GameController c = new GameController();
			GameView view = new GameView(c);
			Application.Run(view);
		}
	}
}
