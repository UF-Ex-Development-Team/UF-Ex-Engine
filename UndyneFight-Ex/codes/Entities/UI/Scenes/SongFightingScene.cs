using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UndyneFight_Ex.Settings;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.DebugState;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities
{
    public class SongFightingScene : FightScene
    {
        private class SongConditionOptimizer : Entity
        {
            readonly SongFightingScene fatherScene;
            public SongConditionOptimizer(SongFightingScene scene)
            {
                UpdateIn120 = true;
                fatherScene = scene;
            }
            const float deltaDured = 3.1f;

            int index = 0;
            readonly float[] data = new float[55];
            readonly float[] cur = new float[10];
            float timer = 0.0f;
            float avg = 0.0f;

            public float GlobalAVG = 0.0f;

            public override void Draw()
            {
#if DEBUG
                FightResources.Font.NormalFont.CentreDraw(GameMain.UpdateCost.ToString("F3"), new(100, 150), Color.White, 0.7f, 0.1f);
                FightResources.Font.NormalFont.CentreDraw(KeyCheckTime1.ToString("F3"), new(50, 200), Color.White, 0.7f, 0.1f);
                FightResources.Font.NormalFont.CentreDraw(KeyCheckTime2.ToString("F3"), new(150, 200), Color.White, 0.7f, 0.1f);
#endif
            }

            public override void Update()
            {
                timer += 0.5f;

                float del2 = GameMain.gameSpeed;
                del2 *= 15;
                del2 -= 15;

                Audio music = fatherScene.music;
                float curTime = music.TryGetPosition(out bool result);

                fatherScene.GlobalDelta = curTime - GametimeF;

                if (!result)
                    fatherScene.GlobalDelta = 0f;

                if (timer > 15f)
                {
                    if (index < data.Length)
                    {
                        data[index++] = fatherScene.GlobalDelta;
                        if (index == data.Length)
                        {
                            foreach (float item in data)
                                avg += item / index;
                        }
                    }
                }
                if (index > data.Length / 2f && timer > 25f)
                {
                    // avg = curDelta - asyncTime - Functions.GametimeDelta;
                    float realTime = fatherScene.music.TryGetPosition(out bool result2);
                    if (!result2)
                        return;
                    cur[^1] = realTime - GametimeF;
                    for (int i = 0; i < cur.Length - 1; i++)
                        cur[i] = cur[i + 1];
                    GlobalAVG = 0;
                    foreach (float item in cur)
                        GlobalAVG += item / cur.Length;
                }
                if (index >= data.Length)
                {
                    if (MathF.Abs(GlobalAVG - avg + del2) > deltaDured)
                        _ = fatherScene.music.TrySetPosition(avg + GametimeF + del2);
                }
            }
        }
        /// <summary>
        /// Sets the current song to the given position
        /// </summary>
        /// <param name="position"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSongPosition(float position) => music.TrySetPosition(position);
        /// <summary>
        /// Sets the paramters of the chart
        /// </summary>
        /// <param name="waveset">The <see cref="IWaveSet"/> interface of the chart</param>
        /// <param name="songIllustration">The illustration of the chart</param>
        /// <param name="difficulty">The current difficulty of the chart</param>
        /// <param name="musicPath">The file path to the music of the chart</param>
        /// <param name="judgeState">The judgement state of the current chart</param>
        /// <param name="mode">The game mode of the chart (Default <see cref="GameMode.None"/></param>
        /// <param name="unload">Unload assets (Default true)</param>
        public class SceneParams(IWaveSet waveset, Texture2D songIllustration, int difficulty, string musicPath, JudgementState judgeState, GameMode mode = GameMode.None, bool unload = true)
        {
            /// <summary>
            /// The <see cref="IWaveSet"/> of the current chart
            /// </summary>
            public IWaveSet Waveset => Activator.CreateInstance(wavesetType) as IWaveSet;

            private readonly Type wavesetType = waveset.GetType();
            /// <summary>
            /// The current difficulty of the chart
            /// </summary>
            public int difficulty = difficulty;
            private readonly string musicPath = musicPath;
            /// <summary>
            /// The current <see cref="GameMode"/> of the chart
            /// </summary>
            public GameMode mode = mode;
            /// <summary>
            /// The current <see cref="JudgementState"/> of the chart
            /// </summary>
            public JudgementState JudgeState = judgeState;
            /// <summary>
            /// Is the music an ogg file
            /// </summary>
            public bool MusicOptimized { get; set; } = false;
            /// <summary>
            /// Loads the music
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LoadMusic()
            {
                Loader.RootDirectory = "";
                Music = !MusicOptimized ? new(musicPath, Loader) : new(musicPath + ".ogg", Loader);
                if (SongIllustration?.IsDisposed ?? false)
                {
                    string name = SongIllustration.Name;
                    name = name.StartsWith("Content") ? name : "Content\\" + name;
                    SongIllustration = GlobalResources.LoadContent<Texture2D>(Path.Combine(name.Split('\\')));
                }
                MusicDuration = (float)Music.SongDuration.TotalSeconds * 62.5f;
            }
            /// <summary>
            /// The current music of the chart
            /// </summary>
            public Audio Music { get; private set; }
            /// <summary>
            /// Whether the music is loaded
            /// </summary>
            public bool MusicLoaded => Music != null;
            /// <summary>
            /// The duration of the music (In frames)
            /// </summary>
            public float MusicDuration { get; private set; }
            /// <summary>
            /// The illustration of the chart
            /// </summary>
            public Texture2D SongIllustration { get; set; } = songIllustration;
            /// <summary>
            /// Whether the assets will be unloaded
            /// </summary>
            public bool IsUnload { get; private set; } = unload;
        }

        internal IWaveSet waveset;
        private readonly SceneParams currentParam;
        private int appearTime = 0;
        public SongFightingScene(SceneParams _params, Challenge challenge = null)
        {
            _challenge = challenge;
            currentParam = _params;
            difficulty = _params.difficulty;
        }
        private readonly Challenge _challenge = null;
        /// <summary>
        /// The accuracy bar
        /// </summary>
        public AccuracyBar Accuracy { get; set; }
        internal StateShower ScoreState { get; set; }
        internal TimeShower Time { get; set; }
        /// <summary>
        /// The current <see cref="JudgementState"/> of the chart
        /// </summary>
        public JudgementState JudgeState => currentParam.JudgeState;
        /// <summary>
        /// The current <see cref="Difficulty"/> of the chart
        /// </summary>
        public Difficulty CurrentDifficulty => (Difficulty)currentParam.difficulty;

        private GameMode mode;
        /// <summary>
        /// The current <see cref="GameMode"/> of the chart
        /// </summary>
        public override GameMode Mode => mode;

        private volatile bool songLoaded = false;
        private Audio music;
        private bool forceEnd = false;
        /// <summary>
        /// The offset of the music of the chart (In frames)
        /// </summary>
        public float PlayOffset { get; set; } = 0;
        /// <summary>
        /// Whether the chart will automatically end after the song ends
        /// </summary>
        public bool AutoEnd { get; set; } = true;
        /// <summary>
        /// Whether the HP had reached 0 in <see cref="GameMode.Practice"/>
        /// </summary>
        internal bool HPReached0 = false;
        /// <summary>
        /// Whether the soul had became green in <see cref="GameMode.NoGreenSoul"/>
        /// </summary>
        internal bool GreenSoulUsed = false;
        /// <summary>
        /// The illustration of the chart
        /// </summary>
        public Texture2D SongIllustration { get; set; } = null;
        private bool endRunned = false;

        private int restartTimer = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ForceEnd() => forceEnd = true;
        public float GlobalDelta { get; private set; }

        public override void Update()
        {
            if (waveset != null)
            {
                restartTimer = IsKeyDown(InputIdentity.Reset) ? restartTimer++ : 0;
                if (restartTimer >= 60 || (IsKeyDown(InputIdentity.Reset) && IsKeyDown(InputIdentity.Alternate)))
                {
                    GameMain.instance.SetGameoverScreen();
                    PlayDeath();
                    return;
                }
            }
            appearTime++;

            //Play Music
            if (appearTime >= 30)
            {
                if (!songLoaded)
                {
                    SetSongFight();
                    music = currentParam.Music;
                    if (PlayOffset < 0)
                        AddInstance(new InstantEvent(-PlayOffset, music.Play));
                    else
                    {
                        music.PlayPosition = PlayOffset;
                        music.Play();
                    }
#if DEBUG
                    InstanceCreate(new SongConditionOptimizer(this));
#endif
                    isInBattle = true;
                    songLoaded = true;
                    ResetTime();
                }
                else if (waveset != null)
                    UpdateSong();
            }

            bool needEnd = waveset != null && appearTime > currentParam.MusicDuration * 2 && (music?.IsEnd ?? false);
            if (needEnd)
            {
                if (!endRunned)
                {
                    StateShower.instance.EndAction?.Invoke();
                    endRunned = true;
                }
            }
            if ((needEnd && AutoEnd) || forceEnd)
            {
                Surface.Normal.drawingAlpha -= 0.015f;
                ScreenDrawing.MasterAlpha -= 0.015f;
                if (Surface.Normal.drawingAlpha < 0 && ScreenDrawing.MasterAlpha < 0)
                {
                    ScreenDrawing.MasterAlpha = 1;
                    if (_challenge == null || !IsInChallenge)
                        WinFight();
                    else
                        ChallengeSave();
                }
            }
            mode = currentParam.mode;
            base.Update();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void WinFight()
            {
                StateShower ss = StateShower.instance;
                ResetFightState(false);
                ResetScene(new WinScene(ss, PlayerInstance.GameAnalyzer));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void ChallengeSave()
            {
                SongResult result;
                result = StateShower.instance.GenerateResult();
                PlayerManager.RecordMark(currentParam.Waveset.FightName, currentParam.difficulty,
                    result.CurrentMark, result.Score, result.AC, result.AP, result.Accuracy);
                PlayerManager.Save();
                ResetFightState(false);
                _challenge.ResultBuffer.Add(result);
                ResetScene(ChallengeCount == ++CurChallengeNum
                    ? new ChallengeWinScene(_challenge)
                    : new SongLoadingScene(_challenge, ChallengeCharts[1..]));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetSongFight()
        {
            bool auto = (mode & GameMode.Autoplay) != 0;
            otherAuto = redShieldAuto = blueShieldAuto = greenShieldAuto = purpleShieldAuto = auto;

            //It's bad
            Pausable = false;
            MathUtil.rander = new Random(seed);
            GameStates.InstanceCreate(Accuracy = new());
            InstanceCreate(ScoreState = new StateShower(waveset = currentParam.Waveset, currentParam.difficulty, JudgeState, currentParam.mode, currentParam.MusicDuration));
            InstanceCreate(Time = new());
            //This function MUST be called before the creation of the player for proper
            //garbage collection to prevent bugs such as:
            //Blue soul not moving after giving force
            StartBattle();
            InstanceCreate(PlayerInstance = new());
            InstanceCreate(HPBar = new());
            if (waveset is GameObject)
                InstanceCreate(waveset as GameObject);
            lock (this)
            {
                waveset.Start();
            }
            SongIllustration = currentParam.SongIllustration;
        }

        private void UpdateSong()
        {
            if (waveset is IWaveSetS)
                (waveset as IWaveSetS).Chart();
            else
            {
                switch (currentParam.difficulty)
                {
                    case 0:
                        waveset.Noob();
                        break;
                    case 1:
                        waveset.Easy();
                        break;
                    case 2:
                        waveset.Normal();
                        break;
                    case 3:
                        waveset.Hard();
                        break;
                    case 4:
                        waveset.Extreme();
                        break;
                    case 5:
                        waveset.ExtremePlus();
                        break;
                }
            }
        }

        //debug
        private void TempIntro()
        {
            // debug
            _challenge.ResultBuffer.Add(new SongResult(SkillMark.Impeccable, 0, 0.99f, false, false));
            _challenge.ResultBuffer.Add(new SongResult(SkillMark.Excellent, 0, 0.98f, true, false));
            _challenge.ResultBuffer.Add(new SongResult(SkillMark.Acceptable, 0, 0.96f, true, true));
            ResetScene(new ChallengeWinScene(_challenge));
        }
        public override void Dispose()
        {
            music?.Stop();
            base.Dispose();
            waveset = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void PlayerDied() => ResetScene(!isReplay ? new TryAgainScene(StateShower.instance) : new TryAgainScene(new RecordSelector()));
    }
}