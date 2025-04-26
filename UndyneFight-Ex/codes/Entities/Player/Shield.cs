using UndyneFight_Ex.Fight;
using static System.Math;
using static System.MathF;
using static UndyneFight_Ex.Fight.Functions;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// The player class
    /// </summary>
    public partial class Player
    {
        /// <summary>
        /// The heart class of <see cref="Player"/>
        /// </summary>
        public partial class Heart
        {
            private interface IShieldImage
            {
                Heart User { get; }
            }
            /// <summary>
            /// The shield. Calculation of angle
            /// </summary>
            public partial class Shield : Entity, IShieldImage
            {
                /// <summary>
                /// The collision checking instance
                /// </summary>
                internal CollisionSide CollisionChecker { get; private set; }

                private class ShieldShadow : Entity, IShieldImage
                {
                    public Heart User { get; }
                    public ShieldShadow(Shield shield, float? missionRotation = null)
                    {
                        this.missionRotation = missionRotation ?? shield.missionRotation;
                        User = shield.user;
                        Rotation = shield.Rotation;
                        rotateStartTime = missionRotation == null ? shield.rotateStartTime : 1;
                        rotateWay = missionRotation == null ? shield.rotateWay : ((missionRotation - Rotation + 360) % 360 < (360 - missionRotation + Rotation) % 360);
                        Image = shield.Image;
                        drawingColor = shield.drawingColor;
                        UpdateIn120 = true;
                        Depth = shield.Depth;
                        Direction = shield.way;
                    }
                    public override void Draw() => FormalDraw(Image, Centre, new Color(drawingColor, 0.6f) * alpha * User.Alpha, MathHelper.ToRadians(Rotation + (User.FixArrow ? 0 : User.Rotation)), ImageCentre);

                    public int Direction { get; init; }
                    private Color drawingColor;
                    private readonly float missionRotation;
                    private readonly bool rotateWay;

                    private float rotateStartTime;
                    private float alpha = 1.0f;
                    public override void Update()
                    {
                        if (alpha < 0)
                            Dispose();
                        alpha -= rotateStartTime * 0.004f;
                        rotateStartTime++;
                        Centre = User.Centre;
                        if (alpha > 0)
                        {
                            float delta = Math.Min((missionRotation - Rotation + 360) % 360, (360 - missionRotation + Rotation) % 360);
                            float scale = Math.Min(Pow(rotateStartTime, 1.5f) / 2.1f * 0.04f, 0.18f);
                            if (delta <= 35f)
                            {
                                scale *= 0.8f * Pow((delta + 37) / 77f, 1.5f) + 0.2f * 1;
                                scale = Math.Min(1, scale * (1 + 15f / (delta * delta + 12)));
                            }
                            Rotation = MathUtil.Posmod(Rotation + delta * scale * (rotateWay ? 1 : -1), 360);
                        }
                    }
                }

                private Color drawingColor;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private protected void Hold(int pos)
                {
                    if (pos != -1)
                        PushDelta *= 0.8f;
                }
                /// <summary>
                /// Whether the shield is enabled
                /// </summary>
                public bool enabled = false;
                /// <summary>
                /// The direction of the shield
                /// </summary>
                public int Way => way;
                internal int way = 0;
                internal int lastWay = 0;

                /// <summary>
                /// 旋转方向, true代表顺时针
                /// </summary>
                private bool rotateWay;
                internal int rotateStartTime = 0;
                private bool rotateStarted = false;

                internal float missionRotation = 0;
                private GreenSoulGB attachedGB = null;
                /// <summary>
                /// Whether the shield is currently attached to a green soul blaster
                /// </summary>
                public bool AttachingGB => attachedGB != null;
                /// <summary>
                /// The color type of the shield
                /// </summary>
                public int ColorType { get; init; }
                /// <summary>
                /// The keys used for changing the direction of the shield (Right, Down, Left, Up)
                /// </summary>
                public InputIdentity[] UpdateKeys { private get; set; } = new InputIdentity[4];

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void Push(GreenSoulGB gb)
                {
                    int color = gb.DrawingColor;
                    bool Auto = (DebugState.blueShieldAuto && color == 0) || (DebugState.redShieldAuto && color == 1) || (DebugState.greenShieldAuto && color == 2) || (DebugState.purpleShieldAuto && color == 3) || (DebugState.otherAuto && color >= 2);

                    if (Auto || GameStates.IsKeyDown(UpdateKeys[gb.Way]))
                        attachedGB = gb;
                    if (Auto || PushDelta > user.controlingBox.CollidingBox.Width - 12)
                        return;

                    PushDelta += 0.1f;
                    PushDelta *= 1.2f;
                    PushDelta = Math.Min(user.controlingBox.CollidingBox.Width - 12, PushDelta);
                }

                internal Heart user;
                /// <summary>
                /// The <see cref="Heart"/> the shield is for
                /// </summary>
                public Heart User => user;
                /// <summary>
                /// <br>The colors for each green soul shield</br>
                /// <br>0-> Blue, 1 -> Red etc</br>
                /// </summary>
                public static Color[] ColorTypes { get; set; } = [new(0, 128, 255, 128), new(255, 0, 0, 128), new(255, 255, 0, 128), new(255, 128, 255, 1)];
                internal float deltaRotation = 0;
                internal float previousRotation;
                internal float PushDelta { get; private set; } = 0;
                /// <summary>
                /// A shield
                /// </summary>
                /// <param name="type">The color type of the shield</param>
                /// <param name="user">The <see cref="Heart"/> to create the shield for</param>
                /// <exception cref="ArgumentException"></exception>
                public Shield(int type, Heart user)
                {
                    UpdateIn120 = true;
                    this.user = user;
                    Depth = 0.43f - type * 0.0001f;
                    Image = FightResources.Sprites.shield;
                    Rotation = 0;
                    ColorType = type;
                }

                /// <inheritdoc/>
                public override void Start()
                {
                    CollisionChecker = new();
                    AddChild(CollisionChecker);
                    base.Start();
                }

                /// <inheritdoc/>
                public override void Draw()
                {
                    if (!enabled)
                        return;
                    drawingColor = ColorTypes[ColorType];

#if DEBUG
                    for (int i = 0; i < 4; i++)
                        if (GameStates.IsKeyDown(UpdateKeys[i]))
                        {
                            Vector2 position = user.Centre + MathUtil.GetVector2(50, i * 90f);
                            Vector2 delta = MathUtil.GetVector2(30, i * 90 + 90);
                            DrawingLab.DrawLine(position + delta, position - delta, 3f, drawingColor * 0.4f, 0.4f);
                        }
#endif
                    FormalDraw(Image, Centre + MathUtil.GetVector2(PushDelta, Rotation + 180 + (user.FixArrow ? 0 : user.Rotation)), drawingColor * user.Alpha, MathHelper.ToRadians(Rotation + user.Rotation), ImageCentre);
                }

                /// <inheritdoc/>
                public override void Update()
                {
                    previousRotation = Rotation;
                    if (attachedGB != null && (attachedGB.Ending || attachedGB.Disposed || attachedGB.Way != way ||
                        (!GameStates.IsKeyDown(UpdateKeys[attachedGB.Way]) && !attachedGB.Auto)))
                        attachedGB = null;
                    while (resetTime > 0)
                    {
                        resetTime--;
                        PushDelta *= 0.6f;
                    }
                    rotateStartTime++;
                    Centre = user.Centre;
                    if (rotateStarted)
                    {
                        float delta = Math.Min((missionRotation - Rotation + 360) % 360, (360 - missionRotation + Rotation) % 360);
                        float scale = Math.Min(Pow(rotateStartTime, 1.5f) / 2.1f * 0.04f, 0.18f);
                        if (delta <= 35f)
                        {
                            scale *= 0.8f * Pow((delta + 37) / 77f, 1.5f) + 0.2f * 1;
                            scale = Math.Min(1, scale * (1 + 15f / (delta * delta + 12)));
                        }
                        Rotation = MathUtil.Posmod(Rotation + delta * scale * (rotateWay ? 1 : -1), 360);
                    }
                    deltaRotation = Rotation - previousRotation;
                }

                private int resetTime = 0;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void Rotate(int missionWay)
                {
                    if (attachedGB != null && PushDelta < 4)
                    {
                        AddChild(new ShieldShadow(this, missionWay * 90));
                        return;
                    }
                    if (way != missionWay)
                    {
                        if (hearts.Count == 1)
                            user.Shields.ShieldRotated();
                        resetTime = 20;
                        if (rotateStartTime < 8f)
                            AddChild(new ShieldShadow(this));
                    }
                    lastWay = way;
                    way = missionWay;
                    missionRotation = missionWay * 90;
                    rotateWay = (missionRotation - Rotation + 360) % 360 < (360 - missionRotation + Rotation) % 360;

                    rotateStartTime = rotateStartTime >= 9f ? 0 : (int)(rotateStartTime / 3f + 1);

                    rotateStarted = true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void CreateShinyEffect(int type, int direction)
                {
                    if (type is < 1 or > 4)
                        return;

                    Color cl = type switch
                    {
                        1 => Color.Green,
                        2 => Color.LightBlue,
                        3 => Color.Gold,
                        _ => Color.Orange,
                    } * (ScreenDrawing.UIColor.A / 255f);

                    ShieldShinyEffect effect = new(this, cl);
                    if (way != direction)
                    {
                        foreach (GameObject obj in ChildObjects)
                        {
                            if (obj is ShieldShadow)
                            {
                                ShieldShadow shadow = obj as ShieldShadow;
                                if (shadow.Direction == direction)
                                {
                                    effect = new(shadow, cl);
                                    break;
                                }
                            }
                        }
                    }
                    GameStates.InstanceCreate(effect);

                    MakeParticle(type);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private void MakeParticle(int type)
                {
                    Vector2 createCentre = Centre + MathUtil.GetVector2(33, missionRotation + Functions.Heart.Rotation);
                    if (type != 0)
                    {
                        int times = type switch
                        {
                            1 => 12,
                            2 => 8,
                            4 => 5,
                            5 => 3,
                            3 => 3,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        for (int i = 0; i < times; i++)
                        {
                            float rotation1 = missionRotation + 90 + Rand(0, 1) * 180;
                            float rdelta = Rand(0, 1f) * Rand(0, 1f) * Rand(0, 1f) * RandSignal();
                            rotation1 += rdelta * 90;
                            GameStates.InstanceCreate(new Particle(type switch
                            {
                                1 => Color.Lime,
                                2 => Color.LightBlue,
                                4 => Color.Orange,
                                5 => Color.Orange,
                                3 => Color.Gold,
                                _ => throw new ArgumentOutOfRangeException()
                            } * Rand(0.67f, 0.85f) * (ScreenDrawing.UIColor.A / 255f), MathUtil.GetVector2(Rand(4f, 8f), rotation1), Rand(6, 10), createCentre, FightResources.Sprites.square)
                            { DarkingSpeed = Rand(10f, 14.6f), SlowLerp = 0.25f });
                        }
                    }
                }

                protected class ShieldShinyEffect(Entity att, Color cl) : Entity
                {
                    private float drawingScale = 1.0f;
                    private readonly float darkerSpeed = 7.9f;

                    private Vector2 missionSize = new(2.6f, 1.5f);

                    /// <inheritdoc/>
                    public override void Update()
                    {
                        if (att != null)
                        {
                            Centre = att.Centre + MathUtil.GetVector2(33, att.Rotation + (att as IShieldImage).User.Rotation);
                            Rotation = att.Rotation;
                            Depth = att.Depth + 0.0101f;
                        }
                        drawingScale += darkerSpeed / 100f;
                        if (drawingScale >= 2f)
                            Dispose();
                    }

                    /// <inheritdoc/>
                    public override void Draw() => FormalDraw(Image = FightResources.Sprites.shinyShield, Centre, cl * MathF.Min(1, Pow(2.1f - drawingScale, 2.1f)), Vector2.Lerp(Vector2.One, missionSize, drawingScale - 1), MathUtil.GetRadian(Rotation + (att as IShieldImage).User.Rotation), ImageCentre);
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void CheckKey()
                {
                    for (int i = 0; i < 4; i++)
                        if (GameStates.IsKeyPressed120f(UpdateKeys[i]))
                            Rotate(i);
                    //Separate loop is required to register rotation before hold
                    for (int i = 0; i < 4; i++)
                        if (GameStates.IsKeyDown(UpdateKeys[i]))
                            Hold(i);
                }
            }
            /// <summary>
            /// The shields of the current heart
            /// </summary>
            public ShieldManager Shields { get; internal set; }
            public class ShieldManager : Entity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void Rotate(int col, int dir)
                {
                    if (shields.TryGetValue(col, out Shield value))
                        value.Rotate(dir);
                }
                /// <summary>
                /// 两个盾牌。0蓝1红
                /// </summary>
                private readonly Dictionary<int, Shield> shields = [];

                private readonly int[] oldDirections = [0, 0, 0, 0];
                public ShieldManager() => UpdateIn120 = true;
                /// <summary>
                /// Red Shield
                /// </summary>
                public Shield RShield { get; private set; }
                /// <summary>
                /// Blue Shield
                /// </summary>
                public Shield BShield { get; private set; }
                /// <summary>
                /// Green Shield
                /// </summary>
                public Shield GShield { get; private set; }
                /// <summary>
                /// Purple Shield
                /// </summary>
                public Shield PShield { get; private set; }
                /// <summary>
                /// Shield background circle
                /// </summary>
                public ShieldCircle Circle { get; private set; }
                /// <inheritdoc/>
                public override void Start()
                {
                    Heart mission = FatherObject as Heart;
                    Shield bshield = new(0, mission) { UpdateKeys = [InputIdentity.MainRight, InputIdentity.MainDown, InputIdentity.MainLeft, InputIdentity.MainUp] };
                    Shield rshield = new(1, mission) { UpdateKeys = [InputIdentity.SecondRight, InputIdentity.SecondDown, InputIdentity.SecondLeft, InputIdentity.SecondUp] };

                    _ = new Shield(2, mission) { UpdateKeys = [InputIdentity.FourthRight, InputIdentity.FourthDown, InputIdentity.FourthLeft, InputIdentity.FourthUp] };

                    _ = new Shield(3, mission) { UpdateKeys = [InputIdentity.ThirdRight, InputIdentity.ThirdDown, InputIdentity.ThirdLeft, InputIdentity.ThirdUp] };
                    shields.Add(0, BShield = bshield);
                    shields.Add(1, RShield = rshield);
                    //shields.Add(2,pshield);
                    //shields.Add(3,gshield);
                    AddChild(bshield);
                    AddChild(rshield);
                    //AddChild(pshield);
                    //AddChild(gshield);
                    AddChild(Circle = new());

                    //this.AddChildObject(pshield);
                    //this.AddChildObject(gshield); 
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void Push(GreenSoulGB greenSoulGB, int shieldID) => shields[shieldID].Push(greenSoulGB);
                internal int DirectionOf(int shieldID) => shields[shieldID].Way;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal bool Exist(int color) => shields.ContainsKey(color);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal bool InSameDir(int color, int dir) => shields[color].AttachingGB ? shields[color].Way == dir : oldDirections[color] == dir;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal bool AttachedGB(int color) => shields[color].AttachingGB;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal int LastDirectionOf(int shieldID) => shields[shieldID].lastWay;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Shield GetShield(int shieldID) => shields[shieldID];
                /// <inheritdoc/>
                public override void Update()
                {
                    Heart mission = FatherObject as Heart;
                    Circle.controlLayer = controlLayer;
                    foreach (KeyValuePair<int, Shield> v in shields)
                        v.Value.controlLayer = controlLayer;
                    if (mission.enabledRedShield && mission.SoulType != 1)
                    {
                        shields[0].enabled = false;
                        shields[1].enabled = true;
                        shields[1].CheckKey();
                    }
                    else if (mission.SoulType == 1)
                    {
                        foreach (KeyValuePair<int, Shield> v in shields)
                        {
                            Shield s = v.Value;
                            s.enabled = true;
                            s.CheckKey();
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<int, Shield> v in shields)
                        {
                            Shield s = v.Value;
                            s.enabled = false;
                        }
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal float RotationOf(int colorType) => shields[colorType].Rotation;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal float PushDelta(int color) => shields[color].PushDelta;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void ShieldShine(int direction, int color, int score)
                {
                    if (score != 0)
                        shields[color].CreateShinyEffect(score, direction);
                    Circle.CheckScore(score);
                    oldDirections[color] = direction;
                }

                /// <summary>
                /// Adds a shield to the current player
                /// </summary>
                /// <param name="shield">The shield to add</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void AddShield(Shield shield)
                {
                    if (shields.ContainsKey(shield.ColorType))
                        return;
                    shields.Add(shield.ColorType, shield);
                    AddChild(shield);
                }
                /// <summary>
                /// Removes a shield form the current player
                /// </summary>
                /// <param name="shield">The shield to remove</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void RemoveShield(Shield shield)
                {
                    _ = ChildObjects.Remove(shield);
                    _ = shields.Remove(shield.ColorType);
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void MakeShieldParticle(Color col, float ang)
                {
                    if (!Functions.Heart.FixArrow)
                        ang -= Functions.Heart.Rotation;
                    Heart mission = FatherObject as Heart;
                    Vector2 createCentre = mission.Centre + MathUtil.GetVector2(33, ang - 180);
                    for (int i = 0, n = Rand(5, 20); i < n; i++)
                    {
                        float rotation1 = ang + 90 + Rand(0, 1) * 180;
                        float rdelta = Rand(0, 1f) * Rand(0, 1f) * Rand(0, 1f) * RandSignal();
                        rotation1 += rdelta * 90;
                        GameStates.InstanceCreate(new Particle(col * Rand(0.67f, 0.85f) * (ScreenDrawing.UIColor.A / 255f), MathUtil.GetVector2(Rand(0, 9f), rotation1), Rand(4, 8), createCentre, RandBool() ? FightResources.Sprites.square : FightResources.Sprites.firePartical)
                        { DarkingSpeed = Rand(15f, 18f), SlowLerp = 0.25f });
                    }
                }

                /// <inheritdoc/>
                public override void Draw() { }

                internal bool OverRotate => RotateConsumption > 8.001f;
                internal float RotateConsumption { get; private set; } = 0;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private void UpdateCircle() => Circle.Consumption = RotateConsumption / 8f;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void ValidRotated()
                {
                    RotateConsumption = MathF.Max(RotateConsumption - 1, 0);
                    UpdateCircle();
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void Consume(float v = 1)
                {
                    RotateConsumption -= v * 0.25f;
                    UpdateCircle();
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void ShieldRotated()
                {
                    RotateConsumption++;
                    UpdateCircle();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Shield.CollisionSide GetCollideChecker(int arrowColor) => shields[arrowColor].CollisionChecker;
            }

            public class ShieldCircle : Entity
            {
                public ShieldCircle()
                {
                    Image = FightResources.Sprites.ShieldCircle;
                    Depth = 0.4f;
                    UpdateIn120 = true;
                }
                private Color DrawingColor => type switch
                {
                    0 => Color.White,
                    1 => Color.Lime,
                    2 => Color.LightBlue,
                    3 => Color.Orange,
                    4 => Color.Gold,
                    _ => throw new ArgumentOutOfRangeException()
                };
                /// <summary>
                /// Consumption of the red circle
                /// </summary>
                public float Consumption { get; set; } = 0.0f;

                private float drawConsumption = 0.0f;

                public override void Draw()
                {
                    if (enabled)
                    {
                        FormalDraw(Image, Centre, curColor * 0.6f * ((FatherObject.FatherObject as Heart).Alpha * ScreenDrawing.UIColor.A / 255f), Rotation, ImageCentre);
                        float scale = MathF.Min(1, drawConsumption);
                        if (drawConsumption > 0.004f)
                            FormalDraw(Image,
                                Centre + new Vector2(0, Image.Height * (1 - scale)),
                                new CollideRect(0, Image.Height * (1 - scale), Image.Width,
                                Image.Height * scale).ToRectangle(),
                                Color.Red * 0.8f * (ScreenDrawing.UIColor.A / 255f), Rotation, ImageCentre);
                    }
                }

                private bool enabled = true;
                private Color curColor = Color.Gold;
                public override void Update()
                {
                    enabled = (FatherObject.FatherObject as Heart).SoulType == 1;
                    Centre = (FatherObject.FatherObject as Heart).Centre;
                    curColor = Color.Lerp(curColor, DrawingColor, 0.1f);
                    drawConsumption = drawConsumption * 0.9f + Consumption * 0.1f;
                }

                private int type = 4;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal void CheckScore(int score)
                {
                    if (score == 3)
                        return;
                    type = score >= 4 ? Min(type, 3) : score == 2 ? Min(type, 2) : score == 1 ? Min(type, 1) : Min(type, 0);
                }
            }
        }
    }
}