namespace UndyneFight_Ex.Settings;

public static partial class SettingsManager
{
	/// <summary>
	/// The list of data used in settings
	/// </summary>
	public static class DataLibrary
	{
		/// <summary>
		/// The drawing quality of the game
		/// </summary>
		public enum DrawingQuality
		{
			/// <summary>
			/// Low quality
			/// </summary>
			Low = 0,
			/// <summary>
			/// Normal quality
			/// </summary>
			Normal = 1,
			/// <summary>
			/// High quality
			/// </summary>
			High = 2
		}
		/// <summary>
		/// The sound used when a spear hits the shield (0 for classic, 1 for new)
		/// </summary>
		public static int SpearBlockSound { get => (int)UserService.Settings.GetSettingsValue("SpearBlockSound"); set => UserService.Settings.SetSettingsValue("SpearBlockSound", value); }
		/// <summary>
		/// The master volume of the game
		/// </summary>
		public static int masterVolume { get => (int)UserService.Settings.GetSettingsValue("MasterVolume"); set => UserService.Settings.SetSettingsValue("MasterVolume", value); }
		/// <summary>
		/// Whether to show dialog
		/// </summary>
		public static bool dialogAvailable { get => (bool)UserService.Settings.GetSettingsValue("DialogAvailable"); set => UserService.Settings.SetSettingsValue("DialogAvailable", value); }
		/// <summary>
		/// Whether the show "early" and "late" texts for green soul
		/// </summary>
		public static bool preciseWarning { get => (bool)UserService.Settings.GetSettingsValue("PreciseWarning"); set => UserService.Settings.SetSettingsValue("PreciseWarning", value); }
		/// <summary>
		/// The amount to reduce blue light in the game, range is [0, 100]
		/// </summary>
		public static int reduceBlueAmount { get => (int)UserService.Settings.GetSettingsValue("ReduceBlueAmount"); set => UserService.Settings.SetSettingsValue("ReduceBlueAmount", value); }
		/// <summary>
		/// The drawing quality of the game
		/// </summary>
		public static DrawingQuality drawingQuality { get => (DrawingQuality)(int)UserService.Settings.GetSettingsValue("DrawingQuality"); set => UserService.Settings.SetSettingsValue("DrawingQuality", (int)value); }
		/// <summary>
		/// The volume of the spear blocking sound, range is [0, 100]
		/// </summary>
		public static int SpearBlockingVolume { get => (int)UserService.Settings.GetSettingsValue("SpearBlockingVolume"); set => UserService.Settings.SetSettingsValue("SpearBlockingVolume", value); }
		/// <summary>
		/// The volume of sound effects, range is [0, 100]
		/// </summary>
		public static int SFXVolume { get => (int)UserService.Settings.GetSettingsValue("SFXVolume"); set => UserService.Settings.SetSettingsValue("SFXVolume", value); }
		/// <summary>
		/// The speed of the arrows, range is [1, 1.5]
		/// </summary>
		public static float ArrowSpeed { get => (float)UserService.Settings.GetSettingsValue("ArrowSpeed"); set => UserService.Settings.SetSettingsValue("ArrowSpeed", value); }
		/// <summary>
		/// The delay of arrows and green soul blasters, range is [-50, 250]
		/// </summary>
		public static float ArrowDelay { get => (float)UserService.Settings.GetSettingsValue("ArrowDelay"); set => UserService.Settings.SetSettingsValue("ArrowDelay", value); }
		/// <summary>
		/// The scale of the arrows, range is [1, 1.25]
		/// </summary>
		public static float ArrowScale { get => (float)UserService.Settings.GetSettingsValue("ArrowScale"); set => UserService.Settings.SetSettingsValue("ArrowScale", value); }
		/// <summary>
		/// Whether the flip blue and red arrows and blasters
		/// </summary>
		public static bool Mirror { get => (bool)UserService.Settings.GetSettingsValue("Mirror"); set => UserService.Settings.SetSettingsValue("Mirror", value); }
		/// <summary>
		/// Whether to display the score percentage along side the perfect percentage
		/// </summary>
		public static bool DisplayScorePercent { get => (bool)UserService.Settings.GetSettingsValue("DisplayScorePercent"); set => UserService.Settings.SetSettingsValue("DisplayScorePercent", value); }
		/// <summary>
		/// I don't...know why this is a thing?
		/// </summary>
		public static float DrawFPS { get => (float)UserService.Settings.GetSettingsValue("DrawFPS"); set => UserService.Settings.SetSettingsValue("DrawDPS", value); }
		/// <summary>
		/// The sampler state used in the game, can be "Nearest" or "3x Linear" or "Anisotropic"
		/// </summary>
		public static string SamplerState { get => (string)UserService.Settings.GetSettingsValue("SamplerState"); set => UserService.Settings.SetSettingsValue("SamplerState", value); }
		/// <summary>
		/// The black outline scale of the arrows
		/// </summary>
		public static float ArrowOutlineScale { get => (float)UserService.Settings.GetSettingsValue("ArrowOutlineScale"); set => UserService.Settings.SetSettingsValue("ArrowOutlineScale", value); }
		/// <summary>
		/// Whether to use the old blaster drawing method or not
		/// </summary>
		public static bool OldBlasterColors { get => (bool)UserService.Settings.GetSettingsValue("OldBlasterColors"); set => UserService.Settings.SetSettingsValue("OldBlasterColors", value); }
		/// <summary>
		/// Whether the enable anomaly replaying
		/// </summary>
		public static bool AnomalyReplay { get => (bool)UserService.Settings.GetSettingsValue("AnomalyReplay"); set => UserService.Settings.SetSettingsValue("AnomalyReplay", value); }
		/// <summary>
		/// The branch the game is on
		/// </summary>
		public static string BuildBranch { get => (string)UserService.Settings.GetSettingsValue("BuildBranch"); set => UserService.Settings.SetSettingsValue("BuildBranch", value); }
		public static void Initialize() => UserService.Settings.Initialize();
	}
}