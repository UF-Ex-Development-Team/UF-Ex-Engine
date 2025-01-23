using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.GlobalResources.Effects;

namespace UndyneFight_Ex.Fight
{
    public static partial class Functions
    {
        /// <summary>
        /// Functions related to drawing
        /// </summary>
        public static class ScreenDrawing
        {
            /// <summary>
            /// A class for creating custom surfaces
            /// </summary>
            /// <param name="surf">The surface to draw</param>
            /// <param name="depth">The depth of the surface</param>
            /// <param name="blendState">The blend state of the surface (Default <see cref="BlendState.AlphaBlend"/>)</param>
            public class CustomSurface(Surface surf, float depth, BlendState blendState = null) : RenderProduction(null, SpriteSortMode.Immediate, blendState ?? BlendState.AlphaBlend, depth)
            {
                /// <summary>
                /// The position of the surface
                /// </summary>
                public Vector2 Position = Vector2.Zero;
                /// <summary>
                /// The blend of the surface
                /// </summary>
                public Color Blend = Color.White;
                /// <summary>
                /// The surface of the custom surface rendering
                /// </summary>
                public Surface Surface = surf;
                public override RenderTarget2D Draw(RenderTarget2D obj)
                {
                    Surface.DisableExpand = true;
                    MissionTarget = obj;
                    DrawTexture(Surface.RenderPaint, Position, Blend, AdaptingScale);
                    return MissionTarget;
                }
                public override void Dispose()
                {
                    base.Dispose();
                    GameStates.CurrentScene.CurrentDrawingSettings.surfaces.Remove(surf.Name);
                }
            }
            /// <summary>
            /// A method for creating custom surfaces
            /// </summary>
            /// <param name="surf">The surface to draw</param>
            /// <param name="depth">The depth of the surface</param>
            /// <param name="blendState">The blend state of the surface (Default <see cref="BlendState.AlphaBlend"/>)</param>
            /// <returns>The rendering class of the surface</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CustomSurface CreateCustomSurface(Surface surf, float depth, BlendState blendState = null)
            {
                CustomSurface production;
                if (!GameStates.CurrentScene.CurrentDrawingSettings.surfaces.TryAdd(surf.Name, surf))
                    return null;
                SceneRendering.InsertProduction(production = new CustomSurface(surf, depth, blendState));
                return production;
            }
            /// <summary>
            /// Variables related to UI
            /// </summary>
            public static class UISettings
            {
                /// <summary>
                /// Position of the Name display (Default (20, 457))
                /// </summary>
                public static Vector2 NameShowerPos
                {
                    set => (GameStates.CurrentScene as FightScene).NameShow.Centre = value;
                    get => (GameStates.CurrentScene as FightScene).NameShow.Centre;
                }
                /// <summary>
                /// Position of the HP display (Default (320, 443))
                /// </summary>
                public static Vector2 HPShowerPos
                {
                    set
                    {
                        HPShower v = (GameStates.CurrentScene as FightScene).HPBar;
                        CollideRect r = v.CurrentArea;
                        r.TopLeft = value;
                        v.ResetArea(r);
                    }
                    get => (GameStates.CurrentScene as FightScene).HPBar.CurrentArea.TopLeft;
                }
                /// <summary>
                /// The render production for dawing the UI surface
                /// </summary>
                /// <param name="uiSurf">The UI surface</param>
                public class UISurfaceDrawing(Surface uiSurf) : RenderProduction(null, SpriteSortMode.Immediate, BlendState.AlphaBlend, 0.50001f)
                {
                    /// <summary>
                    /// The UI surface
                    /// </summary>
                    public Surface UISurface { get; } = uiSurf;
                    public override RenderTarget2D Draw(RenderTarget2D obj)
                    {
                        MissionTarget = obj;
                        DrawTexture(UISurface.RenderPaint, Vector2.Zero, Color.White);
                        return MissionTarget;
                    }
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static void MoveSurface(Surface surface)
                {
                    if (GameStates.CurrentScene is not SongFightingScene scene)
                        return;
                    scene.NameShow.controlLayer = surface;
                    scene.HPBar.controlLayer = surface;
                    scene.Accuracy.controlLayer = surface;
                    scene.Time.controlLayer = surface;
                    scene.ScoreState.controlLayer = surface;
                }
                /// <summary>
                /// Creates a seperate surface for the UI, making the UI not affected by the screen effects
                /// </summary>
                /// <returns></returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static UISurfaceDrawing CreateUISurface()
                {
                    Surface surf;
                    Scene scene = GameStates.CurrentScene;
                    if (!scene.CurrentDrawingSettings.surfaces.TryGetValue("UI", out Surface value))
                        scene.CurrentDrawingSettings.surfaces.Add("UI", surf = new("UI")
                        {
                            BlendState = BlendState.AlphaBlend,
                            SpriteSortMode = SpriteSortMode.FrontToBack,
                            Transfer = Surface.TransferUse.ForceDefault,
                            DisableExpand = true
                        });
                    else
                        surf = value;
                    MoveSurface(surf);
                    UISurfaceDrawing production;
                    SceneRendering.InsertProduction(production = new UISurfaceDrawing(surf));
                    bufferProduction.Add(production);
                    return production;
                }
                private static readonly List<UISurfaceDrawing> bufferProduction = [];
                /// <summary>
                /// Removes the surface created in <see cref="CreateUISurface"/>
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void RemoveUISurface()
                {
                    bufferProduction.ForEach(s => s.Dispose());
                    Dictionary<string, Surface> surfaces = GameStates.CurrentScene.CurrentDrawingSettings.surfaces;
                    MoveSurface(surfaces["normal"]);
                    if (surfaces.TryGetValue("UI", out Surface value))
                    {
                        value.Dispose();
                        _ = surfaces.Remove("UI");
                    }
                }
            }
            /// <summary>
            /// Variables related to the HP Bar
            /// </summary>
            public static class HPBar
            {
                /// <summary>
                /// The color of existing HP
                /// </summary>
                public static Color HPExistColor { get => (GameStates.CurrentScene as FightScene).HPBar.HPExistColor; set => (GameStates.CurrentScene as FightScene).HPBar.HPExistColor = value; }
                /// <summary>
                /// THe color of the HP Bar
                /// </summary>
                public static Color HPLoseColor { get => (GameStates.CurrentScene as FightScene).HPBar.HPLoseColor; set => (GameStates.CurrentScene as FightScene).HPBar.HPLoseColor = value; }
                /// <summary>
                /// The rectangular area occupied by the HP bar
                /// </summary>
                public static CollideRect AreaOccupied
                {
                    set => (GameStates.CurrentScene as FightScene).HPBar.ResetArea(value);
                    get => (GameStates.CurrentScene as FightScene).HPBar.CurrentArea;
                }
                /// <summary>
                /// Sets whether the HP Bar is displayed vertically
                /// </summary>
                public static bool Vertical { set => (GameStates.CurrentScene as FightScene).HPBar.Vertical = value; }
            }
            /// <summary>
            /// Camera effect methods
            /// </summary>
            public static class CameraEffect
            {
                /// <summary>
                /// Rotates the scren by 180 degrees in the given time
                /// </summary>
                /// <param name="time">The duration of the rotation</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Rotate180(float time) => Rotate(180, time);
                /// <summary>
                /// Rotate the camera by the given amount of degrees
                /// </summary>
                /// <param name="rotation">The angle to rotate for</param>
                /// <param name="time">The duration of the rotation</param>
                /// <exception cref="ArgumentOutOfRangeException">Duratoin is less than 0</exception>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Rotate(float rotation, float time)
                {
                    if (time < 0)
                        throw new ArgumentOutOfRangeException(nameof(time), string.Format("参数 {0} 必须为正数 或 0", nameof(time)));
                    float last = rotation;
                    float tick = 0;

                    float progress = 0;

                    AddInstance(new TimeRangedEvent(0, time + 1, () =>
                    {
                        tick += 0.5f;
                        if (tick < time)
                        {
                            float scale = tick / time;
                            float newRot = MathUtil.Sigmoid01(MathF.Pow(scale, 0.7f));
                            float del = newRot - progress;
                            progress = newRot;
                            last -= del * rotation;
                            ScreenAngle += del * rotation;
                        }
                        else
                        {
                            ScreenAngle += last;
                            last = 0;
                        }
                    })
                    { UpdateIn120 = true });
                }
                /// <summary>
                /// Rotates the screen angle to the given angle
                /// </summary>
                /// <param name="rotation">The angle to rotate to</param>
                /// <param name="time">The duration of the rotation</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void RotateTo(float rotation, float time) => Rotate(MathUtil.MinRotate(ScreenAngle, rotation), time);
                /// <summary>
                /// Convulses the screen angle
                /// </summary>  
                /// <param name="direction">Direction of convulsion, true means right and false means left</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Convulse(bool direction) => Convulse(16, 8, direction);
                /// <summary>
                /// Convulses the screen angle
                /// </summary> 
                /// <param name="time">The duration of the convulsion</param>
                /// <param name="direction">Direction of convulsion, true means right and false means left</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Convulse(float time, bool direction) => Convulse(25, time, direction);
                /// <summary>
                /// Convulses the screen angle
                /// </summary>
                /// <param name="intensity">Intensity of the convulsion</param>
                /// <param name="time">The duration of the convulsion</param>
                /// <param name="direction">Direction of convulsion, true means right and false means left</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Convulse(float intensity, float time, bool direction)
                {
                    if (intensity <= 0)
                        throw new ArgumentOutOfRangeException(nameof(intensity), string.Format("参数 {0} 必须为正数", intensity));
                    if (time < 0)
                        throw new ArgumentOutOfRangeException(nameof(time), string.Format("参数 {0} 必须为正数 或 0", time));

                    intensity = MathF.Sqrt(intensity);
                    if (!direction)
                        intensity = -intensity;
                    float last = 0;
                    float tick = 0;

                    float progress = 0;

                    AddInstance(new TimeRangedEvent(0, time + 1, () =>
                    {
                        tick += 0.5f;
                        if (tick < time)
                        {
                            float scale = tick / time;
                            float newRot = AdvanceFunctions.Sin01(MathF.Pow(scale, 0.75f));
                            float del = newRot - progress;
                            progress = newRot;
                            last -= del * intensity;
                            ScreenAngle += del * intensity;
                        }
                        else
                        {
                            ScreenAngle += last;
                            last = 0;
                        }
                    })
                    { UpdateIn120 = true });
                }
                /// <summary>
                /// Expands the screen by the given size and then retracts to the original size
                /// </summary>
                /// <param name="intensity">The amount to expand</param>
                /// <param name="time">The duration of the expansion</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void SizeExpand(float intensity, float time)
                {
                    if (intensity <= 0)
                        throw new ArgumentOutOfRangeException(nameof(intensity), string.Format("参数 {0} 必须为正数", nameof(intensity)));
                    if (time < 0)
                        throw new ArgumentOutOfRangeException(nameof(time), string.Format("参数 {0} 必须为正数 或 0", nameof(time)));

                    intensity = 1 - MathF.Pow(0.98f, intensity);
                    float last = 0, tick = 0, progress = 0;

                    AddInstance(new TimeRangedEvent(0, time + 1, () =>
                    {
                        tick++;
                        if (tick < time)
                        {
                            float scale = tick / time;
                            float newRot = AdvanceFunctions.Sin01(MathF.Pow(scale, 0.75f));
                            float del = newRot - progress;
                            progress = newRot;
                            last -= del * intensity;
                            ScreenScale += del * intensity;
                        }
                        else
                        {
                            ScreenScale += last;
                            last = 0;
                        }
                    }));
                }
                /// <summary>
                /// Retracts the screen by the given size and then expands to the original size
                /// </summary>
                /// <param name="intensity">The amount to retract</param>
                /// <param name="time">The duration of the retraction</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void SizeShrink(float intensity, float time)
                {
                    if (intensity <= 0)
                        throw new ArgumentOutOfRangeException(nameof(intensity), string.Format("参数 {0} 必须为正数", nameof(intensity)));
                    if (time < 0)
                        throw new ArgumentOutOfRangeException(nameof(time), string.Format("参数 {0} 必须为正数 或 0", nameof(time)));

                    intensity = MathF.Pow(0.98f, intensity) - 1;
                    float last = 0, tick = 0, progress = 0;

                    AddInstance(new TimeRangedEvent(0, time + 1, () =>
                    {
                        tick++;
                        if (tick < time)
                        {
                            float scale = tick / time;
                            float newRot = AdvanceFunctions.Sin01(MathF.Pow(scale, 0.75f));
                            float del = newRot - progress;
                            progress = newRot;
                            last -= del * intensity;
                            ScreenScale += del * intensity;
                        }
                        else
                        {
                            ScreenScale += last;
                            last = 0;
                        }
                    }));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Reset()
            {
                BackGroundColor = Color.Black;
                ThemeColor = Color.White;
                UIColor = Color.White;
                ScreenAngle = 0;
                ScreenScale = 1;
                ScreenPositionDelta = Vector2.Zero;
                BoundDistance = Vector4.One;
                BoundColor = Color.Black;
                whiteOutRest = 0;
            }
            internal static float whiteOutRest = 0;
            internal static Vector4 BoundDistance = Vector4.Zero;
            internal static Color flinkerColor = Color.White;

            /// <summary>
            /// Fades out with the given color
            /// </summary>
            /// <param name="col">The color to fade out</param>
            /// <param name="time">The duration of the fading</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SceneOut(Color col, float time)
            {
                flinkerColor = col;
                whiteOutRest = time;
            }
            /// <summary>
            /// Fades out in white
            /// </summary>
            /// <param name="time">The duration of the fading</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void WhiteOut(float time)
            {
                flinkerColor = Color.White;
                whiteOutRest = time;
            }
            /// <summary>
            /// The color of the UI
            /// </summary>
            public static Color UIColor
            {
                set => GameMain.CurrentDrawingSettings.UIColor = value;
                get => GameMain.CurrentDrawingSettings.UIColor;
            }
            /// <summary>
            /// The color of the side bounds
            /// </summary>
            public static Color BoundColor { get; set; }
            /// <summary>
            /// The theme color of the chart
            /// </summary>
            public static Color ThemeColor
            {
                set => GameMain.CurrentDrawingSettings.themeColor = value;
                get => GameMain.CurrentDrawingSettings.themeColor;
            }
            /// <summary>
            /// The background color of the box
            /// </summary>
            public static Color BoxBackColor
            {
                set => Surface.Hidden.BackGroundColor = value;
                get => Surface.Hidden.BackGroundColor;
            }
            /// <summary>
            /// Creates a flicker of the screen
            /// </summary>
            /// <param name="color">The color of the flicker</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void MakeFlicker(Color color)
            {
                if (whiteOutRest > 0)
                    return;
                flinkerColor = color;
                GameStates.MakeFlicker();
            }
            /// <summary>
            /// The distance of the lower bound
            /// </summary>
            public static float DownBoundDistance { get => BoundDistance.X; set => BoundDistance.X = value; }
            /// <summary>
            /// The distance of the leftward bound
            /// </summary>
            public static float LeftBoundDistance { get => BoundDistance.Y; set => BoundDistance.Y = value; }
            /// <summary>
            /// The distance of the upper bound
            /// </summary>
            public static float UpBoundDistance { get => BoundDistance.Z; set => BoundDistance.Z = value; }
            /// <summary>
            /// The distance of the rightward bound
            /// </summary>
            public static float RightBoundDistance { get => BoundDistance.W; set => BoundDistance.W = value; }

            /// <summary>
            /// The background color of the chart
            /// </summary>
            public static Color BackGroundColor
            {
                set => GameMain.CurrentDrawingSettings.backGroundColor = value;
                get => GameMain.CurrentDrawingSettings.backGroundColor;
            }

            /// <summary>
            /// The rotation of the screen
            /// </summary>
            public static float ScreenAngle
            {
                get => GameMain.CurrentDrawingSettings.screenAngle * 180f / MathUtil.PI;
                set => GameMain.CurrentDrawingSettings.screenAngle = value / 180f * MathUtil.PI;
            }

            /// <summary>
            /// The displacement of the screen
            /// </summary>
            public static Vector2 ScreenPositionDelta
            {
                get => GameMain.CurrentDrawingSettings.screenDelta;
                set => GameMain.CurrentDrawingSettings.screenDelta = value;
            }

            /// <summary>
            /// The scale of the screen
            /// </summary>
            public static float ScreenScale
            {
                get => GameMain.CurrentDrawingSettings.screenScale;
                set => GameMain.CurrentDrawingSettings.screenScale = value;
            }
            /// <summary>
            /// The default fading speed of the color fading
            /// </summary>
            public static float SceneOutScale
            {
                get => GameMain.CurrentDrawingSettings.sceneOutScale;
                set => GameMain.CurrentDrawingSettings.sceneOutScale = value;
            }
            /// <summary>
            /// The overall Alpha value of the screen
            /// </summary>
            public static float MasterAlpha
            {
                get => GameMain.CurrentDrawingSettings.masterAlpha;
                set => GameMain.CurrentDrawingSettings.masterAlpha = value;
            }
            /// <summary>
            /// The main scene rendering manager
            /// </summary>
            public static RenderingManager SceneRendering => GameStates.missionScene.SceneRendering;
            /// <summary>
            /// The background scene rendering manager
            /// </summary>
            public static RenderingManager BackGroundRendering => GameStates.missionScene.BackgroundRendering;
            /// <summary>
            /// The distances of the screen bounds (Color bound effect)
            /// </summary>
            public static Vector4 ScreenExtending
            {
                get => GameMain.CurrentDrawingSettings.Extending;
                set => GameMain.CurrentDrawingSettings.Extending = value;
            }
            /// <summary>
            /// The distance of the upwards bound
            /// </summary>
            public static float UpExtending
            {
                get => ScreenExtending.W;
                set => ScreenExtending = new(ScreenExtending.X, ScreenExtending.Y, ScreenExtending.Z, value);
            }
            /// <summary>
            /// The distance of the downwards bound
            /// </summary>
            public static float DownExtending
            {
                get => ScreenExtending.Y;
                set => ScreenExtending = new(ScreenExtending.X, value, ScreenExtending.Z, ScreenExtending.W);
            }
            /// <summary>
            /// The sprite batch to draw sprites
            /// </summary>
            public static SpriteBatchEX SpriteBatch => GameMain.MissionSpriteBatch;
            /// <summary>
            /// Activates a shader effect on the foreground
            /// </summary>
            /// <param name="shader">The shader to activate</param>
            /// <param name="depth">The depth of the shader</param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Shaders.Filter ActivateShader(Shader shader, float depth = 0.5f)
            {
                Shaders.Filter textureFilter = new(shader, depth);
                SceneRendering.InsertProduction(textureFilter);
                return textureFilter;
            }
            /// <summary>
            /// Activates a shader effect in the background
            /// </summary>
            /// <param name="shader">The shader to activate</param>
            /// <param name="depth">The depth of the shader</param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Shaders.Filter ActivateShaderBack(Shader shader, float depth = 0.5f)
            {
                Shaders.Filter textureFilter = new(shader, depth);
                BackGroundRendering.InsertProduction(textureFilter);
                return textureFilter;
            }
            /// <summary>
            /// List of shaders that are built in to the engine, you will need to manually dispose them
            /// </summary>
            public static class Shaders
            {
                public class Converging(float depth) : RenderProduction(null, SpriteSortMode.Immediate, BlendState.Additive, depth)
                {
                    public override RenderTarget2D Draw(RenderTarget2D obj)
                    {
                        MissionTarget = HelperTarget;
                        return obj;
                    }
                }
                /// <summary>
                /// A bloom effect
                /// </summary>
                /// <param name="depth">The depth of the bloom effect</param>
                public class Lighting(float depth) : RenderProduction(null, SpriteSortMode.Immediate, BlendState.Opaque, depth)
                {
                    private static bool Initialized = false;
                    public static RenderTarget2D[] lightSources { get; private set; } = new RenderTarget2D[4];
                    const int lightSize = 100;
                    /// <summary>
                    /// Available modes for blooming, see <see href="https://en.wikipedia.org/wiki/Blend_modes"/> for more information
                    /// </summary>
                    public enum LightMode
                    {
                        /// <summary>
                        /// Liminal bloom
                        /// </summary>
                        Limit = 1,
                        /// <summary>
                        /// Additive bloom
                        /// </summary>
                        Additive = 2,
                        /// <summary>
                        /// Blend multiplication bloom
                        /// </summary>
                        ShaderMul = 3,
                    }
                    /// <summary>
                    /// The mode of the bloom
                    /// </summary>
                    public LightMode LightingMode { private get; set; } = LightMode.Limit;
                    /// <summary>
                    /// The ambient color of the bloom
                    /// </summary>
                    public Color AmbientColor { private get; set; } = Color.Transparent;
                    /// <summary>
                    /// The class for the bloom
                    /// </summary>
                    public class Light
                    {
                        /// <summary>
                        /// The position of the light
                        /// </summary>
                        public Vector2 position;
                        /// <summary>
                        /// The scale of the light
                        /// </summary>
                        public Vector2 scale = Vector2.One;
                        /// <summary>
                        /// The color of the light
                        /// </summary>
                        public Color color;
                        /// <summary>
                        /// The size of the bloom (will be multiplied to <see cref="scale"/>)
                        /// </summary>
                        public float size;
                    }
                    public List<Light> Lights { get; private set; } = [];

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    private void Initialize()
                    {
                        for (int i = 0; i < lightSources.Length; i++)
                        {
                            lightSources[i] = new RenderTarget2D(WindowDevice, lightSize * 2, lightSize * 2);
                        }
                        MissionTarget = lightSources[0];
                        ResetTargetColor(Color.Transparent);
                        Shader = GlobalResources.Effects.Lights[0];
                        DrawTexture(FightResources.Sprites.pixUnit, lightSources[0].Bounds);
                        Initialized = true;
                    }

                    public override RenderTarget2D Draw(RenderTarget2D obj)
                    {
                        if (!Initialized)
                            Initialize();
                        if (Lights == null || Lights.Count == 0)
                            return obj;
                        MissionTarget = HelperTarget;
                        Shader = null;
                        BlendState = BlendState.Additive;
                        ResetTargetColor(AmbientColor);

                        foreach (Light v in Lights)
                        {
                            Vector2 trueSize = v.scale * v.size;
                            DrawTexture(lightSources[0], (v.position - trueSize) * AdaptingScale, v.color, trueSize * AdaptingScale / lightSize);
                        }

                        return LightingMode switch
                        {
                            LightMode.Limit => LightLimitRender(obj),
                            LightMode.Additive => LightAddRender(obj),
                            LightMode.ShaderMul => LightMulRender(obj),
                            _ => throw new ArgumentOutOfRangeException($"{LightingMode} is not a valid render mode")
                        };
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    private RenderTarget2D LightAddRender(RenderTarget2D obj)
                    {
                        BlendState = BlendState.Additive;

                        MissionTarget = HelperTarget2;
                        ResetTargetColor(Color.Transparent);
                        DrawTextures([HelperTarget, obj], MissionTarget.Bounds);

                        return MissionTarget;
                    }
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    private RenderTarget2D LightMulRender(RenderTarget2D obj)
                    {
                        MissionTarget = HelperTarget2;

                        Shader = FightResources.Shaders.ColorBlend;
                        Shader.RegisterTexture(HelperTarget, 1);
                        DrawTexture(obj, MissionTarget.Bounds);

                        return MissionTarget;
                    }
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    private RenderTarget2D LightLimitRender(RenderTarget2D obj)
                    {
                        BlendState = new BlendState()
                        {
                            Name = "LightingBlend",

                            AlphaBlendFunction = BlendFunction.Min,
                            AlphaSourceBlend = Blend.DestinationAlpha,
                            AlphaDestinationBlend = Blend.One,

                            ColorBlendFunction = BlendFunction.Min,
                            ColorSourceBlend = Blend.DestinationColor,
                            ColorDestinationBlend = Blend.One,

                        };

                        MissionTarget = HelperTarget2;
                        ResetTargetColor(Color.White);
                        DrawTextures([HelperTarget, obj], MissionTarget.Bounds);

                        return MissionTarget;
                    }
                }
                /// <summary>
                /// A RGB splitting effect
                /// </summary>
                /// <param name="dep">The depth of the RGB splitting effect (Default 0.5f)</param>
                public class RGBSplitting(float dep = 0.5f) : RenderProduction(null, SpriteSortMode.FrontToBack, BlendState.Additive, dep)
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public override void WindowSizeChanged(Vector2 vec) => screen = new RenderTarget2D(WindowDevice, (int)vec.X, (int)vec.Y, false, SurfaceFormat.Color, DepthFormat.None);

                    private static RenderTarget2D screen;
                    /// <summary>
                    /// The intensity of the effect
                    /// </summary>
                    public float Intensity { get; set; } = 1f;
                    /// <summary>
                    /// The range of the random distrubance of the RGB channel<br/>
                    /// Make sure <see cref="Disturbance"/> is set to true
                    /// </summary>
                    public float RandomDisturb { get; set; } = 0.2f;
                    /// <summary>
                    /// Whether to enable the disturbance or not
                    /// </summary>
                    public bool Disturbance { get; set; } = true;
                    /// <summary>
                    /// The shader used for the disturbance effect (Default <see cref="FightResources.Shaders.Sinwave"/>)
                    /// </summary>
                    public Shader DisturbShader { get; set; } = FightResources.Shaders.Sinwave;

                    private float time1 = 0, time2 = 0, time3 = 0;
                    /// <summary>
                    /// The first color of the color splitting (Default <see cref="Color.Red"/>)
                    /// </summary>
                    public Color SplitColor1 { get; set; } = Color.Red;
                    /// <summary>
                    /// The second color of the color splitting (Default <see cref="Color.Blue"/>)
                    /// </summary>
                    public Color SplitColor2 { get; set; } = Color.Blue;
                    /// <summary>
                    /// The color drawn without the sahder disturbance (Default <see cref="Color.Lime"/>)
                    /// </summary>
                    public Color MainColor { get; set; } = Color.Lime;

                    public override RenderTarget2D Draw(RenderTarget2D obj)
                    {
                        RandomDisturb = Rand(-0.2f, 0.2f);
                        if (Shader == null && MathF.Abs(RandomDisturb + Intensity) * AdaptingScale < 0.8f)
                            return obj;
                        MissionTarget = screen;
                        Shader = Disturbance ? DisturbShader : null;
                        if (Disturbance)
                        {
                            Shader.SetParameters(
                                new("time1", time1 += Rand(0.08f, 0.15f)),
                                new("time2", time2 += Rand(0.18f, 0.35f)),
                                new("time3", time3 += Rand(0.38f, 0.55f)),
                                new("sin1", 0.0005f),
                                new("sin2", 0.0007f),
                                new("sin3", 0.001f));
                        }
                        DrawTexture(obj, new Vector2((Rand(0.5f - RandomDisturb, 0.5f + RandomDisturb) + Intensity) * AdaptingScale, 0), SplitColor1);
                        DrawTexture(obj, new Vector2((Rand(-0.5f - RandomDisturb, -0.5f + RandomDisturb) - Intensity) * AdaptingScale, 0), SplitColor2);

                        Shader = null;

                        DrawTexture(obj, new Vector2(0, Rand(-0.3f, 0.3f) * AdaptingScale), MainColor);

                        return MissionTarget;
                    }
                }
                /// <summary>
                /// A glitching effect (Similar to BktGlitch in Game Maker, if you had used it)
                /// </summary>
                public class Glitching : RenderProduction
                {
                    /// <summary>
                    /// The intensity of the glitching
                    /// </summary>
                    public int Intensity { get; set; } = 1;
                    /// <summary>
                    /// The average interval between each glitch in frames
                    /// </summary>
                    public int AverageInterval { get; set; } = 4;
                    /// <summary>
                    /// The average delta of the glithes
                    /// </summary>
                    public float AverageDelta { get; set; } = 1f;
                    /// <summary>
                    /// The intensity of the RGB splitting when glitching
                    /// </summary>
                    public float RGBSplitIntensity = 0.0f;
                    /// <summary>
                    /// The scale of the glitched blocks
                    /// </summary>
                    public float BlockScale = 1.0f;
                    private class Updater(Glitching father) : GameObject
                    {
                        readonly Glitching father = father;

                        private class MoveBlock : GameObject
                        {
                            public Vector2 Delta { get; private set; }
                            public Rectangle Area { get; private set; }
                            public Vector2 RGBDelta { get; private set; }
                            public MoveBlock(Glitching father)
                            {
                                Area = new Rectangle(Rand(0, (int)AdaptedSize.X), Rand(0, (int)AdaptedSize.Y),
                                        (int)(Rand(10, 30) * father.BlockScale * AdaptingScale),
                                        (int)(Rand(10, 30) * father.BlockScale * AdaptingScale)
                                    );
                                switch (ColorType = Rand(0, 2))
                                {
                                    case 0:
                                        Delta = new(Rand(6, 12f) * RandSignal(), 0);
                                        break;
                                    case 1:
                                        Delta = new(0, Rand(6, 12f) * RandSignal());
                                        break;
                                    case 2:
                                        Delta = new(Rand(-6f, 6), Rand(-6f, 6));
                                        break;
                                }
                                Delta *= father.AverageDelta;
                                lastTime = Rand(18, 40);
                            }
                            int lastTime;
                            public int ColorType { get; private set; }
                            public override void Update()
                            {
                                if (--lastTime <= 0)
                                    Dispose();
                            }
                        }
                        public override void Update()
                        {
                            if (Rand(0, father.AverageInterval) == 0)
                                for (int i = 0; i < father.Intensity; i++)
                                    AddChild(new MoveBlock(father));
                        }
                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        public Tuple<Rectangle, Vector2, int>[] GetMoves()
                        {
                            List<Tuple<Rectangle, Vector2, int>> res = [];
                            ChildObjects.ForEach(s => res.Add(new Tuple<Rectangle, Vector2, int>((s as MoveBlock).Area, (s as MoveBlock).Delta, (s as MoveBlock).ColorType)));
                            return [.. res];
                        }
                    }
                    /// <summary>
                    /// Create the glitching effect
                    /// </summary>
                    /// <param name="dep">The depth of the effect</param>
                    public Glitching(float dep = 0.5f) : base(null, SpriteSortMode.Immediate, BlendState.Opaque, dep) =>
                        GameStates.InstanceCreate(updater = new Updater(this));
                    readonly Updater updater;
                    public override RenderTarget2D Draw(RenderTarget2D obj)
                    {
                        if (!updater.BeingUpdated)
                            GameStates.InstanceCreate(updater);
                        CopyRenderTarget(HelperTarget, obj);
                        MissionTarget = obj;
                        DrawTexture(obj, Vector2.Zero);

                        Tuple<Rectangle, Vector2, int>[] all = updater.GetMoves();
                        foreach (var item in all)
                        {
                            Rectangle rect = item.Item1;
                            Vector2 pos = item.Item2;
                            CollideRect finRect = new CollideRect(rect) + pos;
                            if (MathF.Abs(RGBSplitIntensity) > 0.1f)
                            {
                                DrawTexture(HelperTarget, finRect + new Vector2(-RGBSplitIntensity, 0), rect, Color.Red);
                                DrawTexture(HelperTarget, finRect + new Vector2(RGBSplitIntensity, 0), rect, new Color(0, 0, 1f));
                            }
                            DrawTexture(HelperTarget, finRect.ToRectangle(), rect, Color.White);
                        }
                        return MissionTarget;
                    }
                    public override void Dispose()
                    {
                        updater.Dispose();
                        base.Dispose();
                    }
                }
                /// <summary>
                /// Creates a filter for the <paramref name="shader"/>, commonly associated with <see cref="ActivateShader(Shader, float)"/> and <see cref="ActivateShaderBack(Shader, float)"/><br/>
                /// The filter effects uses the 3rd helper channel in the render pipeline
                /// </summary>
                /// <param name="shader">The shader to apply</param>
                /// <param name="dep">The depth of the effect</param>
                public class Filter(Shader shader, float dep = 0.5f) : RenderProduction(shader, SpriteSortMode.Immediate, BlendState.Opaque, dep)
                {
                    public override void WindowSizeChanged(Vector2 vec)
                    {
                        if (screen == null || screen.Bounds.Size != vec.ToPoint())
                            screen = new RenderTarget2D(WindowDevice, (int)vec.X, (int)vec.Y, false, SurfaceFormat.Color, DepthFormat.None);
                    }

                    private static RenderTarget2D screen;

                    public Shader CurrentShader => Shader;

                    public override RenderTarget2D Draw(RenderTarget2D obj)
                    {
                        MissionTarget = MissionTarget == screen ? HelperTarget3 : screen;

                        DrawTexture(obj, obj.Bounds);
                        return MissionTarget;
                    }
                }
                /// <summary>
                /// Creates a Gaussian Blur or Kawase Blur, depending on the user's Drawing Quality and <see cref="KawaseMode"/>
                /// </summary>
                /// <param name="dep">The depth of the blur</param>
                public class Blur(float dep = 0.5f) : RenderProduction(null, SpriteSortMode.Immediate, BlendState.Additive, dep)
                {
                    private bool pendingClear = true;
                    public override void WindowSizeChanged(Vector2 vec) => screen = new RenderTarget2D(WindowDevice, (int)vec.X, (int)vec.Y, false, SurfaceFormat.Color, DepthFormat.None);
                    /// <summary>
                    /// The blur shader to use, default (<see cref="FightResources.Shaders.Blur"/>)
                    /// </summary>
                    public BlurShader BlurShader { get; set; } = FightResources.Shaders.Blur;
                    private static RenderTarget2D screen;
                    /// <summary>
                    /// The intensity of the blur
                    /// </summary>
                    public float Sigma { get => BlurShader.Sigma; set => BlurShader.Sigma = value; }
                    /// <summary>
                    /// Whether bloom will be enabled for the blurring
                    /// </summary>
                    public bool Glittering { get; set; } = false;
                    /// <summary>
                    /// The scale of the blooming
                    /// </summary>
                    public float GlitterScale { get; set; } = 0.0f;
                    /// <summary>
                    /// Whether the blur will be a Kawase Blur, regardless of the user's drawing quality
                    /// </summary>
                    public bool KawaseMode { get; set; } = true;

                    public override RenderTarget2D Draw(RenderTarget2D obj)
                    {
                        if (Sigma <= 0.05f)
                            return obj;
                        SamplerState = null;
                        if (Glittering && GlitterScale > 0.05f)
                        {
                            if (pendingClear)
                            {
                                pendingClear = false;
                                MissionTarget = screen;
                                ResetTargetColor(Color.Transparent);
                            }
                            BlendState = BlendState.Opaque;
                            CopyRenderTarget(HelperTarget, obj);
                        }
                        Shader = BlurShader = FightResources.Shaders.Blur;

                        float scale = Glittering ? 1.1f : 1;

                        if (!KawaseMode && Settings.SettingsManager.DataLibrary.drawingQuality == Settings.SettingsManager.DataLibrary.DrawingQuality.High)
                        {
                            SamplerState = null;
                            BlendState = BlendState.Additive;
                            MissionTarget = HelperTarget2;
                            BlurShader.Factor = Vector2.UnitX * scale;
                            DrawTexture(obj, Vector2.Zero);

                            MissionTarget = obj;
                            BlurShader.Factor = Vector2.UnitY * scale;
                            DrawTexture(HelperTarget2, Vector2.Zero);

                            MissionTarget = HelperTarget2;
                            BlurShader.Factor = Vector2.One * scale;
                            DrawTexture(obj, Vector2.Zero);

                            MissionTarget = obj;
                            BlurShader.Factor = (Vector2.UnitX - Vector2.UnitY) * scale;
                            DrawTexture(HelperTarget2, Vector2.Zero);
                        }
                        else
                        {
                            //use kawase algorithm
                            SamplerState = SamplerState.LinearClamp;
                            BlurKawaseShader kawase = FightResources.Shaders.BlurKawase;
                            Shader = kawase;
                            BlendState = BlendState.Additive;

                            [MethodImpl(MethodImplOptions.AggressiveInlining)]
                            static Vector2 OriginToFactor(Vector2 delta) => delta / new Vector2(640f, 480f);

                            float sigma2 = Sigma * 0.75f;

                            MissionTarget = HelperTarget2;
                            kawase.Factor = OriginToFactor(new Vector2(0.5f) * sigma2) * scale;
                            DrawTexture(obj, Vector2.Zero);

                            MissionTarget = obj;
                            kawase.Factor = OriginToFactor(new Vector2(1.5f) * sigma2) * scale;
                            DrawTexture(HelperTarget2, Vector2.Zero);

                            MissionTarget = HelperTarget;
                            kawase.Factor = OriginToFactor(new Vector2(2.5f) * sigma2) * scale;
                            DrawTexture(obj, Vector2.Zero);
                        }

                        if (Glittering && GlitterScale > 0.05f)
                        {
                            SamplerState = null;
                            Shader = null;
                            MissionTarget = screen;
                            BlendState = BlendState.Additive;
                            DrawTextures([HelperTarget, MissionTarget], HelperTarget.Bounds, null, [Color.White, Color.White * GlitterScale]);

                            return MissionTarget;
                        }
                        return MissionTarget;
                    }
                }
            }
        }
    }
}