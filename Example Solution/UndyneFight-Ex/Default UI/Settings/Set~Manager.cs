using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.Settings.SettingLibrary;

namespace UndyneFight_Ex.Settings;

internal abstract class Setting(string settingTitle, Vector2 centre) : Entity, ISelectAble
{
	private readonly TextSelection _textSelection = new(settingTitle, centre);
	private readonly string _settingTitle = settingTitle;
	private Vector2 _centre = centre;
	protected string showingValue { set; private get; }
	protected bool IsSelected { private set; get; } = false;

	public override void Draw() => _textSelection.Draw();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DeSelected()
	{
		_textSelection.DeSelected();
		IsSelected = false;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Selected()
	{
		_textSelection.Selected();
		IsSelected = true;
	}

	public abstract void SelectionEvent();
	public abstract void Save();

	public override void Update()
	{
		_textSelection.subText = showingValue;
		_textSelection.Update();
	}
}
/// <summary>
/// The manager for the in-game settings
/// </summary>
public static class SettingsManager
{
	internal static Type[] Settings { get; private set; }

	internal static void Initialize() => Settings =
		[
			typeof(MasterVolume),
			typeof(SpearBlockedVolume),
			typeof(DrawingQualitySetter),
			typeof(ArrowSpeed),
			typeof(ArrowScale),
			typeof(IsMirror),
			typeof(ArrowDelay),
			typeof(DialogAvailable),
			typeof(preciseWarning),
			typeof(ReduceBlue),
		];
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
		public static int SpearBlockSound { get; set; } = 1;
		/// <summary>
		/// The master volume of the game
		/// </summary>
		public static int masterVolume { get; set; } = 100;
		/// <summary>
		/// Whether to show dialog
		/// </summary>
		public static bool dialogAvailable { get; set; } = true;
		/// <summary>
		/// Whether the show "early" and "late" texts for green soul
		/// </summary>
		public static bool preciseWarning { get; set; } = false;
		/// <summary>
		/// The amount to reduce blue light in the game, range is [0, 100]
		/// </summary>
		public static int reduceBlueAmount { get; set; } = 0;
		/// <summary>
		/// The drawing quality of the game
		/// </summary>
		public static DrawingQuality drawingQuality { get; set; } = DrawingQuality.High;
		/// <summary>
		/// The volume of the spear blocking sound, range is [0, 100]
		/// </summary>
		public static int SpearBlockingVolume { get; set; } = 100;
		/// <summary>
		/// The volume of sound effects, range is [0, 100]
		/// </summary>
		public static int SFXVolume { get; set; } = 100;
		/// <summary>
		/// The speed of the arrows, range is [1, 1.5]
		/// </summary>
		public static float ArrowSpeed { get; set; } = 1.0f;
		/// <summary>
		/// The delay of arrows and green soul blasters, range is [-50, 250]
		/// </summary>
		public static int ArrowDelay { get; set; } = 0;
		/// <summary>
		/// The scale of the arrows, range is [1, 1.25]
		/// </summary>
		public static float ArrowScale { get; set; } = 1.0f;
		/// <summary>
		/// Whether the flip blue and red arrows and blasters
		/// </summary>
		public static bool Mirror { get; set; } = false;
		/// <summary>
		/// Whether to display the score percentage along side the perfect percentage
		/// </summary>
		public static bool DisplayScorePercent { get; set; } = false;
		/// <summary>
		/// I don't...know why this is a thing?
		/// </summary>
		public static float DrawFPS { get; set; } = 125f;
		/// <summary>
		/// The sampler state used in the game, can be "Nearest" or "3x Linear" or "Anisotropic"
		/// </summary>
		public static string SamplerState { get; set; } = "Nearest";
		/// <summary>
		/// Whether to use the new arrow drawing method or not
		/// </summary>
		public static bool NewArrowDrawingMethod { get; set; } = false;
	}
}