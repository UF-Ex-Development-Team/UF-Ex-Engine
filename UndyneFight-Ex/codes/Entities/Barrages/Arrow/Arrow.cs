using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;
using static UndyneFight_Ex.Settings.SettingsManager.DataLibrary;

namespace UndyneFight_Ex.Entities
{
    public partial class Arrow : Entity, IComparable
    {
        private const float speedUpPlace = 104;
        private float DrawingScale => GoldenMarkIntensity * 0.1f + 1;
        /// <summary>
        /// The rotating type of the arrow (None, Reverse, Diagonal)
        /// </summary>
        public int RotateType => rotatingType;
        /// <summary>
        /// Whether the sprite of the arrow is a void arrow
        /// </summary>
        public bool VoidMode { get; set; } = false;
        /// <summary>
        /// The volume of the arrow when blocked
        /// </summary>
        public float VolumeFactor { get; internal set; } = 1.0f;
        /// <summary>
        /// Whether the score of the arrow won't be marked
        /// </summary>
        public bool NoScore { get; set; } = false;
        private readonly float settingDelay;
        /// <summary>
        /// Creates an arrow
        /// </summary>
        /// <param name="mission">The heart to aim the arrow with</param>
        /// <param name="shootShieldTime">Time taken to reach the shield</param>
        /// <param name="way">Direction of arrow, 0-> Right, 1-> Down, 2-> Left, 3-> Right</param>
        /// <param name="speed">Speed of arrow</param>
        /// <param name="color">The color of the arrow, 0-> Blue, 1-> Red, 2-> Green, 3-> Purple</param>
        /// <param name="rotatingType">Rotation mode, 0-> None, 1-> Reverse, 2-> Diagonal</param>
        public Arrow(Player.Heart mission, float shootShieldTime, int way, float speed, int color, int rotatingType)
        {
            if (color is < 0 or >= 4)
                throw new ArgumentOutOfRangeException(nameof(color));
            if (Mirror)
                color ^= 1;
            basicScale = ArrowScale;

            Init();
            Centre = new(-50000);
            UpdateIn120 = true;
            arrows.Add(this);

            BlockTime = shootShieldTime + (settingDelay = ArrowDelay / 16);
            Speed = speed * ArrowSpeed;
            this.way = way;
            ArrowColor = color;
            this.rotatingType = rotatingType % 2;
            backColor = rotatingType;
            hasGreenFlag = rotatingType == 2;
            missionRotation = this.way * 90f;
            this.mission = mission;
        }
        private Player.Heart mission;
        /// <summary>
        /// The target heart of the arrow
        /// </summary>
        public Player.Heart Mission
        {
            get => mission;
            set => mission = value;
        }
        public override void Start()
        {
            base.Start();
            if (HasTag())
                foreach (string str in Tags)
                {
                    if (!taggedArrows.TryAdd(str, [this]))
                        taggedArrows[str].Add(this);
                }
        }

        /// <summary>
        /// 距离玩家灵魂的距离
        /// </summary>
        private float distance;
        private readonly bool hasGreenFlag;
        private readonly float basicScale;
        /// <summary>
        /// The color type of the arrow
        /// </summary>
        public int ArrowColor { get; private set; }
        private int backColor;
        private readonly int rotatingType, way;
        /// <summary>
        /// The direction of the arrow (Right, Down, Left, Up)
        /// </summary>
        public int Way => way;

        /// <summary>
        /// The rotation with respect to the target
        /// </summary>
        public float CentreRotationOffset { get; set; } = 0;
        /// <summary>
        /// The rotation of the arrow itself
        /// </summary>
        public float SelfRotationOffset { get; set; } = 0;
        /// <summary>
        /// The speed of the arrow
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// The alpha of the arrow
        /// </summary>
        public float Alpha { private get; set; } = 1;
        private readonly float missionRotation;
        private bool isSpeedUp = false, isRotate = false;
        private float TimeDelta => BlockTime - GametimeF;
        internal bool IsSpeedup { set => isSpeedUp = value; }
        internal bool IsRotate { set => isRotate = value; }
        internal float RotateScale { get; set; } = 1.0f;
        internal int GoldenMarkIntensity { private get; set; }
        internal bool EnableGoldMark { private get; set; } = true;
        internal bool ForceGreenBack { private get; set; } = false;

        public override void Draw()
        {
            //Tap -> Green outline, else no outline
            if (JudgeType != JudgementType.Tap && !taggedArrows.ContainsKey("Tap") && backColor == 2)
                backColor = 0;
            if (ForceGreenBack)
                backColor = 2;
            Image = VoidMode ? Sprites.voidarrow[ArrowColor] : Sprites.arrow[ArrowColor, backColor, 0];
            Depth = 0.5f - ArrowColor / 200f;
            FormalDraw(Image, Centre, new Color(0.98f, 0.98f, 0.98f, ArrowColor == 1 ? 0.75f : 0.25f) * Alpha, DrawingScale * Scale, GetRadian(Rotation + additiveRotation + SelfRotationOffset), ImageCentre);

            if (GoldenMarkIntensity > 0 && EnableGoldMark)
            {
                Depth += 0.02f;
                FormalDraw(brimTex, Centre, Color.White * 0.5f * Alpha, DrawingScale * Scale, GetRadian(Rotation + additiveRotation + SelfRotationOffset), brimDisplace);
            }
        }
        private static readonly Texture2D brimTex = Sprites.goldenBrim;
        private static readonly vec2 brimDisplace = new(brimTex.Width / 2, brimTex.Height / 2);
        public override void Update()
        {
            Vector4 extend = CurrentScene.CurrentDrawingSettings.Extending;
            float max = MathF.Max(MathF.Max(extend.X, extend.Y), MathF.Max(extend.Z, extend.W));
            if (Speed * (BlockTime - GametimeF) + additiveDistance > 640 * (max + 1.5f))
                return;
            PositionCalculate();
            AppearTime += 0.5f;
            if (BlockTime - GametimeF < 15)
                CheckCollide();
        }
        /// <summary>
        /// Sets the color of the arrow
        /// </summary>
        /// <param name="color">The color to set to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetColor(int color)
        {
            Image = Sprites.arrow[ArrowColor = color, rotatingType, 0];
            CreateShinyEffect(Color.White).DarkerSpeed = 6.4f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object obj)
        {
            int res = BlockTime.CompareTo((obj as Arrow).BlockTime);
            if (MathF.Abs((obj as Arrow).BlockTime - BlockTime) < 0.35f)
                res = 0;
            return res != 0 ? res : way.CompareTo((obj as Arrow).way);
        }

        private static List<Arrow> arrows => (GameStates.CurrentScene as SongFightingScene).Accuracy.AllArrows;
        private static Dictionary<string, List<Arrow>> taggedArrows => (GameStates.CurrentScene as SongFightingScene).Accuracy.TaggedArrows;
        /// <summary>
        /// The frames elapsed after creation
        /// </summary>
        public float AppearTime { get; private set; } = 0;
    }
}