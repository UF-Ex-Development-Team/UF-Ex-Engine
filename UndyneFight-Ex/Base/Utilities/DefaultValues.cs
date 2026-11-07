using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex;
/// <summary>
/// Default values used in UF-Ex
/// </summary>
public static class DefaultValues
{
	#region Instance depths
	public const float DEFAULT_HEART_PIECE_DEPTH = 0.9f;
	public const float DEFAULT_GUN_BULLET_DEPTH = 0.64f;
	public const float DEFAULT_NORMAL_GB_DEPTH = 0.6f;
	public const float DEFAULT_BOMB_BLAST_DEPTH = 0.53f;
	public const float DEFAULT_BOMB_DEPTH = 0.501f;
	public const float DEFAULT_FIREBALL_DEPTH = 0.5f;
	public const float DEFAULT_SPIDER_DEPTH = 0.5f;
	public const float DEFAULT_TIME_TIPS_DEPTH = 0.5f;
	public const float DEFAULT_ARROW_DEPTH = 0.5f;
	public const float DEFAULT_ARROW_COLOR_DEPTH_DELTA = 1 / 2000f;
	public const float DEFAULT_GREEN_SOUL_GB_DEPTH = 0.466f;
	public const float DEFAULT_YELLOW_SOUL_BLOCK_DEPTH = 0.45f;
	public const float DEFAULT_SHIELD_DEPTH = 0.43f;
	public const float DEFAULT_GUN_BULLET_CROSSHAIR_DEPTH = 0.41f;
	public const float DEFAULT_COLLIDE_EFFECT_DEPTH = 0.4f;
	public const float DEFAULT_PLATFORM_DEPTH = 0.35f;
	public const float DEFAULT_HEART_DEPTH = 0.3f;
	public const float DEFAULT_ACCURACY_BAR_DEPTH = 0.15f;
	public const float DEFAULT_PURPLE_LINE_DEPTH = 0.1f;
	#endregion
	#region Accuracy bar
	public static readonly Vector2 DEFAULT_ACCURACY_BAR_POSITION = new(320, 582);
	#endregion
	#region HP Bar
	/// <summary>
	/// Default position of the HP bar
	/// </summary>
	public static readonly Vector2 DEFAULT_HP_BAR_POSITION = new(320, 443);
	/// <summary>
	/// Default scale of the hp bar
	/// </summary>
	public static readonly Vector2 DEFAULT_HP_BAR_SCALE = new(100, 24);
	/// <summary>
	/// Default rectangle of the HP bar
	/// </summary>
	public static readonly Rectangle DEFAULT_HP_BAR_BOX = new(320, 443, 100, 24);
	/// <summary>
	/// Default HP bar color of existing HP
	/// </summary>
	public static readonly Color DEFAULT_HP_EXIST_COLOR = Color.Lime;
	/// <summary>
	/// Default HP bar color of lost HP
	/// </summary>
	public static readonly Color DEFAULT_HP_LOSE_COLOR = Color.Red;
	/// <summary>
	/// Default HP bar color of the KR bar
	/// </summary>
	public static readonly Color DEFAUL_HP_KR_COLOR = Color.Fuchsia;
	#endregion
	#region Name shower
	public static readonly Vector2 DEFAULT_NAME_SHOWER_POSITION = new(20, 457);
	#endregion
	#region Score types
	public struct ScoreTextData(string key, Color color)
	{
		/// <summary>
		/// The localization key of the score type
		/// </summary>
		public string Key = key;
		/// <summary>
		/// The name of the score type
		/// </summary>
		public string Name = key;
		/// <summary>
		/// The color assigned to the score type
		/// </summary>
		public Color Color = color;
		/// <summary>
		/// The name and color of the score type
		/// </summary>
		public readonly (string, Color) Data => (Name, Color);
		/// <summary>
		/// Updates the name to the localized version based on the key
		/// </summary>
		public void LoadName() => Name = Localization.GetText($"ChartJudgement.{Key}");
		public static implicit operator string(ScoreTextData data) => data.Name;
	}
	public static readonly ScoreTextData[] ScoreTypes = [
		new("Miss", Color.Red),
		new("Okay", Color.Green),
		new("Nice", Color.LightBlue),
		new("Perfect", Color.Gold),
		new("PerfectE", Color.Orange),
		new("PerfectL", Color.Orange)
	];
	public enum ScoreType
	{
		Miss = 0,
		Okay = 1,
		Nice = 2,
		Perfect = 3,
		PerfectE = 4,
		PerfectL = 5
	}
	#endregion
	#region Difficulty names
	public struct DifficultyTextData(Difficulty key, Color color)
	{
		/// <summary>
		/// The difficulty key of the data
		/// </summary>
		public int Key = (int)key;
		/// <summary>
		/// The name of the difficulty
		/// </summary>
		public string Name;
		/// <summary>
		/// The color assigned to the difficulty
		/// </summary>
		public Color Color = color;
		/// <summary>
		/// The name and color of the difficulty
		/// </summary>
		public readonly (string, Color) Data => (Name, Color);
		/// <summary>
		/// Loads the localized name of the difficulty based on the key
		/// </summary>
		public void LoadName() => Name = Localization.GetText($"Difficulty[{Key}]");
		public readonly string this[Difficulty key] => Name;
		public static implicit operator string(DifficultyTextData data) => data.Name;
	}
	public static readonly DifficultyTextData[] DifficultyTypes = [
		new(Difficulty.Noob, Color.White),
		new(Difficulty.Easy, Color.LawnGreen),
		new(Difficulty.Normal, Color.LightBlue),
		new(Difficulty.Hard, Color.MediumPurple),
		new(Difficulty.Extreme, Color.Orange),
		new(Difficulty.ExtremePlus, Color.Gray)
	];
	#endregion
	public static void RegenerateDynamicValues()
	{
		//Update score type names
		for (int i = 0; i < ScoreTypes.Length; i++)
			ScoreTypes[i].LoadName();
		//Update difficulty names
		for (int i = 0; i < DifficultyTypes.Length; i++)
			DifficultyTypes[i].LoadName();
	}
}