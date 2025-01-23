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
        private class ProgressArrow : Entity
        {
            public ProgressArrow(Vector2 centre, float beginTime)
            {
                Image = Sprites.progressArrow;
                Centre = centre;
                appearTime = beginTime;
            }

            private float appearTime;
            private float alpha;

            public override void Draw()
            {
                if (alpha > 0)
                    FormalDraw(Image, Centre, Color.White * alpha * 0.8f, 0, ImageCentre);
            }

            public override void Update() => alpha = Functions.Sin(appearTime++ * 7.5f) * 0.9f + 0.1f;
        }
        private const int loadingCentreY = 430;
        private class LoadingTexture : Entity
        {
            private Rectangle fullBound;
            float alpha = 0;
            public LoadingTexture()
            {
                Image = Sprites.loadingText;
                fullBound = Image.Bounds;
            }
            public override void Draw()
            {
                Rectangle v = fullBound;
                v.X = 280 - v.Width / 2;
                v.Y = loadingCentreY - v.Height / 2;
                FormalDraw(Image, v, Color.White * alpha);
            }

            public override void Update()
            {
                if (alpha < 1)
                    alpha += 0.05f;
            }
        }
        private class TitleShower : Entity
        {
            private Rectangle fullBound;
            private readonly float scale;
            float alpha = 0;
            public TitleShower(Texture2D texture)
            {
                Centre = GameStartUp.LoadingSettings.TitleCentrePosition;
                Image = texture;
                fullBound = Image.Bounds;
                scale = MathHelper.Min(1, 640f / fullBound.Width);
            }
            public override void Draw() => FormalDraw(Image, Centre, Color.White * alpha, scale, 0, ImageCentre);

            public override void Update()
            {
                if (alpha < 1)
                    alpha += 0.05f;
            }
        }

        internal LoadingScene(Action loadingFinished, Action loadingAction, bool unLoad = true)
        {
            if (CurrentScene != null)
                CurrentScene.CurrentDrawingSettings.defaultWidth = 640f;
            if (unLoad)
                Loader.Unload();
            this.loadingFinished = loadingFinished;
            if (Sprites.loadingTexture != null)
                InstanceCreate(new TitleShower(Sprites.loadingTexture));
            InstanceCreate(new LoadingTexture());
            for (int i = 0; i < 6; i++)
                InstanceCreate(new ProgressArrow(new(395 + i * 20, loadingCentreY), -i * 6 - 20));

            Thread thread = new(new ThreadStart(() =>
            {
                loadingAction.Invoke();
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

        private string Situation = "game";

        public override void Update()
        {
            if (this is SongLoadingScene)
                Situation = "chart";
            appearTime++;
            if (appearTime == 2)
                loadingThread.Start();
            else if (appearTime >= LeastLoadingTime && finishedLoad && !eventInvoked && (AudioCache.Count == file_path_list.Count || SkipLoadingAudio || this is not ResourcesLoadingScene))
            {
                eventInvoked = true;
                loadingFinished.Invoke();
            }
            base.Update();
        }
        public override void Draw() => Font.NormalFont.CentreDraw($"Initializing {Situation}\nPlease wait...", new(320, 120), Color.White, 0.8f, 0f);
    }
    /// <summary>
    /// Loading a song
    /// </summary>
    public class SongLoadingScene : LoadingScene
    {
        readonly SongInformation Information;
        private static SongFightingScene.SceneParams songParams;
        public SongLoadingScene(SongFightingScene.SceneParams songParams) : base(() =>
        {
            // loadingFinished
            GameStates.InstanceCreate(new InstantEvent(30, () => ResetScene(new SongFightingScene(songParams))));
        }, () =>
        {
            // loadingAction
            if (songParams.Waveset.Attributes?.MusicOptimized ?? false)
                songParams.MusicOptimized = true;
            SongLoadingScene.songParams.LoadMusic();
        }, songParams.IsUnload)
        {
            //SongLoadingScene(){...}
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
        {
            // loadingFinished
            GameStates.InstanceCreate(new InstantEvent(30, () => ResetScene(new SongFightingScene(songParams[0], challenge))));
        }, () =>
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
        float alpha = 0;

        readonly string[] additions = [
            "Every character worth your attention",
            "Tips: Do not bite off more than you can chew",
            "Fun Fact: Practice mode starts with 99HP instead of Infinite HP formerly",
            "Fun Fact: 2021 Spring Celebration is the first and only championship with two segments",
            "Fun Fact: 2023 Memory is the first championship with 2 secret charts",
            "Fun Fact: Indihome Paket Phoenix was in Extreme difficulty before upgraded to Extreme plus",
            "Fun Fact: Freedom Dive div 1 final part has twice the blue arrows before the nerf",
            "Fun Fact: Undyne Extreme was nerfed several times with difficulty drastically dropped",
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
                float xscale = 640f / chartPaint.Width,
                    yscale = 480f / chartPaint.Height;
                FormalDraw(chartPaint, new(320 * xscale, 240 * yscale), Color.Lerp(Color.Black, Color.White, 0.3f), new Vector2(xscale, yscale), 0, new(320, 240));
            }
            if (Information != null)
            {
                int CurPos = 270;
                if (Information.BarrageAuthor != "Unknown")
                    Font.NormalFont.CentreDraw("Barrage: " + Information.BarrageAuthor, new(320, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);
                if (Information.SongAuthor != "Unknown")
                    Font.NormalFont.CentreDraw("Song from: " + Information.SongAuthor, new(320, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);
                if (Information.PaintAuthor != "Unknown")
                    Font.NormalFont.CentreDraw("Paint: " + Information.PaintAuthor, new(320, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);
                if (Information.AttributeAuthor != "Unknown")
                    Font.NormalFont.CentreDraw("Effect: " + Information.AttributeAuthor, new(320, CurPos += 30), Color.White * alpha, 0.8f, 0.5f);

                Font.NormalFont.Draw(Information.Extra, Information.ExtraPosition, Information.ExtraColor * alpha, 0.75f, 0.5f);
            }
            Font.NormalFont.Draw(additions[tipID], new(12, 464), Color.White * alpha, 0.48f, 0.5f);
            base.Draw();
        }
        int tipID;
        public override void Update()
        {
            if (IsKeyPressed120f(InputIdentity.Alternate))
            {
                Functions.PlaySound(FightResources.Sounds.Ding);
                tipID = MathUtil.GetRandom(0, additions.Length - 1);
            }
            if (alpha < 1)
                alpha += 0.05f;
            base.Update();
        }
    }
    internal class ResourcesLoadingScene : LoadingScene
    {
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
#if DEBUG
            var list = new List<string>();
            foreach (var item in AudioPreviewPos.Keys)
            {
                if (!AudioCache.ContainsKey(item))
                    list.Add(item);
            }
#endif
            Font.NormalFont.CentreDraw($"Loading audio previews: {AudioCache.Count}/{AudioPreviewPos.Count}", new Vector2(320, 320), Color.White, 0.6f, 1);
            if (appearTime >= LeastLoadingTime && finishedLoad && !eventInvoked)
                Font.NormalFont.CentreDraw($"Press Z to skip loading audio previews", new Vector2(320, 360), Color.White, 0.6f, 1);
            else
            {
                string str = "Loading resources";
                for (int i = 0; i < DateTime.Now.Ticks / 5000000 % 4; i++)
                    str += ".";
                Font.NormalFont.CentreDraw(str, new Vector2(320, 340), Color.White, 0.6f, 1);
            }
        }
        public override void Update()
        {
            base.Update();
            if (appearTime >= LeastLoadingTime && finishedLoad && !eventInvoked && IsKeyPressed120f(InputIdentity.Confirm))
                SkipLoadingAudio = true;
        }
    }
}