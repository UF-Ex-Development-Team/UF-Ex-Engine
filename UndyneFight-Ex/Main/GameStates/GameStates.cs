using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex;

public static partial class GameStates
{
	public static class GameRule
	{
		/// <summary>
		/// The color of the player name, VIP can have blue/orange/colorful instead of only white
		/// </summary>
		public static string nameColor = "White";

	}
	/// <summary>
	/// Whether the player is currently in a challenge
	/// </summary>
	internal static bool IsInChallenge { get; set; } = false;
	/// <summary>
	/// The current challenge
	/// </summary>
	internal static int CurChallengeNum = 0;
	/// <summary>
	/// The amount of challenges
	/// </summary>
	internal static int ChallengeCount = 0;
	internal static SongFightingScene.SceneParams[] ChallengeCharts = [];
	/// <summary>
	/// The sprite batch of the game
	/// </summary>
	public static SpriteBatchEX SpriteBatch => GameMain.MissionSpriteBatch;
	/// <summary>
	/// The graphics device manager of the game
	/// </summary>
	public static GraphicsDeviceManager GameWindow => GameMain.Graphics;
	/// <summary>
	/// Creates an instance
	/// </summary>
	/// <param name="e">The <see cref="GameObject"/> to create</param>

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void InstanceCreate(GameObject e) => missionScene.InstanceCreate(e);
	/// <summary>
	/// Whether an instance already exists
	/// </summary>
	/// <param name="e">The game object to check</param>
	/// <returns></returns>
	public static bool InstanceExists(Type e) => Objects.FindAll(s => s.GetType() == e).Count > 0;
	/// <summary>
	/// Destroy all instances of the given type
	/// </summary>
	/// <param name="e">The object to dispose</param>
	/// <returns></returns>
	public static void InstanceDestroy(Type e) => Objects.FindAll(s => s.GetType() == e).ForEach((s) => s.Dispose());

	internal static Scene currentScene, missionScene;
	/// <summary>
	/// The current scene of the game, i.e. <see cref="SongFightingScene"/>
	/// </summary>
	public static Scene CurrentScene => currentScene;

	internal static Scene.DrawingSettings CurrentSetting => missionScene.CurrentDrawingSettings;
	internal static List<GameObject> Objects => missionScene.Objects;
	internal static IWaveSet waveSet;

	internal static bool isInBattle = false;
	/// <summary>
	/// The difficulty of the current chart in <see cref="int"/>, you can convert it back to <see cref="Difficulty"/>
	/// </summary>
	public static int difficulty { get; set; } = -1;
	/// <summary>   
	/// Whether the time tips (Early, Late) are forcefully disabled
	/// </summary>
	public static bool ForceDisableTimeTips { get; set; } = false;
	/// <summary>
	/// The GameMode used in the previous chart
	/// </summary>
	public static GameMode GameModeMemory { get; set; }

	internal static int seed = -1;
	internal static Texture2D GameoverBackground;
	/// <summary>
	/// Reset <see cref="GameMain.gameTime"/> to 0
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ResetTime() => GameMain.gameTime = 0;
	internal static void StateUpdate()
	{
		if ((DateTime.Now.Second % 5) == 0)
		{
			Process[] all = Process.GetProcesses();
			foreach (Process item in all)
			{
				if (item.ProcessName.Contains("Rhythm Recall") && item.Id != Environment.ProcessId)
					item.Kill();
			}
		}
		GameMain.gameTime += 0.5f;
		if (CurrentScene != null && GameMain.Update120F)
		{
			MainScene.UpdateAll();
			CurrentScene.UpdateRendering();
		}
		currentScene = missionScene;
		if (Fight.Functions.GametimeF > 0 && Fight.Functions.GametimeF % 125 == 0)
			GC.Collect();
		KeysUpdate2();
		CharInput = KeysUpdate();
		if (hacked)
		{
			GameMain.ExitGame();
			throw new Exception("You Dirty Hacker!");
		}
		currentScene.Update();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Entity[] GetEntities()
	{
		List<Entity> result = [];
		CurrentScene.Objects.ForEach(s => result.AddRange(s.GetDrawableTree()));
		result.Add(CurrentScene);
		return [.. result];
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void StartReset() => GravityLine.GravityLines.Clear();
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void StartBattle()
	{
		MathUtil.rander = new Random();
		seed = MathUtil.GetRandom(0, 2 << 16);
		keyEventBuffer = null;
		StartReset();
	}
	/// <summary>
	/// Selects a fight
	/// </summary>
	/// <param name="fightSet">The fight to select</param>
	/// <param name="mode">The gamemode of the fight</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SelectBattle(Fight.IClassicFight fightSet, GameMode mode)
	{
		ResetTime();
		GameMain.gameSpeed = 1.0f;
		Fight.Functions.ScreenDrawing.Reset();
		keyEventBuffer = null;

		ResetScene(new NormalFightingScene(fightSet, mode));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void StartSong() => StartSong(lastParam);

	internal static SongFightingScene.SceneParams lastParam;
	/// <summary>
	/// Starts a chart
	/// </summary>
	/// <param name="wave">The chart wave</param>
	/// <param name="songIllustration">The chart cover</param>
	/// <param name="path">The path to the music file</param>
	/// <param name="dif">The difficulty of the chart</param>
	/// <param name="judgeState">The judgement state of the chart</param>
	/// <param name="mode">The gamemode of the chart</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void StartSong(IWaveSet wave, Texture2D songIllustration, string path, int dif, JudgementState judgeState, GameMode mode)
	{
		waveSet = wave;
		GameModeMemory = mode;
		difficulty = dif;
		SongFightingScene.SceneParams @params = new(waveSet, songIllustration, difficulty, path, judgeState, mode);
		StartSong(@params);
	}
	/// <summary>
	/// Starts a chart
	/// </summary>
	/// <param name="params">The parameters of the chart</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void StartSong(SongFightingScene.SceneParams @params)
	{
		GameModeMemory = @params.mode;
		lastParam = @params;
		Fight.Functions.Loader.RootDirectory = "Content";
		ResetScene(@params.MusicLoaded ? new SongFightingScene(@params) : new SongLoadingScene(@params));
	}
	/// <summary>
	/// Sets the current scene into a new one
	/// </summary>
	/// <param name="scene">The target scene to set to</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ResetScene(Scene scene)
	{
		List<GameObject> crossObjects = null;
		if (currentScene != null)
		{
			crossObjects = currentScene.GlobalObjects();
			currentScene.Dispose();
		}
		missionScene = scene;
		if (currentScene?.CurrentDrawingSettings.Extending != Vector4.Zero)
			missionScene.InstanceCreate(new InstantEvent(1, GameMain.ResetRendering));
		crossObjects?.ForEach(s => missionScene.InstanceCreate(s));
		ResetTime();
		GameMain.ResetRendering();
	}
	/// <summary>
	/// Resets the fight state
	/// </summary>
	public static void ResetFightState()
	{
		Fight.Functions.Reset();
		Surface.Normal.drawingAlpha = 1.0f;
		isInBattle = false;

		Player.Heart.ResetMove();
		NameShower.level = "";
		NameShower.name = null;
		NameShower.OverrideName = "";
		NameShower.nameAlpha = 1;

		Surface.Hidden.BackGroundColor = Color.Black;
		FightBox.boxes = [];

		Fight.FightStates.roundType = false;
		Fight.FightStates.finishSelecting = true;

		Microsoft.Xna.Framework.Media.MediaPlayer.Volume = Settings.SettingsManager.DataLibrary.masterVolume / 100f;
		GameMain.gameSpeed = 1.0f;
	}

	internal static bool hacked = false;
	internal static void CheatAffirmed()
	{
		hacked = true;

		DateTime span = DateTime.Now;

		IOEvent.WriteCustomFile("D:\\Microsoft.CodeAnalysis.dll", IOEvent.StringToByte($"{span.Year},{span.Month},{span.Day},{span.Hour},{span.Minute},{span.Second}"));
		ResetFightState();
		InstanceCreate(new Player.BrokenHeart());
	}
	/// <summary>
	/// Ends the current fight/chart
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void EndFight()
	{
		ResetFightState();
		ResetScene(new GameMenuScene());
		StateShower.DisposeInstance();
	}
	/// <summary>
	/// Changes the speed of the game
	/// </summary>
	/// <param name="SpeedScale"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ChangeSpeedScale(float SpeedScale) => GameMain.GameSpeed = SpeedScale;
	/// <summary>
	/// Writes text onto an external file
	/// </summary>
	/// <param name="name">The name of the file</param>
	/// <param name="data">The text to write</param>
	public static void FileWriteText(string name, string data = "")
	{
		FileStream stream = new(name, FileMode.OpenOrCreate);
		StreamWriter textWriter = new(stream);
		textWriter.Write(data);
		textWriter.Flush();
		stream.Close();
	}
	/// <summary>
	/// Broadcast an event globally
	/// </summary>
	/// <param name="gameEventArgs">The event to broadcast</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Broadcast(GameEventArgs gameEventArgs) => currentScene.Broadcast(gameEventArgs);
	/// <summary>
	/// Detect whether an event (Made from <see cref="Broadcast(GameEventArgs)"/>) has been called
	/// </summary>
	/// <param name="ActionName">The name of the event to detect</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<GameEventArgs> DetectEvent(string ActionName) => currentScene.DetectEvent(ActionName);


	#region Song previews
	/// <summary>
	/// The audio caches for song previews
	/// </summary>
	public static readonly Dictionary<string, Audio> AudioCache = [];
	internal static readonly Dictionary<string, float[]> AudioPreviewPos = [];
	private static readonly HashSet<string> LoadedAudioNames = [];
	internal static CancellationTokenSource cancelTokenSource = new();
	private static readonly CancellationToken cancelToken = cancelTokenSource.Token;
	private static readonly List<Task> AudioLoadingTasks = [];
	internal static float AudioLoadProgress => AudioLoadingTasks.Where(s => s.IsCompletedSuccessfully).Count() / AudioLoadingTasks.Count;
	/// <summary>
	/// The directory of the file path
	/// </summary>
	internal static List<string> file_path_list = [];
	private readonly static string _base_music_path = Path.Combine($"Content\\Musics".Split('\\'));
	/// <summary>
	/// Loads the song previews of the charts
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static async void LoadSongPreviews()
	{
		//Store audio preview positions
		foreach (Type i in FightSystem.AllSongs.Values.Concat(FightSystem.CustomSongs.Values))
		{
			object o = Activator.CreateInstance(i);
			IWaveSet waveSet = o is IWaveSet wave ? wave : (o as IChampionShip).GameContent;
			AudioPreviewPos.TryAdd(waveSet.Music, waveSet.Attributes.MusicPreview);
		}
		//Gets each subfolder
		file_path_list.AddRange(new DirectoryInfo(_base_music_path).GetDirectories().Select(sub_dir_files => Path.Combine((sub_dir_files.Name + "\\song.ogg").Split('\\'))));
		//Gets all non-foldered ogg files
		file_path_list.AddRange(new DirectoryInfo(_base_music_path).GetFiles().Where(s => s.Name.EndsWith(".ogg")).Select(s => s.Name));
		foreach (string file_name in file_path_list)
		{
			//Cache audio preview
			if (!LoadedAudioNames.Contains(file_name.Split(Path.DirectorySeparatorChar)[0]))
			{
				string path = Path.Combine("Musics", file_name);
				AudioLoadingTasks.Add(Task.Factory.StartNew(() =>
				{
					string key = path.Split(Path.DirectorySeparatorChar)[1];
					if (key.EndsWith(".ogg"))
						key = key[..^4];
					if (AudioPreviewPos.TryGetValue(key, out float[] musPreviewPos))
					{
						LoadedAudioNames.Add(key);
						AudioCache.TryAdd(key, new Audio(path, null, musPreviewPos[0], musPreviewPos[1]));
					}
				}, cancelToken));
				await AudioLoadingTasks.Last();
			}
		}
	}
	#endregion
}