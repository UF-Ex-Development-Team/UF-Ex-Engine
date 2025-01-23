using Microsoft.Xna.Framework.Graphics;
using static System.MathF;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.Settings.SettingsManager.DataLibrary;

namespace UndyneFight_Ex
{
    internal partial class GameMain : Game
    {
        private static void InitializeRendering() => GameStates.InitializeRendering();
        private void ResetDrawingSettings()
        {
            Vector2 defaultSize = new Vector2(480 * Aspect, 480) * SurfaceScale;
            float ex_scale = 1;
            #region screen matrix
            screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
            Vector2 size = screenSize;
            //Apply extra scale in loading screen if drawing quality is not high because bugs
            if (drawingQuality != DrawingQuality.High)
                size = defaultSize;
            CurrentDrawingSettings.shakings = Entities.Advanced.ScreenShaker.ScreenShakeDelta;

            float trueX, trueY;
            if (size.X >= size.Y * Aspect)
            { trueX = size.Y * Aspect; trueY = size.Y; }
            else
            { trueY = size.X / Aspect; trueX = size.X; }
            screenDistance = Sqrt(trueX * trueX + trueY * trueY) / 2 * ex_scale;
            basicAngle = Atan2(-trueX, -trueY);

            Vector4 extending = CurrentScene.CurrentDrawingSettings.Extending;
            float f = CurrentDrawingSettings.screenAngle + quarterAngle;
            float true_angle = basicAngle + f;
            // 1/8th of the window size as that is the value of displacement
            //Minimal scale by aspect ratio multiplied by screen scale
            float true_scale = Min(size.X / defaultSize.X, size.Y / defaultSize.Y) * CurrentDrawingSettings.screenScale;
            float x = screenDistance * -Cos(true_angle) * CurrentDrawingSettings.screenScale + (CurrentDrawingSettings.screenDelta.X + CurrentScene.CurrentDrawingSettings.shakings.X) * true_scale + trueX / 2;
            float y = screenDistance * Sin(true_angle) * CurrentDrawingSettings.screenScale + (CurrentDrawingSettings.screenDelta.Y + CurrentScene.CurrentDrawingSettings.shakings.Y) * true_scale + trueY / 2 + extending.W * trueY;
            matrix = new Matrix
                (Sin(f) * true_scale, Cos(f) * true_scale, 0f, 0f,
                -Cos(f) * true_scale, Sin(f) * true_scale, 0f, 0f,
                0f, 0f, 1f, 0f,
                x, y, 0f, 1f);
            #endregion
        }

        private RenderTarget2D Result { set; get; }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
#if DEBUG
            System.Diagnostics.Stopwatch timer = new();
            timer.Start();
            long temp = timer.ElapsedTicks;
            long temp2 = timer.ElapsedMilliseconds;
#endif

            float frames = DrawFPS / 125f; // gameTime.ElapsedGameTime.Milliseconds / 16f;
            Shader.TimeElapsed = 1 / frames;
            RenderProduction.TimeElapsed = frames;
            Surface.DistributeEntity(GetEntities(), matrix);

            finalTarget = DrawAll();
            //  this.finalTarget = Surface.Normal.RenderPaint; 

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            MissionSpriteBatch.Begin(SpriteSortMode.Immediate, null, MissionSpriteBatch.DefaultState);
            MissionSpriteBatch.Draw(finalTarget, screenSize / 2, null, Color.White, 0, finalTarget.Bounds.Size.ToVector2() / 2,
                Min(screenSize.X / finalTarget.Width, screenSize.Y / finalTarget.Height), SpriteEffects.None, 0.5f);
            MissionSpriteBatch.End();

            Result = finalTarget;
#if DEBUG
            DrawDelay1 = timer.ElapsedMilliseconds - temp2;
            DrawDelay2 = timer.ElapsedTicks - temp;
#endif
            base.Draw(gameTime);
        }

        public static long DrawDelay1 { get; private set; }
        public static double DrawDelay2 { get; private set; }
    }
}