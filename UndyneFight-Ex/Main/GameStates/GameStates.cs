using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex
{
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
		public static bool IsInChallenge { get; set; } = false;
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

		internal static Scene.DrawingSettings CurrentSetting { set => missionScene.CurrentDrawingSettings = value; get => missionScene.CurrentDrawingSettings; }
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
			Process[] all = Process.GetProcesses();
			foreach (Process item in all)
			{
				if (item.ProcessName.Contains("Rhythm Recall") && item.Id != Environment.ProcessId)
					item.Kill();
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
#if DEBUG
			Stopwatch watch = new();
			watch.Start();
#endif
			CharInput = KeysUpdate();
#if DEBUG
			KeyCheckTime1 = (float)watch.Elapsed.TotalMilliseconds;
			watch.Stop();
#endif
			if (hacked)
			{
				GameMain.ExitGame();
				throw new Exception("You Dirty Hacker!");
			}
			currentScene.SceneUpdate();
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
		/// <param name="isDead">Whether the player is dead</param>
		public static void ResetFightState(bool isDead)
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

			IOEvent.WriteCustomFile("D:\\Microsoft.CodeAnalysis.dll",
				IOEvent.StringToByte([ span.Year + "," + span.Month
				+ "," + span.Day + "," + span.Hour + "," + span.Minute + "," + span.Second]));
			ResetFightState(true);
			InstanceCreate(new Player.BrokenHeart());
		}
		/// <summary>
		/// Ends the current fight/chart
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void EndFight()
		{
			ResetFightState(true);
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
		private static Task AudLoadTask;
		/// <summary>
		/// [Directory, File Path]
		/// </summary>
		internal static Dictionary<string, string> file_path_list = [];
		private static readonly string[] Extensions = [".ogg", ".mp3", ".wav"];
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static async void LoadSongPreviews()
		{
			//Store audio preview positions
			foreach (Type i in FightSystem.AllSongs.Values)
			{
				object o = Activator.CreateInstance(i);
				IWaveSet waveSet = o is IWaveSet wave ? wave : (o as IChampionShip).GameContent;
				AudioPreviewPos.TryAdd(waveSet.Music, waveSet.Attributes.MusicPreview);
			}
			string path = Path.Combine($"{AppContext.BaseDirectory}Content\\Musics".Split('\\'));
			if (!Directory.Exists(path))
				return;
			//Unfoldered files
			FileInfo[] FileList = new DirectoryInfo(path).GetFiles();
			//Folder names
			DirectoryInfo[] sub_dirs = new DirectoryInfo(path).GetDirectories();
			foreach (DirectoryInfo sub_dir_files in sub_dirs)
			{
				path = Path.Combine($"{AppContext.BaseDirectory}Content\\Musics\\{sub_dir_files.Name}\\song.ogg".Split('\\'));
				bool exist_ogg = File.Exists(path);
				file_path_list.Add(Path.Combine((sub_dir_files.Name + "\\song").Split('\\')), exist_ogg
					? Path.Combine($"Content\\Musics\\{sub_dir_files.Name}\\song".Split('\\'))
					: Path.Combine($"Content\\Musics\\{sub_dir_files.Name}\\song.xnb".Split('\\')));
			}
			//Loads all non-foldered ogg files
			foreach (FileInfo files in FileList)
			{
				for (int i = 0; i < Extensions.Length; i++)
					if (files.Name.EndsWith(Extensions[i]))
						file_path_list.Add(files.Name, Path.Combine($"Content\\Musics\\{files.Name}".Split('\\')));
			}
			Debug.WriteLine(file_path_list);
			AudLoadTask = new(() =>
			{
				foreach (KeyValuePair<string, string> file_name in file_path_list)
				{
					string path = Path.Combine(("Musics\\" + file_name.Key).Split('\\'));
					//Cache audio preview
					if (!AudioCache.TryGetValue(file_name.Key.Split(Path.DirectorySeparatorChar)[0], out Audio value))
					{
						//Foldered files
						if (!File.Exists(path) && Directory.Exists(path[..^4]))
						{
							path = path[..path.LastIndexOf(Path.DirectorySeparatorChar)];
							FileInfo[] files_contained = new DirectoryInfo(path).GetFiles();
							foreach (FileInfo sub_files_contaied in files_contained)
							{
								for (int i = 0; i < Extensions.Length; i++)
									if (sub_files_contaied.Name.EndsWith(Extensions[i]))
									{
										path = sub_files_contaied.Name;
										break;
									}
							}
						}
						for (int i = 0; i < Extensions.Length; i++)
						{
							if (File.Exists(Path.Combine($"Content\\{path}{Extensions[i]}".Split('\\'))))
							{
								path += Extensions[i];
								break;
							}
						}
						string key = path;
						key = key.LastIndexOf(Path.DirectorySeparatorChar) != key.IndexOf(Path.DirectorySeparatorChar) ? key[(key.IndexOf(Path.DirectorySeparatorChar) + 1)..key.LastIndexOf(Path.DirectorySeparatorChar)] : key[(key.IndexOf(Path.DirectorySeparatorChar) + 1)..];
						if (!key.EndsWith("song.ogg") && key.EndsWith(".ogg"))
							key = key[..^4];
						if (AudioPreviewPos.TryGetValue(key, out float[] musPreviewPos))
						{
							Audio LoadingAudio = new(path, null, musPreviewPos[0], musPreviewPos[1]);
							AudioCache.TryAdd(file_name.Key.Split(Path.DirectorySeparatorChar)[0], LoadingAudio);
							//Debug.WriteLine($"Stored {key} in cache");
						}
						else
							Debug.WriteLine($"Error storing to cache at {key}, No key present in AudioPreviewPos");
					}
				}
			});
			AudLoadTask.Start();
			await AudLoadTask;
		}
		#endregion
	}
}