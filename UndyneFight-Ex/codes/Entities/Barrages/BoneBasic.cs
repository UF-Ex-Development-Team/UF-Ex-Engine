using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using static System.Math;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// The base class for a bone, you should not call this
    /// </summary>
    public class Bone : Barrage
    {
        private protected FightBox controlingBox;
        /// <inheritdoc/>
        public FightBox ControlingBox => controlingBox;

        private int score = 3;
        protected float alpha = 0;
        private bool hasHit = false;

        private protected bool autoDispose = true;
        private bool hasBeenInside = false;
        private static CollideRect screen = new(-50, -50, 740, 580);
        /// <summary>
        /// The length of the bone
        /// </summary>
        public float Length { get; set; }
        /// <summary>
        /// Whether the bone is masked inside of the box
        /// </summary>
        public bool IsMasked { get; set; } = true;

        private protected Color drawingColor;
        /// <summary>
        /// The alpha of the bone
        /// </summary>
        public float Alpha { get => alpha; set => alpha = value; }

        private int colorType = 0;
        /// <summary>
        /// The color of the bone, 0-> White, 1-> Blue, 2-> Orange
        /// </summary>
        public new float ColorType
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
                    default:
                        throw new ArgumentOutOfRangeException("rvalue", value, "The rvalue can only be 0, 1 or 2");
                }
            }
            get => colorType;
        }
        /// <summary>
        /// Sets the drawing color of the bone
        /// </summary>
        /// <param name="color"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetColor(Color color) => drawingColor = color;
        /// <summary>
        /// Whether the depth will be automatically sorted by their color type (Will override the original depth)
        /// </summary>
        public bool AutoDepth = true;
        private readonly SpriteBatchEX spb = GameMain.MissionSpriteBatch;
        /// <inheritdoc/>
        public override void Draw()
        {
            if (Length < 0)
                return;
            if (AutoDepth)
                Depth = 0.5f - colorType * 0.02f;
            CollideRect cl1 = new(0, 3, 6, Length - 3);
            Vector2 delta = GetVector2(Length / 2f, Rotation + 90);
            col Col = Color.Lerp(Color.Transparent, drawingColor, alpha);
            spb.Draw(Sprites.boneBody, Centre, cl1, Col, GetRadian(Rotation), new Vector2(3, Length / 2), 1.0f, SpriteEffects.None, Depth);
            spb.Draw(Sprites.boneHead, Centre - delta, null, Col, GetRadian(Rotation), new Vector2(5, 6), 1.0f, SpriteEffects.None, Depth);
            spb.Draw(Sprites.boneHead, Centre + delta, null, Col, GetRadian(Rotation + 180f), new Vector2(5, 3), 1.0f, SpriteEffects.None, Depth);
        }
        /// <inheritdoc/>
        public override void Update()
        {
            controlLayer = IsMasked ? Surface.Hidden : Surface.Normal;
            if (autoDispose)
            {
                bool ins = GetType() != typeof(CustomBone) ? screen.Contain(Centre) : (this as CustomBone).screenC.Contain(Centre);
                if (ins && (!hasBeenInside))
                    hasBeenInside = true;
                if (hasBeenInside && (!ins))
                    Dispose();
            }
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void GetCollide(Player.Heart Heart)
        {
            if (alpha <= 0.9f)
                return;
            float A, B, C, dist;
            bool needAP = ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0;
            if (Rotation == 0)
                dist = Centre.X - Heart.Centre.X;
            else
            {
                float k = MathF.Tan(GetRadian(Rotation + 90));
                A = k;
                B = -1;
                C = -A * Centre.X - B * Centre.Y;
                dist = (A * Heart.Centre.X + B * Heart.Centre.Y + C) / MathF.Sqrt(A * A + B * B);
            }

            float res = Max(Abs(dist) - 4.5f, GetDistance(Heart.Centre, Centre) - Length / 2f - 3.5f);

            int offset = 3 - (int)JudgeState;

            if (colorType == 1 && Heart.IsStable)
                return;
            if (colorType == 2 && Heart.IsMoved)
                return;
            if (PlayerInstance.hpControl.ScoreProtected && PlayerInstance.hpControl.protectTime > 0)
                return;
            if (res < 0)
            {
                if (!hasHit)
                    PushScore(0);
                LoseHP(Heart);
                hasHit = true;
            }
            else if (res <= 1.6f - offset * 0.4f)
            {
                if (score >= 2)
                { score = 1; Player.CreateCollideEffect(Color.LawnGreen, 3f); }
            }
            else if (res <= 4.2f - offset * 1.2f)
            {
                if (score >= 3)
                { score = 2; Player.CreateCollideEffect(Color.LightBlue, 6f); }
            }
            if (score != 3 && needAP && MarkScore)
            {
                if (!hasHit)
                {
                    PushScore(0);
                    LoseHP(Heart);
                    hasHit = true;
                }
            }
        }
        /// <inheritdoc/>
        public override void Dispose()
        {
            if (!hasHit && MarkScore)
                PushScore(score);
            base.Dispose();
        }
        /// <inheritdoc/>
        public Bone()
        {
            drawingColor = GameMain.CurrentDrawingSettings.themeColor;
            UpdateIn120 = true;
            controlingBox = FightBox.instance as RectangleBox;
            if (controlingBox == null)
                throw new NotImplementedException();
        }
    }
}