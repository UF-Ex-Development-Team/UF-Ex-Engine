using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// A boneslab (The bones on the side of the box)
    /// </summary>
    public class Boneslab : Barrage, ICustomLength
    {
        private const int boneslabOuttime = 8;
        private const float quarterAngle = MathF.PI / 2;

        internal static Texture2D BoneSlabTexture => Sprites.boneSlab;
        internal static Texture2D WarningLine => Sprites.warningLine;
        private int appearTime, score = 3, colorType = 0;
        private bool hasHit;
        private Vector2 renderPlace, _warningLine;
        private float currentHeight, missionHeight;
        private readonly int appearDelay, totalTime;
        private readonly float trueRotation;
        private Color drawingColor;
        /// <summary>
        /// The <see cref="Action"/> to execute when the boneslab is created (When the warning ends)
        /// </summary>
        public Action BoneProtruded { get; set; }

        public override void Dispose()
        {
            if (!hasHit && MarkScore)
                PushScore(score);
            base.Dispose();
        }
        /// <summary>
        /// The color of the boneslab, 0-> White, 1-> Blue (Aqua), 2-> Orange
        /// </summary>
        public new int ColorType
        {
            set
            {
                switch (value)
                {
                    case 0:
                        drawingColor = Color.White;
                        colorType = 0;
                        break;
                    case 1:
                        drawingColor = new Color(110, 203, 255, 255);
                        colorType = 1;
                        break;
                    case 2:
                        drawingColor = Color.Orange;
                        colorType = 2;
                        break;
                }
            }
        }
        public new float AppearTime => appearTime - appearDelay;

        private FightBox controlingBox;
        public FightBox ControlingBox
        {
            set => controlingBox = value;
        }
        /// <summary>
        /// The length easing for the boneslab
        /// </summary>
        public Func<ICustomLength, float> LengthRoute { get; set; }
        /// <summary>
        /// The parameters for the <see cref="LengthRoute"/>
        /// </summary>
        public float[] LengthRouteParam { get; set; }

        /// <summary>
        /// Craetes a boneslab
        /// </summary>
        /// <param name="rotation">The rotation of the wall (Must be a multiple of 90)</param>
        /// <param name="appearDelay">The duration of the warning before spawning</param>
        /// <param name="totalTime">The duration of the boneslab</param>
        /// <param name="lengthRoute">The route of the height of the boneslab</param>
        /// <param name="lengthRouteParam">The parameters of the route</param>
        public Boneslab(float rotation, int appearDelay, int totalTime, Func<ICustomLength, float> lengthRoute, float[] lengthRouteParam)
        {
            drawingColor = GameMain.CurrentDrawingSettings.themeColor;
            controlingBox = FightBox.instance;
            LengthRoute = lengthRoute;
            LengthRouteParam = lengthRouteParam;
            rotation %= 360;
            trueRotation = rotation;
            Rotation = rotation;
            this.totalTime = totalTime;
            this.appearDelay = appearDelay;
        }
        /// <summary>
        /// Creates a boneslab
        /// </summary>
        /// <param name="rotation">The rotation of the wall (Must be a multiple of 90)</param>
        /// <param name="height">The height of the boneslab</param>
        /// <param name="appearDelay">The duration of the warning before spawning</param>
        /// <param name="totalTime">The duration of the boneslab</param>
        public Boneslab(float rotation, float height, float appearDelay, float totalTime)
        {
            controlLayer = Surface.Hidden;
            drawingColor = GameMain.CurrentDrawingSettings.themeColor;
            controlingBox = FightBox.instance;
            rotation %= 360;
            trueRotation = rotation;
            Rotation = rotation;
            missionHeight = height;
            this.totalTime = (int)totalTime;
            this.appearDelay = (int)appearDelay;
        }

        public override void Draw()
        {
            if (trueRotation == 90 || trueRotation == 270)
                GameMain.MissionSpriteBatch.Draw(BoneSlabTexture, renderPlace, new Rectangle(0, 320 - (int)currentHeight, (int)(controlingBox as RectangleBox).Height, (int)currentHeight),
                    drawingColor, GetRadian(Rotation) + (float)Math.PI, new Vector2((controlingBox as RectangleBox).Height / 2, 0), 1.0f, SpriteEffects.None, 0.499f);
            if (trueRotation == 0 || trueRotation == 180)
                GameMain.MissionSpriteBatch.Draw(BoneSlabTexture, renderPlace, new Rectangle(0, 320 - (int)currentHeight, (int)(controlingBox as RectangleBox).Width, (int)currentHeight),
                    drawingColor, GetRadian(Rotation) + (float)Math.PI, new Vector2((controlingBox as RectangleBox).Width / 2, 0), 1.0f, SpriteEffects.None, 0.499f);
            if (appearTime < appearDelay)
            {
                if (trueRotation == 90 || trueRotation == 270)
                    GameMain.MissionSpriteBatch.Draw(WarningLine, _warningLine,
                    new Rectangle(0, 0, (int)(controlingBox as RectangleBox).Height, 2),
                    appearTime % 6 < 3 ? Color.Red : Color.Yellow,
                    GetRadian(Rotation) + (float)Math.PI, new Vector2((int)(controlingBox as RectangleBox).Height / 2, 0),
                    1.0f, SpriteEffects.None, 0.3f);

                else
                    GameMain.MissionSpriteBatch.Draw(WarningLine, _warningLine,
                    new Rectangle(0, 0, (int)(controlingBox as RectangleBox).Width, 2),
                    appearTime % 6 < 3 ? Color.Red : Color.Yellow, GetRadian(Rotation) + (float)Math.PI, new Vector2((int)(controlingBox as RectangleBox).Width / 2, 0), 1.0f, SpriteEffects.None, 0.3f);
            }
        }

        public override void Update()
        {
            FightBox box = controlingBox;
            if (++appearTime >= appearDelay)
            {
                if (appearTime == appearDelay + 1)
                    BoneProtruded?.Invoke();
                if (LengthRoute != null && LengthRouteParam != null)
                {
                    if (appearTime <= appearDelay + boneslabOuttime * 2)
                    {
                        float d = (appearTime - appearDelay * 1.0f) / (boneslabOuttime * 2);
                        float e = d * d * 0.85f + 0.15f;
                        missionHeight = LengthRoute(this as ICustomLength);
                        currentHeight = missionHeight * e + currentHeight * (1 - e);
                    }
                    else if (appearTime <= appearDelay + totalTime)
                        currentHeight = LengthRoute(this as ICustomLength);
                    else
                        currentHeight -= ((appearTime - appearDelay - totalTime) / 1.2f + 0.5f) * MathF.Sqrt(missionHeight) / 7 * (7f / boneslabOuttime);
                    goto A;
                }
                if (appearTime <= appearDelay + boneslabOuttime)
                {
                    currentHeight += missionHeight / 20f;
                    currentHeight = missionHeight * 0.22f + currentHeight * 0.78f;
                    currentHeight = Math.Min(currentHeight, missionHeight);
                }
                else if (appearTime >= appearDelay + totalTime)
                    currentHeight -= ((appearTime - appearDelay - totalTime) / 1.2f + 0.5f) * MathF.Sqrt(missionHeight) / 7 * (7f / boneslabOuttime);
                else
                    currentHeight = missionHeight;

                A:
                if (currentHeight < -4)
                    Dispose();
            }
            float angle = quarterAngle + GetRadian(Rotation);
            renderPlace.X = MathF.Cos(angle) * box.CollidingBox.Width / 2 + box.Centre.X;
            renderPlace.Y = MathF.Sin(angle) * box.CollidingBox.Height / 2 + box.Centre.Y;
            _warningLine.X = -MathF.Cos(angle) * missionHeight + renderPlace.X;
            _warningLine.Y = -MathF.Sin(angle) * missionHeight + renderPlace.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void GetCollide(Player.Heart heart)
        {
            RectangleBox box = controlingBox as RectangleBox;
            if (!box.CollidingBox.Contain(heart.Centre))
                return;
            if (currentHeight <= 1)
                return;
            if (colorType == 1 && heart.IsStable)
                return;
            if (colorType == 2 && heart.IsMoved)
                return;

            float res = 0x3f3f3f3f;
            if (trueRotation == 0)
                res = box.Down - currentHeight - heart.Centre.Y;
            else if (trueRotation == 270)
                res = box.Right - currentHeight - heart.Centre.X;
            else if (trueRotation == 180)
                res = -(box.Up + currentHeight - heart.Centre.Y);
            else if (trueRotation == 90)
                res = -(box.Left + currentHeight - heart.Centre.X);

            if (appearTime <= appearDelay)
                return;

            if (res < 0.7f)
            {
                if (!hasHit)
                    PushScore(0);
                LoseHP(heart);
                hasHit = true;
            }
            else if (res <= 2.1f)
            {
                if (score >= 2)
                { score = 1; heart.CreateCollideEffect2(Color.LawnGreen, 3f); }
            }
            else if (res <= 4.5f)
            {
                if (score >= 3)
                { score = 2; heart.CreateCollideEffect2(Color.LightBlue, 6f); }
            }

            if (score != 3 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
            {
                if (!hasHit)
                {
                    PushScore(0);
                    LoseHP(heart);
                    hasHit = true;
                }
            }
        }
    }
}