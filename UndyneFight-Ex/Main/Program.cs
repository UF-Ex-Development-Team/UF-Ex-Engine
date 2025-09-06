namespace UndyneFight_Ex
{
#if WINDOWS || LINUX || DEBUG
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			using GameMain game = new();
			game.Run();
		}
	}
#endif
}
