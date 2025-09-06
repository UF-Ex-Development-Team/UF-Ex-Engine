using System;
using System.Collections.Generic;
using UndyneFight_Ex;
using UndyneFight_Ex.GameInterface;
namespace UF_Ex_Environment //You can define your own namespace name
{
	public static class Program
	{
		[STAThread]
		static void Main()
		{
			//For each chart, you will have to manually load the type of the class
			GameStartUp.SetMainSongs(
			[
				typeof(MyFirstChart),
				typeof(ChampionshipChart)
			]);
			GameStartUp.StartGame();
		}
	}
}