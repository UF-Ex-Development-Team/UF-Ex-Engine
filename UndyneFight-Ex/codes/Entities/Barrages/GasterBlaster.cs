using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// The base class for a Gaster Blaster, you should not call this
    /// </summary>
    public abstract class GasterBlaster : Barrage
    {
        /// <summary>
        /// The volume of the blaster spawning (Default 0.85f)
        /// </summary>
        public float AppearVolume { get; set; } = 0.85f;
        /// <summary>
        /// The volume of the blaster shooting (Default 0.8f)
        /// </summary>
        public float ShootVolume { get; set; } = 0.8f;
        /// <summary>
        /// Whether will the blaster have no sound played
        /// </summary>
        public static bool IsGBMute { set => spawnSoundPlayed = value; }
        /// <summary>
        /// Whether the blaster shakes the screen when fired
        /// </summary>
        public bool IsShake { get; set; } = false;
        /// <summary>
        /// Whether the color of the blaster is the theme color
        /// </summary>
        public bool ColorIsTheme { get; set; } = false;
        /// <summary>
        /// Overrides the default rotating behavior
        /// </summary>
        public bool OverrideRotation { get; set; } = false;

        public GasterBlaster() => Image = Sprites.GBStart[0];

        private protected float depth_ = 0.6f;
        private protected static int _blasterCount = 0;
        private protected Color drawingColor = Color.White;
        private protected static CollideRect screen = new CollideRect(-250, -250, 890, 730) * ScreenDrawing.ScreenScale;
        internal static bool spawnSoundPlayed = false, shootSoundPlayed = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetDelta() => OverrideRotation ? 0 : Math.Min((missionRotation - Rotation + 360) % 360, (360 - missionRotation + Rotation) % 360);

        private protected float missionRotation, waitingTime, appearTime = 0, recoilSpeed = 0, laserAffectTime = 1, duration;
        private protected Vector2 missionPlace, size, laserPlace, laserSize;
        private protected bool rotateWay, laserIncreasing = true;

        private protected int score = 3;
        private protected float alpha = 0, beamAlpha = 1f, movingScale = 0.9f;
        private protected bool hasHit = false;
        private readonly vec2 rotDisplace = new(0, 35);

        public override void Draw()
        {
            Depth = depth_;
            FormalDraw(Image, Centre, drawingColor * alpha, size, GetRadian(this is NormalGB ? Rotation : missionRotation), ImageCentre);
            if (appearTime >= waitingTime && laserSize.Y > 0 && this is not GreenSoulGB)
            {
                Depth -= 0.001f;
                FormalDraw(Sprites.GBLaser, laserPlace, drawingColor * beamAlpha, laserSize * size, GetRadian(this is NormalGB ? Rotation : missionRotation), rotDisplace);
            }
        }

        public override void Update()
        {
            if (ColorIsTheme)
                drawingColor = ScreenDrawing.ThemeColor;
            appearTime++;
            if (this is NormalGB && (appearTime < waitingTime || (appearTime >= waitingTime && screen.Contain(laserPlace))))
                laserPlace = GetVector2(27 * size.Y, this is NormalGB ? Rotation : missionRotation) + Centre;

            if ((int)(appearTime - waitingTime) == -12)
                Image = Sprites.GBStart[1];
            else if ((int)(appearTime - waitingTime) == -9)
                Image = Sprites.GBStart[2];
            else if ((int)(appearTime - waitingTime) == -6)
                Image = Sprites.GBStart[3];
            else if ((int)(appearTime - waitingTime) == -3)
            {
                Image = Sprites.GBStart[4];
                if (IsShake)
                    GameStates.InstanceCreate(new Advanced.ScreenShaker(3, 9 * MathF.Min(size.X, size.Y), 3));
            }
            else if ((int)(appearTime - waitingTime) == -1 && !shootSoundPlayed)
            {
                shootSoundPlayed = true;
                PlaySound(Sounds.GBShoot, ShootVolume);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected void MoveToMission()
        {
            if (alpha <= 1f)
                alpha += 0.06f * (1 / movingScale);

            if (appearTime < waitingTime)
                Centre = Centre * movingScale + missionPlace * (1 - movingScale);

            Rotation += GetDelta() * (0.98f - movingScale) * (rotateWay ? 1 : -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected void PushDown()
        {
            Centre -= GetVector2(recoilSpeed += 0.4f, Rotation);
            Image = Sprites.GBShooting[(int)Convert.ToSingle(appearTime % 6 <= 3)];
            if (laserIncreasing)
            {
                beamAlpha = beamAlpha * 0.8f + 0.2f;
                laserSize.Y = laserSize.Y * 0.8f + 0.21f;
                if (laserSize.Y >= 0.88f)
                    laserIncreasing = false;
            }
            else
            {
                beamAlpha = beamAlpha * 0.8f + 0.2f;
                laserSize.Y = 0.9f + Sin(laserAffectTime * 15) * 0.18f;
                laserAffectTime++;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected void BeamDisappear()
        {
            Centre -= GetVector2(recoilSpeed += 0.45f, Rotation);

            if (recoilSpeed >= 5f)
                beamAlpha *= 0.9f;

            float delta = appearTime - waitingTime - duration;
            laserSize.Y -= MathF.Sqrt(delta) / 36f;
            if (laserSize.Y <= 0 && (!screen.Contain(Centre)))
                Dispose();
        }
        private class DelayControl : GameObject
        {
            internal enum DelayType
            {
                Pull = 0,
                Stop = 1
            }
            private float delay = 0;
            private readonly DelayType type;
            public DelayControl(float delay, DelayType delayType)
            {
                UpdateIn120 = true;
                type = delayType;
                this.delay = delay;
            }
            public override void Update()
            {
                GasterBlaster control = FatherObject as GasterBlaster;
                float del = type == DelayType.Pull
                    ? Math.Max(0.5f, MathF.Min(3, delay * 0.1f))
                    : Math.Max(0.4f, MathF.Min(1, (delay > 10 ? 10 : MathF.Sqrt(delay * 2)) * 0.3f));
                if (delay < del)
                    del = delay;
                del /= 2;
                control.waitingTime += del;
                delay -= del;
                if (delay <= 0f)
                    Dispose();
            }
            public override void Dispose() => base.Dispose();
        }
        /// <summary>
        /// Delays the blaster by the given frames
        /// </summary>
        /// <param name="delay">The frames to delay</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delay(float delay) => AddChild(new DelayControl(delay, DelayControl.DelayType.Pull));
        /// <summary>
        /// Stops the blaster for the given frames
        /// </summary>
        /// <param name="delay">The frames to stop</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop(float delay) => AddChild(new DelayControl(delay, DelayControl.DelayType.Stop));
        public abstract override void GetCollide(Player.Heart player);
    }
    /// <summary>
    /// A Green Soul Blaster
    /// </summary>
    public class GreenSoulGB : GasterBlaster
    {
        private Texture2D StuckTexture => (appearTime % 6 > 2) ? Sprites.stuck1 : Sprites.stuck2;

        private readonly Player.Heart missionPlayer;
        /// <summary>
        /// The direction of the blaster
        /// </summary>
        public int Way { get; }

        private Vector2 Position;

        private bool stucked = false;
        private float pushDelta;
        private float distanceToSoul = 0;

        private readonly float timeDelta;
        /// <summary>
        /// Creates a green soul blaster
        /// </summary>
        /// <param name="shootShieldTime">The time for the blaster to fire</param>
        /// <param name="way">The string direction of the blaster</param>
        /// <param name="color">The color type of the blaster</param>
        /// <param name="duration">The duration of the blaster</param>
        public GreenSoulGB(float shootShieldTime, string way, int color, float duration) : this(shootShieldTime, GetWayFromTag(way), color, duration) { }
        /// <summary>
        /// Creates a green soul blaster
        /// </summary>
        /// <param name="shootShieldTime">The time for the blaster to fire</param>
        /// <param name="way">The direction of the blaster</param>
        /// <param name="color">The color type of the blaster</param>
        /// <param name="duration">The duration of the blaster</param>
        public GreenSoulGB(float shootShieldTime, int way, int color, float duration)
        {
            if (Settings.SettingsManager.DataLibrary.Mirror)
                color ^= 1;
            way = Posmod(way, 4);
            timeDelta = Settings.SettingsManager.DataLibrary.ArrowDelay / 16f;
            depth_ = 0.466f + _blasterCount++ / 1000f;
            shootShieldTime += Gametime;
            laserSize.X = 1.0f;
            size = new Vector2(1.0f, 0.7f);
            missionPlayer = Player.heartInstance;
            waitingTime = shootShieldTime - Gametime;
            this.duration = duration;
            Way = way;
            drawingColor = ColorTypes[DrawingColor = color];
            basicRotation = Rotation = (way * 90 + 180) % 360;
            Position = way switch
            {
                0 => new Vector2(270, 0),
                1 => new Vector2(0, 190),
                2 => new Vector2(-270, 0),
                3 => new Vector2(0, -190)
            };
        }
        private readonly float basicRotation;

        internal bool Follow { private get; set; } = false;
        internal bool Ending { get; private set; } = false;
        /// <summary>
        /// The drawing color type of the blaster
        /// </summary>
        public int DrawingColor { get; }
        private int ShieldDirection => missionPlayer.Shields.DirectionOf(DrawingColor);
        internal bool Auto => (DebugState.blueShieldAuto && DrawingColor == 0) || (DebugState.redShieldAuto && DrawingColor == 1) || (DebugState.greenShieldAuto && DrawingColor == 2) || (DebugState.purpleShieldAuto && DrawingColor == 3) || (DebugState.otherAuto && DrawingColor >= 2);

        private Vector2 _lastPlayerPos;
        public override void Update()
        {
            if (!missionPlayer.FixArrow)
            {
                float resultRotation = basicRotation + missionPlayer.Rotation;
                if (missionRotation != resultRotation)
                {
                    Rotation += resultRotation - missionRotation;
                    missionRotation = basicRotation + missionPlayer.Rotation;
                }
            }
            else
                missionRotation = basicRotation;
            int dir = ShieldDirection;
            base.Update();
            float adjustedWaitingTime = waitingTime + timeDelta;
            if ((int)(appearTime - adjustedWaitingTime) >= 0)
            {
                if (appearTime <= adjustedWaitingTime + duration)
                {
                    if (Auto && dir != Way)
                        foreach (Player.Heart p in Player.hearts)
                        {
                            p.Shields.Rotate(DrawingColor, Way);
                            p.Shields.ValidRotated();
                        }
                    if (appearTime - adjustedWaitingTime >= 0)
                    {
                        missionPlayer.Shields.MakeShieldParticle(drawingColor, missionPlayer.FixArrow ? missionRotation : missionRotation + missionPlayer.Rotation);
                        //check collision
                        CalcPush(dir);
                        PushDown();
                        distanceToSoul = (Centre - missionPlayer.Centre).Length();
                        Centre = missionPlayer.Centre + GetVector2(distanceToSoul, Way * 90);
                        if (Follow && (missionPlayer.Centre - _lastPlayerPos).LengthSquared() > 0.10f)
                            ArrangePos();
                        GetCollide();
                    }
                    Rotation = missionRotation * 0.12f + Rotation * 0.88f;
                    if (appearTime <= adjustedWaitingTime + 10)
                        Centre = Centre * movingScale + missionPlace * (1 - movingScale);
                }
            }
            else
            {
                missionPlace = Rotate(Position, missionPlayer.Rotation) + missionPlayer.Centre;
                if (adjustedWaitingTime - appearTime <= 54)
                {
                    if (appearTime < adjustedWaitingTime)
                        Centre = Centre * movingScale + missionPlace * (1 - movingScale);

                    Rotation = missionRotation * 0.12f + Rotation * 0.88f;
                    if (alpha < 1)
                        alpha += 0.1f;
                }
                if ((int)(adjustedWaitingTime - appearTime) == 55)
                {
                    GameStates.InstanceCreate(new ParticleGather(missionPlace, 21, 55, drawingColor));
                    Centre = GetVector2(120, Rotation) + missionPlace;
                    Rotation += Rand(-40, 40);
                    missionPlayer.Shields.Consume();
                    if (!spawnSoundPlayed)
                    {
                        PlaySound(Sounds.GBSpawn, AppearVolume);
                        spawnSoundPlayed = true;
                    }
                }
            }

            if (appearTime >= waitingTime + duration)
            {
                if (!Ending)
                {
                    missionPlayer.Shields.ShieldShine(Way, DrawingColor, score);
                    missionPlayer.Shields.GetCollideChecker(DrawingColor).ArrowBlock(Way);
                }
                Ending = true;
                BeamDisappear();
            }
            _lastPlayerPos = missionPlayer.Centre;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ArrangePos()
        {
            float rotation = missionRotation;
            Vector2 unitU = GetVector2(1, rotation);
            float distance = Vector2.Dot(unitU, Centre - Heart.Centre);
            Centre = Heart.Centre + unitU * distance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalcPush(int dir)
        {
            if (stucked = dir == Way)
                missionPlayer.Shields.Push(this, DrawingColor);
            laserPlace = missionPlayer.Centre + GetVector2(stucked ? -38 + pushDelta : 38, missionRotation);
            pushDelta = missionPlayer.Shields.PushDelta(DrawingColor);
        }

        public override void Draw()
        {
            base.Draw();
            if (appearTime >= waitingTime + timeDelta && laserSize.Y > 0)
            {
                Color finCol = drawingColor * beamAlpha;
                Depth -= 0.001f;
                //Override beam drawing
                DrawingLab.DrawLine(laserPlace + GetVector2(2, missionRotation), Centre + GetVector2(Image.Width * size.X / 2 + 10, missionRotation), (laserSize * size).Y * Sprites.GBLaser.Height, finCol, Depth);
                for (int i = 0; i < 3; i++)
                    DrawingLab.DrawLine(Centre + GetVector2(Image.Width * size.X * (0.5f - i * 0.1f) + 10, missionRotation), Centre + GetVector2(Image.Width * size.X * (0.4f - i * 0.1f) + 10, missionRotation), (laserSize * size).Y * Sprites.GBLaser.Height * (0.8f - i * 0.2f), finCol, Depth);
                //Stuck drawing
                FormalDraw(StuckTexture, laserPlace + GetVector2(2, missionRotation), finCol, 1.33f * laserSize.Y, GetRadian(missionRotation + 180), new Vector2(0, 35));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void GetCollide(Player.Heart p = null)
        {
            _ = p ?? missionPlayer;
            bool alw = Auto;
            if (!stucked || pushDelta > 22)
            {
                if (alw || alpha < 0.5f)
                    return;

                if (appearTime - waitingTime - timeDelta < 4.5f)
                {
                    if (appearTime - waitingTime - timeDelta < 2.5f)
                        return;
                    score = Math.Min(2, score);
                    return;
                }
                if (alpha < 0.97f)
                {
                    if (alpha > 0.6f)
                    {
                        score = Math.Min(2, score);
                        return;
                    }
                    score = 1;
                }
                LoseHP(missionPlayer);
                if (!hasHit)
                {
                    PushScore(0);
                    score = 0;
                    hasHit = true;
                }
            }
            else if (pushDelta > 14 && stucked)
                score = Math.Min(2, score);
            else if (pushDelta > 6 && stucked)
                score = 1;
        }

        public override void Dispose()
        {
            _blasterCount--;
            if (score != 3 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
            {
                if (!hasHit)
                    PushScore(0);

                LoseHP(missionPlayer);
                hasHit = true;
            }
            if (!hasHit)
            {
                if (MarkScore)
                    PushScore(score);
                missionPlayer.Shields.ValidRotated();
            }

            base.Dispose();
        }
    }
    /// <summary>
    /// A normal gaster blaster
    /// </summary>
    public class NormalGB : GasterBlaster, ICollideAble
    {
        /// <summary>
        /// Creates a blaster that automatically aims towards the player
        /// </summary>
        /// <param name="missionPlace">Target position</param>
        /// <param name="spawnPlace">Initial position</param>
        /// <param name="size">Size of the blaster(Width, Height), a small blaster is (1, 0.5f) and a big blaster is (1, 1)</param>
        /// <param name="waitingTime">Time required to pass before firing</param>
        /// <param name="duration">Duration of the blast</param>
        public NormalGB(Vector2 missionPlace, Vector2 spawnPlace, Vector2 size, float waitingTime, float duration) : this(missionPlace, spawnPlace, size,
            (float)(Math.Atan2(Heart.Centre.Y - missionPlace.Y, Heart.Centre.X - missionPlace.X) * 180 / Math.PI), waitingTime, duration)
        { }
        /// <summary>
        /// Creates a blaster that aims to the given angle
        /// </summary>
        /// <param name="missionPlace">Target position</param>
        /// <param name="spawnPlace">Initial position</param>
        /// <param name="size">Size of the blaster(Width, Height), a small blaster is (1, 0.55f) and a big blaster is (1, 1)</param>
        /// <param name="rotation">The target rotation of the blaster</param>
        /// <param name="waitingTime">Time required to pass before firing</param>
        /// <param name="duration">Duration of the blast</param>
        public NormalGB(Vector2 missionPlace, Vector2 spawnPlace, Vector2 size, float rotation, float waitingTime, float duration)
        {
            movingScale = waitingTime < 30 ? 0.5f + waitingTime / 90f : 0.93334f - 3f / waitingTime;

            if (!spawnSoundPlayed && AppearVolume > 0)
            {
                PlaySound(Sounds.GBSpawn, AppearVolume);
                spawnSoundPlayed = true;
            }
            Centre = spawnPlace;
            missionRotation = rotation;
            Rotation = GetRandom(0, 359);
            this.missionPlace = missionPlace;
            this.size = size;
            laserSize.X = 1.0f;
            Depth = 0.6f;
            this.waitingTime = waitingTime;
            this.duration = duration;

            rotateWay = (missionRotation - Rotation + 360) % 360 < (360 - missionRotation + Rotation) % 360;
        }

        public override void Draw() => base.Draw();

        public override void Update()
        {
            MoveToMission();

            base.Update();

            if ((int)(appearTime - waitingTime) >= 0 && appearTime <= waitingTime + duration)
                PushDown();

            if (appearTime >= waitingTime + duration)
                BeamDisappear();
        }
        /// <summary>
        /// Whether the enable the bug fix for reverse collision
        /// </summary>
        public bool BugFix { get; set; } = true;
        public override void GetCollide(Player.Heart heart)
        {
            //If the Cos of the angle is < 0, then Theta is (90, 180), therefore the heart is behind blaster
            if (BugFix && Cos(GetVector2(1, Rotation), Centre - Heart.Centre) > 0)
                return;
            if (appearTime <= waitingTime + duration + 2)
            {
                if ((appearTime < waitingTime - 2) || heart.SoulType == 1 || alpha <= 0.8f)
                    return;

                float A, B, C, dist;
                if (Rotation == 0)
                    dist = Centre.X - heart.Centre.X;
                else
                {
                    float k = (float)Math.Tan(GetRadian(Rotation));
                    A = k;
                    B = -1;
                    C = -A * Centre.X - B * Centre.Y;
                    dist = (float)((A * heart.Centre.X + B * heart.Centre.Y + C) / Math.Sqrt(A * A + B * B));
                }
                float res = Math.Abs(dist) - (32 * laserSize.Y * size.Y - 2);

                if (res < 0)
                {
                    if (!hasHit)
                    {
                        PushScore(0);
                    }
                    LoseHP(heart);
                    hasHit = true;
                }
                if (!hasHit && MarkScore)
                {
                    if (res <= 2)
                    {
                        if (score >= 2)
                        { score = 1; Player.CreateCollideEffect(Color.LawnGreen, 3f); }
                    }
                    else if (res <= 5.4f)
                    {
                        if (score >= 3)
                        { score = 2; Player.CreateCollideEffect(Color.LightBlue, 6f); }
                    }
                    if (score != 3 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
                    {
                        if (!hasHit)
                            PushScore(0);

                        LoseHP(heart);
                        hasHit = true;
                    }
                }
            }
        }

        public override void Dispose()
        {
            if (!hasHit && MarkScore)
                PushScore(score);

            base.Dispose();
        }
    }
}