using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex
{
    public static partial class GameStates
    {
        internal static class GameRule
        {
            /// <summary>
            /// The color of the player name, VIP can have blue/orange/colorful instead of only white
            /// </summary>
            public static string nameColor = "White";

        }
        /// <summary>
        /// Whether the current engine used is the UF-Ex RE engine
        /// </summary>
        public static bool IsReEngine;
        /// <summary>
        /// Whether the player is currently in a challenge
        /// </summary>
        public static bool IsInChallenge = false;
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
        /// The game window
        /// </summary>
        public static GraphicsDeviceManager GameWindow => GameMain.Graphics;
        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="e">The <see cref="GameObject"/> to create</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InstanceCreate(GameObject e) => missionScene.InstanceCreate(e);

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
        public static int difficulty = -1;
        /// <summary>
        /// Whether the time tips (Early, Late) are forcefully disabled
        /// </summary>
        public static bool ForceDisableTimeTips = false;
        /// <summary>
        /// The GameMode used in the previous chart
        /// </summary>
        public static GameMode GameModeMemory;

        internal static bool isReplay = false, isRecord = false;

        internal static int seed = -1;
        internal static Texture2D GameoverBackground;
        /// <summary>
        /// Reset <see cref="GameMain.gameTime"/> to 0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetTime() => GameMain.gameTime = 0;
        internal static void StateUpdate()
        {
            GameMain.gameTime += 0.5f;
            if (CurrentScene != null && GameMain.Update120F)
            {
                MainScene.UpdateAll();
                CurrentScene.UpdateRendering();
            }
            currentScene = missionScene;
            if (Fight.Functions.GametimeF  > 0 && Fight.Functions.GametimeF % 125 == 0)
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
            if (isRecord)
                keyEventBuffer = new Recorder();
            if (isReplay)
            {
                MathUtil.rander = new Random(seed);
                keyEventBuffer = new Replayer();
            }
            else
            {
                MathUtil.rander = new Random();
                seed = MathUtil.GetRandom(0, 2 << 16);
            }
            if (!(isReplay || isRecord))
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
        /// <param name="path">The music path</param>
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
        /// <param name="params">The paramters of the chart</param>
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
            if (isRecord && GameInterface.UFEXSettings.RecordEnabled)
            {
                (keyEventBuffer as Recorder).Flush();
                if (!isDead && GameInterface.UFEXSettings.RecordEnabled)
                    Recorder.Save();
            }
            Fight.Functions.Reset();
            Surface.Normal.drawingAlpha = 1.0f;
            isInBattle = false;

            Player.Heart.ResetMove();
            NameShower.level = "";
            NameShower.name = null;
            NameShower.OverrideName = "";

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
        /// <param name="gameEventArgs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(GameEventArgs gameEventArgs) => currentScene.Broadcast(gameEventArgs);
        /// <summary>
        /// Detect whether an event (Made from <see cref="Broadcast(GameEventArgs)"/> has been called
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static async void LoadSongPreviews()
        {
            //Store audio preview positions

            foreach (Type i in FightSystem.AllSongs.Values)
            {
                var o = Activator.CreateInstance(i);
                IWaveSet waveSet = o is IWaveSet ? o as IWaveSet : (o as IChampionShip).GameContent;
                AudioPreviewPos.TryAdd(waveSet.Music, waveSet.Attributes.MusicPreview);
            }
            string path = Path.Combine($"{AppContext.BaseDirectory}Content\\Musics".Split('\\'));
            if (!Directory.Exists(path))
                return;
            //Unfoldered files
            var FileList = new DirectoryInfo(path).GetFiles();
            //Folder names
            var sub_dirs = new DirectoryInfo(path).GetDirectories();
            foreach (var sub_dir_files in sub_dirs)
            {
                path = Path.Combine($"{AppContext.BaseDirectory}Content\\Musics\\{sub_dir_files.Name}\\song.ogg".Split('\\'));
                bool exist_ogg = File.Exists(path);
                file_path_list.Add(Path.Combine((sub_dir_files.Name + "\\song").Split('\\')), exist_ogg
                    ? Path.Combine($"Content\\Musics\\{sub_dir_files.Name}\\song".Split('\\'))
                    : Path.Combine($"Content\\Musics\\{sub_dir_files.Name}\\song.xnb".Split('\\')));
            }
            //Loads all non-foldered ogg files
            foreach (var files in FileList)
            {
                if (files.Name.EndsWith(".ogg"))
                    file_path_list.Add(files.Name, Path.Combine($"Content\\{files.Name}".Split('\\')));
            }
            AudLoadTask = new(() => {
                foreach (var file_name in file_path_list)
                {
                    string path = Path.Combine(("Musics\\" + file_name.Key).Split('\\'));
                    //Cache audio preview
                    if (!AudioCache.TryGetValue(path, out Audio value))
                    {
                        //Foldered files
                        if (!File.Exists(path))
                        {
                            if (Directory.Exists(path[..^4]))
                            {
                                var last_slash_pos = path.LastIndexOf(Path.DirectorySeparatorChar);
                                path = path[..last_slash_pos];
                                var files_contained = new DirectoryInfo(path).GetFiles();
                                foreach (var sub_files_contaied in files_contained)
                                {
                                    if (sub_files_contaied.Name.EndsWith(".ogg"))
                                        path = sub_files_contaied.Name;
                                }
                            }
                        }
                        if (File.Exists(Path.Combine($"Content\\{path}.ogg".Split('\\'))))
                            path += ".ogg";
                        Debug.WriteLine(path);
                        string musFileName = path;
                        try
                        {
                            musFileName = path.Contains(Path.DirectorySeparatorChar) ? path[7..path.LastIndexOf(Path.DirectorySeparatorChar)] : path[7..];
                        }
                        catch
                        {
                            Debug.WriteLine($"Error on {path}");
                        }
                        if (AudioPreviewPos.TryGetValue(musFileName, out float[] musPreviewPos))
                        {
                            Audio LoadingAudio = new(Scene.Loader.RootDirectory.StartsWith("Content") ? path : $"Content\\{path}", null, musPreviewPos[0], musPreviewPos[1]);
                            AudioCache.TryAdd(musFileName, LoadingAudio);
                        }
                    }
                }
            });
            AudLoadTask.Start();
            await AudLoadTask;
        }
        #endregion
    }
}