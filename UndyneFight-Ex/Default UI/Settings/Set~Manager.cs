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
public static partial class SettingsManager
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
}