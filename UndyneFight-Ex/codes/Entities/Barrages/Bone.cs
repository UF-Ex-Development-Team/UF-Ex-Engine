using static Microsoft.Xna.Framework.MathHelper;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// The base class of a side bone, being <see cref="UpBone"/>, <see cref="DownBone"/>, <see cref="LeftBone"/>, <see cref="RightBone"/>
    /// </summary>
    public abstract class SideBone : Bone
    {
        private protected float speed;
        /// <summary>
        /// The speed of the bone
        /// </summary>
        public float Speed { set => speed = value; get => speed; }
        private protected float missionLength;
        /// <summary>
        /// The target length of the bone
        /// </summary>
        public float MissionLength { get => missionLength; set => missionLength = value; }
        /// <summary>
        /// The scale of the lerp animation of the length of the bone
        /// </summary>
        public float LengthLerpScale { get; set; } = 0.1f;
    }
    /// <summary>
    /// A bone that sticks at the bottom of the box
    /// </summary>
    public class DownBone : SideBone
    {
        /// <summary>
        /// Creates a bone at the bottom of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the left or right side, true-> right, false-> left</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public DownBone(bool way, float speed, float length)
        {
            alpha = 1.0f;
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.X = way ? controlingBox.Right + 2 : controlingBox.Left - 2;

            Length = missionLength = length;
            this.speed = speed;
            movingWay = way;
        }
        /// <summary>
        /// Creates a bone at the bottom of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the left or right side, true-> right, false-> left</param>
        /// <param name="position">The initial x coordinate of the bone</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public DownBone(bool way, float position, float speed, float length)
        {
            alpha = 1.0f;
            collidingBox.X = position;

            missionLength = length;
            this.speed = speed;
            movingWay = way;
        }

        private readonly bool movingWay;

        public override void Draw() => base.Draw();

        public override void Update()
        {
            Length = Lerp(Length, missionLength, LengthLerpScale);
            collidingBox.X += speed * 0.5f * (movingWay ? -1 : 1);
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.Y = controlingBox.Down - Length / 2 - 3;
            base.Update();
        }
    }
    /// <summary>
    /// A bone that sticks to the top of the box
    /// </summary>
    public class UpBone : SideBone
    {
        /// <summary>
        /// Creates a bone at the top of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the left or right side, true-> right, false-> left</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public UpBone(bool way, float speed, float length)
        {
            alpha = 1.0f;
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.X = way ? controlingBox.Right + 2 : controlingBox.Left - 2;

            Length = missionLength = length;
            this.speed = speed;
            movingWay = way;
        }
        /// <summary>
        /// Creates a bone at the top of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the left or right side, true-> right, false-> left</param>
        /// <param name="position">The initial x coordinate of the bone</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public UpBone(bool way, float position, float speed, float length)
        {
            alpha = 1.0f;
            collidingBox.X = position;

            missionLength = length;
            this.speed = speed;
            movingWay = way;
        }

        private readonly bool movingWay;

        public override void Draw() => base.Draw();

        public override void Update()
        {
            Length = Lerp(Length, missionLength, LengthLerpScale);
            collidingBox.X += speed * 0.5f * (movingWay ? -1 : 1);
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.Y = controlingBox.Up + Length / 2 + 3;
            base.Update();
        }
    }
    /// <summary>
    /// A bone that sticks to the left side of the box
    /// </summary>
    public class LeftBone : SideBone
    {
        /// <summary>
        /// Creates a bone at the left side of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the upper or lower side, true-> lower, false-> upper</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public LeftBone(bool way, float speed, float length)
        {
            Rotation = 90f;
            alpha = 1.0f;
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.Y = way ? controlingBox.Down + 2 : controlingBox.Up - 2;

            Length = missionLength = length;
            this.speed = speed;
            movingWay = way;
        }
        /// <summary>
        /// Creates a bone at the left side of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the upper or lower side, true-> lower, false-> upper</param>
        /// <param name="position">The initial y coordinate of the bone</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public LeftBone(bool way, float position, float speed, float length)
        {
            Rotation = 90f;
            alpha = 1.0f;
            collidingBox.Y = position;

            missionLength = length;
            this.speed = speed;
            movingWay = way;
        }

        private readonly bool movingWay;

        public override void Draw() => base.Draw();

        public override void Update()
        {
            Length = Lerp(Length, missionLength, LengthLerpScale);
            collidingBox.Y += speed * 0.5f * (movingWay ? -1 : 1);
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.X = controlingBox.Left + Length / 2 + 3;
            base.Update();
        }
    }
    /// <summary>
    /// A bone that sticks to the right side of the box
    /// </summary>
    public class RightBone : SideBone
    {
        /// <summary>
        /// Creates a bone at the right side of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the upper or lower side, true-> lower, false-> upper</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public RightBone(bool way, float speed, float length)
        {
            Rotation = 90f;
            alpha = 1.0f;
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.Y = way ? controlingBox.Down + 2 : controlingBox.Up - 2;

            Length = missionLength = length;
            this.speed = speed;
            movingWay = way;
        }
        /// <summary>
        /// Creates a bone at the right side of the box
        /// </summary>
        /// <param name="way">Whether to spawn on the upper or lower side, true-> lower, false-> upper</param>
        /// <param name="position">The initial y coordinate of the bone</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="speed">The speed of the bone</param>
        public RightBone(bool way, float position, float speed, float length)
        {
            Rotation = 90f;
            alpha = 1.0f;
            collidingBox.Y = position;

            missionLength = length;
            this.speed = speed;
            movingWay = way;
        }

        private readonly bool movingWay;

        public override void Draw() => base.Draw();

        public override void Update()
        {
            Length = Lerp(Length, missionLength, LengthLerpScale);
            collidingBox.Y += speed * 0.5f * (movingWay ? -1 : 1);
            RectangleBox controlingBox = this.controlingBox as RectangleBox;
            collidingBox.X = controlingBox.Right - Length / 2 - 3;
            base.Update();
        }
    }
    /// <summary>
    /// A rotating bone at the center of the box
    /// </summary>
    public class CentreCircleBone : Bone
    {
        private float rotateSpeed;
        /// <summary>
        /// The rotation speed of the bone
        /// </summary>
        public float RotateSpeed { set => rotateSpeed = value; }
        /// <summary>
        /// The target length of the bone
        /// </summary>
        public float MissionLength { set => missionLength = value; }

        private readonly float duration;
        private float missionLength;
        private float appearTime = 0;
        /// <summary>
        /// Creates a rotating bone at the center of the box
        /// </summary>
        /// <param name="startRotation">The initial angle of the bone</param>
        /// <param name="rotateSpeed">The rotation speed of the bone</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="duration">The duration of the bone</param>
        public CentreCircleBone(float startRotation, float rotateSpeed, float length, float duration)
        {
            Centre = controlingBox.Centre;
            alpha = 1.0f;
            IsMasked = false;
            missionLength = length;
            this.duration = duration;
            this.rotateSpeed = rotateSpeed;
            Rotation = startRotation;
        }

        public override void Draw() => base.Draw();

        public override void Update()
        {
            Centre = controlingBox.Centre;
            appearTime += 0.5f;
            Length = appearTime >= duration ? Length * 0.96f - 0.25f : Length * 0.96f + missionLength * 0.04f;
            Rotation += rotateSpeed * 0.5f;
            base.Update();
            if (Length <= 1f)
                Dispose();
        }
    }
    /// <summary>
    /// A rotating bone at the side of the box (The box must be a square)
    /// </summary>
    public class SideCircleBone : Bone
    {
        /// <summary>
        /// Creates a rotating bone at the side of the box (The box must be a square)
        /// </summary>
        /// <param name="startRotation">The initial angle of the bone</param>
        /// <param name="rotateSpeed">The angular speed of the bone</param>
        /// <param name="length">The length of the bone</param>
        /// <param name="duration">The duration of the bone</param>
        public SideCircleBone(float startRotation, float rotateSpeed, float length, float duration)
        {
            autoDispose = false;
            IsMasked = false;
            Rotation = startRotation + 360;
            alpha = 1;
            RotateSpeed = rotateSpeed;
            missionLength = length;
            length1 = -controlingBox.CollidingBox.Width * 0.25f;
            this.duration = duration;
        }

        private float length1, appearTime = 0;
        private readonly float duration, missionLength;
        /// <summary>
        /// The angular speed of the bone
        /// </summary>
        public float RotateSpeed { set; get; }

        public override void Draw() => base.Draw();

        public override void Update()
        {
            if (appearTime <= duration)
                length1 = length1 * 0.93f + missionLength * 0.07f;
            appearTime += 0.5f;
            float r = controlingBox.CollidingBox.Width / 2,
                  alpha = (Rotation + 270) % 90, cosV;
            cosV = MathF.Cos(GetRadian(alpha <= 45f ? alpha : 90 - alpha));

            float dist1 = r - length1 / 2,
                  dist2 = r / cosV - 3;

            if (appearTime > duration)
                dist1 += MathF.Pow(appearTime - duration, 2) / 3;
            float dist = (dist1 + dist2) / 2;

            Length = dist2 - dist1;
            if (Length < -50)
                Dispose();

            Centre = controlingBox.Centre + GetVector2(dist, Rotation + 90);

            Rotation += RotateSpeed * 0.5f;
            if (Rotation < 360)
                Rotation += 360;

            base.Update();
        }
    }
    /// <summary>
    /// A bone that cycles inside the box
    /// </summary>
    public class SwarmBone : Bone
    {
        /// <summary>
        /// Creates a bone that cycles inside the box
        /// </summary>
        /// <param name="length">The length of the bone</param>
        /// <param name="roundTime">The duration of one cycle</param>
        /// <param name="startTime">The initial time in the cycle</param>
        /// <param name="duration">The duration of the bone</param>
        public SwarmBone(float length, float roundTime, float startTime, float duration)
        {
            missionLength = length;
            this.duration = duration;
            this.startTime = startTime;
            this.roundTime = roundTime;
            alpha = 1f;
        }

        private readonly float missionLength, duration, roundTime, startTime;
        private float appearTime = 0;

        public override void Update()
        {
            if (appearTime >= duration)
            {
                Length -= 0.5f;
                Length *= 0.82f;
                if (Length < 0)
                    Dispose();
            }
            else
                Length = Length * 0.86f + missionLength * 0.14f;

            float trueVal = (appearTime + startTime) / roundTime * 360 % 360;
            float X = Cos(trueVal) * (controlingBox.CollidingBox.Width - 2) / 2;
            float heightDelta = controlingBox.CollidingBox.Height - Length - 2;
            float res1 = Math.Abs(Sin(trueVal));
            float Y = trueVal > 180 ? -MathF.Sqrt(res1) : MathF.Sqrt(res1);

            Centre = new Vector2(X, Y * heightDelta / 2) + controlingBox.Centre;

            appearTime += 0.5f;

            base.Update();
        }
    }
    /// <summary>
    /// A bone with custom parameters
    /// </summary>
    public class CustomBone : Bone, ICustomMotion, ICustomLength
    {
        /// <summary>
        /// Whether the bone will have a ffade in animation
        /// </summary>
        public bool AlphaIncrease { get; set; } = false;
        /// <summary>
        /// The range the bone can exist in (Once left this rectangle, the bone will be disposed)
        /// </summary>

        public CollideRect screenC = new(-50, -50, 740, 580);

        private readonly Vector2 startPos;
        public Func<ICustomLength, float> LengthRoute { get; set; }

        public new Vector2 CentrePosition => delta;
        public float[] LengthRouteParam { get; set; }

        public Func<CustomBone, float> AlphaRoute { private get; set; }

        public new float AppearTime { get; private set; } = 0;
        /// <summary>
        /// The extra rotation angle of the bone
        /// </summary>
        public float RotationDelta { set => rotationDelta = value; }

        private float rotationDelta;
        private Vector2 delta;

        /// <summary>
        /// Creates a custom bone with position easing, rotation easing, and fixed length
        /// </summary>
        /// <param name="positionRoute">The easing of the position of the bone</param>
        /// <param name="rotationRoute">The easing of the rotation of the bone</param>
        /// <param name="length">The length of the bone</param>
        public CustomBone(EaseUnit<Vector2> positionRoute, EaseUnit<float> rotationRoute, float length) : this(Vector2.Zero, positionRoute.Easing, (s) => length, rotationRoute.Easing) { }
        /// <summary>
        /// Craetes a custom bone with a custom position route, rotation easing, fixed length, and specified duration
        /// </summary>
        /// <param name="startPos">The initial position of the bone</param>
        /// <param name="positionRoute">The route of the position of the bone (Delta positioning, therefore the position of the bone will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
        /// <param name="rotationRoute">The easing of the rotation of the bone</param>
        /// <param name="len">The length of the bone</param>
        /// <param name="duration">The duration of the bone</param>
        public CustomBone(Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, EaseUnit<float> rotationRoute, float len, float duration) : this(startPos, positionRoute, Motions.LengthRoute.autoFold, Motions.RotationRoute.stableValue)
        {
            RotationRoute = rotationRoute;
            LengthRouteParam = [len, duration];
        }
        /// <summary>
        /// Craetes a custom bone with a custom position route, fixed rotation, fixed length, and specified duration
        /// </summary>
        /// <param name="startPos">The initial position of the bone</param>
        /// <param name="positionRoute">The route of the position of the bone (Delta positioning, therefore the position of the bone will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
        /// <param name="rot">The rotation of the bone</param>
        /// <param name="len">The length of the bone</param>
        /// <param name="duration">The duration of the bone</param>
        public CustomBone(Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, float rot, float len, float duration) : this(startPos, positionRoute, Motions.LengthRoute.autoFold, Motions.RotationRoute.stableValue)
        {
            RotationRouteParam = [rot];
            LengthRouteParam = [len, duration];
        }

        /// <summary>
        /// Craetes a custom bone with a custom position route, fixed rotation, and fixed length
        /// </summary>
        /// <param name="startPos">The initial position of the bone</param>
        /// <param name="positionRoute">The route of the position of the bone (Delta positioning, therefore the position of the bone will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
        /// <param name="rot">The rotation of the bone</param>
        /// <param name="len">The length of the bone</param>
        public CustomBone(Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, float rot, float len) : this(startPos, positionRoute, Motions.LengthRoute.stableValue, Motions.RotationRoute.stableValue)
        {
            RotationRouteParam = [rot];
            LengthRouteParam = [len];
        }
        /// <summary>
        /// Creates a custom bone with custom position route, custom length route, and custom position route
        /// </summary>
        /// <param name="startPos">The initial position of the bone</param>
        /// <param name="positionRoute">The route of the position of the bone (Delta positioning, therefore the position of the bone will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
        /// <param name="rotationRoute">The route of the position of the bone (Delta positioning, therefore the position of the bone will be the sum of <see cref="RotationDelta"/> and <paramref name="rotationRoute"/></param>
        /// <param name="lengthRoute">The route of the length of the bone</param>
        public CustomBone(Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, Func<ICustomLength, float> lengthRoute, Func<ICustomMotion, float> rotationRoute)
        {
            alpha = 1.0f;

            Centre = startPos;
            this.startPos = startPos;
            PositionRoute = positionRoute;
            LengthRoute = lengthRoute;
            RotationRoute = rotationRoute;
        }
        /// <summary>
        /// Creates a custom bone with custom position route, custom position route, and a fixed length
        /// </summary>
        /// <param name="startPos">The initial position of the bone</param>
        /// <param name="positionRoute">The route of the position of the bone (Delta positioning, therefore the position of the bone will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
        /// <param name="rotationRoute">The route of the position of the bone (Delta positioning, therefore the position of the bone will be the sum of <see cref="RotationDelta"/> and <paramref name="rotationRoute"/></param>
        /// <param name="length">The length of the bone</param>
        public CustomBone(Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, Func<ICustomMotion, float> rotationRoute, float length)
        {
            alpha = 1.0f;

            LengthRouteParam = [length];
            Centre = startPos;
            this.startPos = startPos;
            PositionRoute = positionRoute;
            LengthRoute = Motions.LengthRoute.stableValue;
            RotationRoute = rotationRoute;
        }
        public override void Draw() => base.Draw();

        public override void Update()
        {
            alpha = AlphaIncrease ? Math.Min(1, AppearTime / 20f) : AlphaRoute?.Invoke(this) ?? alpha;
            AppearTime += 0.5f;
            Centre = startPos + (delta = PositionRoute(this));
            Rotation = RotationRoute(this) + rotationDelta;
            Length = LengthRoute(this);

            if (Length < -1)
                Dispose();

            base.Update();
        }
    }
}