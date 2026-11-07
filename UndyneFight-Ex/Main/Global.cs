//Assign global usings
global using Microsoft.Xna.Framework;
global using MonoGame.Extended.Graphics;
global using System;
global using System.Runtime.CompilerServices;
global using col = Microsoft.Xna.Framework.Color;
global using vec2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.StringUtil;

/// <summary>
/// Global data, usually for caching and accessing such data
/// </summary>
public static class GlobalData
{
	/// <summary>
	/// The global loader
	/// </summary>
	public static ContentManager Loader;
	#region Waveset utilities
	/// <summary>
	/// The cache for paints of charts, note that key is the <see cref="IWaveSet.FightName"/> of the waveset, not the display name
	/// </summary>
	public static Dictionary<string, Texture2D?> WavePaint { get; set; } = [];
	/// <summary>
	/// The cache for charts, note that key is the <see cref="IWaveSet.FightName"/> of the waveset, not the display name
	/// </summary>
	public static Dictionary<string, (float Min, float Max)> WaveBPM { get; set; } = [];
	/// <summary>
	/// Gets the name of the waveset
	/// </summary>
	/// <param name="set">The waveset to get</param>
	/// <returns>The <see cref="IWaveSet.Attributes.DisplayName"/> of the waveset, if any, <see cref="IWaveSet.FightName"/> if not</returns>
	public static string GetWavesetDisplayName(IWaveSet set) => set.Attributes.DisplayName.DefaultIfNullOrEmpty(set.FightName);
	/// <summary>
	/// Gets the paint of the given waveset
	/// </summary>
	/// <param name="set">The waveset to get</param>
	/// <returns>The paint if any, null if not</returns>
	public static Texture2D GetWavePaint(IWaveSet set) => WavePaint.TryGetValue(set.FightName, out Texture2D paint) ? paint : null;
	/// <summary>
	/// The highest AP constant of all charts
	/// </summary>
	public static float MaxChartAPConstant = 0;
	/// <summary>
	/// The highest FC constant of all charts
	/// </summary>
	public static float MaxChartCPXConstant = 0;
	/// <summary>
	/// The top 7 highest complex constants of all charts
	/// </summary>
	public static List<float> MaxChartCPXConstants = [];
	/// <summary>
	/// The highest Clear constant of all charts
	/// </summary>
	public static float MaxChartCLRConstant = 0;
	#endregion
	#region Audio Preview
	/// <summary>
	/// The data for audio previews
	/// </summary>
	public struct AudioPreviewData(string path, float begin, float end, Audio preview)
	{
		/// <summary>
		/// The path to the audio preview
		/// </summary>
		[JsonPropertyName("Path")]
		public string Path = path;
		/// <summary>
		/// The second the preview begins
		/// </summary>
		[JsonPropertyName("BeginSecond")]
		public float BeginSecond = begin;
		/// <summary>
		/// The second the preview ends
		/// </summary>
		[JsonPropertyName("EndSecond")]
		public float EndSecond = end;
		/// <summary>
		/// The preview audio stored
		/// </summary>
		[JsonIgnore]
		public Audio PreviewAudio = preview;
		/// <summary>
		/// Whether the audio is loaded
		/// </summary>
		[JsonIgnore]
		public readonly bool IsLoaded => PreviewAudio != null;
		/// <inheritdoc/>
		public override readonly string ToString() => $"{Path} between [{BeginSecond} -> {EndSecond}] is {(IsLoaded ? "loaded" : "not loaded")}";
	}
	/// <summary>
	/// The list of audio preview datas
	/// </summary>
	public static List<AudioPreviewData> AudioPreviewDatas { get; } = [];
	/// <summary>
	/// The amount of loaded audio previews
	/// </summary>
	public static int LoadedPreviewAudioCount => AudioPreviewDatas.Count(s => s.IsLoaded);
	/// <summary>
	/// Gets the audio preview data from the path
	/// </summary>
	/// <param name="path">The name of the music</param>
	/// <returns>The audio preview data of the music</returns>
	public static AudioPreviewData GetPreviewDataFromPath(string path) => AudioPreviewDatas.Find(s => s.Path == path);
	internal static CancellationTokenSource cancelTokenSource = new();
	private static readonly CancellationToken cancelToken = cancelTokenSource.Token;
	private static readonly List<Task> AudioLoadingTasks = [];
	internal static float AudioLoadProgress => AudioLoadingTasks.Count(s => s.IsCompletedSuccessfully) / AudioLoadingTasks.Count;
	internal static HashSet<string> QueueLoadSongs = [];
	/// <summary>
	/// The default difficulty names for <see cref="IWaveSet"/>
	/// </summary>
	private static readonly Dictionary<Difficulty, string> DefaultDiffNames = new()
	{
		[Difficulty.Noob] = DefaultValues.DifficultyTypes[0],
		[Difficulty.Easy] = DefaultValues.DifficultyTypes[1],
		[Difficulty.Normal] = DefaultValues.DifficultyTypes[2],
		[Difficulty.Hard] = DefaultValues.DifficultyTypes[3],
		[Difficulty.Extreme] = DefaultValues.DifficultyTypes[4]
	};
	private static readonly string AudioPreviewDataJSON = $"{GameStates.SavePath}\\Audio Previews\\AudioPreviewData.json";
	private static readonly JsonSerializerOptions JsonOption = new() { IncludeFields = true };
	/// <summary>
	/// Loads the song previews of the charts
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static async Task<bool> LoadSongPreviews()
	{
		DebugState.Log("Loading song previews...", true);
		//Ensure preview audio folder exists
		if (!Directory.Exists(GetTruePath($"{GameStates.SavePath}\\Audio Previews")))
			_ = Directory.CreateDirectory(GetTruePath($"{GameStates.SavePath}\\Audio Previews"));
		//Ensure FFMpeg exists
		DebugState.Log("Checking FFMpeg availability for audio preview processing...", true);
		bool isFFMpegAvailable = await EnsureFFMpegInstalled();
		//Store audio preview positions
		List<Type> allCharts = [..FightSystem.AllSongs.Values.Concat(FightSystem.CustomSongs.Values)];
		foreach (SongSet extraSongs in FightSystem.ExtraSongSets)
			allCharts.AddRange([.. extraSongs.Values]);
		//Pre-load existing audio preview data from JSON file if it exists
		if (File.Exists(AudioPreviewDataJSON))
		{
			DebugState.Log("Loading audio preview data from AudioPreviewData.json...", true);
			string json = await File.ReadAllTextAsync(AudioPreviewDataJSON);
			List<AudioPreviewData> loadedData = JsonSerializer.Deserialize<List<AudioPreviewData>>(json, JsonOption);
			if (loadedData != null)
				AudioPreviewDatas.AddRange(loadedData);
		}
		foreach (Type i in allCharts)
		{
			object o = Activator.CreateInstance(i);
			//Store difficulty name of championships
			if (o is IChampionShip cmp)
			{
				Dictionary<Difficulty, string> curDiffNames = [];
				//Read the user defined difficulty names of the championship chart
				foreach (Difficulty val in cmp.DifficultyPanel.Values)
					curDiffNames.Add(val, Localization.GetText($"Difficulty[{(int)val}]"));
				_ = ChartDifficultyNames.TryAdd(cmp.GameContent.FightName, curDiffNames);
			}
			IWaveSet waveSet = o is IWaveSet wave ? wave : (o as IChampionShip).GameContent;
			//Store max constants for charts
			if (waveSet.Attributes.APDifficulty?.Values.Count > 0)
				MaxChartAPConstant = float.Max(waveSet.Attributes.APDifficulty.Values.Max(), MaxChartAPConstant);
			if (waveSet.Attributes.ComplexDifficulty?.Values.Count > 0)
			{
				MaxChartCPXConstant = float.Max(waveSet.Attributes.ComplexDifficulty.Values.Max(), MaxChartCPXConstant);
				MaxChartCPXConstants.AddRange(waveSet.Attributes.ComplexDifficulty.Values);
			}
			if (waveSet.Attributes.CompleteDifficulty?.Values.Count > 0)
				MaxChartCLRConstant = float.Max(waveSet.Attributes.CompleteDifficulty.Values.Max(), MaxChartCLRConstant);
			//Store difficulty name of wavesets
			if (o is IWaveSet)
				_ = ChartDifficultyNames.TryAdd(waveSet.FightName, DefaultDiffNames);
			//Queue audio preview for loading if file does not exist
			if (!QueueLoadSongs.Contains(GetTruePath($"{waveSet.Music}//song.ogg")))
			{
				if (!AudioPreviewDatas.Any(item => item.Path == waveSet.Music))
					AudioPreviewDatas.Add(new(waveSet.Music, waveSet.Attributes.MusicPreview[0], waveSet.Attributes.MusicPreview[1], null));
				_ = QueueLoadSongs.Add(GetTruePath($"{waveSet.Music}//song.ogg"));
				DebugState.Log($"Queued audio preview for {waveSet.Music} at [{waveSet.Attributes.MusicPreview[0]} -> {waveSet.Attributes.MusicPreview[1]}]", true);
			}
			//Queue audio preview for loading if JSON data mismatches updated data
			else if (GetPreviewDataFromPath(waveSet.Music) is AudioPreviewData prevD && (prevD.BeginSecond != waveSet.Attributes.MusicPreview[0] || prevD.EndSecond != waveSet.Attributes.MusicPreview[1]))
			{
				AudioPreviewDatas.Remove(prevD);
				AudioPreviewDatas.Add(new(waveSet.Music, waveSet.Attributes.MusicPreview[0], waveSet.Attributes.MusicPreview[1], null));
				_ = QueueLoadSongs.Add(GetTruePath($"{waveSet.Music}//song.ogg"));
				DebugState.Log($"Queued audio preview for {waveSet.Music} at [{waveSet.Attributes.MusicPreview[0]} -> {waveSet.Attributes.MusicPreview[1]}]", true);
			}
			string dir = GetTruePath($"{GameStates.SavePath}\\Audio Previews\\{waveSet.Music}");
			//Cache wave paint as well
			if (File.Exists(GetTruePath($"Content//Musics//{waveSet.Music}//paint.xnb")))
				_ = WavePaint.TryAdd(waveSet.FightName, LoadContent<Texture2D>($"Musics//{waveSet.Music}//paint", Loader));
			if (waveSet is WaveConstructor wc)
			{
				WaveBPM.TryAdd(waveSet.FightName, (wc.MinBPM, wc.MaxBPM));
				wc.Dispose();
			}
		}
		//Create JSON file for all audio preview data if it does not exist
		if (!File.Exists(AudioPreviewDataJSON))
		{
			DebugState.Log("AudioPreviewData.json not found, creating new file...", true);
			string json = JsonSerializer.Serialize(AudioPreviewDatas, JsonOption);
			await File.WriteAllTextAsync(AudioPreviewDataJSON, json);
		}
		//Sort max complex rating
		MaxChartCPXConstants.Sort((a, b) => b.CompareTo(a));
		//Remove all but top 7
		MaxChartCPXConstants = MaxChartCPXConstants[..7];
		foreach (string file_name in QueueLoadSongs)
		{
			//Won't load non-copied files
			if (!File.Exists(GetTruePath($"Content\\Musics\\{file_name}")))
				continue;
			//Cache audio preview
			string path = GetTruePath($"Musics\\{file_name}");
			_ = await Task.Factory.StartNew(async () =>
			{
				//Perform key string trim
				string key = path.Split(Path.DirectorySeparatorChar)[1];
				if (key.EndsWith(".ogg"))
					key = key[..^4];
				float[] musPreviewPos = GetPreviewDataFromPath(key) is AudioPreviewData prevD ? [prevD.BeginSecond, prevD.EndSecond] : null;
				if (musPreviewPos == null)
					return;
				//Extract preview audio file if it does not exist or audio preview does not match data file
				string preview_file_name = GetTruePath($"Content\\{path[..^4]}_preview.wav"),
						preview_file_final_name = GetTruePath($"{GameStates.SavePath}\\Audio Previews\\{key}_preview.ogg");
				//Trim song preview and cache it on a separate file if no cache is present or cache preview position mismatch
				if (File.Exists(preview_file_final_name))
					goto GetCacheFromFile;
				//Read source ogg file
				VorbisWaveReader vorbis = new(Path.Combine($"Content", path));
				OffsetSampleProvider sample = new(vorbis)
				{
					SkipOver = TimeSpan.FromSeconds(musPreviewPos[0]),
					Take = TimeSpan.FromSeconds(musPreviewPos[1] - musPreviewPos[0])
				};
				//Convert to wave file for trimming
				DebugState.Log("Converting to wav for trimming...", true);
				WaveFileWriter.CreateWaveFile(preview_file_name, new SampleToWaveProvider(sample));
				await vorbis.DisposeAsync();
				//Convert to ogg using ffmpeg
				DebugState.Log($"Trimming preview for {key} using FFMpeg...", true);
				string cmdPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/bin/bash" : "/bin/zsh");
				Process process = new()
				{
					StartInfo = new ProcessStartInfo(cmdPath)
					{
						Arguments = $"/c ffmpeg -i \"{AppContext.BaseDirectory}{preview_file_name}\" \"{preview_file_final_name}\"",
						UseShellExecute = false,
						CreateNoWindow = true,
						RedirectStandardError = true,
					}
				};
				process.ErrorDataReceived += (s, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
						DebugState.Log($"FFMpeg: {e.Data}", true);
				};
				try
				{
					_ = process.Start();
					await process.WaitForExitAsync();
				}
				finally
				{
					process.Dispose();
				}
				DebugState.Log($"Successfully converted {key} preview to ogg format.", true);
				//Delete tempoary wav file
				File.Delete(preview_file_name);
				DebugState.Log($"Removed temporary wav file for {key} preview.", true);
			GetCacheFromFile:
				//Add to audio cache
				AudioPreviewData prevDat = GetPreviewDataFromPath(key);
				int i = AudioPreviewDatas.IndexOf(prevDat);
				prevDat.PreviewAudio = new Audio(preview_file_final_name);
				AudioPreviewDatas[i] = prevDat;
				DebugState.Log($"Stored preview audio for {key} in cache.", true);
			}, cancelToken);
		}
		return true;
	}
	#endregion
	#region FFMPEG Utilities
	private static async Task<bool> EnsureFFMpegInstalled()
	{
		//Check if FFMpeg is already installed in the system path
		if (await IsFFMpegGloballyAvailable())
		{
			DebugState.Log("FFMpeg found in system PATH.", true);
			return true;
		}
		DebugState.Log("FFMpeg not found in PATH. Attempting to download binaries...", true);

		//If not, download and extract
		try
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				try
				{
					DebugState.Log("Downloading FFMpeg binaries...", true);
					InstallFFmpeg();
					DebugState.Log("FFMpeg installation process completed. Restarting game for shell to register...", true);
				}
				catch (Exception ex)
				{
					DebugState.Log($"Error downloading FFMpeg binaries: {ex.Message}", true);
					return false;
				}
			}
			else
				throw new PlatformNotSupportedException("Operating system not supported for automatic download.");

			//Then verify the installation after downloading
			return await IsFFMpegGloballyAvailable();
		}
		catch (Exception ex)
		{
			DebugState.Log($"Error installing FFMpeg: {ex.Message}", true);
			return false;
		}
	}
	private static async void InstallFFmpeg()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			DebugState.Log("Attempting to install FFMpeg using winget...", true);
			_ = await Microsoft.Xna.Framework.Input.MessageBox.Show(GameStates.GameName, "FFMpeg not found, a window will appear for installation, process may take minutes, please do not close that window. Please reopen the game after installation.", ["OK"]);
			RunCommand("/c winget install ffmpeg");
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			RunCommand("-c sudo apt-get update && sudo apt-get install ffmpeg -y");
			DebugState.Log("Attempting to install FFMpeg using apt-get...", true);
			_ = Process.Start("notify-send", "\"UF-Ex\" \"FFMpeg not found, a window will appear for installation, process may take minutes, please do not close that window.Please reopen the game after installation.\"");
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			DebugState.Log("Attempting to install FFMpeg using Homebrew...", true);
			RunCommand("-c brew install ffmpeg");
		}
	}

	private static void RunCommand(string commands)
	{
		string file = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/bin/bash" : "/bin/zsh");
		ProcessStartInfo startInfo = new(file, commands)
		{
			UseShellExecute = false,
			CreateNoWindow = false,
		};
		using Process process = Process.Start(startInfo);
		process.WaitForExit();
	}
	private static async Task<bool> IsFFMpegGloballyAvailable()
	{
		try
		{
			DebugState.Log("Checking if FFMpeg is available in system PATH...", true);
			using Process process = new()
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg",
					Arguments = "-version",
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			_ = process.Start();
			await process.WaitForExitAsync();
			return process.ExitCode == 0;
		}
		catch
		{
			return false;
		}
	}
	#endregion
	#region Loading content
	/// <summary>
	/// Loads an asset with the given path (auto convert to cross platform)
	/// </summary>
	/// <typeparam name="T">he type of content to load</typeparam>
	/// <param name="path">The path of the asset to load</param>
	/// <param name="cm">The content manager to use for loading (Default <see cref="Scene.Loader"/>)</param>
	/// <returns>The loaded asset</returns>
	public static T LoadContent<T>(string path, ContentManager cm = null)
	{
		path = GetTruePath(path.EndsWith(".xnb") ? path[..^4] : path);
		return (cm ??= Scene.Loader).Load<T>($"{(path.StartsWith(cm.RootDirectory) && cm.RootDirectory != string.Empty ? path.Replace(cm.RootDirectory + Path.DirectorySeparatorChar, null) : path)}");
	}
	#endregion
	#region Chart Data
	/// <summary>
	/// The names of the difficulties of the charts
	/// </summary>
	public static readonly Dictionary<string, Dictionary<Difficulty, string>> ChartDifficultyNames = [];
	#endregion
	#region Pathing
	/// <summary>
	/// Gets the true path of a file, which is the path with all separators replaced with the current system's directory separator<br/>
	/// </summary>
	/// <param name="path">The default UNIX file path</param>
	/// <returns>The current system's path</returns>
	public static string GetTruePath(string path)
	{
		StringBuilder pathBuilder = new(path);
		_ = pathBuilder.Replace("//", Path.DirectorySeparatorChar.ToString()).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
		return pathBuilder.ToString();
	}
	#endregion
}