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
	public const string Version = "0.4.0";

}