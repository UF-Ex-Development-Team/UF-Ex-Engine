[assembly: System.Reflection.AssemblyVersion("0.5.0")]
namespace UndyneFight_Ex;

#if WINDOWS || LINUX || DEBUG
/// <summary>
/// The main class.
/// </summary>
public static class Program
{
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	private static void Main()
	{
		//Debug logger
		if (File.Exists("debug.log"))
			File.WriteAllText("debug.log", string.Empty);
		else
			File.Create("debug.log").Close();
		//Main game
		using GameMain game = new();
		game.Run();
	}
}
#endif
