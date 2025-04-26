using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// Base class of a spear (You should not create this)
    /// </summary>
    public class Spear : Barrage
    {
        /// <summary>
        /// Whether the spear will be drawn exclusively inside the box or not
        /// </summary>
        public bool IsHidden { set => Hidden = value; private protected get => Hidden; }
        private int score = 3;
        protected float alpha = 0;
        /// <summary>
        /// The alpha of the spear
        /// </summary>
        public float Alpha { protected set => alpha = value; get => alpha; }

        private bool hasHit = false;
        /// <summary>
        /// Forces the spear to dispose when offscreen
        /// </summary>
        private bool ForceDispose { set; get; } = false;

        protected bool autoDispose = true;

        protected Color drawingColor = Color.White;
        /// <summary>
        /// The drawing color of the spear
        /// </summary>
        public Color DrawingColor { set => drawingColor = value; }
        /// <inheritdoc/>
        public Spear()
        {
            Depth = 0.5f;
            Image = FightResources.Sprites.spear;
        }
        /// <inheritdoc/>
        public override void Draw() => FormalDraw(Image, Centre, drawingColor * alpha, GetRadian(Rotation), ImageCentre);
        /// <inheritdoc/>
        public override void Dispose()
        {
            if (!hasHit && MarkScore)
                PushScore(score);
            base.Dispose();
        }

        private static CollideRect screen = new(-50, -50, 740, 580);
        /// <inheritdoc/>
        public override void Update()
        {
            controlLayer = IsHidden ? Surface.Hidden : Surface.Normal;
            if (autoDispose)
            {
                bool ins = screen.Contain(Centre);
                if (ins && (!ForceDispose))
                    ForceDispose = true;
                if (ForceDispose && (!ins))
                {
                    if (this is not NormalSpear)
                        Dispose();
                    else
                    {
                        NormalSpear _ = this as NormalSpear;
                        if (_.Rebound && _.ReboundCount > -1)
                        {
                            int Normal = 0;
                            //Left
                            if (Centre.X <= 30)
                                Normal = 270;
                            //Left
                            else if (Centre.X >= 610)
                                Normal = 90;
                            //Top
                            if (Centre.Y <= 30)
                                Normal = 0;
                            //Down
                            else if (Centre.Y >= 450)
                                Normal = 180;

                            Rotation = 2 * Normal - Rotation;
                            _.ReboundCount--;
                        }
                        else
                            Dispose();
                    }
                }
            }
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void GetCollide(Player.Heart heart)
        {
            if (alpha <= 0.9f)
                return;
            float A, B = -1, C, dist;
            if (MathF.Abs((Rotation + 90 + 180) % 180) < 0.01f)
                dist = Centre.X - heart.Centre.X;
            else
            {
                float k = (float)Math.Tan(GetRadian(Rotation));
                A = k;
                C = -A * Centre.X - B * Centre.Y;
                dist = (float)((A * heart.Centre.X + B * heart.Centre.Y + C) / Math.Sqrt(A * A + B * B));
            }

            float res = Math.Max(Math.Abs(dist) - 6.5f, GetDistance(heart.Centre, Centre + GetVector2(12, Rotation)) - 31 + 12);

            if (!hasHit)
            {
                int offset = 3 - (int)JudgeState;
                if (res < 0)
                {
                    //Miss
                    if (!hasHit)
                        PushScore(0);
                    LoseHP(heart);
                    hasHit = true;
                }
                else if (res <= 1.6f - offset * 0.4f)
                {
                    //Okay
                    if (score >= 2)
                    { score = 1; heart.CreateCollideEffect2(Color.LawnGreen, 3f); }
                }
                else if (res <= 3.9f - offset * 1.1f)
                {
                    //Nice
                    if (score >= 3)
                    { score = 2; heart.CreateCollideEffect2(Color.LightBlue, 6f); }
                }
                if (score != 3 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
                {
                    //Perfect Only
                    if (!hasHit)
                        PushScore(0);
                    LoseHP(heart);
                    hasHit = true;
                }
            }
        }
    }
}