using UndyneFight_Ex;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.SongSystem;
using static Extends.DrawingUtil;
using static UndyneFight_Ex.Entities.EasingUtil;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;

namespace Extends
{
    #region Misc. Entities
    /// <summary>
    /// A star (Used in Hopes and Dreams) (Please use something else)
    /// </summary>
    public class Star : Entity, ICollideAble, ICustomMotion
    {
        private class StarShadow : Entity
        {
            public float alpha, rotatespeed, scale;
            public Color color = Color.White;
            public StarShadow(float alpha, float angle, Vector2 center, float rotatespeed, float scale, Color color)
            {
                Image = Sprites.star;
                this.alpha = alpha;
                Rotation = angle;
                Centre = center;
                this.rotatespeed = rotatespeed;
                this.scale = scale;
                this.color = color;
            }
            public override void Draw()
            {
                Depth = 0.74f;
                alpha -= 0.08f;
                FormalDraw(Image, Centre, color * alpha, scale, Rotation * MathF.PI / 180, ImageCentre);
            }
            public override void Update()
            {
                Rotation += rotatespeed;
                if (alpha <= 0)
                    Dispose();
            }
        }

        private int colorType = 0;
        private Color drawcolor = Color.White;
        /// <summary>
        /// The color type of the star
        /// </summary>
        public int ColorType
        {
            set
            {
                switch (value)
                {
                    case 0:
                        drawcolor = Color.White;
                        colorType = 0;
                        break;
                    case 1:
                        drawcolor = new Color(110, 203, 255, 255);
                        colorType = 1;
                        break;
                    case 2:
                        drawcolor = Color.Orange;
                        colorType = 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("rvalue", value, "The rvalue can only be 0, 1 or 2");
                }
            }
        }
        /// <inheritdoc/>
        public Func<ICustomMotion, Vector2> PositionRoute { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public Func<ICustomMotion, float> RotationRoute { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public float[] RotationRouteParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public float[] PositionRouteParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public float AppearTime => appeartime;
        /// <inheritdoc/>
        public Vector2 CentrePosition => Centre;
        /// <summary>
        /// The rotation speed of the star
        /// </summary>
        public float rotatespeed = 6f;
        /// <summary>
        /// Whether the star has a shadow
        /// </summary>
        public bool starshadow = true;

        private readonly Func<ICustomMotion, Vector2> ease;
        private readonly bool easeif = false;
        /// <summary>
        /// Creates a star with static position
        /// </summary>
        /// <param name="centre">The centre of the star</param>
        /// <param name="scale">The scale of the star</param>
        public Star(Vector2 centre, float scale)
        {
            Image = Sprites.star;
            Rotation = Rand(0, 359f);
            Centre = centre;
            Scale = scale;
        }
        /// <summary>
        /// Creates a star with an easing motion
        /// </summary>
        /// <param name="ease">The easing motion of the star</param>
        /// <param name="scale">The scale of the star</param>
        public Star(Func<ICustomMotion, Vector2> ease, float scale)
        {
            easeif = true;
            Image = Sprites.star;
            Rotation = Rand(0, 359f);
            this.ease = ease;
            Scale = scale;
        }
        private readonly float appeartime = 0;
        private Color light = Color.White * 0.25f;
        /// <inheritdoc/>
        public override void Draw()
        {
            Depth = 0.75f;
            FormalDraw(Image, Centre, new Color(drawcolor.R + light.R, drawcolor.G + light.G, drawcolor.B + light.B, drawcolor.A + light.A), Scale, Rotation * MathF.PI / 180, ImageCentre);
        }
        /// <inheritdoc/>
        public override void Update()
        {
            if (easeif)
                ease(this);
            Rotation += rotatespeed;
            TestDispose();
            if (appeartime % 2 == 0 && starshadow)
            {
                Shadow(Centre, Rotation);
                if (appeartime % 22 == 0)
                    light = ChangeLight(light);
            }
            if (appeartime >= 720)
                Dispose();
        }
        private int scoreResult = 3;
        private bool hasHit = false;
        private static JudgementState JudgeState => GameStates.CurrentScene is SongFightingScene
                    ? (GameStates.CurrentScene as SongFightingScene).JudgeState
                    : JudgementState.Lenient;
        /// <summary>
        /// Whether the star counts to the score or not
        /// </summary>
        public bool MarkScore { private get; set; } = true;
        /// <summary>
        /// Whether the star will automatically dispose when it leaves the screen
        /// </summary>
        public bool AutoDispose { private get; set; } = true;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetCollide(Player.Heart heart)
        {
            float res = Collide(heart.Centre);
            if ((colorType == 1 && !heart.IsMoved) || (colorType == 2 && heart.IsMoved))
                return;

            if (res < 0)
            {
                scoreResult = 0;
                LoseHP(heart);
            }

            int offset = 3 - (int)JudgeState;
            bool needAP = ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0;
            if (res < 0)
            {
                if (!hasHit)
                    UndyneFight_Ex.Fight.AdvanceFunctions.PushScore(0);
                LoseHP(Heart);
                hasHit = true;
            }
            else if (res <= 1.6f - offset * 0.4f)
            {
                if (scoreResult >= 2)
                { scoreResult = 1; Player.CreateCollideEffect(Color.LawnGreen, 3f); }
            }
            else if (res <= 4.2f - offset * 1.2f)
            {
                if (scoreResult >= 3)
                { scoreResult = 2; Player.CreateCollideEffect(Color.LightBlue, 6f); }
            }
            if (scoreResult != 3 && needAP && MarkScore)
            {
                if (!hasHit)
                {
                    UndyneFight_Ex.Fight.AdvanceFunctions.PushScore(0);
                    LoseHP(Heart);
                    hasHit = true;
                }
            }
        }
        /// <inheritdoc/>
        public override void Dispose()
        {
            if (!hasHit && MarkScore)
                UndyneFight_Ex.Fight.AdvanceFunctions.PushScore(scoreResult);
            base.Dispose();
        }
        private bool hasBeenInside = false;
        /// <summary>
        /// The dimension of the screen to check during <see cref="AutoDispose"/>
        /// </summary>
        public static readonly CollideRect screen = new(-150, -150, 940, 780);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TestDispose()
        {
            bool ins = screen.Contain(Centre);
            if (ins && (!hasBeenInside))
                hasBeenInside = true;
            if (hasBeenInside && (!ins))
                Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Shadow(Vector2 center, float rotate) => AddChild(new StarShadow(0.5f, rotate, center, rotatespeed, Scale, drawcolor));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color ChangeLight(Color light) => light == Color.White * 0.25f ? new(0, 0, 0, 0) : Color.White * 0.25f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Collide(Vector2 org) => MathUtil.GetDistance(Centre, org) - 32f * Scale;
    }
    /// <summary>
    /// A fireball
    /// </summary>
    public class Fireball : Entity, ICustomMotion, ICollideAble
    {
        private int colorType = 0;
        private Color drawcolor = Color.White;
        /// <summary>
        /// The color type of the fireball
        /// </summary>
        public int ColorType
        {
            set
            {
                switch (value)
                {
                    case 0:
                        drawcolor = Color.White;
                        colorType = 0;
                        break;
                    case 1:
                        drawcolor = new Color(110, 203, 255, 255);
                        colorType = 1;
                        break;
                    case 2:
                        drawcolor = Color.Orange;
                        colorType = 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("rvalue", value, "The rvalue can only be 0, 1 or 2");
                }
            }
        }
        /// <inheritdoc/>
        public Func<ICustomMotion, Vector2> PositionRoute { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public Func<ICustomMotion, float> RotationRoute { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public float[] RotationRouteParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public float[] PositionRouteParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public float AppearTime => appeartime;
        private readonly float appeartime = 0;
        /// <inheritdoc/>
        public Vector2 CentrePosition => Centre;
        private readonly Func<ICustomMotion, Vector2> ease;
        /// <summary>
        /// Creates a fireball with easing motion
        /// </summary>
        /// <param name="ease">The easing function</param>
        /// <param name="scale">The scale of the fireball</param>
        public Fireball(Func<ICustomMotion, Vector2> ease, float scale)
        {
            this.ease = ease;
            Scale = scale;
        }
        /// <summary>
        /// Creates a fireball with static position
        /// </summary>
        /// <param name="centre">The position of the fireball</param>
        /// <param name="scale">The scale of the fireball</param>
        public Fireball(Vector2 centre, float scale)
        {
            Centre = centre;
            Scale = scale;
        }
        private int index = 0;
        /// <summary>
        /// The alpha of the fireball
        /// </summary>
        public float Alpha = 1;

        /// <inheritdoc/>
        public override void Draw()
        {
            Depth = 0.9f;
            DrawEvent();
            FormalDraw(Image = Sprites.fireball, Centre, drawcolor * Alpha, Scale * new Vector2(index == 1 ? -1 : 1, 1), Rotation, ImageCentre);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetCollide(Player.Heart heart)
        {
            if (Collide(heart.Centre) && (colorType == 0 || (colorType == 1 && heart.IsMoved) || (colorType == 2 && !heart.IsMoved)))
                LoseHP(heart);
        }
        /// <inheritdoc/>
        public override void Update()
        {
            controlLayer = IsHidden ? Surface.Hidden : Surface.Normal;
            Centre = ease?.Invoke(this) ?? Centre;
            if (Centre.X >= 880 || Centre.X <= -240 || Centre.Y >= 480 + 240 || Centre.Y <= -240 || Alpha <= 0)
                Dispose();
        }
        /// <summary>
        /// Whether the fireball is masked inside of the board
        /// </summary>
        public bool IsHidden { set; private get; } = false;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawEvent()
        {
            if (appeartime % 64 == 0)
            {
                index++;
                index %= 2;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Collide(Vector2 org) => MathUtil.GetDistance(Centre, org) <= 6.5f * Scale;
    }
    #endregion
    [Obsolete("There are better functions")]
    public static class Someway
    {
        /// <summary>
        /// 使用一个特殊的节奏数组,并且支持在定点释放事件。
        /// 规则如下：
        /// eventsname和events是对应的。例如第二个eventsname对应第二个events。
        /// GB的创建即
        /// "G001"，G表示固定方向，第二个字符为方向，第三个字符为颜色，第三个之后的字符表示持续beat拍（默认-5）
        /// "WR01"，W表示随机方向，第二个字符没有效果，第三个字符为颜色，第三个之后的字符表示持续beat拍（默认-5）
        /// "/"为空拍,x表示切分拍
        /// 箭头第一个字符-方向 的规则: R 表示随机 D 表示与上次不同 +x 表示上一个方向的 +x方向 -x 表示上一个方向的 -x方向 $x 表示固定某个方向。
        /// 箭头第二个字符-颜色 的规则：0蓝1红。
        /// 箭头第三个字符-旋转type 的规则：0普通1旋转2斜矛。
        /// 箭头修饰字符-箭头效果 的规则：前面加上~表示*1.2速加速，!表示右旋转，@表示左旋转
        /// 箭头组合规则：字符串内加括号表示不仅创建一个箭头,例如"(R)(R1)"。
        /// PS："R(+0)" ≠ "(R)(+0)",后者才是两个矛叠一块，第一个会无效。
        /// </summary>
        /// <param name="beat">节奏数组的间隔拍，推荐三十二分音符为间隔</param>
        /// <param name="arrowspeed">假如创建出来箭头，那么这个箭头的速度</param>
        /// <param name="starttime">延迟的时间</param>
        /// <param name="rhythm">节奏数组</param>
        /// <param name="eventsname">节奏数组内事件的名称</param>
        /// <param name="events">节奏数组内的事件</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use CreateChart()")]
        public static void SpecialRhythmCreate(float beat, float arrowspeed, float starttime, string[] rhythm, string[] eventsname, Action[] events)
        {
            float b = starttime;
            WaveConstructor a = new(beat);
            for (int i = 0; i < rhythm.Length; i++)
            {
                int c = 0;
                foreach (string eventname in eventsname)
                {
                    if (rhythm[i] == eventname)
                        AddInstance(new InstantEvent(b, events[c]));
                    c++;
                }
                //Empty beat
                if (rhythm[i] is "/" or "")
                    b += beat;
                //Blaster
                else if (rhythm[i][0] == 'G')
                {
                    string times = "";
                    for (int j = 3; j < rhythm[i].Length; j++)
                        times += rhythm[i][j];
                    CreateGB(new GreenSoulGB(b, rhythm[i][1].ToString(), rhythm[i][2] - '0', StringToFloat(times) * beat - 5));
                }
                //Blaster and arrow
                else if (rhythm[i][0] == 'W')
                {
                    int way = Rand(0, 3);
                    string times = "";
                    for (int j = 3; j < rhythm[i].Length; j++)
                        times += rhythm[i][j];
                    CreateGB(new GreenSoulGB(b, way, rhythm[i][2] - '0', StringToFloat(times) * beat - 5));
                    CreateArrow(b, way, arrowspeed, rhythm[i][2] - '0', 0);
                }
                //Right rotation arrows
                else if (rhythm[i][0] == '!')
                {
                    a.CreateArrows(b, arrowspeed * 1.05f, rhythm[i].Replace("!", ""), ArrowAttribute.RotateR);
                    b += beat;
                }
                //Left rotation arrows
                else if (rhythm[i][0] == '@')
                {
                    a.CreateArrows(b, arrowspeed * 1.05f, rhythm[i].Replace("@", ""), ArrowAttribute.RotateL);
                    b += beat;
                }
                //Speed up arrows
                else if (rhythm[i][0] == '~')
                {
                    a.CreateArrows(b, arrowspeed * 1.35f, rhythm[i].Replace("~", ""), ArrowAttribute.SpeedUp);
                    b += beat;
                }

                else if (rhythm[i][0] is '+' or 'R' or '$' or '(' or '-' or 'D' or '0'
                    or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9')
                {
                    if (rhythm[i].Length > 2)
                    {
                        if (rhythm[i][1] == '<')
                        {
                            a.CreateArrows(b, arrowspeed, rhythm[i].Replace(rhythm[i][0].ToString() + rhythm[i][1].ToString(), ""));
                            b += beat * 8 / (rhythm[i][0] - '0');
                        }
                        else
                        {
                            a.CreateArrows(b, arrowspeed, rhythm[i]);
                            b += beat;
                        }
                    }
                    else
                    {
                        a.CreateArrows(b, arrowspeed, rhythm[i]);
                        b += beat;
                    }

                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("There are better functions")]
        public static float ManySquare(float org, int squaretimes)
        {
            float ret = org;
            if (squaretimes <= 0)
                for (int i = squaretimes - 1; i < 0; i++)
                    ret *= 1 / org;
            else
                for (int i = 1; i < squaretimes; i++)
                    ret *= org;
            return ret;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("There are better functions")]
        public static float StringToFloat(string number)
        {
            float ret = 0;
            for (int a = 0; a < number.Length; a++)
                ret += (number[number.Length - a - 1] - '0') * ManySquare(10, a);
            return ret;
        }
    }
    /// <summary>
    /// Utilities for bones
    /// </summary>
    [Obsolete("There are better functions")]
    public class FightUtil
    {
        public class DownBonesea(float quantity, float distance, float length, bool way, float speed, float duration, int colortype = 0) : Entity
        {
            private readonly float duration = duration, quantity = quantity, distance = distance, length = length, speed = speed;
            private readonly int colortype = colortype;
            private readonly bool way = way;
            public override void Draw() { }
            private float time = 0;
            public bool markscore = true;
            private int appearTime;
            public string[] tags = ["noany"];
            public override void Update()
            {
                appearTime++;
                if (appearTime == 1)
                {
                    for (int a = 0; a < quantity; a++)
                    {
                        float b = a * distance * speed;
                        CreateBone(new DownBone(!way, way ? BoxStates.Left - b : BoxStates.Right + b, speed, length) { ColorType = colortype, MarkScore = markscore, Tags = tags });
                    }
                }
                if (++time >= duration)
                    Dispose();
            }
        }
        public class UpBonesea(float quantity, float distance, float length, bool way, float speed, float duration, int colortype = 0) : Entity
        {
            public float duration = duration, quantity = quantity, distance = distance, length = length, speed = speed;
            public int colortype = colortype;
            public bool way = way;
            public override void Draw() { }
            private int appearTime;
            private float time = 0;
            public bool markscore = true;
            public string[] tags = ["noany"];
            public override void Update()
            {
                appearTime++;
                if (appearTime == 1)
                {
                    for (int a = 0; a < quantity; a++)
                    {
                        float b = a * distance * speed;
                        CreateBone(new UpBone(!way, way ? BoxStates.Left - b : BoxStates.Right + b, speed, length) { ColorType = colortype, MarkScore = markscore, Tags = tags });
                    }
                }
                if (time++ >= duration)
                    Dispose();
            }
        }
        public class LeftBonesea(float quantity, float distance, float length, bool way, float speed, float duration, int colortype = 0) : Entity
        {
            public float duration = duration, quantity = quantity, distance = distance, length = length, speed = speed;
            public int colortype = colortype;
            public bool way = way;
            public override void Draw() { }
            private int appearTime;
            private float time = 0;
            public bool markscore;
            public string[] tags = ["noany"];
            public override void Update()
            {
                appearTime++;
                if (appearTime == 1)
                {
                    for (int a = 0; a < quantity; a++)
                    {
                        float b = a * distance * speed;
                        CreateBone(new LeftBone(way, way ? BoxStates.Left + b : BoxStates.Right + b, speed, length) { ColorType = colortype, MarkScore = markscore, Tags = tags });
                    }
                }
                if (time++ >= duration)
                    Dispose();
            }
        }
        public class RightBonesea(float quantity, float distance, float length, bool way, float speed, float duration, int colortype = 0) : Entity
        {
            public float duration = duration, quantity = quantity, distance = distance, length = length, speed = speed;
            public int colortype = colortype;
            public bool way = way;
            public override void Draw() { }
            private int appearTime;
            private float time = 0;
            public bool markscore;
            public string[] tags = ["noany"];
            public override void Update()
            {
                appearTime++;
                if (appearTime == 1)
                {
                    for (int a = 0; a < quantity; a++)
                    {
                        float b = a * distance * speed;
                        CreateBone(new RightBone(way, way ? BoxStates.Left + b : BoxStates.Right + b, speed, length) { ColorType = colortype, MarkScore = markscore, Tags = tags });
                    }
                }
                if (time++ >= duration)
                    Dispose();
            }
        }
        [Obsolete("Use CustomBone")]
        public class RotBone(float length, float speed, float rotate, bool way, int pointtype) : Entity
        {
            public float rotate = rotate, length = length, speed = speed;
            public bool way = way;
            public static Vector2 point, deviation, distance;
            public int pointtype = pointtype;
            public override void Draw() { }
            private int appeartime = 0;
            public override void Update()
            {
                appeartime++;
                if (appeartime == 1)
                {
                    switch (pointtype)
                    {
                        case 0:
                            CreateBone(new CustomBone(
                            new Vector2(BoxStates.Left, BoxStates.Down)
                            + new Vector2(length / Tan(rotate) * Cos(rotate), length * Cos(rotate))
                            + new Vector2(Cos(90 - rotate) * length / 2, -Sin(90 - rotate) * length / 2),
                            Motions.PositionRoute.linear, rotate, length)
                            { PositionRouteParam = [Cos(rotate - 180) * speed, Sin(rotate - 180) * speed] });
                            break;
                        case 1:
                            CreateBone(new CustomBone(
                            new Vector2(BoxStates.Right, BoxStates.Down)
                              + new Vector2(-length / Tan(rotate + 90) * Cos(rotate + 90), -length / Tan(rotate + 90) * Sin(rotate + 90))
                              + new Vector2(Cos(90 - rotate + 90) * length / 2, -Sin(90 - rotate + 90) * length / 2),
                                Motions.PositionRoute.linear, rotate + 90, length)
                            { PositionRouteParam = [Cos(rotate - 180 + 90) * speed, Sin(rotate - 180 + 90) * speed] });
                            break;
                        case 2:
                            CreateBone(new CustomBone(
                                new Vector2(BoxStates.Right, BoxStates.Up)
                                + new Vector2(length / Tan(rotate + 180) * Cos(rotate + 180), length / Tan(rotate + 180) * Sin(rotate + 180))
                                + new Vector2(Cos(90 - rotate + 180) * length / 2, -Sin(90 - rotate + 180) * length / 2),
                                Motions.PositionRoute.linear, rotate + 180, length)
                            { PositionRouteParam = [Cos(rotate - 180 + 180) * speed, Sin(rotate - 180 + 180) * speed] });
                            break;
                        case 3:
                            CreateBone(new CustomBone(
                                new Vector2(BoxStates.Left, BoxStates.Up)
                                + new Vector2(-length / Tan(rotate + 270) * Cos(rotate + 270), -length / Tan(rotate + 270) * Sin(rotate + 270))
                                + new Vector2(Cos(90 - rotate + 270) * length / 2, -Sin(90 - rotate + 270) * length / 2),
                                Motions.PositionRoute.linear, rotate + 270, length)
                            { PositionRouteParam = [Cos(rotate - 180 + 270) * speed, Sin(rotate - 180 + 270) * speed] });
                            break;
                    }
                }
            }
        }
    }
    [Obsolete("You can use a Particle instead")]
    public class FakeNote
    {
        public class LeftNote : Entity
        {
            private readonly Func<ICustomMotion, Vector2> Eases;
            private readonly float EasesTime = 0, Delay = 0, Speed = 0;

            public LeftNote(float delay, float speed, int color, int type, Func<ICustomMotion, Vector2> eases, float easestime)
            {
                Image = Sprites.arrow[color, type, 0];
                Delay = delay;
                Eases = eases;
                Speed = speed;
                Centre = new(Heart.Centre.X - (Delay * Speed + 42), Heart.Centre.Y);
                Rotation = 180;
                EasesTime = easestime;
            }
            private int Timer = 0;
            public Vector2 Offset;
            public override void Update()
            {
                Depth = 0.99f;
                if (++Timer == 1)
                {
                    CentreEasing.EaseBuilder ce = new();
                    ce.Insert(0, CentreEasing.Stable(Heart.Centre.X - (Delay * Speed + 42) + Offset.X, Heart.Centre.Y + Offset.Y));
                    ce.Insert(Delay, CentreEasing.Linear(Speed));
                    ce.Insert(EasesTime, Eases);
                    ce.Run((s) => Centre = s);
                }
            }

            public override void Draw() => FormalDraw(Image, Centre, Color.White, Rotation / 180 * MathF.PI, ImageCentre);
        }
        public class RightNote : Entity
        {
            private readonly Func<ICustomMotion, Vector2> Eases;
            private readonly float EasesTime = 0, Delay = 0, Speed = 0;

            public RightNote(float delay, float speed, int color, int type, Func<ICustomMotion, Vector2> eases, float easestime)
            {
                Image = Sprites.arrow[color, type, 0];
                Delay = delay;
                Eases = eases;
                Speed = speed;
                Centre = new(Heart.Centre.X + (Delay * Speed + 42), Heart.Centre.Y);
                Rotation = 0;
                EasesTime = easestime;
            }
            private int Timer = 0;
            public Vector2 Offset;
            public override void Update()
            {
                Depth = 0.99f;
                if (++Timer == 1)
                {
                    CentreEasing.EaseBuilder ce = new();
                    ce.Insert(0, CentreEasing.Stable(Heart.Centre.X + (Delay * Speed + 42) + Offset.X, Heart.Centre.Y + Offset.Y));
                    ce.Insert(Delay, CentreEasing.Linear(-Speed));
                    ce.Insert(EasesTime, Eases);
                    ce.Run((s) => Centre = s);
                }
            }

            public override void Draw() => FormalDraw(Image, Centre, Color.White, Rotation / 180 * MathF.PI, ImageCentre);
        }
    }
    /// <summary>
    /// Utilities for drawing
    /// </summary>
    public static class DrawingUtil
    {
        /// <summary>
        /// Creates a list of bones with provided motion and rotation
        /// </summary>
        /// <param name="start">The initial position of the bones</param>
        /// <param name="speed">The speed of the bones</param>
        /// <param name="length">The length of the bones</param>
        /// <param name="num">The amount of bones</param>
        /// <param name="color">The color of the bones</param>
        /// <param name="RotSpeed">The rotation speed of the bones (Default 4)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("This function is the same as using a for loop to create multiple bones")]
        public static void CrossBone(Vector2 start, Vector2 speed, float length, float num, int color = 0, float RotSpeed = 4)
        {
            for (int i = 0; i < num; i++)
            {
                CreateBone(new CustomBone(start, Motions.PositionRoute.linear, Motions.LengthRoute.autoFold, Motions.RotationRoute.linear)
                {
                    PositionRouteParam = [speed.X, speed.Y],
                    LengthRouteParam = [length, 114514],
                    RotationRouteParam = [RotSpeed, 180 / num * i],
                    ColorType = color
                });
            }
        }
        /// <summary>
        /// Sets the screen scale to the target size in the given duration using Quadratic easing (<see cref="SimplifiedEasing.EaseState.Quad"/>)
        /// </summary>
        /// <param name="size">The target size of the screen</param>
        /// <param name="duration">The duration of the lerp</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScreenScale(float size, float duration)
        {
            float start = ScreenDrawing.ScreenScale, end = size, del = start - end, t = 0;
            AddInstance(new TimeRangedEvent(duration, () =>
            {
                float x = t / (duration - 1), f = 2 * x - x * x;
                ScreenDrawing.ScreenScale = start - del * f;
                t++;
            }));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("There are better functions")]
        public static void MinusScreenScale(float MaxSize, float time)
        {
            float start = ScreenDrawing.ScreenScale, end = start - MaxSize, del = start - end, t = 0;
            AddInstance(new TimeRangedEvent(0, time / 2f, () =>
            {
                float x = t / (time / 2f - 1), f = 2 * x - x * x;
                ScreenDrawing.ScreenScale = start - del * f;
                t++;
            }));
            float t2 = 0, start2 = start - MaxSize, end2 = start, del2 = start2 - end2;
            AddInstance(new TimeRangedEvent(time / 2f, time / 2f, () =>
            {
                float x = t2 / (time / 2f - 1), f = x * x;
                ScreenDrawing.ScreenScale = start2 - del2 * f;
                t2++;
            }));
        }
        /// <summary>
        /// Sets the screen angle to the target angle in the given duration using Quadratic easing (<see cref="SimplifiedEasing.EaseState.Quad"/>)
        /// </summary>
        /// <param name="angle">The target angle of the screen</param>
        /// <param name="time">The duration of the lerp</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenAngle(float angle, float time)
        {
            float start = ScreenDrawing.ScreenAngle, end = angle, del = start - end, t = 0;
            AddInstance(new TimeRangedEvent(0, time, () =>
            {
                float x = t / (time - 1), f = 2 * x - x * x;
                ScreenDrawing.ScreenAngle = start - del * f;
                t++;
            }));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("There are better functions")]
        public static void PlusRotate(float MaxAngle, float time)
        {
            float start = ScreenDrawing.ScreenAngle, end = start + MaxAngle, del = start - end, t = 0;
            AddInstance(new TimeRangedEvent(0, time / 2f, () =>
            {
                float x = t / (time / 2f), f = 2 * x - x * x;
                ScreenDrawing.ScreenAngle = start - del * f;
                t++;
            }));
            float t2 = 0, start2 = start + MaxAngle, end2 = start, del2 = start2 - end2;
            AddInstance(new TimeRangedEvent(time / 2f + 1, time / 2f, () =>
            {
                float x = t2 / (time / 2f), f = x * x;
                ScreenDrawing.ScreenAngle = start2 - del2 * f;
                t2++;
            }));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("There are better functions")]
        public static void PlusScreenScale(float MaxSize, float time)
        {
            time = (int)time;
            float start = ScreenDrawing.ScreenScale, end = start + MaxSize, del = start - end, t = 0;
            AddInstance(new TimeRangedEvent(0, time / 2f, () =>
            {
                float x = t / (time / 2f), f = 2 * x - x * x;
                ScreenDrawing.ScreenScale = start - del * f;
                t++;
            }));
            float t2 = 0, start2 = start + MaxSize, end2 = start, del2 = start2 - end2;
            AddInstance(new TimeRangedEvent(time / 2f + 1, time / 2f, () =>
            {
                float x = t2 / (time / 2f), f = x * x;
                ScreenDrawing.ScreenScale = start2 - del2 * f;
                t2++;
            }));
        }
        /// <summary>
        /// Shakes the screen
        /// </summary>
        /// <param name="interval">The interval between each shake (Default 2 frames)</param>
        /// <param name="range">The shake intensity (Default 2 pixels)</param>
        /// <param name="times">Times to shake (Default 4 times)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shock(float interval = 2, float range = 2, float times = 4) => Shock(interval, range, range, times);
        /// <summary>
        /// Shakes the screen
        /// </summary>
        /// <param name="interval">The interval between each shake</param>
        /// <param name="rangeX">The shake intensity of the x-coordinate</param>
        /// <param name="rangeY">The shake intensity of the y-coordinate</param>
        /// <param name="times">Times to shake</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shock(float interval, float rangeX, float rangeY, float times)
        {
            for (int a = 0; a < times; a++)
            {
                AddInstance(new TimeRangedEvent(a * interval, 1, () => ScreenDrawing.ScreenPositionDelta = new Vector2(Rand(-rangeX, rangeX), Rand(-rangeY, rangeY))));
            }
            AddInstance(new InstantEvent((times + 1) * interval, () => ScreenDrawing.ScreenPositionDelta = Vector2.Zero));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("There are better functions")]
        public static void Rain(float speed, float rotate, bool way)
        {
            float a = way ? -45 : 45;
            Linerotatelong rain = new(Rand(-200, 860), a, rotate + 270 + Rand(-2.5f, 2.5f), 180, Rand(0.2f, 0.4f), Rand(9, 55), Color.White)
            { width = Rand(2, 4) };
            if (Rand(1, 3) == 1)
                CreateEntity(rain);
            else
                for (int b = 0; b < 2; b++)
                    CreateEntity(rain);
            AddInstance(new TimeRangedEvent(0, 180, () =>
            {
                rain.xCenter += Cos(rotate + 90) * speed;
                rain.yCenter += Sin(rotate + 90) * speed;
            }));
        }
        /// <summary>
        /// Creates a screen fading in and out
        /// </summary>
        /// <param name="inDuration">The duration for the screen to fade in</param>
        /// <param name="duration">The duration of the fade screen</param>
        /// <param name="outDuration">The duration for the screen to fade out</param>
        /// <param name="color">The color of the fading (Default black)</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FadeScreen(float inDuration, float duration, float outDuration, Color? color = null)
        {
            if (inDuration == 0)
            {
                MaskSquare maskSquare = new(-320, -240, 960, 720, (int)(inDuration + duration + outDuration), color ?? Color.Black, 1);
                CreateEntity(maskSquare);
                AddInstance(new TimeRangedEvent(duration, outDuration + 1, () => maskSquare.alpha -= 1 / outDuration));
            }
            else if (inDuration <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inDuration), string.Format("the parameter {0} must be greater than 0.", nameof(inDuration)));
            }
            else
            {
                MaskSquare maskSquare = new(-320, -240, 960, 720, (int)(inDuration + duration + outDuration), color ?? Color.Black, 0);
                CreateEntity(maskSquare);
                AddInstance(new TimeRangedEvent(inDuration + 1, () => maskSquare.alpha += 1 / inDuration));
                AddInstance(new TimeRangedEvent(inDuration + 1 + duration, outDuration + 1, () => maskSquare.alpha -= 1 / outDuration));
            }
        }
        /// <summary>
        /// Rotates the camera and rotates back to origin
        /// </summary>
        /// <param name="duration">The duration of the rotations</param>
        /// <param name="range">The magnitude of the rotation</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RotateWithBack(float duration, float range)
        {
            ScreenDrawing.CameraEffect.RotateTo(range, duration / 2);
            AddInstance(new InstantEvent(duration / 2 + 1, () => ScreenDrawing.CameraEffect.RotateTo(0, duration / 2 - 1)));
        }
        /// <summary>
        /// Rotates the camera and rotates it to the negation of it before rotating it to the origin (10 -> 25 -> -25 -> 0)
        /// </summary>
        /// <param name="duration">The duration of the rotations</param>
        /// <param name="range">The magnitude of the rotations</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RotateSymmetricBack(float duration, float range)
        {
            ScreenDrawing.CameraEffect.RotateTo(range, duration / 3);
            AddInstance(new InstantEvent(duration / 3 + 1, () =>
                ScreenDrawing.CameraEffect.RotateTo(-range, duration / 3 - 1)));
            AddInstance(new InstantEvent(duration / 3 * 2 + 1, () =>
                ScreenDrawing.CameraEffect.RotateTo(0, duration / 3 - 1)));
        }
        /// <summary>
        /// Lerps the box and heart to green soul position (If the duration is shorter than the required lerp time, then the lerp will be incomplete. If the required lerp duration is shorter than the duration, then the lerp duration will be shortened)
        /// </summary>
        /// <param name="duration">The duration of the lerp</param>
        /// <param name="getto">The target position</param>
        /// <param name="lerpcount">The lerp amount</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use easing functions instead")]
        public static void LerpGreenBox(float duration, Vector2 getto, float lerpcount) => AddInstance(new TimeRangedEvent(duration, () =>
                                                                                                    {
                                                                                                        InstantSetBox(BoxStates.Centre * (1 - lerpcount) + getto * lerpcount, 84, 84);
                                                                                                        InstantTP(Heart.Centre * (1 - lerpcount) + getto * lerpcount);
                                                                                                    }));
        /// <summary>
        /// Lerps the screen position (If the duration is shorter than the required lerp time, then the lerp will be incomplete. If the required lerp duration is shorter than the duration, then the lerp duration will be shortened)
        /// </summary>
        /// <param name="duration">The duration of the lerp</param>
        /// <param name="getto">The target position</param>
        /// <param name="lerpcount">The lerp amount</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use easing functions instead")]
        public static void LerpScreenPos(float duration, Vector2 getto, float lerpcount) => AddInstance(new TimeRangedEvent(duration, () =>
                                                                                                         ScreenDrawing.ScreenPositionDelta = ScreenDrawing.ScreenPositionDelta * (1 - lerpcount) + getto * lerpcount));
        /// <summary>
        /// Lerps the screen scale (If the duration is shorter than the required lerp time, then the lerp will be incomplete. If the required lerp duration is shorter than the duration, then the lerp duration will be shortened)
        /// </summary>
        /// <param name="duration">The duration of the lerp</param>
        /// <param name="getto">The target position</param>
        /// <param name="lerpcount">The lerp amount</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use easing functions instead")]
        public static void LerpScreenScale(float duration, float getto, float lerpcount) => AddInstance(new TimeRangedEvent(duration, () =>
                                                                                                         ScreenDrawing.ScreenScale = ScreenDrawing.ScreenScale * (1 - lerpcount) + getto * lerpcount));
        /// <summary>
        /// Creates a masking rectangle
        /// </summary>
        /// <param name="LeftUpX">The x coordinate of the top left corner of the rectangle</param>
        /// <param name="LeftUpY">The y coordinate of the top left corner of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        /// <param name="duration">The duration of the masking</param>
        /// <param name="color">The color of the rectangle</param>
        /// <param name="alpha">The alpha of the rectangle</param>
        [Obsolete("You can use ImageEntity")]
        public class MaskSquare(float LeftUpX, float LeftUpY, float width, float height, float duration, Color color, float alpha) : Entity
        {
            public float duration = duration, LeftUpX = LeftUpX, LeftUpY = LeftUpY, width = width, height = height;
            public Color color = color;
            public float alpha = alpha, time = 0, speed = 1;
            /// <inheritdoc>
            public override void Draw()
            {
                FormalDraw(Sprites.pixUnit, new CollideRect(LeftUpX, LeftUpY, width, height).ToRectangle(), color * alpha);
                Depth = 0.99f;
            }
            /// <inheritdoc>
            public override void Update()
            {
                if (++time == duration)
                    Dispose();
            }
        }
        [Obsolete("This class serves zero purpose")]
        public class SpecialBox : RectangleBox
        {
            public float duration = 0, width = 84 - 2, height = 84 - 2, rotate = 0;
            public SpecialBox(float duration, float rotate, Player.Heart p) : base(p)
            {
                this.duration = duration;
                this.rotate = rotate;
                collidingBox = new CollideRect(0, 0, 40, 40);
            }
            public float alpha = 1, speed = 1;
            public int time = 0;
            private static readonly float dist = MathF.Sqrt(42 * 42 * 2);
            public override void Draw()
            {
                for (int a = 0; a < 4; a++)
                    DrawingLab.DrawLine(Heart.Centre + MathUtil.GetVector2(dist, 45 + rotate + a * 90), Heart.Centre + MathUtil.GetVector2(dist, 45 + rotate + 90 + a * 90), 4.2f, Color.White * 0.5f, 0.99f);
            }
            public override void Update()
            {
                if (++time == duration)
                    Dispose();
            }

            public override void MoveTo(object v) { }
            public override void InstanceMove(object v) { }
        }
        #region Obsolete lines
        [Obsolete("Use Line")]
        public class NormalLine(float x1, float y1, float x2, float y2, float duration, float alpha, Color? color = null) : Entity
        {
            public float duration = duration, x1 = x1, y1 = y1, x2 = x2, y2 = y2, width = 3;
            public Color color = color ?? Color.White;

            public NormalLine(Vector2 pos1, Vector2 pos2, float duration, float alpha, Color color) : this(pos1.X, pos1.Y, pos2.X, pos2.Y, duration, alpha, color) { }
            public int time = 0;
            public float alpha = alpha, speed = 1, depth = 0.99f;
            public override void Draw()
            {
                DrawingLab.DrawLine(new(x1, y1), new(x2, y2), width, color * alpha, depth);
                Depth = depth;
            }
            public override void Update()
            {
                if (++time >= duration)
                    Dispose();
            }
        }
        [Obsolete("Use Line")]
        public class Linerotate(float xCenter, float yCenter, float rotate, float duration, float alpha, Color? color = null) : Entity
        {
            public float duration = duration, xCenter = xCenter, yCenter = yCenter, rotate = rotate, width = 4;
            public Color color = color ?? Color.White;
            public int time = 0;
            public float alpha = alpha, speed = 1, depth = 0.2f;
            public override void Draw()
            {
                DrawingLab.DrawLine(
                    (rotate % 180 != 0) ? new(xCenter - 1f / Tan(rotate) * yCenter, 0) : new(0, yCenter),
                    (rotate % 180 != 0) ? new(xCenter + 1f / Tan(rotate) * (480 - yCenter), 480) : new(640, yCenter),
                    width, color * alpha, depth);
                Depth = 0.2f;
            }

            public override void Update()
            {
                if (++time >= duration)
                    Dispose();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use Line")]
        public static void CreateTagLine(Linerotate Line, string[] Tags)
        {
            CreateEntity(Line);
            Line.Tags = Tags;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use Line")]
        public static void CreateTagLine(Linerotate Line, string Tag)
        {
            CreateEntity(Line);
            Line.Tags = [Tag];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use Line")]
        public static void CreateTagLine(Linerotatelong Line, string Tag)
        {
            CreateEntity(Line);
            Line.Tags = [Tag];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use Line")]
        public static void CreateTagLine(NormalLine Line, string[] Tags)
        {
            CreateEntity(Line);
            Line.Tags = Tags;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), Obsolete("Use Line")]
        public static void CreateTagLine(NormalLine Line, string Tag)
        {
            CreateEntity(Line);
            Line.Tags = [Tag];
        }
        [Obsolete("Use Line")]
        public class Linerotatelong(float xCenter, float yCenter, float rotate, float duration, float alpha, float length, Color? color = null) : Entity
        {
            public float duration = duration, xCenter = xCenter, yCenter = yCenter, rotate = rotate, length = length, width = 4;
            public Color color = color ?? Color.White;
            public int time = 0;
            public float alpha = 1, speed = 1;
            public override void Draw()
            {
                DrawingLab.DrawLine(
                    new Vector2(xCenter, yCenter),
                    new Vector2(xCenter + Cos(rotate) * length, yCenter + Sin(rotate) * length),
                        width, color * alpha, 0.99f);
                Depth = 0.99f;
            }

            public override void Update()
            {
                if (++time == duration)
                    Dispose();
            }
        }
        #endregion
        /// <summary>
        /// A circle
        /// </summary>
        public class Circle : Entity
        {
            /// <summary>
            /// The alpha of the circle
            /// </summary>
            public float Alpha = 0.5f;
            /// <summary>
            /// The starting angle of the circle
            /// </summary>
            public float StartAng = 0;
            /// <summary>
            /// The ending angle of the circle
            /// </summary>
            public float EndAng = 360;
            /// <summary>
            /// The radius of the circle
            /// </summary>
            public float Radius;
            /// <summary>
            /// The thickness of the circle
            /// </summary>
            public float Thickness;
            /// <summary>
            /// The color of the circle
            /// </summary>
            public Color color;
            /// <summary>
            /// Creates a circle
            /// </summary>
            /// <param name="pos">The position of the circle</param>
            /// <param name="rad">The radius of the circle</param>
            /// <param name="thick">The thickness of the circle (Default full)</param>
            /// <param name="col">The color of the circle (Default black)</param>
            public Circle(Vector2 pos, float rad, float? thick = null, Color? col = null)
            {
                Centre = pos;
                Radius = rad;
                Thickness = thick ?? rad * 2;
                color = col ?? Color.Lerp(ScreenDrawing.ThemeColor, Color.Black, Alpha);
            }
            public override void Update() { }
            public override void Draw() => DrawingLab.DrawCircleSections(Centre, Radius, 512, Thickness, color, 1, StartAng, EndAng);
        }
    }
    [Obsolete("LineMoveLibrary is overhauled by Line")]
    public static class LineMoveLibrary
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlphaSin(NormalLine Line, float duration, float range, float startrange, float frequency, float startfrequency)
        {
            float sin = startfrequency;
            AddInstance(new TimeRangedEvent(duration, () => Line.alpha = startrange + Sin(sin += frequency / duration) * range));
        }
        /// <summary>
        /// 线段的Alpha-Sin,该重载表示经过duration时间闪烁一次(alpha=1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlphaSin(Linerotate Line, float duration)
        {
            float sin = 0;
            AddInstance(new TimeRangedEvent(duration, () => Line.alpha = Sin(sin += 360 / duration)));
        }
        /// <summary>
        /// 提供一些线段有的Tag,赋予这些线段的Alpha以Sin缓动
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlphaSin(string LineTags, float duration, float range, float startrange, float frequency, float startfrequency)
        {
            Linerotate[] Line = GetAll<Linerotate>(LineTags);
            NormalLine[] L = GetAll<NormalLine>(LineTags);
            for (int a = 0; a < L.Length; a++)
            {
                int x = a;
                float speed = startfrequency;
                AddInstance(new TimeRangedEvent(duration, () => L[x].alpha = startrange + Sin(speed += 360 / frequency) * range));
            }
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                float sin = startfrequency;
                AddInstance(new TimeRangedEvent(duration, () => Line[x].alpha = startrange + Sin(sin += 360 / frequency) * range));
            }
        }
        /// <summary>
        /// 提供一些线段有的Tag,赋予这些线段的Alpha以Lerp缓动
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlphaLerp(string LineTags, float duration, float lerpto, float count)
        {
            Linerotate[] Line = GetAll<Linerotate>(LineTags);
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                AddInstance(new TimeRangedEvent(duration, () =>
                    Line[x].alpha = lerpto * count + Line[x].alpha * (1 - count)));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LAlphaLerp(string LineTags, float duration, float lerpto, float count)
        {
            Linerotatelong[] Line = GetAll<Linerotatelong>(LineTags);
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                AddInstance(new TimeRangedEvent(duration, () =>
                    Line[x].alpha = lerpto * count + Line[x].alpha * (1 - count)));
            }
        }
        /// <summary>
        /// 提供一些线段有的Tag,赋予这些线段的Vector2以Lerp缓动
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VecLerp(string LineTags, float duration, Vector2 lerpto, float count)
        {
            Linerotate[] Line = GetAll<Linerotate>(LineTags);
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                AddInstance(new TimeRangedEvent(duration, () =>
                {
                    Line[x].xCenter = Line[x].xCenter * (1 - count) + lerpto.X * count;
                    Line[x].yCenter = Line[x].yCenter * (1 - count) + lerpto.Y * count;
                }));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LVecLerp(string LineTags, float duration, Vector2 lerpto, float count)
        {
            Linerotatelong[] Line = GetAll<Linerotatelong>(LineTags);
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                AddInstance(new TimeRangedEvent(duration, () =>
                {
                    Line[x].xCenter = Line[x].xCenter * (1 - count) + lerpto.X * count;
                    Line[x].yCenter = Line[x].yCenter * (1 - count) + lerpto.Y * count;
                }));
            }
        }
        /// <summary>
        /// 提供一些线段有的Tag,赋予这些线段的Vector2给一个Vec的速度并且这个速度是lerp缓动
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VecLerpAdd(string LineTags, float duration, Vector2 speed, float count)
        {
            Linerotate[] Line = GetAll<Linerotate>(LineTags);
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                float addx = 0, addy = 0;
                AddInstance(new TimeRangedEvent(duration, () =>
                {
                    Line[x].xCenter += addx;
                    Line[x].yCenter += addy;
                    addx = addx * (1 - count) + speed.X * count;
                    addy = addy * (1 - count) + speed.Y * count;
                }));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VecLinear(string LineTags, float duration, Vector2 speed)
        {
            Linerotate[] Line = GetAll<Linerotate>(LineTags);
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                float addx = speed.X, addy = speed.Y;
                AddInstance(new TimeRangedEvent(duration, () =>
                {
                    Line[x].xCenter += addx;
                    Line[x].yCenter += addy;
                }));
            }
        }
        /// <summary>
        /// 线段的Rotate-Lerp缓动,count为目标的lerp插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RotLerp(Linerotate Line, float duration, float lerpto, float count) => AddInstance(new TimeRangedEvent(duration, () =>
                Line.rotate = lerpto * count + Line.rotate * (1 - count)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LRotLerp(string LineTags, float duration, float lerpto, float count)
        {
            Linerotatelong[] Line = GetAll<Linerotatelong>(LineTags);
            for (int a = 0; a < Line.Length; a++)
            {
                int x = a;
                AddInstance(new TimeRangedEvent(duration, () =>
                    Line[x].rotate = lerpto * count + Line[x].rotate * (1 - count)));
            }
        }
    }
    [Obsolete("ShadowLibrary is overhauled by Line")]
    public static class ShadowLibrary
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LineShadow(int times, Line line)
        {
            for (int i = 0; i < times; i++)
                line.InsertRetention(new(i / 2, 0.24f - 0.24f / times * i));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LineShadow(float delay, float deep, int times, Line line)
        {
            for (int i = 0; i < times; i++)
                line.InsertRetention(new(i * delay, deep - deep / times * i));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetOffset(Arrow arrow, float offset)
        {
            if (arrow.Way == 0)
                arrow.Offset = new(0, offset);
            if (arrow.Way == 1)
                arrow.Offset = new(offset, 0);
            if (arrow.Way == 2)
                arrow.Offset = new(0, -offset);
            if (arrow.Way == 3)
                arrow.Offset = new(-offset, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetOffset2(Arrow arrow, float offset)
        {
            if (arrow.Way == 0)
                arrow.Offset = new(0, offset);
            if (arrow.Way == 1)
                arrow.Offset = new(offset, 0);
            if (arrow.Way == 2)
                arrow.Offset = new(0, offset);
            if (arrow.Way == 3)
                arrow.Offset = new(offset, 0);
        }
    }
}