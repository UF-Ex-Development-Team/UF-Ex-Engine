using UndyneFight_Ex.IO;
using static UndyneFight_Ex.Settings.SettingsManager.DataLibrary;

namespace UndyneFight_Ex.UserService;
/// <summary>
/// User custom settings
/// </summary>
public class Settings : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => throw new NotImplementedException();
	/// <summary>
	/// Initializes the default settings
	/// </summary>
	internal static void Initialize()
	{
		AddSetting(new SettingsInfo<int>("masterVolume", "MasterVolume", 100));
		AddSetting(new SettingsInfo<int>("sfxVolume", "SFXVolume", 100));
		AddSetting(new SettingsInfo<int>("spearBlockingVolume", "SpearBlockingVolume", 100));
		AddSetting(new SettingsInfo<int>("spearBlockSound", "SpearBlockSound", 1));
		AddSetting(new SettingsInfo<int>("reduceBlueAmount", "ReduceBlueAmount", 0));
		AddSetting(new SettingsInfo<float>("arrowDelay", "ArrowDelay", 0f));
		AddSetting(new SettingsInfo<float>("arrowSpeed", "ArrowSpeed", 1f));
		AddSetting(new SettingsInfo<float>("arrowScale", "ArrowScale", 1f));
		AddSetting(new SettingsInfo<float>("fps", "DrawFPS", 125f));
		AddSetting(new SettingsInfo<bool>("mirror", "Mirror", false));
		AddSetting(new SettingsInfo<bool>("dialogAvailable", "DialogAvailable", true));
		AddSetting(new SettingsInfo<bool>("preciseWarning", "PreciseWarning", false));
		AddSetting(new SettingsInfo<int>("drawingQuality", "DrawingQuality", (int)DrawingQuality.High));
		AddSetting(new SettingsInfo<string>("samplerState", "SamplerState", "Nearest"));
		AddSetting(new SettingsInfo<bool>("dispScorePercent", "DisplayScorePercent", false));
		AddSetting(new SettingsInfo<float>("arrOutScale", "ArrowOutlineScale", 1f));
		AddSetting(new SettingsInfo<bool>("anomalyReplay", "AnomalyReplay", false));
		AddSetting(new SettingsInfo<bool>("advInd", "AdvancedIndicators", false));
		AddSetting(new SettingsInfo<string>("branch", "BuildBranch", DebugState.IsBeta ? "Beta" : "Stable"));
		AddSetting(new SettingsInfo<bool>("oldBlasters", "OldBlasterColors", false));
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		foreach (SettingsBase item in settingsList.Values)
		{
			if (!info.Nexts.TryGetValue(item.SaveName, out SaveInfo value))
				continue;
			Type targetType = item.GetType().GetProperty("DataType")?.GetValue(item) as Type;
			if (targetType != typeof(float) && targetType != typeof(int))
			{
				item.GetType().GetProperty("CurrentValue")?.SetValue(item, Convert.ChangeType(value.StringValue, targetType));
			}
			else
			{
				float floatValue = (value.StringValue).ToFloat();
				//Do NOT remove the (object) cast
				item.GetType().GetProperty("CurrentValue")?.SetValue(item, targetType == typeof(int) ? (int)floatValue : (object)floatValue);
			}
		}
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo info = new("Settings{");
		foreach (SettingsBase item in settingsList.Values)
			info.PushNext(new SaveInfo($"{item.SaveName}:{item.GetType().GetProperty("CurrentValue")?.GetValue(item)}"));
		return info;
	}
	public abstract class SettingsBase(string saveName, string getName)
	{
		/// <summary>
		/// The save name of the settings
		/// </summary>
		public string SaveName { get; init; } = saveName;
		/// <summary>
		/// The name you use in code
		/// </summary>
		public string GetName { get; init; } = getName;
	}
	/// <summary>
	/// Custom settings info
	/// </summary>
	/// <typeparam name="T">The type of the setting</typeparam>
	/// <param name="saveName">The save name of the setting</param>
	/// <param name="getName">The name you use in code</param>
	/// <param name="defVal">The default value</param>
	public class SettingsInfo<T>(string saveName, string getName, object defVal) : SettingsBase(saveName, getName)
	{
		/// <summary>
		/// The data type of the setting
		/// </summary>
		public Type DataType => typeof(T);
		/// <summary>
		/// The default value of the setting
		/// </summary>
		public T DefaultValue { get; init; } = (T)defVal;
		/// <summary>
		/// The current value of the setting
		/// </summary>
		public T CurrentValue { get; set; } = (T)defVal;
	}
	/// <summary>
	/// The list of included settings
	/// </summary>
	internal static readonly Dictionary<string, SettingsBase> settingsList = [];
	/// <summary>
	/// Adds a new setting to the list of included settings
	/// </summary>
	/// <param name="info">The information of the setting to add</param>
	public static void AddSetting(SettingsBase info) => settingsList.Add(info.GetName, info);
	/// <summary>
	/// Gets the value of the setting with the specified name
	/// </summary>
	/// <param name="name">The <see cref="SettingsBase.GetName"/> of the setting</param>
	/// <returns>The current value of the setting</returns>
	/// <remarks>Note that <see cref="SettingsBase"/> cannot be casted as <see cref="SettingsInfo{T}"/> directly, therefore a reflection is required until I come up with something better</remarks>
	public static object GetSettingsValue(string name) => settingsList.TryGetValue(name, out SettingsBase info) ? (info.GetType().GetProperty("CurrentValue")?.GetValue(info)) : null;
	/// <summary>
	/// Sets the value of the setting with the specified name
	/// </summary>
	/// <param name="name">The <see cref="SettingsBase.GetName"/> of the setting</param>
	/// <param name="value">The value to set to</param>
	public static void SetSettingsValue(string name, object value)
	{
		if (!settingsList.TryGetValue(name, out SettingsBase info))
			return;
		Type dataType = info.GetType().GetProperty("DataType")?.GetValue(info) as Type;
		if (dataType != null && value.GetType() == dataType)
			info.GetType().GetProperty("CurrentValue")?.SetValue(info, value);
		PlayerManager.SaveSettings();
	}
}