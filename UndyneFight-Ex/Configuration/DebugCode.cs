using System.Diagnostics;

namespace UndyneFight_Ex;
/// <summary>
/// Debug variables for the SDK
/// </summary>
public static class DebugState
{
	internal static bool[] ShieldAuto = [false, false, false, false];
	/// <summary>
	/// Show the cost of rendering on screen during a chart
	/// </summary>
#if DEBUG
	public const bool ShowRenderCost = true;
#else
    public const bool ShowRenderCost = false;
#endif
	/// <summary>
	/// Displays the intended hitbox of barrages during a chart
	/// </summary>
#if DEBUG
	public const bool ShowIntendedHitbox = false;
#else
    public const bool ShowIntendedHitbox = false;
#endif
	/// <summary>
	/// The version of UF-Ex
	/// </summary>
	public const string Version = "0.5.0";
	/// <summary>
	/// Whether the game is at beta stage
	/// </summary>
#if DEBUG || BETA
	public const bool IsBeta = true;
#elif RELEASE
	public const bool IsBeta = false;
#endif
	/// <summary>
	/// Logs a message to console and debug output for ease of debugging
	/// </summary>
	/// <param name="message">The message to log</param>
	/// <param name="is_internal">Whether the logging is conducted internally</param>
	public static void Log(string message, bool is_internal = false)
	{
		string finText = $"{(is_internal ? "UF-Ex" : GameStates.GameName)}: {message}";
		Console.WriteLine(finText);
#if DEBUG
		Debug.WriteLine(finText);
#endif
		lock (_streamLock)
		{
			using StreamWriter logWriter = new("debug.log", append: true);
			logWriter.WriteLine($"[{DateTime.Now}] {finText}");
		}
	}
	private static readonly Lock _streamLock = new();
}