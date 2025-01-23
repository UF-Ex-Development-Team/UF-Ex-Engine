using static System.Math;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// A normal spear
    /// </summary>
    public class NormalSpear : Spear
    {
        private readonly float missionRotation = 0;
        /// <summary>
        /// The speed of the spear (Default 1.7f)
        /// </summary>
        public float Speed { set; private get; } = 1.7f;
        /// <summary>
        /// The acceleration of the spear (Default 0.12f)
        /// </summary>
        public float Acceleration { set; private get; } = 0.12f;
        private int appearTime = 0;
        /// <summary>
        /// Whether the spear won't play the sound effects
        /// </summary>
        public bool IsMute { set; private get; }
        /// <summary>
        /// Whether will it aim at the soul or not
        /// </summary>
        public bool DelayTargeting { set; private get; }
        /// <summary>
        /// The time to wait before launch (Default 59 frames, a little less than 1 second (62.5f))
        /// </summary>
        public float WaitingTime { get; set; } = 59;
        /// <summary>
        /// Whether the spear will bounce when reaching the <see cref="ReboundVertices"/>
        /// </summary>
        public bool Rebound { get; set; } = false;
        /// <summary>
        /// The amount of times to bounce before stopping to bounce
        /// </summary>
        public int ReboundCount { get; set; } = 3;
        /// <summary>
        /// The list of vertices to bounce off from
        /// </summary>
        public Vector2[] ReboundVertices { get; set; } = [new(0, 0), new(640, 0), new(640, 480), new(0, 480)];
        /// <summary>
        /// The duration of the spear (Default 200 frames)
        /// </summary>
        public float Duration { private get; set; } = 200;
        /// <summary>
        /// Creates a normal spear
        /// </summary>
        /// <param name="centre">The position to create the spear</param>
        /// <param name="rotation">The angle of the spear (Default aiming towards the player)</param>
        /// <param name="speed">The speed of the spear (Default 1.7f)</param>
        public NormalSpear(Vector2 centre, float? rotation = null, float speed = 1.7f)
        {
            Rotation = Rand(0, 359f);
            Centre = centre;
            missionRotation = rotation ?? (MathF.Atan2(Heart.Centre.Y - centre.Y, Heart.Centre.X - centre.X) * 180 / MathF.PI);
            Speed = speed;
        }
        /// <inheritdoc/>
        public override void Update()
        {
            if (appearTime == 0 && !IsMute)
                FightResources.Sounds.spearAppear.CreateInstance().Play();
            appearTime++;
            if (appearTime < WaitingTime)
                Rotation += MathF.Pow(WaitingTime + 8 - appearTime, 1.5f) / 31 * (59 / WaitingTime);
            else if (appearTime == (int)WaitingTime + 1)
            {
                Rotation = DelayTargeting ? MathF.Atan2(Heart.Centre.Y - Centre.Y, Heart.Centre.X - Centre.X) * 180 / MathF.PI
                    : missionRotation;

                if (!IsMute)
                    FightResources.Sounds.spearShoot.CreateInstance().Play();
            }
            else
                Centre += GetVector2(Speed += Acceleration, Rotation);
            if (appearTime >= Duration)
            {
                alpha -= 0.16f;
                if (alpha <= 0)
                    Dispose();
            }
            else
                alpha = Min(appearTime, 25) * 0.04f;

            base.Update();
        }
        /// <inheritdoc/>
        public override void Draw() => base.Draw();
    }
    /// <summary>
    /// A spear with alpha fade in animation
    /// </summary>
    public class Pike : Spear
    {
        private float speed = 9.7f;
        private readonly float waitingTime;
        private int appearTime = 0;

        private bool isSpawnMute = false, isShootMute = false;
        internal static bool shootSoundPlayed = false, spawnSoundPlayed = false;
        /// <summary>
        /// The speed of the spear (Default 9.7f)
        /// </summary>
        public float Speed { set => speed = value; }
        /// <summary>
        /// The acceleration of the spear
        /// </summary>
        public float Acceleration { private get; set; } = 0.41f;
        /// <summary>
        /// Whether the spawn sound is muted
        /// </summary>
        public bool IsSpawnMute { set => isSpawnMute = value; }
        /// <summary>
        /// Whether the shooting sound is muted
        /// </summary>
        public bool IsShootMute { set => isShootMute = value; }

        private readonly float alphaChangeTime = 20;
        /// <summary>
        /// Creates a spear with alpha fade in animation
        /// </summary>
        /// <param name="centre">The position to create the spear</param>
        /// <param name="rotation">The angle of the spear</param>
        /// <param name="waitingTime">The delay before the spear shoots</param>
        public Pike(Vector2 centre, float rotation, float waitingTime)
        {
            alphaChangeTime = 20 - 12 / waitingTime;
            Centre = centre;
            Rotation = rotation;
            this.waitingTime = waitingTime;
        }
        /// <summary>
        /// Creates a spear with alpha fade in animation
        /// </summary>
        /// <param name="centre">The position to create the spear</param>
        /// <param name="rotation">The angle of the spear</param>
        /// <param name="speed">The speed of the spear</param>
        /// <param name="waitingTime">The delay before the spear shoots</param>
        public Pike(Vector2 centre, float rotation, float speed, float waitingTime) : this(centre, rotation, waitingTime) => this.speed = speed;
        /// <inheritdoc/>
        public override void Update()
        {
            if (appearTime == 0 && (!isSpawnMute) && (!spawnSoundPlayed))
            {
                spawnSoundPlayed = true;
                FightResources.Sounds.spearAppear.CreateInstance().Play();
            }
            appearTime++;
            alpha = Max(alpha, Min(appearTime, alphaChangeTime) / alphaChangeTime);
            if (appearTime < waitingTime && appearTime <= 72)
                Centre += GetVector2((float)Math.Cos(appearTime / 25f), Rotation);
            else if (appearTime >= waitingTime)
            {
                if (appearTime == waitingTime && (!shootSoundPlayed) && (!isShootMute))
                {
                    shootSoundPlayed = true;
                    FightResources.Sounds.spearShoot.CreateInstance().Play();
                }
                Centre += GetVector2(speed += Acceleration, Rotation);
            }
            if (appearTime >= waitingTime + 240)
                Dispose();

            base.Update();
        }
    }
    /// <summary>
    /// Creates a spear that aims towards a center
    /// </summary>
    /// <param name="rotateCentre">The center of the circle</param>
    /// <param name="linearSpeed">The speed of the spear</param>
    /// <param name="distance">The initial distance between the spear and the target</param>
    /// <param name="rotation">The angle of the spear with respect to the center</param>
    /// <param name="waitingTime">The time delay before shooting</param>
    public class SwarmSpear(Vector2 rotateCentre, float linearSpeed, float distance, float rotation, float waitingTime) : Spear
    {
        private Vector2 missionCentre = rotateCentre;
        private int appearTime = 0;
        private readonly float acclen = 0.15f;
        private float linearSpeed = linearSpeed;
        private float distance = distance;
        private readonly float waitingTime = waitingTime;
        private readonly float missionRotation = rotation;
        private float appearRotation = 320f;
        /// <inheritdoc/>
        public override void Update()
        {
            appearTime++;
            if (appearTime <= 22f)
                alpha = appearTime / 22f;

            if (appearTime > waitingTime)
            {
                linearSpeed += acclen;
                distance -= linearSpeed;
            }
            else
            {
                if (waitingTime < 34)
                {
                    appearRotation *= 1 - 1.3f / waitingTime;
                    appearRotation -= 5f / waitingTime;
                }
                appearRotation -= 0.8f;
                appearRotation = Max(appearRotation * 0.9f, 0);
            }

            Rotation = missionRotation;

            Centre = missionCentre + GetVector2(distance, Rotation + 180);

            Rotation += appearRotation;

            if (distance < 10)
                alpha -= 0.08f;

            if (alpha < 0.12f && distance < 10f)
                Dispose();

            base.Update();
        }
        /// <inheritdoc/>
        public override void Draw() => base.Draw();
    }
    /// <summary>
    /// Creates a spear that moves around the target with a circular motion
    /// </summary>
    public class CircleSpear : Spear
    {
        public Vector2 rotateCentre;
        private int appearTime = 0;
        public float rotateSpeed;
        public float linearSpeed;
        public float distance;
        public readonly float rotateFriction = 0.01f;
        public readonly float rotateAngleDisplace = 0;
        /// <summary>
        /// Creates a spear that moves around the target with a circular motion
        /// </summary>
        /// <param name="rotateCentre">The center of the circle</param>
        /// <param name="rotateSpeed">The angular speed of the spear</param>
        /// <param name="linearSpeed">The speed of the spear after shooting</param>
        /// <param name="distance">The initial distance between the spear and the target</param>
        /// <param name="rotation">The angle of the spear with respect to the center</param>
        /// <param name="rotateFriction">The friction of the angular rotation (Default 0.01f)</param>
        /// <param name="rotate_extra">The extra angle of the spear (Default 0)</param>
        public CircleSpear(Vector2 rotateCentre, float rotateSpeed, float linearSpeed, float distance, float rotation, float rotateFriction = 0.01f, float rotate_extra = 0)
        {
            autoDispose = false;
            this.rotateSpeed = rotateSpeed;
            this.linearSpeed = linearSpeed;
            this.distance = distance;
            Rotation = rotation;
            this.rotateCentre = rotateCentre;
            this.rotateFriction = rotateFriction;
            rotateAngleDisplace = rotate_extra;
        }
        /// <inheritdoc/>
        public override void Update()
        {
            appearTime++;
            if (appearTime <= 15f)
                alpha = appearTime / 15f;

            Rotation += rotateSpeed;
            rotateSpeed *= 1 - rotateFriction;

            Centre = rotateCentre + GetVector2(distance -= linearSpeed, Rotation + 180);

            if (distance < 10)
                alpha -= 0.08f;

            if (alpha < 0.12f && distance < 10f)
                Dispose();

            base.Update();
        }
        /// <inheritdoc/>
        public override void Draw() => FormalDraw(Image, Centre, drawingColor * alpha, GetRadian(Rotation + rotateAngleDisplace), ImageCentre);
    }
    /// <summary>
    /// Warning, this spear type is rarely used and TK is unsure whether it will function properly as it did not function properly during the development of TaS
    /// </summary>
    public class CustomSpear : Spear, ICustomMotion
    {
        /// <inheritdoc/>
        public Vector2 CentrePosition => delta;
        /// <inheritdoc/>
        public float[] PositionRouteParam { get; set; }
        /// <inheritdoc/>
        public float[] RotationRouteParam { get; set; }
        /// <inheritdoc/>
        public float AppearTime { get; private set; } = 0;
        /// <inheritdoc/>
        public Func<ICustomMotion, Vector2> PositionRoute { get; set; }
        /// <inheritdoc/>
        public Func<ICustomMotion, float> RotationRoute { get; set; }
        private Vector2 delta, startPos;
        /// <inheritdoc/>
        public CustomSpear(Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, Func<ICustomMotion, float> rotationRoute)
        {
            UpdateIn120 = true;
            alpha = 0.0f;
            Centre = startPos;
            this.startPos = startPos;
            PositionRoute = positionRoute;
            RotationRoute = rotationRoute;
        }
        /// <inheritdoc/>
        public override void Update()
        {
            AppearTime += 0.5f;
            Centre = startPos + (delta = PositionRoute(this));
            Rotation = RotationRoute(this);
            base.Update();
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlphaIncrease(float time)
        {
            float del = (1 - alpha) / (time * 2f);
            AddInstance(new TimeRangedEvent(time, () => alpha = Min(1, alpha + del)) { UpdateIn120 = true });
            AddInstance(new InstantEvent(time + 0.5f, () => alpha = 1));
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlphaDecrease(float time)
        {
            float del = alpha / (time * 2f);
            AddInstance(new TimeRangedEvent(time, () => alpha = Max(0, alpha - del)) { UpdateIn120 = true });
            AddInstance(new InstantEvent(time + 0.5f, () => { alpha = 0; Dispose(); }));
        }
    }
}