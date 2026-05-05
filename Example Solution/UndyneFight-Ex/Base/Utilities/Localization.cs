using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace UndyneFight_Ex;

/// <summary>
/// Utilities for localization
/// </summary>
public static class Localization
{
	/// <summary>
	/// Whether to enable hot-reloading of localization files
	/// </summary>
	public static bool EnableHotReload { get; set; } = false;
	/// <summary>
	/// Gets the current language, can be used as key
	/// </summary>
	public static string CurrentLanguage
	{
		get => currentLanguage;
		set
		{
			if (_TranslationData.ContainsKey(value))
				currentLanguage = value;
			else
				Debug.WriteLine($"Language {value} not found, keeping current language {currentLanguage}.");
		}
	}
	private static string currentLanguage = "English";
	/// <summary>
	/// The common translation data
	/// </summary>
	/// <param name="translations">The list of translations</param>
	private readonly struct TranslationData(JObject translations)
	{
		/// <summary>
		/// The localized fonts
		/// </summary>
		public readonly Dictionary<string, (GLFont Font, float Scale)> FontData = [];
		/// <summary>
		/// The list of translations
		/// </summary>
		public readonly JObject _translations = translations;
	}
	/// <summary>
	/// Internal mapping for font keys on file load
	/// </summary>
	private static readonly Dictionary<string, GLFont> DefaultLocalizedFonts = new()
	{
		["NormalFont"] = GlobalResources.Font.NormalFont,
		["FightFont"] = GlobalResources.Font.FightFont,
		["DamageFont"] = GlobalResources.Font.DamageFont,
	};
	/// <summary>
	/// The dictionary for all translations
	/// </summary>
	private static readonly Dictionary<string, TranslationData> _TranslationData = [];
	/// <summary>
	/// The directory for the localization files
	/// </summary>
	private static readonly string _localizationDirectory = $"{GameStates.SavePath}\\Localization";
	/// <summary>
	/// Initializes the font and string data
	/// </summary>
	public static void Initialize()
	{
		//Ensure directory exists
		if (!Directory.Exists(_localizationDirectory))
			_ = Directory.CreateDirectory(_localizationDirectory);
		//Copy default localization file to the directory if it doesn't exist
		if (!File.Exists(Path.Combine(_localizationDirectory, "English.json")))
			File.Copy(Path.Combine("Content", "DefaultEnglish.json"), Path.Combine(_localizationDirectory, "English.json"), true);
		else //Check if the current localization file is the same as the default
		{
			string defaultJson = File.ReadAllText(Path.Combine("Content", "DefaultEnglish.json"));
			string currentJson = File.ReadAllText(Path.Combine(_localizationDirectory, "English.json"));
			if (defaultJson != currentJson)
				File.Copy(Path.Combine("Content", "DefaultEnglish.json"), Path.Combine(_localizationDirectory, "English.json"), true);
			else
				Debug.WriteLine("Current English localization file is up to date.");
		}
		//Load localization data
		foreach (string file in Directory.GetFiles(_localizationDirectory, "*.json"))
		{
			string lang = Path.GetFileNameWithoutExtension(file);
			string json = File.ReadAllText(file);
			TranslationData data = new(JObject.Parse(json));
			//Store the font for the language
			foreach (string FontKey in DefaultLocalizedFonts.Keys)
			{
				string fontPath = _localizationDirectory + "\\" + data._translations["Fonts"][FontKey]["Path"].ToString();
				//Load font file if exists
				if (File.Exists(fontPath))
					data.FontData[FontKey] = (new GLFont(fontPath, GameMain.instance.Content), data._translations["Fonts"][FontKey]["Scale"].Value<float>());
				else //Check if it's a built-in font
				{
					//If the font specified is a built-in font
					switch (fontPath)
					{
						case "NormalFont":
							data.FontData[FontKey] = (FightResources.Font.NormalFont, data._translations["Fonts"][FontKey]["Scale"].Value<float>());
							break;
						case "SansFont":
							data.FontData[FontKey] = (FightResources.Font.SansFont, data._translations["Fonts"][FontKey]["Scale"].Value<float>());
							break;
						case "DamageFont":
							data.FontData[FontKey] = (FightResources.Font.DamageFont, data._translations["Fonts"][FontKey]["Scale"].Value<float>());
							break;
						case "FightFont":
							data.FontData[FontKey] = (FightResources.Font.FightFont, data._translations["Fonts"][FontKey]["Scale"].Value<float>());
							break;
						default:
							Debug.WriteLine($"Font {fontPath} for language {lang} not recognized, using default font.");
							data.FontData[FontKey] = (FightResources.Font.NormalFont, data._translations["Fonts"][FontKey]["Scale"].Value<float>());
							break;
					}
				}
				_ = data._translations.Remove(FontKey); //Remove the font entry from the localization data for minor optimization
			}
			_TranslationData.Add(lang, data);
		}
	}
	/// <summary>
	/// Logic for hot reloading
	/// </summary>
	internal static void ProcessHotReload()
	{
		if (!EnableHotReload)
			return;
		foreach (string file in Directory.GetFiles(_localizationDirectory, "*.json"))
			if (FileModified(file))
				Initialize();
		(string name, bool removed) = FileRemoved();
		if (removed && name.EndsWith(".json"))
			Initialize();
	}
	#region Hot Reload
	private static readonly Dictionary<string, (DateTime, long)> _FileData = [];
	/// <summary>
	/// Check if any files are removed (Cached files)
	/// </summary>
	/// <returns>The name of the file removed</returns>
	private static (string name, bool removed) FileRemoved()
	{
		//Deleted files
		foreach (string item in _FileData.Keys)
		{
			if (!File.Exists(Path.Combine(item.Split('\\'))))
			{
				_ = _FileData.Remove(item);
				return (item, true);
			}
		}
		return ("", false);
	}
	/// <summary>
	/// Checks if a file had been modified
	/// </summary>
	/// <param name="path">The file path</param>
	/// <returns>Whether the file was modified</returns>
	private static bool FileModified(string path)
	{
		//New files
		if (!_FileData.TryGetValue(path, out (DateTime lastModifyTime, long fileSize) entry))
		{
			entry = new(File.GetLastWriteTime(path), new FileInfo(path).Length);
			_FileData[path] = entry;
			return true;
		}
		//Modified files
		for (int i = 0; i < _FileData.Values.Count; i++)
		{
			DateTime LastMod = File.GetLastWriteTime(path);
			if (entry.lastModifyTime != LastMod || entry.fileSize != new FileInfo(path).Length)
			{
				_FileData[path] = new(LastMod, new FileInfo(path).Length);
				return true;
			}
		}
		return false;
	}
	#endregion
	/// <summary>
	/// Returns the target font for the current language
	/// </summary>
	/// <param name="key">The sub-font to get</param>
	/// <returns>The font you are trying to get</returns>
	public static GLFont GetFont(string key) => _TranslationData.TryGetValue(CurrentLanguage, out TranslationData fonts) && fonts.FontData.TryGetValue(key, out (GLFont Font, float Scale) font) ? font.Font : DefaultLocalizedFonts[key];
	/// <summary>
	/// Gets the scale of the localized font
	/// </summary>
	/// <param name="key">The font to get</param>
	/// <returns></returns>
	public static float GetFontScale(string key) => _TranslationData[CurrentLanguage].FontData[key].Scale;
	/// <summary>
	/// Gets the font data of the localized font
	/// </summary>
	/// <param name="key">The font to get</param>
	/// <returns></returns>
	public static (GLFont Font, float Scale) GetFontData(string key) => _TranslationData[CurrentLanguage].FontData[key];
	/// <summary>
	/// Gets the text of the specified key in the current language
	/// </summary>
	/// <param name="key">The key in the .json file</param>
	/// <param name="arguments">The arguments to format the text with, if the text contains {0}, {1} etc.</param>
	/// <returns>The localized text</returns>
	public static string GetText(string key, params object[] arguments)
	{
		string finalText = _TranslationData[CurrentLanguage]._translations.SelectToken(key).ToString();
		for (int i = 0; i < arguments?.Length; i++)
			finalText = finalText.Replace($"{{{i}}}", arguments[i].ToString());
		return finalText;
	}
	/// <summary>
	/// Gets the current translation data
	/// </summary>
	/// <returns></returns>
	public static JObject GetTranslationData() => _TranslationData[CurrentLanguage]._translations;
	/// <summary>
	/// Draws the localized text with the given parameters
	/// </summary>
	/// <param name="key">The key used in the localization file</param>
	/// <param name="position">The position to draw the text</param>
	/// <param name="param">The optional parameters used in the text</param>
	/// <param name="font">The font used to draw the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="rotation">The rotation of the text</param>
	/// <param name="depth">The depth of the text</param>
	/// <param name="align">The alignment of the text</param>
	public static void DrawLocalizedText(string key, Vector2 position, object[] param = null, string font = "NormalFont", Vector2? scale = null, Color? color = null, float rotation = 0, float depth = 0, DrawAlign align = DrawAlign.Left)
	{
		if (GetFont(font) is null)
			return;
		Vector2 finSize = (scale ?? Vector2.One) * _TranslationData[CurrentLanguage].FontData[font].Scale;
		string targetString = GetText(key, param);
		switch (align)
		{
			case DrawAlign.Left:
				GetFont(font).Draw(targetString, position, color ?? Color.White, rotation, finSize, depth);
				break;
			case DrawAlign.Middle:
				GetFont(font).CentreDraw(targetString, position, color ?? Color.White, finSize, rotation, depth);
				break;
			case DrawAlign.Right:
				break;
		}
	}
	/// <summary>
	/// The drawing alignment of the localized text for <see cref="DrawLocalizedText(string, Vector2, object[], string, Vector2?, Color?, float, float, DrawAlign)"/>
	/// </summary>
	public enum DrawAlign
	{
		Left, Middle, Right
	}
}