using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.Settings.SettingLibrary;

namespace UndyneFight_Ex.Settings
{
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
	public static class SettingsManager
	{
		public static Type[] Settings { get; private set; }

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

		public static class DataLibrary
		{
			public enum DrawingQuality
			{
				Low = 0,
				Normal = 1,
				High = 2
			}
			public static int SpearBlockSound = 1;
			public static int masterVolume = 100;
			public static bool debugMessage = false;
			public static bool dialogAvailable = true;
			public static bool preciseWarning = false;
			public static int reduceBlueAmount = 0;
			public static DrawingQuality drawingQuality = DrawingQuality.High;

			public static int SpearBlockingVolume { get; set; } = 100;
			public static int SFXVolume { get; set; } = 100;
			public static float ArrowSpeed { get; set; } = 1.0f;
			public static int ArrowDelay { get; set; } = 0;
			public static float ArrowScale { get; set; } = 1.0f;
			public static bool Mirror { get; set; } = false;
			public static bool DisplayScorePercent { get; set; } = false;
			public static float DrawFPS { get; set; } = 125f;
			public static string SamplerState { get; set; } = "Nearest";
			public static void Reset(string curSet = "")
			{
				if (curSet is "" or "Audio")
				{
					SpearBlockSound = 1;
					masterVolume = 100;
					SpearBlockingVolume = 100;
					SFXVolume = 100;
				}
				if (curSet is "" or "Video")
				{
					reduceBlueAmount = 0;
					drawingQuality = DrawingQuality.High;
					DrawFPS = 60;
					SamplerState = "Nearest";
				}
				if (curSet is "" or "Gameplay")
				{
					preciseWarning = false;
					ArrowSpeed = 1;
					ArrowScale = 1;
					Mirror = false;
					DisplayScorePercent = false;
				}
				if (curSet is "" or "Input")
					ArrowDelay = 0;
			}
		}
	}
}