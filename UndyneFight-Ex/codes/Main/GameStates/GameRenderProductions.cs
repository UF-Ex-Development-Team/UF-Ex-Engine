using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UndyneFight_Ex.Entities;

namespace UndyneFight_Ex
{
    public static partial class GameStates
    {
        /// <summary>
        /// Aspect ratio of the game
        /// </summary>
        public static float Aspect { set => GameMain.Aspect = value; get => GameMain.Aspect; }
        /// <summary>
        /// The surface scale of the game
        /// </summary>
        public static float SurfaceScale => CurrentScene == null ? 1 : CurrentScene.CurrentDrawingSettings.SurfaceScale;
        internal static Settings.SettingsManager.DataLibrary.DrawingQuality Quality => Settings.SettingsManager.DataLibrary.drawingQuality;

        public static Matrix ResizeMatrix => GameMain.ResizeMatrix;
        /// <summary>
        /// The current game window
        /// </summary>
        public static GameWindow CurrentWindow => GameMain.CurrentWindow;
        /// <summary>
        /// Whether the game window is focused (Not tabbed out)
        /// </summary>
        public static bool GameOnFocus => GameMain.OnFocus;
        /// <summary>
        /// Resets rendering settings
        /// </summary>
        public static void ResetRendering() => GameMain.ResetRendering();

        internal static readonly RenderingManager MainScene = new();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InitializeRendering()
        {
            MainScene.InsertProduction(new GameRenderProductions.RenderBackGround());
            MainScene.InsertProduction(new GameRenderProductions.RenderEntities());
            MainScene.InsertProduction(new GameRenderProductions.OutScreenShader());
            MainScene.InsertProduction(new GameRenderProductions.AntiBlueProduction());
        }

        private static Scene.DrawingSettings CurrentDrawingSettings => missionScene.CurrentDrawingSettings;

        private static class GameRenderProductions
        {
            private static RenderTarget2D normalTarget1, normalTarget2, bufferTarget;
            public class RenderBackGround : RenderProduction, IDisposable
            {
                public RenderBackGround() : base(GlobalResources.Effects.backGroundShader, SpriteSortMode.Immediate, BlendState.AlphaBlend, 0.05f)
                {
                    normalTarget1 = new RenderTarget2D(WindowDevice, (int)(480 * GameMain.Aspect * SurfaceScale), (int)(480 * SurfaceScale), false, SurfaceFormat.Color, DepthFormat.None);
                    normalTarget2 = new RenderTarget2D(WindowDevice, (int)(480 * GameMain.Aspect * SurfaceScale), (int)(480 * SurfaceScale), false, SurfaceFormat.Color, DepthFormat.None);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public override void WindowSizeChanged(Vector2 vec)
                {
                    Vector4 extending = Vector4.Zero;

                    Point p = new((int)vec.X, (int)(vec.Y * (extending.W + extending.Y + 1)));
                    if (normalTarget1 == null || p != normalTarget1.Bounds.Size)
                    {
                        normalTarget1 = new RenderTarget2D(WindowDevice, p.X, p.Y, false, SurfaceFormat.Color, DepthFormat.None);
                        normalTarget2 = new RenderTarget2D(WindowDevice, p.X, p.Y, false, SurfaceFormat.Color, DepthFormat.None);
                    }
                    extending = CurrentScene != null ? CurrentScene.CurrentDrawingSettings.Extending : Vector4.Zero;

                    bufferTarget = new RenderTarget2D(WindowDevice, (int)vec.X, (int)(vec.Y * (extending.W + extending.Y + 1)), false, SurfaceFormat.Color, DepthFormat.None);
                }

                public override void Dispose()
                {
                    base.Dispose();
                    normalTarget1.Dispose();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static Vector4 GetBoundDistance()
                {
                    Vector4 vec = Fight.Functions.ScreenDrawing.BoundDistance;
                    if (CurrentScene is FightScene && ((CurrentScene as FightScene).Mode & SongSystem.GameMode.Buffed) != 0 && (CurrentScene as FightScene).PlayerInstance != null)
                    {
                        float scale = (CurrentScene as FightScene).PlayerInstance.hpControl.LostSpeed / 3 - 0.125f;
                        scale = MathHelper.Clamp(scale, 0, 0.5f) + (CurrentScene as FightScene).PlayerInstance.hpControl.Under1HPScale * 0.5f;
                        vec.Y = MathHelper.Lerp(vec.Y, 100, scale * 0.9f);
                        vec.W = MathHelper.Lerp(vec.W, 100, scale * 0.9f);
                    }
                    return vec;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static Vector4 GetMixColor()
                {
                    Vector4 vec = Fight.Functions.ScreenDrawing.BoundColor.ToVector4();

                    if (CurrentScene is FightScene && ((CurrentScene as FightScene).Mode & SongSystem.GameMode.Buffed) != 0 && (CurrentScene as FightScene).PlayerInstance != null)
                    {
                        float scale = (CurrentScene as FightScene).PlayerInstance.hpControl.LostSpeed / 3 - 0.125f;
                        scale = MathHelper.Clamp(scale, 0, 0.5f) + (CurrentScene as FightScene).PlayerInstance.hpControl.Under1HPScale * 0.5f;
                        vec = Vector4.Lerp(vec, Color.DarkRed.ToVector4(), scale * 0.9f);
                    }
                    return vec;
                }
                public override RenderTarget2D Draw(RenderTarget2D obj)
                {
                    Vector4 boundDist = GetBoundDistance();
                    bool needExtra = missionScene.BackgroundRendering.ExistProduction || boundDist.LengthSquared() >= 5;
                    if (needExtra)
                    {
                        Shader.SetParameters(new("boundDistance", boundDist), new("mixColor", GetMixColor()));
                        MissionTarget = normalTarget2;
                        ResetTargetColor(CurrentDrawingSettings.backGroundColor);
                        normalTarget2 = missionScene.BackgroundRendering.Draw(normalTarget2);
                    }
                    MissionTarget = normalTarget1;
                    ResetTargetColor(CurrentDrawingSettings.backGroundColor);
                    if (needExtra)
                        DrawTexture(normalTarget2, Vector2.Zero);
                    return MissionTarget;
                }
            }
            public class RenderEntities : RenderProduction
            {
                public RenderEntities() : base(null, SpriteSortMode.FrontToBack, BlendState.AlphaBlend, 0.1f) { }

                public override RenderTarget2D Draw(RenderTarget2D obj)
                {
                    MissionTarget = bufferTarget;
                    DrawTexture(normalTarget1, Vector2.Zero);
                    DrawTexture(Surface.Normal.RenderPaint, Vector2.Zero, Color.White * CurrentDrawingSettings.masterAlpha);

                    return missionScene.DrawAll(bufferTarget);
                }
            }
            public class OutScreenShader : RenderProduction
            {
                public OutScreenShader() : base(null, SpriteSortMode.Immediate, BlendState.AlphaBlend, 0.15f) { }

                private Color flickerColor;
                private float alpha = 0;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void MakeFlicker() => isFlickerMade = true;
                private static bool isFlickerMade = false;
                public override void Update()
                {
                    if (isFlickerMade)
                    {
                        isFlickerMade = false;
                        alpha = 0.9999f;
                    }
                    if (Fight.Functions.ScreenDrawing.whiteOutRest > 0)
                    {
                        Fight.Functions.ScreenDrawing.whiteOutRest--;
                        float scale = MathF.Min(0.955f, Fight.Functions.ScreenDrawing.SceneOutScale / (Fight.Functions.ScreenDrawing.whiteOutRest + 1f));
                        alpha = alpha * (1 - scale) + scale;
                    }
                    else
                    {
                        alpha *= 0.86f;
                    }
                    flickerColor = Fight.Functions.ScreenDrawing.flickerColor;
                }
                public override RenderTarget2D Draw(RenderTarget2D obj)
                {
                    float alp = alpha;
                    Color col = flickerColor;
                    if (CurrentScene is FightScene && ((CurrentScene as FightScene).Mode & SongSystem.GameMode.Buffed) != 0 && (CurrentScene as FightScene).PlayerInstance != null)
                    {
                        float scale = (CurrentScene as FightScene).PlayerInstance.hpControl.LostSpeed / 3 - 0.125f;
                        scale = MathHelper.Clamp(scale, 0, 0.5f) + (CurrentScene as FightScene).PlayerInstance.hpControl.Under1HPScale * 0.3f;
                        alp = MathHelper.Lerp(alp, 0.8f, scale);
                        col = Color.Lerp(alpha < 0.05f ? Color.Transparent : col, Color.DarkRed, scale * 1.2f);
                    }
                    if (alp <= 0.001f)
                        return obj;
                    MissionTarget = normalTarget2;
                    DrawTexture(obj, Vector2.Zero);
                    DrawTexture(FightResources.Sprites.pixUnit, new CollideRect(Vector2.Zero, AdaptedSize).ToRectangle(), col * alp * (col.A / 255f));
                    return MissionTarget;
                }
            }
            public class AntiBlueProduction : RenderProduction
            {
                private class Text : Entity
                {
                    public Text() => UpdateIn120 = true;

                    private double valueMemory;

                    public override void Draw()
                    {
#if DEBUG
                        FightResources.Font.NormalFont.Draw("Memory:" + Math.Round(valueMemory, 3), new(40, 150), Color.Aqua * 0.5f);
                        FightResources.Font.NormalFont.Draw("Draw Cost:" + GameMain.DrawDelay1 + "/" + Math.Round(GameMain.DrawDelay2 / 10000.0, 3), new(40, 190), Color.White);
#endif
                    }

                    public override void Update()
                    {
                        Process cur = Process.GetCurrentProcess();
                        valueMemory = cur.PrivateMemorySize64 / 1048764.0;
                    }
                }
                public AntiBlueProduction() : base(GlobalResources.Effects.reduceBlueShader, SpriteSortMode.Immediate, BlendState.Opaque, 0.25f) => debugEntity = new Text();

                private readonly Entity debugEntity;

                public override RenderTarget2D Draw(RenderTarget2D obj)
                {
                    if (Settings.SettingsManager.DataLibrary.reduceBlueAmount == 0)
                        return obj;
                    debugEntity.TreeUpdate();
                    MissionTarget = normalTarget2;
                    DrawTexture(obj, Vector2.Zero);
                    if (Settings.SettingsManager.DataLibrary.debugMessage)
                        DrawEntities([debugEntity]);
                    return MissionTarget;
                }
            }
        }
        internal static RenderTarget2D DrawAll()
        {
            RenderTarget2D temp = MainScene.Draw(null);
            return temp;
            return missionScene.DrawAll(temp);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void MakeFlicker() => GameRenderProductions.OutScreenShader.MakeFlicker();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WindowSizeChanged(Vector2 vec)
        {
            if (Settings.SettingsManager.DataLibrary.drawingQuality != Settings.SettingsManager.DataLibrary.DrawingQuality.High)
                vec = new Vector2(480 * GameMain.Aspect, 480) * SurfaceScale;
            MainScene.WindowSizeChanged(vec);
            missionScene.BackgroundRendering.WindowSizeChanged(vec);
            missionScene.SceneRendering.WindowSizeChanged(vec);
        }
    }
}