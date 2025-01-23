using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.DrawingLab;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex
{
    /// <summary>
    /// An interface for colliding shapes
    /// </summary>
    public interface ICollidingComponent
    {
        /// <summary>
        /// Checks if it is colliding to another <see cref="ICollidingComponent"/>
        /// </summary>
        /// <param name="_component">The <see cref="ICollidingComponent"/> to check collision with</param>
        /// <returns>Whether they are collided</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CollideWith(ICollidingComponent _component);
    }
    internal static class CheckCollision
    {
        internal static Dictionary<Type, Dictionary<Type, Func<ICollidingComponent, ICollidingComponent, bool>>> collisionCheck = [];
        internal static void Initialize() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TakeCheck(ICollidingComponent s1, ICollidingComponent s2) => collisionCheck[s1.GetType()].ContainsKey(s2.GetType())
                ? collisionCheck[s1.GetType()][s2.GetType()].Invoke(s1, s2)
                : collisionCheck[s2.GetType()].ContainsKey(s1.GetType())
                ? collisionCheck[s2.GetType()][s1.GetType()].Invoke(s2, s1)
                : throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CircleWithCircle(CollidingCircle s1, CollidingCircle s2) => MathUtil.InRange(GetDistance(s1.position, s2.position), MathF.Abs(s1.radius - s2.radius), s1.radius + s2.radius);
    }
    /// <summary>
    /// A line segment with collision
    /// </summary>
    public struct CollidingSegment : ICollidingComponent
    {
        internal Vector2 v1, v2;
        /// <summary>
        /// Creates a line segment
        /// </summary>
        /// <param name="v1">The first vertex of the line</param>
        /// <param name="v2">The otehr vertex of the line</param>
        public CollidingSegment(Vector2 v1, Vector2 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }
        /// <summary>
        /// Creates a line segment
        /// </summary>
        /// <param name="centre">The center of the line</param>
        /// <param name="length">The length of the line</param>
        /// <param name="rotation">The rotation of the line</param>
        public CollidingSegment(Vector2 centre, float length, float rotation)
        {
            v1 = centre + GetVector2(length / 2, rotation);
            v2 = centre - GetVector2(length / 2, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool CollideWith(ICollidingComponent _component) => CheckCollision.TakeCheck(this, _component);
    }
    /// <summary>
    /// A circle with collision
    /// </summary>
    /// <param name="position">The position of the circle</param>
    /// <param name="radius">The radius of the circle</param>
    public struct CollidingCircle(Vector2 position, float radius) : ICollidingComponent
    {
        internal Vector2 position = position;
        internal float radius = radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool CollideWith(ICollidingComponent _component) => CheckCollision.TakeCheck(this, _component);
    }
    /// <summary>
    /// Used for detecting blue soul platforms
    /// </summary>
    public class GravityLine
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reload() => reloadTime = 4;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Recover() => reloadTime--;

        private static int reloadTime = 0;
        public bool enabled = true;

        private bool IsEnable => reloadTime <= 0 && enabled;

#if DEBUG
        private Vector2 v1, v2;
#endif
        public static HashSet<GravityLine> GravityLines = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => GravityLines.Remove(this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(Vector2 v1, Vector2 v2)
        {
#if DEBUG
            this.v1 = v1;
            this.v2 = v2;
#endif
            Vector2 v = (v1 + v2) / 2, old = Centre;
            Centre = v;
            float old_ = Rotation;
            Rotation = MathF.Atan2(v1.Y - v2.Y, v1.X - v2.X);
            float delta = Rotation - old_;
            if (v1.X == v2.X)
            {
                A = 1;
                B = 0;
                C = -Centre.X;
            }
            else
            {
                float k = (v1.Y - v2.Y) / (v1.X - v2.X);
                A = k;
                B = -1;
                C = -A * Centre.X - B * Centre.Y;
            }
            if (isCollide && Heart.SoulType == 2)
            {
                collidePlayers.ForEach(s =>
                {
                    Vector2 oldPos = s.Centre;
                    float dx = s.Centre.X - Centre.X, dy = s.Centre.Y - Centre.Y;
                    Vector2 delta_ = v - old;
                    if (Math.Abs(delta) > 1e-5f)
                    {
                        float ori = MathF.Atan2(dy, dx);
                        float length = GetDistance(Centre, s.Centre);
                        delta_ += Centre + GetVector2(length, (ori + delta) / PI * 180) - s.Centre;
                    }
                    if (sticky)
                        s.Centre += delta_;
                    else
                    {
                        Vector2 v_ = new(MathF.Cos(NormalRotation), MathF.Sin(NormalRotation));
                        if (delta_.Length() > 0.001f)
                            s.Centre += v_ * Cos(v_, delta_) * delta_.Length();
                    }
                });
            }
            collidePlayers.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(float length) => this.length = length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWidth(float width) => this.width = width;

        public GravityLine(Vector2 v1, Vector2 v2)
        {
#if DEBUG
            this.v1 = v1;
            this.v2 = v2;
#endif
            GravityLines.Add(this);
            Centre = (v1 + v2) / 2;
            Rotation = MathF.Atan2(v1.Y - v2.Y, v1.X - v2.X);
            if (v1.Y == v2.Y)
            {
                A = 1;
                B = 0;
                C = -Centre.X;
            }
            else
            {
                float k = (v1.Y - v2.Y) / (v1.X - v2.X);
                A = k;
                B = -1;
                C = -A * Centre.X - B * Centre.Y;
            }
        }

        private float A, B, C, length = 1000, width = 0;
        private Vector2 Centre;

        public float Rotation { get; private set; } = 0;
        public float NormalRotation => Rotation - PI / 2f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Distance(Player.Heart heart) => (float)((A * heart.Centre.X + B * heart.Centre.Y + C) / Math.Sqrt(A * A + B * B));

        private bool isCollide;
        public bool sticky = true;
        private readonly List<Player.Heart> collidePlayers = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCollideWith(Player.Heart player)
        {
            if (!IsEnable)
                return false;
            if (isCollide = Math.Abs(Distance(player)) <= 8.01f + width && GetDistance(player.Centre, Centre) <= (length / 2 + 6))
            {
                float dx = player.Centre.X - Centre.X, dy = player.Centre.Y - Centre.Y;
                Vector2 v1 = new(dx, dy);
                Vector2 v2 = GetVector2(1, player.Rotation - 90);
                if (Vector2.Dot(v1, v2) < 0)
                {
                    isCollide = false;
                    goto A;
                }
                collidePlayers.Add(player);
                return true;
            }
        A:
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 CorrectPosition(Player.Heart player)
        {
            Vector2 v_ = new(MathF.Cos(NormalRotation), MathF.Sin(NormalRotation));
            Vector2 change = v_ * (8 + width - Cos(player.Centre - Centre, v_) * GetDistance(player.Centre, Centre)) * 0.25f;
            return change;
        }
        public void Draw()
        {
#if DEBUG
            if (!ModeLab.showCollide)
                return;
            if (!IsEnable)
                DrawLine(v1, v2, 2, Color.Red, 0.999f);
            else if (!isCollide)
                DrawLine(v1, v2, 2, Color.Green, 0.999f);
            else
                DrawLine(v1, v2, 2, Color.Gold, 0.999f);
            DrawVector(Centre, NormalRotation);
#endif
        }
    }
    /// <summary>
    /// A rectangle with collision
    /// </summary>
    /// <param name="X">The x coordinate of the top left corner of the rectangle</param>
    /// <param name="Y">The y coordinate of the top left corner of the rectangle</param>
    /// <param name="Width">The width of the rectangle</param>
    /// <param name="Height">The height of the rectangle</param>
    public struct CollideRect(float X, float Y, float Width, float Height) : ICollidingComponent
    {
        /// <summary>
        /// The width of the rectangle
        /// </summary>
        public float Width { get; set; } = Width;
        /// <summary>
        /// The height of the rectangle
        /// </summary>
        public float Height { get; set; } = Height;
        /// <summary>
        /// The x coordiante of the center of the rectangle
        /// </summary>
        public float X { get; set; } = X;
        /// <summary>
        /// The y coordiante of the center of the rectangle
        /// </summary>
        public float Y { get; set; } = Y;
        /// <summary>
        /// Gets the <see cref="Vector2"/> coordinates of the vertices
        /// </summary>
        /// <returns>The array of vertices</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2[] GetVertices() => [new(Left, Up), new(Right, Up), new(Right, Down), new(Left, Down)];
        /// <summary>
        /// Whether the rectangle is colliding with another <see cref="ICollidingComponent"/>
        /// </summary>
        /// <param name="_component">The <see cref="ICollidingComponent"/> to check collision with</param>
        /// <returns>Whether the two <see cref="ICollidingComponent"/>s are colliding</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool CollideWith(ICollidingComponent _component) => CheckCollision.TakeCheck(this, _component);
        /// <summary>
        /// Creates a rectangle with collision with the given position and size
        /// </summary>
        /// <param name="pos">The position of the rectangle</param>
        /// <param name="size">The dimensions of the rectangle</param>
        public CollideRect(Vector2 pos, Vector2 size) : this(pos.X, pos.Y, size.X, size.Y) { }
        /// <summary>
        /// Crates a reatangle with collision from an <see cref="Rectangle"/>
        /// </summary>
        /// <param name="rec">The <see cref="Rectangle"/> to create from</param>
        public CollideRect(Rectangle rec) : this(rec.X, rec.Y, rec.Width, rec.Height) { }
        /// <summary>
        /// Offsets the position of the rectangle
        /// </summary>
        /// <param name="vect">The vector displacement of the rectangle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(Vector2 vect) { X += vect.X; Y += vect.Y; }
        /// <summary>
        /// Gets the centre of the rectangle
        /// </summary>
        /// <returns>The centre of the rectangle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 GetCentre() => new(X + Width / 2, Y + Height / 2);
        /// <summary>
        /// Sets the centre of the rectangle
        /// </summary>
        /// <param name="Centre">The coordinates of the centre</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCentre(Vector2 Centre) => Offset(Centre - GetCentre());
        /// <summary>
        /// Sets the centre of the rectangle
        /// </summary>
        /// <param name="X">The x coordiante of the center of the rectangle</param>
        /// <param name="Y">The y coordiante of the center of the rectangle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCentre(float X, float Y) => Offset(new Vector2(X, Y) - GetCentre());
        /// <summary>
        /// Whether the rectangle is colliding with another <see cref="CollideRect"/>
        /// </summary>
        /// <param name="collideRectAno">The other <see cref="CollideRect"/> to check with</param>
        /// <returns>Whether there is collision</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Intersects(CollideRect collideRectAno)
        {
            Vector2 C1 = GetCentre(), C2 = collideRectAno.GetCentre();
            float X_Max = Width / 2 + collideRectAno.Width / 2;
            float Y_Max = Height / 2 + collideRectAno.Height / 2;
            return Math.Abs((C1 - C2).X) <= X_Max && Math.Abs((C1 - C2).Y) <= Y_Max;
        }
        /// <summary>
        /// Checks whether does the rectangle contain the given point
        /// </summary>
        /// <param name="vect">The point to check</param>
        /// <returns>Whether the point is inside the rectangle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contain(Vector2 vect)
        {
            Vector2 anyz = GetCentre() - vect;
            return Math.Abs(anyz.X) <= Width / 2 && Math.Abs(anyz.Y) <= Height / 2;
        }
        /// <summary>
        /// Converts a <see cref="Rectangle"/> to <see cref="CollideRect"/>
        /// </summary>
        /// <returns>The <see cref="CollideRect"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Rectangle ToRectangle() => new((int)X, (int)Y, (int)Width, (int)Height);
        /// <summary>
        /// The bottom left corner of the rectangle
        /// </summary>
        public Vector2 BottomLeft
        {
            get => new(X, Y + Size.Y);
            set
            {
                X = value.X;
                Y = value.Y - Size.Y;
            }
        }
        /// <summary>
        /// The bottom right corner of the rectangle
        /// </summary>
        public Vector2 BottomRight
        {
            get => new(X + Size.X, Y + Size.Y);
            set
            {
                X = value.X - Size.X;
                Y = value.Y - Size.Y;
            }
        }/// <summary>
        /// The top left corner of the rectangle
        /// </summary>
        public Vector2 TopLeft
        {
            get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        /// <summary>
        /// The top right corner of the rectangle
        /// </summary>
        public Vector2 TopRight
        {
            get => new(X + Size.X, Y);
            set
            {
                X = value.X - Size.X;
                Y = value.Y;
            }
        }
        /// <summary>
        /// The dimensions of the rectangle
        /// </summary>
        public Vector2 Size
        {
            get => new(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        public static implicit operator Rectangle(CollideRect rect) => rect.ToRectangle();
        public static implicit operator CollideRect(Rectangle rect) => new(rect);
        /// <summary>
        /// The y coordinate of the upper side of the rectangle
        /// </summary>
        public float Up => Y;
        /// <summary>
        /// The y coordinate of the lower side of the rectangle
        /// </summary>
        public float Down => Y + Height;
        /// <summary>
        /// The x coordinate of the right side of the rectangle
        /// </summary>
        public float Right => X + Width;
        /// <summary>
        /// The x coordinate of the left side of the rectangle
        /// </summary>
        public float Left => X;
        /// <summary>
        /// Displaces the rectangle by the given vector
        /// </summary>
        /// <param name="left">The rectangle to displace</param>
        /// <param name="right">The vector to displace it with</param>
        /// <returns>The displaced rectangle</returns>
        public static CollideRect operator +(CollideRect left, Vector2 right)
        {
            left.Offset(right);
            return left;
        }
        /// <summary>
        /// Displaces the rectangle by the given vector
        /// </summary>
        /// <param name="left">The rectangle to displace</param>
        /// <param name="right">The vector to displace it with</param>
        /// <returns>The displaced rectangle</returns>
        public static CollideRect operator -(CollideRect left, Vector2 right)
        {
            left.Offset(-right);
            return left;
        }
        /// <summary>
        /// Displaces the rectangle by the given vector
        /// </summary>
        /// <param name="left">The vector to displace it with</param>
        /// <param name="right">The rectangle to displace</param>
        /// <returns>The displaced rectangle</returns>
        public static CollideRect operator +(Vector2 left, CollideRect right)
        {
            right.Offset(left);
            return right;
        }

        /// <summary>
        /// Scales the rectangle
        /// </summary>
        /// <param name="left">The rectangle to scale</param>
        /// <param name="right">The scalr to multiply</param>
        /// <returns>The scaled rectangle</returns>
        public static CollideRect operator *(CollideRect left, float right)
        {
            Vector2 vect = left.GetCentre();
            return new(vect.X - left.Width * right / 2, vect.Y - left.Height * right / 2, left.Width * right, left.Height * right);
        }
    }
}