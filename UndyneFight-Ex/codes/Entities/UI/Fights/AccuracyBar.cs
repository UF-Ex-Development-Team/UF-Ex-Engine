using static UndyneFight_Ex.FightResources;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// The accuracy bar at the bottom of the screen
    /// </summary>
    public class AccuracyBar : Entity
    {
        internal class AccuracyPointer : Entity
        {
            private Color drawingColor;
            private readonly float timeDel;
            private float alpha = 4;
            private readonly int areaBelong;
            public AccuracyPointer(float timedel, int remark)
            {
                UpdateIn120 = true;
                if (remark == 0)
                    areaBelong = -1;
                drawingColor = remark switch
                {
                    0 => Color.Silver,
                    1 => Color.LawnGreen,
                    2 => Color.CadetBlue,
                    3 => Color.Gold,
                    4 => Color.Orange,
                    5 => Color.Orange,
                    _ => throw new ArgumentOutOfRangeException(nameof(remark))
                };
                timeDel = timedel;
            }
            public override void Start()
            {
                controlLayer = (FatherObject as Entity).controlLayer;
                if (areaBelong == 0)
                    Centre = (FatherObject as Entity).Centre + new Vector2(MathUtil.Clamp(-70, MathUtil.SignedPow(timeDel, 1.3f) * 6f, 70), 0);
                else if (areaBelong == -1)
                    Centre = (FatherObject as Entity).Centre + new Vector2(-75, 0);
            }

            public override void Draw()
            {
                float ra = MathF.Min(alpha, 1) * (Fight.Functions.ScreenDrawing.UIColor.A / 255f);
                Depth = 0.2f;
                FormalDraw(Sprites.accuracyPointers[1], Centre, drawingColor * ra, 0, Sprites.accuracyPointers[1].Bounds.Size.ToVector2() / 2);
                Depth = 0.01f;
                FormalDraw(Sprites.accuracyPointers[0], Centre - new Vector2(3, 0), Color.White * ra, 0, Sprites.accuracyPointers[0].Bounds.Size.ToVector2() / 2);
                FormalDraw(Sprites.accuracyPointers[2], Centre + new Vector2(3, 0), Color.White * ra, 0, Sprites.accuracyPointers[2].Bounds.Size.ToVector2() / 2);
            }

            public override void Update()
            {
                if ((alpha -= 0.009f) < 0)
                    Dispose();
            }
        }
        public AccuracyBar()
        {
            Depth = 0.15f;
            Image = Sprites.accuracyBar;
            Centre = new(320, 582);
            UpdateIn120 = true;
        }

        public override void Draw()
        {
            Depth = 0.15f;
            FormalDraw(Image, Centre, Color.White * (Fight.Functions.ScreenDrawing.UIColor.A / 255f), 0, ImageCentre);
        }

        private int appearTime = 0;
        public override void Update()
        {
            appearTime++;
            Centre = Centre * 0.9f + new Vector2(320, 482) * 0.1f;

            if (appearTime % 4 == 0 && EnabledGolden)
            {
                Arrow[] arrows = [.. AllArrows];
                Array.Sort(arrows);
                for (int i = 0; i < arrows.Length; i++)
                    arrows[i].GoldenMarkIntensity = 0;
                List<Arrow> timeSame = [];
                void add()
                {
                    if (timeSame.Count < 2)
                    {
                        timeSame.Clear();
                        return;
                    }
                    int[] counts = [0, 0, 0, 0];
                    timeSame.ForEach(s => { s.GoldenMarkIntensity = 1; counts[s.Way]++; });
                    for (int i = 0; i < 4; i++)
                    {
                        if (counts[i] <= 1)
                            continue;
                        timeSame.ForEach(s => { if (s.Way == i) s.GoldenMarkIntensity = 2; });
                    }
                    timeSame.Clear();
                }
                for (int i = 0; i < arrows.Length; i++)
                {
                    if (i != 0 && arrows[i].BlockTime - arrows[i - 1].BlockTime > SpecifyTime)
                    {
                        add();
                    }
                    timeSame.Add(arrows[i]);
                }
                add();
            }
        }
        public float SpecifyTime { get; set; } = 0.6f;
        /// <summary>
        /// Whether the golden outline of arrows are enabled
        /// </summary>
        public bool EnabledGolden { get; set; } = true;

        internal List<Arrow> AllArrows { get; init; } = [];
        internal Dictionary<string, List<Arrow>> TaggedArrows { get; init; } = [];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PushDelta(float time, int remark, int col, int way, Player.Heart.ShieldManager shieldManager)
        {
            AddChild(new AccuracyPointer(time, remark));
            shieldManager.ShieldShine(way, col, remark);
        }
    }
}