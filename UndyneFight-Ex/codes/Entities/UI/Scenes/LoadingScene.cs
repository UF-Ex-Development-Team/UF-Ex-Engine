using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.Fight;
using UndyneFight_Ex.GameInterface;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.GlobalResources;

namespace UndyneFight_Ex
{
    /// <summary>
    /// Loading scene
    /// </summary>
    public class LoadingScene : Scene
    {
        internal LoadingScene(Action loadingFinished, Action loadingAction, bool unLoad = true)
        {
            if (CurrentScene != null)
                CurrentScene.CurrentDrawingSettings.defaultWidth = 640f;
            if (unLoad)
                Loader.Unload();
            this.loadingFinished = loadingFinished;

            Thread thread = new(new ThreadStart(() =>
            {
                loadingAction();
                finishedLoad = true;
            }));
            loadingThread = thread;
            if (this is ResourcesLoadingScene)
                LoadSongPreviews();
        }

        private readonly Action loadingFinished;
        private readonly Thread loadingThread;
        internal int appearTime = 0;

        internal bool finishedLoad = false;
        internal int LeastLoadingTime { get; set; } = 120;

        internal bool eventInvoked = false;
        internal bool SkipLoadingAudio = false;

        public override void Update()
        {
            appearTime++;
            if (appearTime == 2)
                loadingThread.Start();
            else if (appearTime >= LeastLoadingTime && finishedLoad && !eventInvoked && (AudioCache.Count == AudioPreviewPos.Count || SkipLoadingAudio || this is not ResourcesLoadingScene))
            {
                eventInvoked = true;
                loadingFinished();
            }
            base.Update();
        }
    }
    /// <summary>
    /// Loading a song
    /// </summary>
    public class SongLoadingScene : LoadingScene
    {
        private readonly SongInformation Information;
        private static SongFightingScene.SceneParams songParams;
        public SongLoadingScene(SongFightingScene.SceneParams songParams) : base(() =>
        {
            GameStates.InstanceCreate(new InstantEvent(45, () => ResetScene(new SongFightingScene(songParams))));
            Loaded = true;
        }, () =>
        {
            if (songParams.Waveset.Attributes?.MusicOptimized ?? false)
                songParams.MusicOptimized = true;
            SongLoadingScene.songParams.LoadMusic();
        }, songParams.IsUnload)
        {
            Loaded = false;
            SongLoadingScene.songParams = songParams;
            Information = songParams.Waveset.Attributes;
            tipID = MathUtil.GetRandom(0, additions.Length - 1);
        }
        /// <summary>
        /// New load challenge method
        /// </summary>
        /// <param name="challenge">The challenge to start</param>
        /// <param name="songParams"></param>
        public SongLoadingScene(Challenge challenge, params SongFightingScene.SceneParams[] songParams) : base(() =>
            // loadingFinished
            GameStates.InstanceCreate(new InstantEvent(30, () => ResetScene(new SongFightingScene(songParams[0], challenge)))), () =>
        {
            // loadingAction
            if (songParams[0].Waveset.Attributes?.MusicOptimized ?? false)
                songParams[0].MusicOptimized = true;
            songParams[0].LoadMusic();
        })
        {
            if (IsInChallenge)
            {
                ChallengeCount = songParams.Length;
                CurChallengeNum = 0;
                ChallengeCharts = songParams;
            }
            SongLoadingScene.songParams = songParams[0];
            Information = songParams[0].Waveset.Attributes;
            tipID = MathUtil.GetRandom(0, additions.Length - 1);
        }
        private float alpha = 0, tipY = 500, infoX = -640, titleY = -80, titleAlpha = IsReEngine ? 1 : 0.3f;
        private static bool Loaded = false;
        private Vector2 initSize = new(280 / 1.5f / 640f, 210 / 1.5f / 480f), initPos = new(887 / 1.5f, 251 / 1.5f);

        private readonly string[] additions = [
            "Every character worth your attention",
            "Tips: Do not bite off more than you can chew",
            "Fun Fact: Practice mode starts with 99HP instead of Infinite HP formerly",
            "Fun Fact: 2021 Spring Celebration is the first championship with two segments",
            "Fun Fact: 2023 Memory is the first championship with 2 secret charts",
            "Fun Fact: Indihome Paket Phoenix div 1 was had Ex difficulty before becoming Ex+",
            "Fun Fact: Freedom Dive div 1 final part had twice the blue arrows before the nerf",
            "Fun Fact: Undyne Extreme was nerfed several times and the difficulty drastically dropped",
            "Fun Fact: Did you notice that arrows have reduced speed between the SOUL and shields?",
            "Nagareteku toki no naka de demo Kedarusa ga hora guru guru mawatte",
            "In a desperate conflict   With a ruthless enemy",
            "We're no strangers to love",
            "Right after the break",
            "Creeper...Aww man",
            "Fun Fact: Did you know one of the developers didn't even study computer at school?",
        ];

        public override void Draw()
        {
            Depth = -0.1f;
            Texture2D chartPaint = songParams.SongIllustration;
            if (chartPaint != null)
            {
                Vector2 scale = IsReEngine && !IsInChallenge ? initSize = Vector2.LerpPrecise(initSize, new Vector2(640f / chartPaint.Width, 480f / chartPaint.Height), 0.12f) : new Vector2(640f / chartPaint.Width, 480f / chartPaint.Height);
                Vector2 pos = IsReEngine && !IsInChallenge ?
                    initPos = Vector2.LerpPrecise(initPos, new Vector2(320, 240), 0.12f) : new Vector2(320, 240);
                GeneralDraw(chartPaint, pos, Color.Lerp(Color.Black, Color.White, (titleAlpha = MathHelper.LerpPrecise(titleAlpha, 0.3f, 0.16f)) * (Loaded ? alpha : 1)), scale, spriteOrigin: new Vector2(chartPaint.Width / 2f, chartPaint.Height / 2f));
            }
            titleY = MathHelper.LerpPrecise(titleY, Loaded ? -80 : 20, 0.12f);
            tipY = MathHelper.LerpPrecise(tipY, Loaded ? 500 : 464, 0.12f);
            infoX = MathHelper.LerpPrecise(infoX, Loaded ? -640 : 20, 0.12f);
            string curDispName = songParams.Waveset.Attributes.DisplayName;
            string songName = curDispName == string.Empty ? songParams.Waveset.FightName : curDispName;
            Font.NormalFont.CentreDraw(songName, new Vector2(320, titleY), Color.White, new Vector2(MathF.Min(1, 600f / Font.NormalFont.SFX.MeasureString(songName).X), 1), 1);
            string DiffText = difficulty switch
            {
                0 => "Noob",
                1 => "Easy",
                2 => "Normal",
                3 => "Hard",
                4 => "Extreme",
                5 => "Extreme",
                _ => "?"
            };
            Color DiffCol = difficulty switch
            {
                0 => Color.White,
                1 => Color.LawnGreen,
                2 => Color.LightBlue,
                3 => Color.MediumPurple,
                4 => Color.Orange,
                _ => Color.Gray
            };
            string ChartDiff = songParams.Waveset.Attributes.ComplexDifficulty.ContainsKey((Difficulty)difficulty) ? songParams.Waveset.Attributes.ComplexDifficulty[(Difficulty)difficulty].ToString() : "?";
            Font.NormalFont.CentreDraw(DiffText + " " + ChartDiff, new Vector2(320, titleY + 35), DiffCol, 1, 0.1f);
            if (Information != null)
            {
                int CurPos = 270;
                if (Information.BarrageAuthor != "Unknown")
                    Font.NormalFont.Draw("Barrage: " + Information.BarrageAuthor, new(infoX, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);
                if (Information.SongAuthor != "Unknown")
                    Font.NormalFont.Draw("Song from: " + Information.SongAuthor, new(infoX, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);
                if (Information.PaintAuthor != "Unknown")
                    Font.NormalFont.Draw("Paint: " + Information.PaintAuthor, new(infoX, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);
                if (Information.AttributeAuthor != "Unknown")
                    Font.NormalFont.Draw("Effect: " + Information.AttributeAuthor, new(infoX, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);

                Font.NormalFont.Draw(Information.Extra, Information.ExtraPosition, Information.ExtraColor * alpha, 0.75f, 0.5f);
            }
            Font.NormalFont.Draw(additions[tipID], new(12, tipY), Color.White * alpha, MathF.Min(0.48f, 600f / Font.NormalFont.SFX.MeasureString(additions[tipID]).X), 0.5f);
            base.Draw();
            GeneralDraw(Sprites.loadingText, new Vector2(280, 430), Color.White * alpha);
            for (int i = 0; i < 6; i++)
                GeneralDraw(Sprites.progressArrow, new(395 + i * 20, 430), Color.White * (Functions.Sin((appearTime - i * 6 - 20) * 3.75f) * 0.9f + 0.1f) * 0.8f * alpha);
        }
        private int tipID;
        public override void Update()
        {
            if (IsKeyPressed120f(InputIdentity.Alternate))
            {
                Functions.PlaySound(FightResources.Sounds.Ding);
                tipID = MathUtil.GetRandom(0, additions.Length - 1);
            }
            alpha = MathHelper.LerpPrecise(alpha, Loaded ? 0 : 1, 0.16f);
            base.Update();
        }
    }
    internal class ResourcesLoadingScene : LoadingScene
    {
        private float loadProgress = 0;
        private static ContentManager loader;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MainResourcesLoad()
        {
            FightResources.Initialize(loader);
            GameStartUp.Initialize?.Invoke(loader);
        }
        public ResourcesLoadingScene(ContentManager loader) : base(() => ResetScene(new GameMenuScene()), MainResourcesLoad) => ResourcesLoadingScene.loader = loader;
        public override void Draw()
        {
            base.Draw();
            float alpha = appearTime / 20f;
            GeneralDraw(Sprites.loadingText, new Vector2(280, 430), Color.White * alpha);
            for (int i = 0; i < 6; i++)
                GeneralDraw(Sprites.progressArrow, new(395 + i * 20, 430), Color.White * (Functions.Sin((appearTime - i * 6 - 20) * 3.75f) * 0.9f + 0.1f) * 0.8f);
#if DEBUG
            List<string> list = [];
            foreach (string item in AudioPreviewPos.Keys)
            {
                if (!AudioCache.ContainsKey(item))
                    list.Add(item);
            }
#endif
            Font.NormalFont.CentreDraw($"Initializing game\nPlease wait...", new(320, 120), Color.White, 0.8f, 0f);
            Font.NormalFont.CentreDraw($"Loading audio previews: {AudioCache.Count}/{AudioPreviewPos.Count}", new Vector2(320, 320), Color.White * MathF.Abs(Functions.Sin(appearTime)), 0.6f, 1);
            float loadPercentage = (float)AudioCache.Count / AudioPreviewPos.Count;
            loadProgress = MathHelper.LerpPrecise(loadProgress, 200 * loadPercentage, 0.04f);
            GeneralDraw(FightResources.Sprites.pixUnit, new Vector2(320, 320), Color.White, new Vector2(404, 24));
            GeneralDraw(FightResources.Sprites.pixUnit, new Vector2(320, 320), Color.Gray, new Vector2(400, 20));
            GeneralDraw(FightResources.Sprites.pixUnit, new Vector2(120 + loadProgress, 320), Color.LimeGreen, new Vector2(loadProgress * 2, 20));
            if (appearTime >= LeastLoadingTime && finishedLoad && !eventInvoked)
                Font.NormalFont.CentreDraw($"Press Z to skip loading audio previews", new Vector2(320, 360), Color.White * MathF.Round(MathF.Abs(Functions.Sin(appearTime * 1.3f))), 0.6f, 1);
            else
            {
                string str = "Loading resources";
                for (int i = 0; i < DateTime.Now.Ticks / 5000000 % 4; i++)
                    str += ".";
                Font.NormalFont.CentreDraw(str, new Vector2(320, 360), Color.White, 0.6f, 1);
            }
            if (Sprites.loadingTexture != null)
                GeneralDraw(Sprites.loadingTexture, GameStartUp.LoadingSettings.TitleCentrePosition, Color.White * (appearTime / 20f), new Vector2(MathF.Min(640f / Sprites.loadingTexture.Width, 1)));
        }
        public override void Update()
        {
            base.Update();
            if (appearTime >= LeastLoadingTime && finishedLoad && !eventInvoked && IsKeyPressed120f(InputIdentity.Confirm))
                SkipLoadingAudio = true;
        }
    }
}