using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;

namespace UndyneFight_Ex
{
    /// <summary>
    /// THe class for most rendering actions, see the documentation for more information
    /// </summary>
    public abstract class RenderProduction : IComparable<RenderProduction>
    {
        private static bool HighQuality => Settings.SettingsManager.DataLibrary.drawingQuality == Settings.SettingsManager.DataLibrary.DrawingQuality.High;
        /// <summary>
        /// The scale factor of the surface
        /// </summary>
        protected static float AdaptingScale => HighQuality ? MathF.Min(ScreenSize.X / (480f * GameMain.Aspect * GameStates.SurfaceScale), ScreenSize.Y / 480f) : 1;
        /// <summary>
        /// The current size of the screen
        /// </summary>
        protected static Vector2 ScreenSize => HighQuality ? GameMain.ScreenSize : new Vector2(480f * GameMain.Aspect, 480) * GameStates.SurfaceScale;

        protected internal static GraphicsDevice WindowDevice => GameMain.Graphics.GraphicsDevice;
        protected internal static SpriteBatchEX SpriteBatch => GameMain.MissionSpriteBatch;

        private static readonly HashSet<Type> updatedTypes = [];

        internal bool disposed = false;
        public virtual void Dispose() => disposed = true;
        public virtual void Update() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 Adapt(Vector2 origin)
        {
            if (!HighQuality)
                return new Vector2(480f * GameMain.Aspect, 480) * GameStates.SurfaceScale;

            float trueX, trueY;
            if (origin.X >= origin.Y * GameMain.Aspect)
            {
                trueX = origin.Y * GameMain.Aspect;
                trueY = origin.Y;
            }
            else
            {
                trueY = origin.X / GameMain.Aspect;
                trueX = origin.X;
            }

            return new(trueX, trueY);
        }
        /// <summary>
        /// The adapted size of the screen
        /// </summary>
        protected static Vector2 AdaptedSize => Adapt(ScreenSize);
        /// <summary>
        /// Creates a render production
        /// </summary>
        /// <param name="shader">The shader to apply</param>
        /// <param name="sortMode">The sorting mode of the sprites</param>
        /// <param name="blendState">The blending method</param>
        /// <param name="depth">The depth of the render production</param>
        /// <exception cref="ArgumentException"></exception>
        protected RenderProduction(Shader shader, SpriteSortMode sortMode, BlendState blendState, float depth)
        {
            Type type = GetType();
            if (!updatedTypes.Contains(type))
            {
                _ = updatedTypes.Add(type);
                WindowSizeChanged(AdaptedSize);
            }
            Shader = shader;
            SpriteSortMode = sortMode;
            BlendState = blendState;
            this.depth = depth;
            if (depth is > 1 or < 0)
                throw new ArgumentException(string.Format("the value {0} have to be in 0~1", nameof(depth)), nameof(depth));
        }
        /// <summary>
        /// The target <see cref="RenderTarget2D"/> to draw on
        /// </summary>
        protected RenderTarget2D MissionTarget { get; set; }
        /// <summary>
        /// The sorting mode of the rendering production, see <see href="https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.SpriteSortMode.html"/> for more information
        /// </summary>
        public SpriteSortMode SpriteSortMode { private get; set; }
        /// <summary>
        /// The blending state of the rendering production, see <see href="https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.BlendState.html"/> for more information
        /// </summary>
        public BlendState BlendState { private get; set; }

        private bool enabledMatrix = false;
        private Matrix matrix;
        protected Matrix Transform { set { matrix = value; enabledMatrix = true; } }
        private readonly float depth;
        /// <summary>
        /// The shader to apply on the rendering production
        /// </summary>
        protected Shader Shader { set; get; }
        /// <summary>
        /// The sampling state of the rendering, see <see href="https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.SamplerState.html"/> for more information
        /// </summary>
        protected SamplerState SamplerState { get; set; } = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(RenderProduction r)
        {
            RenderProduction obj = r;
            return obj == null ? throw new NotImplementedException() : depth < obj.depth ? -1 : depth == obj.depth ? 0 : 1;
        }
        /// <summary>
        /// Clears resource buffers and sets the given color in all buffers
        /// </summary>
        /// <param name="color">The color to set</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ResetTargetColor(Color color)
        {
            TrySetTarget();
            GameDevice.Clear(color);
        }
        private static readonly int[] _indices = [0, 1, 2, 1, 3, 2];
        /// <summary>
        /// Draws a texture with the given primitives
        /// </summary>
        /// <param name="vertexArray"></param>
        /// <param name="texture"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawPrimitives(VertexPositionColorTexture[] vertexArray, Texture2D texture = null)
        {
            GraphicsDevice obj = WindowDevice;
            obj.BlendState = BlendState;
            obj.DepthStencilState = DepthStencilState.None;
            obj.RasterizerState = RasterizerState.CullNone;
            obj.SamplerStates[0] = SamplerState ?? SamplerState.LinearClamp;

            VertexPositionColorTexture[] ver = new VertexPositionColorTexture[6];
            for (int i = 0; i < 6; i++)
                ver[i] = vertexArray[_indices[i]];
            GameMain.SpritePass.Apply();
            Effect eff = Shader;
            eff?.CurrentTechnique.Passes[0].Apply();
            WindowDevice.Textures[0] = texture;
            WindowDevice.DrawUserPrimitives(PrimitiveType.TriangleList, ver, 0, 2);

        }
        /// <summary>
        /// Draws the list of entities
        /// </summary>
        /// <param name="entities"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawEntities(Entity[] entities)
        {
            TrySetTarget();
            if (Shader == null)
            {
                GameMain.MissionSpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, transform: enabledMatrix ? matrix : null);
                for (int i = 0; i < entities.Length; i++)
                    entities[i].Draw();
                GameMain.MissionSpriteBatch.End();
                return;
            }
            Shader.Update();
            GameMain.MissionSpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, effect: Shader, transform: enabledMatrix ? matrix : null);
            for (int i = 0; i < entities.Length; i++)
                entities[i].Draw();
            GameMain.MissionSpriteBatch.End();
        }
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="s">The texture to draw</param>
        /// <param name="pos">The position to draw the texture in</param>
        /// <param name="from">The part of the texture to draw</param>
        /// <param name="color">The color to draw the texture in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTexture(Texture2D s, Rectangle pos, Rectangle? from, Color color)
        {
            if (s == MissionTarget)
            {
                CopyRenderTarget(screenSizedTarget, s);
                s = screenSizedTarget;
            }
            TrySetTarget();
            if (Shader != null)
            {
                Shader.Update();
                GameMain.MissionSpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, effect: Shader, transform: enabledMatrix ? matrix : null);
            }
            else
            {
                GameMain.MissionSpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, transform: enabledMatrix ? matrix : null);
            }
            GameMain.MissionSpriteBatch.Draw(s, pos, from, color);
            GameMain.MissionSpriteBatch.End();
        }
        /// <summary>
        /// Draws multiple textures
        /// </summary>
        /// <param name="tex">The textures to draw</param>
        /// <param name="pos">The position to draw the texture in</param>
        /// <param name="from">The part of the texture to draw</param>
        /// <param name="colors">The colors to draw the textures in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTextures(Texture2D[] tex, Rectangle pos, Rectangle? from, Color[] colors)
        {
            for (int i = 0; i < tex.Length; i++)
                if (tex[i] == MissionTarget)
                {
                    CopyRenderTarget(screenSizedTarget, tex[i]);
                    tex[i] = screenSizedTarget;
                }
            TrySetTarget();
            if (Shader != null)
            {
                Shader.Update();
                GameMain.MissionSpriteBatch.Begin(SpriteSortMode, BlendState, effect: Shader, transform: enabledMatrix ? matrix : null);
            }
            else
            {
                GameMain.MissionSpriteBatch.Begin(SpriteSortMode, BlendState, transform: enabledMatrix ? matrix : null);
            }
            for (int i = 0; i < tex.Length; ++i)
                GameMain.MissionSpriteBatch.Draw(tex[i], pos, from, colors[i]);
            GameMain.MissionSpriteBatch.End();
        }
        /// <summary>
        /// Draws multiple textures
        /// </summary>
        /// <param name="tex">The textures to draw</param>
        /// <param name="pos">The position to draw the texture in</param>
        /// <param name="from">The part of the texture to draw</param>
        /// <param name="color">The color to draw the textures in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTextures(Texture2D[] tex, Rectangle pos, Rectangle? from, Color color)
        {
            Color[] colors = new Color[tex.Length];
            for (int i = 0; i < tex.Length; i++)
                colors[i] = color;
            DrawTextures(tex, pos, from, colors);
        }
        /// <summary>
        /// Draws multiple textures
        /// </summary>
        /// <param name="s">The textures to draw</param>
        /// <param name="bound">The rectangle to draw the textures in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTextures(Texture2D[] s, Rectangle bound) => DrawTextures(s, bound, null, Color.White);
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="s">The texture to draw</param>
        /// <param name="pos">The position to the texture draw in</param>
        /// <param name="color">The color to draw the texture in</param>
        protected void DrawTexture(Texture2D s, Vector2 pos, Color color) => DrawTexture(s, new Rectangle(pos.ToPoint(), s.Bounds.Size), null, color);
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="s">The texture to draw</param>
        /// <param name="pos">The position to draw the texture in</param>
        /// <param name="color">The color to draw the texture in</param>
        /// <param name="size">The size of the texture to draw in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTexture(Texture2D s, Vector2 pos, Color color, float size) => DrawTexture(s, new Rectangle(pos.ToPoint(), (s.Bounds.Size.ToVector2() * size).ToPoint()), null, color);
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="s">The texture to draw</param>
        /// <param name="pos">The position to draw the texture in</param>
        /// <param name="color">The color to draw the texture in</param>
        /// <param name="size">The size of the texture to draw in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTexture(Texture2D s, Vector2 pos, Color color, Vector2 size) => DrawTexture(s, new Rectangle(pos.ToPoint(), (s.Bounds.Size.ToVector2() * size).ToPoint()), null, color);
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="s">The texture to draw</param>
        /// <param name="bound">The rectangle to draw the texture in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTexture(Texture2D s, Rectangle bound) => DrawTexture(s, bound, null, Color.White);
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="s">The texture to draw</param>
        /// <param name="bound">The rectangle to draw the texture in</param>
        /// <param name="color">The color to draw the texture in</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTexture(Texture2D s, Rectangle bound, Color color) => DrawTexture(s, bound, null, color);
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="s">The texture to draw</param>
        /// <param name="pos">The position to draw the texture</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawTexture(Texture2D s, Vector2 pos) => DrawTexture(s, pos, Color.White);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WindowSizeChanged(Vector2 vec) => updatedTypes.Add(GetType());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateBase(Vector2 vec)
        {
            vec = Adapt(vec);
            updatedTypes.Clear();
            if (vec == Vector2.Zero)
                return;
            if (screenSizedTarget == null || screenSizedTarget.Width != (int)vec.X || screenSizedTarget.Height != (int)vec.Y)
                screenSizedTarget = new RenderTarget2D(WindowDevice, (int)vec.X, (int)vec.Y, false, SurfaceFormat.Color, DepthFormat.None);
            if (HelperTarget == null || HelperTarget.Width != (int)vec.X || HelperTarget.Height != (int)vec.Y)
            {
                //Do NOT compile
                HelperTarget = new(WindowDevice, (int)vec.X, (int)vec.Y, false, SurfaceFormat.Color, DepthFormat.None);
                HelperTarget2 = new(WindowDevice, (int)vec.X, (int)vec.Y, false, SurfaceFormat.Color, DepthFormat.None);
                HelperTarget3 = new(WindowDevice, (int)vec.X, (int)vec.Y, false, SurfaceFormat.Color, DepthFormat.None);
            }
        }
        public abstract RenderTarget2D Draw(RenderTarget2D obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TrySetTarget()
        {
            if (currentTarget == MissionTarget)
                return;
            currentTarget = MissionTarget;
            GameDevice.SetRenderTarget(MissionTarget);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TrySetTarget(RenderTarget2D mission)
        {
            if (currentTarget == mission)
                return;
            currentTarget = mission;
            GameDevice.SetRenderTarget(mission);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void CopyRenderTarget(RenderTarget2D distin, Texture2D source)
        {
            TrySetTarget(distin);
            //ResetTargetColor(Color.Transparent);
            SpriteBatch.Begin(SpriteSortMode.Immediate);
            SpriteBatch.Draw(source, distin.Bounds, Color.White);
            SpriteBatch.End();
        }

        private static GraphicsDevice GameDevice => GameMain.Graphics.GraphicsDevice;

        private static RenderTarget2D currentTarget;
        private static RenderTarget2D screenSizedTarget;
        protected static RenderTarget2D HelperTarget { private set; get; }
        protected static RenderTarget2D HelperTarget2 { private set; get; }
        protected static RenderTarget2D HelperTarget3 { private set; get; }
        /// <summary>
        /// Whether the drawing action is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// The time elapsed in the game
        /// </summary>
        public static float TimeElapsed { get; internal set; }
    }
    /// <summary>
    /// A surface
    /// </summary>
    public class Surface : RenderProduction
    {
        /// <summary>
        /// The alpha of the surface
        /// </summary>
        public float drawingAlpha { get; set; } = 1;
        /// <summary>
        /// The area of the surface to restrict in
        /// </summary>
        public BoxVertex[] RestrictArea = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Initialize()
        {
            Normal = new("normal") { BlendState = BlendState.AlphaBlend, SpriteSortMode = SpriteSortMode.FrontToBack, Transfer = TransferUse.ForceNormal };
            Hidden = new("hidden", true) { BlendState = BlendState.AlphaBlend, SpriteSortMode = SpriteSortMode.FrontToBack, BackGroundColor = Color.Black };
        }
        /// <summary>
        /// Creates a surface (Note that this does not add the surface to the rendering pipeline)
        /// </summary>
        /// <param name="name">The name of the surface</param>
        /// <param name="lockSize">Whether the size is forced at 640x480 or will change dynamically based on the window size</param>
        public Surface(string name, bool lockSize = false) : base(null, SpriteSortMode.Deferred, null, 0.0f)
        {
            Name = name;
            RenderPaint = (SizeLock = lockSize) ? new RenderTarget2D(WindowDevice, (int)ScreenSize.X, (int)ScreenSize.Y) : new RenderTarget2D(WindowDevice, (int)(480 * GameMain.Aspect * GameStates.SurfaceScale), (int)(480 * GameStates.SurfaceScale));
        }
        public override void Dispose()
        {
            RenderPaint.Dispose();
            base.Dispose();
        }
        private class BoxPartDrawer : Entity
        {
            private readonly BoxVertex[] _vertexs;
            public BoxPartDrawer(RenderTarget2D target, BoxVertex[] Vertices)
            {
                _vertexs = Vertices;
                Image = target;
                Depth = 0.39f;
            }
            public override void Draw()
            {
                if (_vertexs is null)
                    return;
                VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[_vertexs.Length];
                for (int i = 0; i < _vertexs.Length; i++)
                {
                    Vertices[i] = new(new(_vertexs[i].CurrentPosition, 0.395f), Color.White, _vertexs[i].CurrentPosition / new Vector2(640, 480));
                }
                if (Vertices.Length == 4)
                    SpriteBatch.DrawVertex(Image, 0.39f, Vertices);
                else
                {
                    List<Tuple<int, int, int>> indices = DrawingLab.GetIndices(Vertices);
                    int[] input = new int[indices.Count * 3];
                    int x = 0;
                    for (int i = 0; i < indices.Count; i++)
                    {
                        input[x++] = indices[i].Item1;
                        input[x++] = indices[i].Item2;
                        input[x++] = indices[i].Item3;
                    }
                    SpriteBatch.DrawVertex(Image, 0.39f, input, Vertices);
                }
            }

            public override void Update() => throw new NotImplementedException();
        }
        /// <summary>
        /// The rendered texture of the surface
        /// </summary>
        public RenderTarget2D RenderPaint { get; private set; }
        /// <summary>
        /// The surface for drawing on screen
        /// </summary>
        public static Surface Normal { get; private set; }
        /// <summary>
        /// The surface for drawing inside the box
        /// </summary>
        public static Surface Hidden { get; private set; }
        /// <summary>
        /// The color of the background
        /// </summary>
        public Color BackGroundColor { get; set; } = Color.Transparent;
        /// <summary>
        /// Disables <see cref="Fight.Functions.ScreenDrawing.ScreenExtending"/> in this surface
        /// </summary>
        public bool DisableExpand { get; set; } = false;
        /// <summary>
        /// Custom update logic
        /// </summary>
        public event Action DoUpdate;

        public override void Update()
        {
            DoUpdate?.Invoke();
            Vector4 extending = DisableExpand ? Vector4.Zero : GameStates.CurrentScene.CurrentDrawingSettings.Extending;
            Vector2 size = !SizeLock ? AdaptedSize : new Vector2(480 * GameMain.Aspect, 480) * GameStates.SurfaceScale;
            int missionX = (int)size.X, missionY = (int)(size.Y * (1 + extending.W));
            if (RenderPaint.Bounds.Size != new Point(missionX, missionY))
            {
                RenderPaint.Dispose();
                RenderPaint = new RenderTarget2D(WindowDevice, missionX, missionY);
            }
        }
        public enum TransferUse
        {
            ForceDefault = 0,
            ForceNormal = 1,
            Custom = 2
        }
        /// <summary>
        /// Whether the size of the surface is locked at 640x480
        /// </summary>
        public bool SizeLock { get; private set; } = false;
        public TransferUse Transfer { private get; set; } = TransferUse.Custom;
        public static Matrix NormalTransfer { get; private set; }
        /// <summary>
        /// The custom matrix to apply on the surface (Requires <see cref="Transfer"/> to be <see cref="TransferUse.Custom"/>
        /// </summary>
        public Matrix CustomMatrix { get; set; } = Matrix.Identity;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(Entity[] entities, Matrix transfer)
        {
            if (Transfer == TransferUse.ForceDefault)
            {
                transfer = Matrix.CreateScale(AdaptingScale / GameStates.SurfaceScale);
                transfer.M33 = 1;
            }
            else if (Transfer == TransferUse.Custom)
                transfer = CustomMatrix;
            MissionTarget = RenderPaint;
            ResetTargetColor(BackGroundColor);
            Transform = transfer;
            DrawEntities(entities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DistributeEntity(Entity[] entities, Matrix transfer)
        {
            NormalTransfer = transfer;
            Dictionary<string, Surface> surfaces = GameStates.CurrentScene.CurrentDrawingSettings.surfaces;
            foreach (KeyValuePair<string, Surface> kvp in surfaces)
                kvp.Value.Update();
            Dictionary<Surface, List<Entity>> distributer = [];
            foreach (Entity entity in entities)
            {
                distributer.TryAdd(entity.controlLayer, []);
                distributer[entity.controlLayer].Add(entity);
            }
            distributer.TryAdd(Hidden, []);
            Hidden.Draw([.. distributer[Hidden]], transfer);
            _ = distributer.Remove(Hidden);
            for (int i = 0; i < FightBox.boxes.Count; i++)
                distributer[Normal].Add(new BoxPartDrawer(Hidden.RenderPaint, FightBox.boxes[i].Vertices));
            foreach (Surface kvp in surfaces.Values)
                if (kvp.RestrictArea.Length > 0)
                    distributer[Normal].Add(new BoxPartDrawer(kvp.RenderPaint, kvp.RestrictArea));
            foreach (KeyValuePair<Surface, List<Entity>> kvp in distributer)
                kvp.Key.Draw([.. kvp.Value], transfer);
        }

        public override RenderTarget2D Draw(RenderTarget2D obj) => throw new NotImplementedException();
        /// <summary>
        /// The name of the surface
        /// </summary>
        public string Name { get; }
    }
    /// <summary>
    /// Manages <see cref="RenderProduction"/>
    /// </summary>
    public class RenderingManager
    {
        private readonly SortedSet<RenderProduction> surfaces = [];
        public bool ExistProduction => surfaces.Count >= 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderTarget2D Draw(RenderTarget2D startTarget)
        {
            _ = surfaces.RemoveWhere((s) => s.disposed);
            RenderTarget2D cur = startTarget;

            foreach (RenderProduction itor in surfaces)
            {
                if (itor.Enabled)
                    cur = itor.Draw(cur);
            }
            return cur;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WindowSizeChanged(Vector2 vec)
        {
            for (int i = 0; i < surfaces.Count; i++)
                surfaces.ElementAt(i).WindowSizeChanged(vec);
        }
        /// <summary>
        /// Adds a production to the rendering pipeline
        /// </summary>
        /// <param name="production"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertProduction(RenderProduction production)
        {
            production.disposed = false;
            _ = surfaces.Add(production);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UpdateAll()
        {
            for (int i = 0; i < surfaces.Count; i++)
                surfaces.ElementAt(i).Update();
        }
        /// <summary>
        /// Resets the production state (Removes all <see cref="RenderProduction"/>)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetProduction()
        {
            for (int i = 0; i < surfaces.Count; i++)
                surfaces.ElementAt(i).Dispose();
            surfaces.Clear();
        }
    }
}