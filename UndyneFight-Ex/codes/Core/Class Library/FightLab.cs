using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.SongSystem;
using static System.Math;
using static System.MathF;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Fight
{
    public static class AdvanceFunctions
    {
        public static class Interactive
        {
            /// <summary>
            /// Adds an event to be executed when a Miss was gained by the player
            /// </summary>
            /// <param name="action">The action to invoke</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddMissEvent(Action action) => StateShower.instance.MissAction += action;
            /// <summary>
            /// Adds an event to be executed when a Okay was gained by the player
            /// </summary>
            /// <param name="action">The action to invoke</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddOkayEvent(Action action) => StateShower.instance.OkayAction += action;
            /// <summary>
            /// Adds an event to be executed when a Nice was gained by the player
            /// </summary>
            /// <param name="action">The action to invoke</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddNiceEvent(Action action) => StateShower.instance.NiceAction += action;
            /// <summary>
            /// Adds an event to be executed when a Perfect was gained by the player
            /// </summary>
            /// <param name="action">The action to invoke</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddPerfectEvent(Action action) => StateShower.instance.PerfectAction += action;
            /// <summary>
            /// Adds an event to be executed when the chart ends
            /// </summary>
            /// <param name="action">The action to invoke</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AddEndEvent(Action action) => StateShower.instance.EndAction += action;
        }
        /// <summary>
        /// The same as cos(<paramref name="v"/> * PI), cos(0) = cos(1) = 0, cos(0.5) = 1
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos01(float v) => Cos(v * MathF.PI);
        /// <summary>
        /// The same as sin(<paramref name="v"/> * PI), sin(0) = sin(1) = 0, sin(0.5) = 1
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin01(float v) => Sin(v * MathF.PI);
        /// <summary>
        /// Adds a score type, generally used in <see cref="ICollideAble"/> for score check
        /// </summary>
        /// <param name="score">0-> Miss, 1-> Okay, 2-> Nice, 3-> Perfect</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushScore(int score)
        {
            if (StateShower.instance == null)
            {
                if (score == 0 && Functions.PlayerInstance.hpControl.KR)
                    Functions.PlayerInstance.hpControl.GiveKR(1);
                return;
            }
            int actual = score;
            Player.Heart p = null;
            foreach (Player.Heart v in Player.hearts)
            {
                if (v.Shields == null)
                    continue;
                if (v.Shields.OverRotate)
                {
                    p = v;
                    break;
                }
            }
            if (p != null && actual >= 3)
            {
                actual = 2;
                p.Shields.Consume();
            }
            StateShower.instance.PushType(actual);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void PushBonus(float bonus) => StateShower.instance?.PushBonus((int)bonus);
    }
    public static partial class Functions
    {
        /// <summary>
        /// Use this static class to access some attributes of the Heart
        /// </summary>
        public static class HeartAttribute
        {
            /// <summary>
            /// Sets whether the blue soul will have a softer falling (Smoother but longer)
            /// </summary>
            public static bool SoftFalling { set => Heart.SoftFalling = value; get => Heart.SoftFalling; }
            /// <summary>
            /// Determine whether the arrows rotate along the soul
            /// </summary>
            public static bool ArrowFixed { set => Heart.FixArrow = value; get => Heart.FixArrow; }
            /// <summary>
            /// Is the player at full HP
            /// </summary>
            public static bool IsFullHP => PlayerInstance.hpControl.HP == PlayerInstance.hpControl.maxHP;

            /// <summary>
            /// Damage taken by the player per hit
            /// </summary>
            public static int DamageTaken { set => PlayerInstance.hpControl.DamageTaken = value; get => PlayerInstance.hpControl.DamageTaken; }
            /// <summary>
            /// Level of HP drain of the player
            /// </summary>
            public static float BuffedLevel { set => PlayerInstance.hpControl.BuffedLevel = value; get => PlayerInstance.hpControl.BuffedLevel; }
            /// <summary>
            /// The amount of purple lines in purple soul mode
            /// </summary>
            public static int PurpleLineCount { set => Heart.PurpleLineCount = value; }
            /// <summary>
            /// Gravity of the blue soul (Default 9.8f)
            /// </summary>
            public static float Gravity { set => Heart.Gravity = value; }
            /// <summary>
            /// Initial jump speed of blue soul (Default 6)
            /// </summary>
            public static float JumpSpeed { set => Heart.JumpSpeed = value; }
            /// <summary>
            /// Amount of times a player can jump (Default 2)
            /// </summary>
            public static int JumpTimeLimit { set => Heart.JumpTimeLimit = value; }
            /// <summary>
            /// Whether to enable KR
            /// </summary>
            public static bool KR { set => PlayerInstance.hpControl.KR = value; get => PlayerInstance.hpControl.KR; }
            /// <summary>
            /// The damage the KR deals (Default 4)
            /// </summary>
            public static float KRDamage { set => PlayerInstance.hpControl.KRDamage = value; get => PlayerInstance.hpControl.KRDamage; }
            /// <summary>
            /// Speed of the player (Default 2.5f)
            /// </summary>
            public static float Speed { set => Heart.Speed = value; }
            /// <summary>
            /// The max HP of the player (Also automatically sets the current HP of the player)
            /// </summary>
            public static float MaxHP { set => PlayerInstance.hpControl.ResetMaxHP(value); get => PlayerInstance.hpControl.maxHP; }
            /// <summary>
            /// Whether the player is immune to physical damage
            /// </summary>
            public static bool InvincibleToPhysics { get => PlayerInstance.hpControl.InvincibleToPhysic; set => PlayerInstance.hpControl.InvincibleToPhysic = value; }
            /// <summary>
            /// The current HP of the player
            /// </summary>
            public static float HP { set => PlayerInstance.hpControl.HP = value; get => PlayerInstance.hpControl.HP; }
            /// <summary>
            /// The current KR of the player
            /// </summary>
            public static float KRHP { set => PlayerInstance.hpControl.KRHP = value; get => PlayerInstance.hpControl.KRHP; }
            /// <summary>
            /// Whether the blue soul can descend slower by holding spacebar (Default false)
            /// </summary>
            public static bool UmbrellaAvailable { set => Heart.UmbrellaAvailable = value; }
            /// <summary>
            /// The falling speed of the player when descending using umbrella (Default 2/3f)
            /// </summary>
            public static float UmbrellaSpeed { set => Heart.UmbrellaSpeed = value; }
        }
        /// <summary>
        /// Use this static class to access variables of the box
        /// </summary>
        public static class BoxStates
        {
            /// <summary>
            /// The current box you are controlling
            /// </summary>
            public static FightBox CurrentBox => FightBox.instance;
            /// <summary>
            /// The x coordinate of the left side of the box
            /// </summary>
            public static float Left => (FightBox.instance as RectangleBox).Left;
            /// <summary>
            /// The x coordinate of the right side of the box
            /// </summary>
            public static float Right => (FightBox.instance as RectangleBox).Right;
            /// <summary>
            /// The y coordinate of the upper side of the box
            /// </summary>
            public static float Up { get => (FightBox.instance as RectangleBox).Up; set => (FightBox.instance as RectangleBox).Up = value; }
            /// <summary>
            /// The y coordinate of the lower side of the box
            /// </summary>
            public static float Down { get => (FightBox.instance as RectangleBox).Down; set => (FightBox.instance as RectangleBox).Down = value; }
            /// <summary>
            /// The center of the box
            /// </summary>
            public static Vector2 Centre
            {
                get => (FightBox.instance as RectangleBox).Centre; set
                {
                    CollideRect area = (FightBox.instance as RectangleBox).CollidingBox;
                    area.SetCentre(value);
                    (FightBox.instance as RectangleBox).InstanceMove(area);
                }
            }
            /// <summary>
            /// The width of the box
            /// </summary>
            public static float Width => (FightBox.instance as RectangleBox).CollidingBox.Width;
            /// <summary>
            /// The height of the box
            /// </summary>
            public static float Height
            {
                get => (FightBox.instance as RectangleBox).CollidingBox.Height;
                set
                {
                    CollideRect old = (FightBox.instance as RectangleBox).CollidingBox;
                    old.Height = value;
                    (FightBox.instance as RectangleBox).InstanceMove(old);
                }
            }
            /// <summary>
            /// Lerp value of box movement, range is [0, 1]
            /// </summary>
            public static float BoxMovingScale { get => FightBox.instance.MovingScale; set => FightBox.instance.MovingScale = value; }
        }
        /// <summary>
        /// The content loader, you can use this to load dynamic assets
        /// </summary>
        public static ContentManager Loader => Scene.Loader;
        /// <summary>
        /// The cover of the current chart (If any)
        /// </summary>
        public static Texture2D SongIllustration => (CurrentScene as SongFightingScene).SongIllustration;

        /// <summary>
        /// Whether the chart automatically switch to the result screen after ending
        /// </summary>
        public static bool AutoEnd { set => (CurrentScene as SongFightingScene).AutoEnd = value; get => (CurrentScene as SongFightingScene).AutoEnd; }
        /// <summary>
        /// When the song will begin playing (If the value is negative, <see cref="SongInformation.MusicOptimized"/> MUST be false)
        /// </summary>
        public static float PlayOffset { set => (CurrentScene as SongFightingScene).PlayOffset = value; }
        /// <summary>
        /// Gametime displacement of the chart (Initialization only)
        /// </summary>
        public static float GametimeDelta { get; set; } = 0;
        /// <summary>
        /// Frames elapsed in integers (Readonly, Not recommended to use)
        /// </summary>
        public static int Gametime => (int)(GameMain.gameTime + GametimeDelta);
        /// <summary>
        /// Frames elapsed in float (Readonly, Recommended)
        /// </summary>
        public static float GametimeF => GameMain.gameTime + GametimeDelta;
        /// <summary>
        /// The current difficulty of the chart
        /// </summary>
        public static Difficulty CurrentDifficulty => (CurrentScene as SongFightingScene).CurrentDifficulty;
        /// <summary>
        /// The soul you are currently controlling
        /// </summary>
        public static Player.Heart Heart => Player.heartInstance;
        /// <summary>
        /// The player you are currently controlling
        /// </summary>
        public static Player PlayerInstance => (CurrentScene as FightScene).PlayerInstance;
        /// <summary>
        /// Sets which player you are currently controlling
        /// </summary>
        /// <param name="val">The ID of the player (Default 0)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerMission(int val)
        {
            if (Player.hearts.Count <= val)
                return;

            Player.heartInstance = Player.hearts[val];
        }
        /// <summary>
        /// Sets which player you are currently controlling
        /// </summary>
        /// <param name="p">The player to control</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerMission(Player.Heart p) => Player.heartInstance = p;
        /// <summary>
        /// Sets which player and box you are currently controlling
        /// </summary>
        /// <param name="p">The player to control</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerBoxMission(Player.Heart p)
        {
            Player.heartInstance = p;
            SetBoxMission(p.controlingBox);
        }
        /// <summary>
        /// Sets which player and box you are currently controlling
        /// </summary>
        /// <param name="val">The ID of the player and box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayerBoxMission(int val)
        {
            SetBoxMission(val);
            SetPlayerMission(val);
        }
        /// <summary>
        /// Sets which box you are currently controlling
        /// </summary>
        /// <param name="val">The ID of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBoxMission(int val)
        {
            if (FightBox.boxes.Count <= val)
                return;

            FightBox.instance = FightBox.boxes[val];
        }
        /// <summary>
        /// Sets the box you are currently controlling
        /// </summary>
        /// <param name="box">The ID of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBoxMission(FightBox box) => FightBox.instance = box;
        /// <summary>
        /// Moves the player to the target location
        /// </summary>
        /// <param name="vect">The target position</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TP(Vector2 vect) => Player.heartInstance.Teleport(vect);
        /// <summary>
        /// Moves the player to the target position
        /// </summary>
        /// <param name="x">The x-coordinate to teleport to</param>
        /// <param name="y">The y-coordinate to teleport to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TP(float x = 320, float y = 240) => TP(new Vector2(x, y));
        /// <summary>
        /// Moves the player to the target position
        /// </summary>
        /// <param name="vec">The vector location to teleport the box to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InstantTP(Vector2 vec) => Player.heartInstance.InstantTP(vec);
        /// <summary>
        /// Moves the player to the target position
        /// </summary>
        /// <param name="x">The x-coordinate to teleport to</param>
        /// <param name="y">The y-coordinate to teleport to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InstantTP(float x, float y) => InstantTP(new Vector2(x, y));
        /// <summary>
        /// Recovers HP
        /// </summary>
        /// <param name="HP">The amount of HP to recover</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Regenerate(int HP) => PlayerInstance.hpControl.Regenerate(HP);
        /// <summary>
        /// Recovers full HP
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Regenerate() => PlayerInstance.hpControl.Regenerate();
        /// <summary>
        /// Reduces HP from the given heart
        /// </summary>
        /// <param name="heart">The heart to apply the damage to (For particle effect)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoseHP(Player.Heart heart) => PlayerInstance.hpControl.LoseHP(heart);
        /// <summary>
        /// Applies KR damage to the player
        /// </summary>
        /// <param name="scale">The scale of the KR damage (X times <see cref="HeartAttribute.KRDamage"/>)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GiveKR(float scale) => PlayerInstance.hpControl.GiveKR(scale);
        /// <summary>
        /// Gets all the objects of the given type with the given tag
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <param name="tag">The tag to contain</param>
        /// <returns>The array of objects of the type <typeparamref name="T"/> that contains the <paramref name="tag"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetAll<T>(string tag) where T : GameObject => [.. (from x in Objects where x.ContainTag(tag) select x).OfType<T>()];
        /// <summary>
        /// Gets all the objects of the given type
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>The array of objects of the type <typeparamref name="T"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetAll<T>() where T : GameObject => [.. Objects.OfType<T>()];

        /// <summary>
        /// Change the soul state
        /// </summary>
        /// <param name="type">0: Red, 1: Green, 2: Blue, 3: Orange, 4: Purple, 5: Gray</param>
        /// <param name="resetGravSpd">Whether the gravity speed will be reset to 0 to prevent bugs (Default false)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSoul(int type, bool resetGravSpd = false) => Player.heartInstance.ChangeColor(type, resetGravSpd);
        /// <summary>
        /// Change the soul state of the player
        /// </summary> 
        /// <param name="state">The moving state of the player (Mostly for custom souls)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSoul(Player.MoveState state) => Player.heartInstance.ChangeState(state);

        /// <summary>
        /// Attributes of an arrow
        /// </summary>
        [Flags]
        public enum ArrowAttribute
        {
            /// <summary>
            /// Normal arrow
            /// </summary>
            None = 0,
            /// <summary>
            /// Speeds up mid-way
            /// </summary>
            SpeedUp = 1,
            /// <summary>
            /// Rotates clockwise during movement
            /// </summary>
            RotateR = 2,
            /// <summary>
            /// Rotates counterclockwise during movement
            /// </summary>
            RotateL = 4,
            /// <summary>
            /// Hold arrow judgement
            /// </summary>
            Hold = 8,
            /// <summary>
            /// Tap arrow judgement
            /// </summary>
            Tap = 16,
            /// <summary>
            /// Void arrow drawing
            /// </summary>
            Void = 32,
            /// <summary>
            /// No score given
            /// </summary>
            NoScore = 64,
            /// <summary>
            /// Force display as green arrow
            /// </summary>
            ForceGreen = 128,
            /// <summary>
            /// No golden tag when drawn
            /// </summary>
            NoGoldTag = 256,
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GiveAttribute(Arrow arr, ArrowAttribute attribute)
        {
            if ((attribute & ArrowAttribute.SpeedUp) == ArrowAttribute.SpeedUp)
                arr.IsSpeedup = true;

            if ((attribute & ArrowAttribute.RotateR) == ArrowAttribute.RotateR)
                arr.IsRotate = true;

            if ((attribute & ArrowAttribute.RotateL) == ArrowAttribute.RotateL)
            {
                arr.IsRotate = true;
                arr.RotateScale = -1f;
            }

            if ((attribute & ArrowAttribute.Tap) == ArrowAttribute.Tap)
                arr.JudgeType = Arrow.JudgementType.Tap;
            if ((attribute & ArrowAttribute.Hold) == ArrowAttribute.Hold)
                arr.JudgeType = Arrow.JudgementType.Hold;
            if ((attribute & ArrowAttribute.Void) == ArrowAttribute.Void)
                arr.VoidMode = true;

            if ((attribute & ArrowAttribute.NoScore) == ArrowAttribute.NoScore)
                arr.NoScore = true;
            if ((attribute & ArrowAttribute.ForceGreen) == ArrowAttribute.ForceGreen)
                arr.ForceGreenBack = true;
            if ((attribute & ArrowAttribute.NoGoldTag) == ArrowAttribute.NoGoldTag)
                arr.EnableGoldMark = false;
        }

        /// <summary>
        /// Creates an arrow
        /// </summary>
        /// <param name="shootShieldTime">Time taken to reach the shield</param>
        /// <param name="way">Direction of arrow, 0-> Right, 1-> Down, 2-> Left, 3-> Up</param>
        /// <param name="speed">Speed of arrow</param>
        /// <param name="color">Color of arrow, 0-> Blue, 1-> Red</param>
        /// <param name="rotatingType">Rotation Type, 0-> None, 1-> Reverse, 2-> Diagonal</param>
        /// <param name="attribute">Arrow attributes (Default <see cref="ArrowAttribute.None"/>)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateArrow(float shootShieldTime, int way, float speed, int color, int rotatingType, ArrowAttribute attribute = ArrowAttribute.None)
        {
            Arrow arr = new(Heart, shootShieldTime + GametimeF, (way + 16) % 4, speed, color, rotatingType);
            GiveAttribute(arr, attribute);
            InstanceCreate(arr);
        }
        /// <summary>
        /// Creates an arrow type but DOES NOT create it to the current game
        /// </summary>
        /// <param name="shootShieldTime">Time taken to reach the shield</param>
        /// <param name="way">Direction of arrow, 0-> Right, 1-> Down, 2-> Left, 3-> Up</param>
        /// <param name="speed">Speed of arrow</param>
        /// <param name="color">Color of arrow, 0-> Blue, 1-> Red</param>
        /// <param name="rotatingType">Rotation Type, 0-> None, 1-> Reverse, 2-> Diagonal</param>
        /// <param name="attribute">Arrow attributes (Default <see cref="ArrowAttribute.None"/>)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Arrow MakeArrow(float shootShieldTime, int way, float speed, int color, int rotatingType, ArrowAttribute attribute = ArrowAttribute.None)
        {
            Arrow arr = new(Heart, shootShieldTime + GametimeF, Posmod(way, 4), speed, color, rotatingType);
            if ((attribute & ArrowAttribute.SpeedUp) == ArrowAttribute.SpeedUp)
                arr.IsSpeedup = true;

            GiveAttribute(arr, attribute);
            return arr;
        }
        private static int lastArrow;
        private static int[] colorLastArrow = new int[10];
        /// <summary>
        /// The allocated direction for arrows, use 'A' to access
        /// </summary>
        public static int[] DirectionAllocate { get; set; } = new int[10];

        public static HashSet<char> OneElementArrows { get; set; } = [];

        public static Func<char, int> CustomAnalyzer { private get; set; } = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetWayFromTag(string wayTag)
        {
            int cur;
            int color = 0;
            if (wayTag.Length > 1)
            {
                if (OneElementArrows.Contains(wayTag[0]))
                {
                    if (wayTag.Length >= 2)
                    {
                        color = wayTag[1] == ' ' && wayTag.Length >= 3 ? MathUtil.Clamp(0, wayTag[2] - '0', 9) : MathUtil.Clamp(0, wayTag[1] - '0', 9);
                    }
                }
                else if (wayTag.Length >= 3)
                    color = MathUtil.Clamp(0, wayTag[2] - '0', 9);
            }
            switch (wayTag[0])
            {
                case 'R':
                    return colorLastArrow[color] = lastArrow = Rand(0, 3);
                case 'D':
                case 'd':
                    cur = Rand(0, 3);
                    while (colorLastArrow[color] == cur)
                        cur = Rand(0, 3);
                    return colorLastArrow[color] = lastArrow = cur;
                case 'N':
                case 'n':
                    int none = wayTag[1] - '0';
                    cur = Rand(0, 3);
                    while (cur == none)
                        cur = Rand(0, 3);
                    return colorLastArrow[color] = lastArrow = cur;
                case '+':
                    return colorLastArrow[color] = lastArrow += wayTag[1] - '0';
                case '-':
                    return colorLastArrow[color] = lastArrow -= wayTag[1] - '0';
                case '$':
                    return colorLastArrow[color] = lastArrow = wayTag[1] - '0';
                case 'A':
                    return colorLastArrow[color] = lastArrow = DirectionAllocate[wayTag[1] - '0'];
                case 'C':
                    return colorLastArrow[color] = lastArrow = CustomAnalyzer(wayTag[1]);
                default:
                    return colorLastArrow[color] = lastArrow = wayTag[0] - '0';
            }
        }
        /// <summary>
        /// Creates an arrow with a string as it's tag
        /// </summary>
        /// <param name="shootShieldTime">Time taken to reach the shield</param>
        /// <param name="wayTag">String tag of the way of the arrow</param>
        /// <param name="speed">Moving speed of the arrow</param>
        /// <param name="color">The color of the arrow, 0: Blue, 1: Red, 2: Green, 3: Purple</param>
        /// <param name="rotatingType">Rotation mode, 0: Normal, 1: Reverse, 2: Diagonal</param> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateArrow(float shootShieldTime, string wayTag, float speed, int color, int rotatingType) => CreateArrow(shootShieldTime, GetWayFromTag(wayTag), speed, color, rotatingType);
        /// <summary>
        /// Creates an arrow with a string as it's tag
        /// </summary>
        /// <param name="shootShieldTime">Time taken to reach the shield</param>
        /// <param name="wayTag">String tag of the way of the arrow</param>
        /// <param name="speed">Moving speed of the arrow</param>
        /// <param name="color">The color of the arrow, 0: Blue, 1: Red, 2: Green, 3: Purple</param>
        /// <param name="rotatingType">Rotation mode, 0: Normal, 1: Reverse, 2: Diagonal</param> 
        /// <returns>The created <see cref="Arrow"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Arrow MakeArrow(float shootShieldTime, string wayTag, float speed, int color, int rotatingType) => MakeArrow(shootShieldTime, GetWayFromTag(wayTag), speed, color, rotatingType);
        /// <summary>
        /// Creates an arrow with a string as it's tag
        /// </summary>
        /// <param name="shootShieldTime">Time taken to reach the shield</param>
        /// <param name="wayTag">String tag of the way of the arrow</param>
        /// <param name="speed">Moving speed of the arrow</param>
        /// <param name="color">The color of the arrow, 0: Blue, 1: Red, 2: Green, 3: Purple</param>
        /// <param name="rotatingType">Rotation mode, 0: Normal, 1: Reverse, 2: Diagonal</param>
        /// <param name="arrowattribute">The arrow attribute of the arrow, use <see cref="ArrowAttribute"/>, to combine multiple, use 'attr1 | attr2'</param>
        /// <returns>The created <see cref="Arrow"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Arrow MakeArrow(float shootShieldTime, string wayTag, float speed, int color, int rotatingType, ArrowAttribute arrowattribute) => MakeArrow(shootShieldTime, GetWayFromTag(wayTag), speed, color, rotatingType, arrowattribute);
        /// <summary>
        /// Creates an arrow with a string as it's tag
        /// </summary>
        /// <param name="shootShieldTime">Time taken to reach the shield</param>
        /// <param name="wayTag">String tag of the way of the arrow</param>
        /// <param name="speed">Moving speed of the arrow</param>
        /// <param name="color">The color of the arrow, 0: Blue, 1: Red, 2: Green, 3: Purple</param>
        /// <param name="rotatingType">Rotation mode, 0: Normal, 1: Reverse, 2: Diagonal</param>
        /// <param name="attribute">The arrow attribute of the arrow, use <see cref="ArrowAttribute"/>, to combine multiple, use 'attr1 | attr2'</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateArrow(float shootShieldTime, string wayTag, float speed, int color, int rotatingType, ArrowAttribute attribute) => CreateArrow(shootShieldTime, GetWayFromTag(wayTag), speed, color, rotatingType, attribute);

        /// <summary>
        /// Create a spear
        /// </summary>
        /// <param name="spear">The spear to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateSpear(Spear spear) => InstanceCreate(spear);
        /// <summary>
        /// Create a bone
        /// </summary>
        /// <param name="bone">The bone to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateBone(Bone bone) => InstanceCreate(bone);
        /// <summary>
        /// Create a blaster
        /// </summary>
        /// <param name="gb">The blaster to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateGB(GasterBlaster gb) => InstanceCreate(gb);
        /// <summary>
        /// Create a platform
        /// </summary>
        /// <param name="plt">The platform to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePlatform(Platform plt) => InstanceCreate(plt);

        /// <summary>
        /// Create an <see cref="Entity"/>
        /// </summary>
        /// <param name="et">The entity to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateEntity(Entity et) => InstanceCreate(et);
        /// <summary>
        /// Create multiple <see cref="Entity"/>
        /// </summary>
        /// <param name="et">The entities to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateEntity(params Entity[] et)
        {
            foreach (Entity en in et)
                InstanceCreate(en);
        }
        /// <summary>
        /// Create an instance of <see cref="GameObject"/>
        /// </summary>
        /// <param name="go">The object to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInstance(GameObject go) => InstanceCreate(go);
        /// <summary>
        /// Create instances of <see cref="GameObject"/>
        /// </summary>
        /// <param name="go">The objects to create</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInstances(params GameObject[] go) => Array.ForEach(go, InstanceCreate);
        /// <summary>
        /// Applies behavior to all arrows marked with the tag with the given <see cref="Action"/>
        /// </summary>
        /// <param name="tag">The tag of the arrows to apply to</param>
        /// <param name="action">The action applied to the arrows</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArrowApply(string tag, Action<Arrow> action)
        {
            if (CurrentScene is SongFightingScene)
                AddInstance(new InstantEvent(1.2f, () =>
                {
                    Dictionary<string, List<Arrow>> map = (CurrentScene as SongFightingScene).Accuracy.TaggedArrows;
                    if (!map.TryGetValue(tag, out List<Arrow> value))
                        return;
                    value.ForEach(s => action(s));
                }));
        }
        public static class ArrowEase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void UnitRotation(string tag, EaseUnit<float> rotationEase)
            {
                if (CurrentScene is SongFightingScene)
                {
                    Arrow.UnitEasing ease = new()
                    {
                        ApplyTime = rotationEase.Time,
                        RotationEase = rotationEase
                    };
                    ease.TagApply(tag);
                    AddInstance(ease);
                    ease.AutoDispose = true;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void UnitDistance(string tag, EaseUnit<float> distanceEase)
            {
                if (CurrentScene is SongFightingScene)
                {
                    Arrow.UnitEasing ease = new()
                    {
                        ApplyTime = distanceEase.Time,
                        DistanceEase = distanceEase
                    };
                    ease.TagApply(tag);
                    AddInstance(ease);
                    ease.AutoDispose = true;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void UnitPosition(string tag, EaseUnit<Vector2> positionEase)
            {
                if (CurrentScene is SongFightingScene)
                {
                    Arrow.UnitEasing ease = new()
                    {
                        ApplyTime = positionEase.Time,
                        PositionEase = positionEase
                    };
                    ease.TagApply(tag);
                    AddInstance(ease);
                    ease.AutoDispose = true;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void UnitAlpha(string tag, EaseUnit<float> alphaEase)
            {
                if (CurrentScene is SongFightingScene)
                {
                    Arrow.UnitEasing ease = new()
                    {
                        ApplyTime = alphaEase.Time,
                        AlphaEase = alphaEase
                    };
                    ease.TagApply(tag);
                    AddInstance(ease);
                    ease.AutoDispose = true;
                }
            }
        }
        /// <summary>
        /// Lerps the position of the box to the given location
        /// </summary>
        /// <param name="x1">x-coordinate of the left side of the box</param>
        /// <param name="x2">x-coordinate of the right side of the box</param>
        /// <param name="y1">y-coordinate of the top side of the box</param>
        /// <param name="y2">y-coordinate of the bottom side of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBox(float x1, float x2, float y1, float y2)
        {
            if (FightBox.instance is RectangleBox)
                (FightBox.instance as RectangleBox).MoveTo(new CollideRect(x1, y1, x2 - x1, y2 - y1));
            else
                throw new NotImplementedException();
        }
        /// <summary>
        /// Lerps the position of the box to the given location
        /// </summary>
        /// <param name="centre">The centre of the box to move to</param>
        /// <param name="width">The width of the box</param>
        /// <param name="height">The height of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBox(Vector2 centre, float width, float height)
        {
            if (FightBox.instance is RectangleBox)
                (FightBox.instance as RectangleBox).MoveTo(new CollideRect(centre - new Vector2(width, height) / 2, new(width, height)));
            else
                throw new NotImplementedException();
        }
        /// <summary>
        /// Lerps the position of the box to the given location (X is forced to be 320)
        /// </summary>
        /// <param name="YCentre">The y-coordinate of the box to move to</param>
        /// <param name="width">The width of the box</param>
        /// <param name="height">The height of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBox(float YCentre, float width, float height) => SetBox(320 - width / 2, 320 + width / 2, YCentre - height / 2, YCentre + height / 2);
        /// <summary>
        /// Sets the position of the box instantly
        /// </summary>
        /// <param name="centre">The centre of the box to move to</param>
        /// <param name="width">The width of the box</param>
        /// <param name="height">The height of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InstantSetBox(Vector2 centre, float width, float height)
        {
            if (FightBox.instance is RectangleBox)
                (FightBox.instance as RectangleBox).InstanceMove(new CollideRect(centre - new Vector2(width, height) / 2, new(width, height)));
            else
                throw new NotImplementedException();
        }
        /// <summary>
        /// Sets the position of the box instantly
        /// </summary>
        /// <param name="x1">x-coordinate of the left side of the box</param>
        /// <param name="x2">x-coordinate of the right side of the box</param>
        /// <param name="y1">y-coordinate of the top side of the box</param>
        /// <param name="y2">y-coordinate of the bottom side of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InstantSetBox(float x1, float x2, float y1, float y2)
        {
            if (FightBox.instance is RectangleBox)
                (FightBox.instance as RectangleBox).InstanceMove(new CollideRect(x1, y1, x2 - x1, y2 - y1));
            else
                throw new NotImplementedException();
        }
        /// <summary>
        /// Sets the position of the box instantly
        /// </summary>
        /// <param name="YCentre">The y-coordinate of the box to move to</param>
        /// <param name="width">The width of the box</param>
        /// <param name="height">The height of the box</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InstantSetBox(float YCentre, float width, float height) => InstantSetBox(320 - width / 2, 320 + width / 2, YCentre - height / 2, YCentre + height / 2);
        /// <summary>
        /// Lerps the box to the position and size for green soul
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetGreenBox() => SetBox(320 - 42, 320 + 42, 240 - 42, 240 + 42);
        /// <summary>
        /// Sets the box to the position and size for green soul instantly
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InstantSetGreenBox() => InstantSetBox(320 - 42, 320 + 42, 240 - 42, 240 + 42);
        /// <summary>
        /// Carry an action with the given times
        /// </summary>
        /// <param name="times">Times to invoke the action with</param>
        /// <param name="action">The <see cref="Action"/> to invoke</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fortimes(int times, Action action)
        {
            for (int i = 1; i <= times; i++)
                action.Invoke();
        }
        /// <summary>
        /// Carry an action with the given times
        /// </summary>
        /// <param name="times">Times to invoke the action with</param>
        /// <param name="action">The <see cref="Action"/> to invoke, with an integer parameter</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fortimes(int times, Action<int> action)
        {
            for (int i = 0; i < times; i++)
                action.Invoke(i);
        }

        /// <summary>
        /// Creates a random integer
        /// </summary>
        /// <param name="s">The lower bound of the integer</param>
        /// <param name="e">The upper bound of the integer</param>
        /// <returns>The random integer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Rand(int s, int e) => LastRand = GetRandom(s, e);
        /// <summary>
        /// Creates a random <see cref="float"/>
        /// </summary>
        /// <param name="s">The lower bound of the float</param>
        /// <param name="e">The upper bound of the float</param>
        /// <returns>The random float</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Rand(float s, float e) => LastRandFloat = GetRandom(s, e);
        /// <summary>
        /// Gets a random sign
        /// </summary>
        /// <returns>1 or -1</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RandSignal() => LastRand = RandBool() ? 1 : -1;
        /// <summary>
        /// Returns a random boolean
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RandBool() => (LastRand = GetRandom(0, 1)) == 0;

        /// <summary>
        /// Returns the tangent of the specified angle in radians
        /// </summary>
        /// <param name="rot">The angle (In radians)</param>
        /// <returns>The tangent value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan(float rot) => MathF.Tan(GetRadian(rot));
        /// <summary>
        /// Returns the cosine of the specified angles in radians
        /// </summary>
        /// <param name="rot">The angle (In radians)</param>
        /// <returns>The cosine value, range: [-1, 1] and <see cref="float.NaN"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float rot) => MathF.Cos(GetRadian(rot));
        /// <summary>
        /// Returns the sine value of the angle in radians
        /// </summary>
        /// <param name="rot">The angle in radians</param>
        /// <returns>The sin value, range: [-1, 1] and <see cref="float.NaN"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float rot) => MathF.Sin(GetRadian(rot));

        /// <summary>
        /// The last random integer generated
        /// </summary>
        public static int LastRand { get; set; }
        /// <summary>
        /// The last random float generated
        /// </summary>
        public static float LastRandFloat { get; set; }

        /// <summary>
        /// Play a sound effect, you should play multiple sounds for volume that is larger than 1
        /// </summary>
        /// <param name="effect">The sfx to play</param>
        /// <param name="soundVolume">The volume of the sound, range: [0, 1]</param>
        public static void PlaySound(SoundEffect effect, float soundVolume = 1)
        {
            if (soundVolume > 1)
                throw new Exception("Sound volume cannot exceed 1");
            float trueVal = soundVolume * soundVolume;
            if (effect != FightResources.Sounds.ArrowStuck)
                trueVal *= Settings.SettingsManager.DataLibrary.SFXVolume / 100f;
            SoundEffectInstance v = effect.CreateInstance();
            v.Volume = trueVal;
            v.Play();
        }
        /// <summary>
        /// Play multiple sound effects
        /// </summary>
        /// <param name="effect"></param>
        public static void PlaySound(params SoundEffect[] effect) => Array.ForEach(effect, (s) => PlaySound(s));
        /// <summary>
        /// Creates a black screen
        /// </summary>
        /// <param name="time">The duration of the black screen</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlackScreen(float time)
        {
            PlaySound(FightResources.Sounds.change);
            (CurrentScene as FightScene).stopTime += time;
        }

        /// <summary>
        /// Kills all barrages on screen (Does not invoke their .Dispose() event)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetBarrage() => Objects.ForEach(s => { if (s is Barrage) s.Kill(); });
        /// <summary>
        /// End the current song
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndSong() => (CurrentScene as SongFightingScene).ForceEnd();
        /// <summary>
        /// Creates a heart with the new box in the given position
        /// </summary>
        /// <param name="startingBoxPos">The position of the box of the new heart to create</param>
        /// <returns>The created heart</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Player.Heart CreateHeart(CollideRect startingBoxPos) => Heart.InstantSplit(startingBoxPos);
        /// <summary>
        /// Creates a heart with the new box in the given position
        /// </summary>
        /// <param name="yCentre">THe y-coordinate of the box to create</param>
        /// <param name="width">The width of the box to create</param>
        /// <param name="height">The height of the box to create</param>
        /// <returns>The created heart</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Player.Heart CreateHeart(float yCentre, float width, float height) => Heart.InstantSplit(new(new Vector2(320 - width / 2, yCentre - height / 2), new(width, height)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reset()
        {
            GametimeDelta = 0;
            CustomAnalyzer = null;
            colorLastArrow = new int[10];
            DirectionAllocate = new int[10];
        }
    }
}