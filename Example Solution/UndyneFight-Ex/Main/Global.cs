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
	public static Dictionary<string, Texture2D?> WavePaint = [];
	/// <summary>
	/// The cache for charts, note that key is the <see cref="IWaveSet.FightName"/> of the waveset, not the display name
	/// </summary>
	public static Dictionary<string, IWaveSet> WaveCache = [];
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
		public string Path = path;
		/// <summary>
		/// The second the preview begins
		/// </summary>
		public float BeginSecond = begin;
		/// <summary>
		/// The second the preview ends
		/// </summary>
		public float EndSecond = end;
		/// <summary>
		/// The preview audio stored
		/// </summary>
		public Audio PreviewAudio = preview;
		/// <summary>
		/// Whether the audio is loaded
		/// </summary>
		public readonly bool IsLoaded => PreviewAudio != null;
		/// <inheritdoc/>
		public override readonly string ToString() => $"{path} between [{begin} -> {end}] is {(IsLoaded ? "loaded" : "not loaded")}";
	}
	/// <summary>
	/// The list of audio preview datas
	/// </summary>
	public static List<AudioPreviewData> AudioPreviewDatas = [];
	/// <summary>
	/// The amount of loaded audio previews
	/// </summary>
	public static int LoadedPreviewAudioCount => AudioPreviewDatas.Where(s => s.IsLoaded).Count();
	/// <summary>
	/// Gets the audio preview data from the path
	/// </summary>
	/// <param name="path">The name of the music</param>
	/// <returns>The audio preview data of the music</returns>
	public static AudioPreviewData? GetPreviewDataFromPath(string path) => AudioPreviewDatas.Find(s => s.Path == path);
	internal static CancellationTokenSource cancelTokenSource = new();
	private static readonly CancellationToken cancelToken = cancelTokenSource.Token;
	private static readonly List<Task> AudioLoadingTasks = [];
	internal static float AudioLoadProgress => AudioLoadingTasks.Where(s => s.IsCompletedSuccessfully).Count() / AudioLoadingTasks.Count;
	internal static HashSet<string> QueueLoadSongs = [];
	/// <summary>
	/// Loads the song previews of the charts
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static async void LoadSongPreviews()
	{
		//Ensure FFMpeg exists
		if (!File.Exists(Path.Combine("Content", "FFMpeg", "ffmpeg.exe")))
			System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine("Content", "FFMpeg.zip"), "Content");
		//Store audio preview positions
		List<Type> allCharts = [..FightSystem.AllSongs.Values.Concat(FightSystem.CustomSongs.Values)];
		foreach (var extraSongs in FightSystem.ExtraSongSets)
			allCharts.AddRange([..extraSongs.Values]);
		foreach (Type i in allCharts)
		{
			object o = Activator.CreateInstance(i);
			//Store difficulty name of championships
			if (o is IChampionShip cmp)
			{
				Dictionary<Difficulty, string> curDiffNames = [];
				foreach (var val in cmp.DifficultyPanel)
					curDiffNames.Add(val.Value, val.Key);
				ChartDifficultyNames.Add(cmp.GameContent.FightName, curDiffNames);
			}
			IWaveSet waveSet = o is IWaveSet wave ? wave : (o as IChampionShip).GameContent;
			WaveCache.Add(waveSet.FightName, waveSet);
			//Store difficulty name of wavesets
			if (o is IWaveSet)
			{
				ChartDifficultyNames.Add(waveSet.FightName, new()
				{
					[Difficulty.Noob] = "Noob",
					[Difficulty.Easy] = "Easy",
					[Difficulty.Normal] = "Normal",
					[Difficulty.Hard] = "Hard",
					[Difficulty.Extreme] = "Extreme",
					[Difficulty.ExtremePlus] = "Extreme+",
				});
			}
			bool test = File.Exists(Path.Combine($"Content\\Musics\\{waveSet.Music + "\\song.ogg"}".Split('\\')));
			if (!QueueLoadSongs.Contains(waveSet.Music + "\\song.ogg") && File.Exists(Path.Combine($"Content\\Musics\\{waveSet.Music + "\\song.ogg"}".Split('\\'))))
			{
				AudioPreviewDatas.Add(new(waveSet.Music, waveSet.Attributes.MusicPreview[0], waveSet.Attributes.MusicPreview[1], null));
				QueueLoadSongs.Add(waveSet.Music + "\\song.ogg");
			}
			string dir = Path.Combine($"Content\\Musics\\{waveSet.Music}".Split('\\'));
			//Cache wave paint as well
			if (Directory.Exists(dir) && File.Exists(Path.Combine((dir + "\\paint.xnb").Split('\\'))))
				WavePaint.Add(waveSet.FightName, DrawingLab.LoadContent<Texture2D>(dir + "\\paint", Loader));
			if (o is WaveConstructor wc)
				wc.Dispose();
		}
		foreach (string file_name in QueueLoadSongs)
		{
			//Won't load non-copied files
			if (!File.Exists(Path.Combine($"Content\\Musics\\{file_name}".Split('\\'))))
				continue;
			//Cache audio preview
			string path = Path.Combine("Musics", file_name);
			AudioLoadingTasks.Add(Task.Factory.StartNew(() =>
			{
				//Perform key string trim
				string key = path.Split(Path.DirectorySeparatorChar)[1];
				if (key.EndsWith(".ogg"))
					key = key[..^4];
				float[] musPreviewPos = GetPreviewDataFromPath(key) is AudioPreviewData prevD ? [prevD.BeginSecond, prevD.EndSecond] : null;
				if (musPreviewPos == null)
					return;
				//Extract preview audio file if it does not exist or audio preview does not match data file
				string preview_file_name = Path.Combine("Content", $"{path[..^4]}_preview.wav"),
						preview_file_final_name = Path.Combine("Content", $"{path[..^4]}_preview.ogg");
				//Check if data file exists
				bool MatchData = true;
				FileStream stream;
				string datFilePath = Path.Combine("Content", path[..path.LastIndexOf('\\')], "Dat.dat");
				if (File.Exists(datFilePath))
				{
					stream = new(datFilePath, FileMode.OpenOrCreate);
					StreamReader textReader = new(stream);
					string[] data = textReader.ReadToEnd().Split(',');
					stream.Close();
					//If so, check if preview data is a match
					if (data.Length == 2 && float.TryParse(data[0], out float startPos) && float.TryParse(data[1], out float endPos))
						MatchData = startPos == musPreviewPos[0] && endPos == musPreviewPos[1];
				}
				//Trim song preview and cache it on a separate file if no cache is present or cache preview position mismatch
				if (!File.Exists(preview_file_final_name) || !MatchData)
				{
					//Read source ogg file
					using (VorbisWaveReader vorbis = new(Path.Combine("Content", path)))
					{
						OffsetSampleProvider sample = new(vorbis)
						{
							SkipOver = TimeSpan.FromSeconds(musPreviewPos[0]),
							Take = TimeSpan.FromSeconds(musPreviewPos[1] - musPreviewPos[0])
						};
						//Convert to wave file for trimming
						WaveFileWriter.CreateWaveFile(preview_file_name, new SampleToWaveProvider(sample));
					}
					//Convert to ogg using ffmpeg
					Process process = Process.Start(new ProcessStartInfo()
					{
						FileName = Path.Combine("Content", "FFMpeg", "ffmpeg"),
						Arguments = $"-i \"{preview_file_name}\" \"{preview_file_final_name}\"",
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						CreateNoWindow = true
					});
					process.WaitForExit();
					//Ensure file exists and file was not used by previous process to avoid CPU spike
					while (!File.Exists(preview_file_final_name) || !process.HasExited)
						Thread.Sleep(1);
					//Delete tempoary wav file
					File.Delete(preview_file_name);
				}
				//Add to audio cache
				AudioPreviewData? prevDat = GetPreviewDataFromPath(key);
				if (prevDat.HasValue)
				{
					var prevDatActVal = prevDat.Value;
					int i = AudioPreviewDatas.IndexOf(prevDatActVal);
					prevDatActVal.PreviewAudio = new Audio(preview_file_final_name);
					AudioPreviewDatas[i] = prevDatActVal;
				}
			}, cancelToken));
			await AudioLoadingTasks.Last();
		}
	}
	#endregion
	#region Chart Data
	/// <summary>
	/// The names of the difficulties of the charts
	/// </summary>
	public static Dictionary<string, Dictionary<Difficulty, string>> ChartDifficultyNames = [];
	#endregion
}