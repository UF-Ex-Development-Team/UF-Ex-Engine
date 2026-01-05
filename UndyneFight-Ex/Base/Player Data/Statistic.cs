using UndyneFight_Ex.IO;
using UndyneFight_Ex.Settings;
using static UndyneFight_Ex.Settings.SettingsManager.DataLibrary;

namespace UndyneFight_Ex.UserService;
/// <summary>
/// User custom settings
/// </summary>
public class Settings : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => throw new NotImplementedException();

	private int masterVolume, spearBlockingVolume, reduceBlueAmount, drawingQuality;
	private float arrowDelay, arrowSpeed, arrowScale, fps;
	private bool dialogAvailable, preciseWarning, mirror, displayScorePercent, newArrowDrawMethod;
	private string samplerState;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		masterVolume = info.Nexts.TryGetValue("masterVolume", out SaveInfo value) ? value.IntValue : SettingsManager.DataLibrary.masterVolume;
		spearBlockingVolume = info.Nexts.TryGetValue("spearBlockingVolume", out value) ? value.IntValue
			: SpearBlockingVolume;
		reduceBlueAmount = info.Nexts.TryGetValue("reduceBlueAmount", out value) ? value.IntValue
			: SettingsManager.DataLibrary.reduceBlueAmount;
		arrowDelay = info.Nexts.TryGetValue("arrowDelay", out value) ? value.FloatValue : ArrowDelay;
		arrowScale = info.Nexts.TryGetValue("arrowScale", out value) ? value.FloatValue : ArrowScale;
		arrowSpeed = info.Nexts.TryGetValue("arrowSpeed", out value) ? value.FloatValue : ArrowSpeed;
		fps = info.Nexts.TryGetValue("fps", out value) ? value.FloatValue : DrawFPS;
		mirror = info.Nexts.TryGetValue("mirror", out value) ? value.BoolValue : Mirror;
		dialogAvailable = info.Nexts.TryGetValue("dialogAvailable", out value) ? value.BoolValue
			: SettingsManager.DataLibrary.dialogAvailable;
		preciseWarning = info.Nexts.TryGetValue("preciseWarning", out value) ? value.BoolValue : SettingsManager.DataLibrary.preciseWarning;
		drawingQuality = info.Nexts.TryGetValue("drawingQuality", out value) ? value.IntValue
			: (int)SettingsManager.DataLibrary.drawingQuality;
		samplerState = info.Nexts.TryGetValue("samplerState", out value) ? value.StringValue
			: SamplerState;
		displayScorePercent = info.Nexts.TryGetValue("dispScorePercent", out value) ? value.BoolValue
			: DisplayScorePercent;
		newArrowDrawMethod = info.Nexts.TryGetValue("newArrMeth", out value) ? value.BoolValue
			: NewArrowDrawingMethod;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Apply()
	{
		SettingsManager.DataLibrary.masterVolume = masterVolume;
		SpearBlockingVolume = spearBlockingVolume;
		SettingsManager.DataLibrary.reduceBlueAmount = reduceBlueAmount;
		ArrowDelay = (int)arrowDelay;
		ArrowSpeed = arrowSpeed;
		ArrowScale = arrowScale;
		Mirror = mirror;
		DrawFPS = float.Clamp(fps, 25, 125);
		SettingsManager.DataLibrary.preciseWarning = preciseWarning;
		SettingsManager.DataLibrary.dialogAvailable = dialogAvailable;
		SettingsManager.DataLibrary.drawingQuality = (DrawingQuality)drawingQuality;
		SamplerState = samplerState;
		DisplayScorePercent = displayScorePercent;
		newArrowDrawMethod = NewArrowDrawingMethod;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		masterVolume = SettingsManager.DataLibrary.masterVolume;
		spearBlockingVolume = SpearBlockingVolume;
		reduceBlueAmount = SettingsManager.DataLibrary.reduceBlueAmount;
		arrowDelay = ArrowDelay;
		arrowSpeed = ArrowSpeed;
		arrowScale = ArrowScale;
		fps = DrawFPS;
		mirror = Mirror;
		preciseWarning = SettingsManager.DataLibrary.preciseWarning;
		dialogAvailable = SettingsManager.DataLibrary.dialogAvailable;
		drawingQuality = (int)SettingsManager.DataLibrary.drawingQuality;
		samplerState = SamplerState;
		displayScorePercent = DisplayScorePercent;
		newArrowDrawMethod = NewArrowDrawingMethod;

		SaveInfo info = new("Settings{");
		info.PushNext(new SaveInfo("masterVolume:" + masterVolume));
		info.PushNext(new SaveInfo("spearBlockingVolume:" + spearBlockingVolume));
		info.PushNext(new SaveInfo("reduceBlueAmount:" + reduceBlueAmount));
		info.PushNext(new SaveInfo("arrowDelay:" + MathUtil.FloatToString(arrowDelay, 3)));
		info.PushNext(new SaveInfo("arrowSpeed:" + MathUtil.FloatToString(arrowSpeed, 3)));
		info.PushNext(new SaveInfo("arrowScale:" + MathUtil.FloatToString(arrowScale, 3)));
		info.PushNext(new SaveInfo("fps:" + MathUtil.FloatToString(fps, 3)));
		info.PushNext(new SaveInfo("mirror:" + (mirror ? "true" : "false")));
		info.PushNext(new SaveInfo("preciseWarning:" + (preciseWarning ? "true" : "false")));
		info.PushNext(new SaveInfo("dialogAvailable:" + (dialogAvailable ? "true" : "false")));
		info.PushNext(new SaveInfo("drawingQuality:" + drawingQuality));
		info.PushNext(new SaveInfo("samplerState:" + samplerState));
		info.PushNext(new SaveInfo("dispScorePercent:" + (displayScorePercent ? "true" : "false")));
		info.PushNext(new SaveInfo("newArrMeth:" + (newArrowDrawMethod ? "true" : "false")));
		return info;
	}
}
/// <summary>
/// User's statistic
/// </summary>
public class Statistic : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => null;
	/// <summary>
	/// The death count of the user
	/// </summary>
	public int DeathCount { get; private set; } = 0;
	/// <summary>
	/// The play time of the user
	/// </summary>
	public int PlayedTime
	{
		get
		{
			UpdateTime();
			return (int)playedTime;
		}
	}
	private float playedTime = 0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateTime()
	{
		TimeSpan del = DateTime.Now - span;
		playedTime += (float)del.TotalSeconds;
		span = DateTime.Now;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddDeath() => DeathCount++;
	private DateTime span = DateTime.Now;
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		DeathCount = info.Nexts["DeathCount"].IntValue;
		playedTime = info.Nexts["PlayedTime"].FloatValue;
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo info = new("Statistic{");
		info.PushNext(new SaveInfo("DeathCount:" + DeathCount));
		info.PushNext(new SaveInfo("PlayedTime:" + MathUtil.FloatToString(playedTime, 4)));
		return info;
	}
}