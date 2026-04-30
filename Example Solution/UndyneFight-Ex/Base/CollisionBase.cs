using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.DrawingLab;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex;
/// <summary>
/// Used for detecting blue soul platforms
/// </summary>
public class GravityLine
{
	/// <summary>
	/// Resets the buffer of the gravity line
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Reload() => reloadTime = 4;
	/// <summary>
	/// Reduces the buffer
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Recover() => reloadTime--;

	private static int reloadTime = 0;
	/// <summary>
	/// Whether the gravity line is enabled
	/// </summary>
	public bool enabled = true;
	private bool IsEnable => reloadTime <= 0 && enabled;
	private Vector2 v1, v2;
	internal static HashSet<GravityLine> GravityLines = [];
	/// <summary>
	/// Removes the current gravity line from the global list
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose() => GravityLines.Remove(this);
	/// <summary>
	/// Sets the vertices of the gravity line
	/// </summary>
	/// <param name="v1">The first end of the line</param>
	/// <param name="v2">The other end of the line</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetPosition(Vector2 v1, Vector2 v2)
	{
		this.v1 = v1;
		this.v2 = v2;
		Vector2 newCen = (v1 + v2) / 2, prevCen = Centre;
		Centre = newCen;
		float prevRot = Rotation;
		Rotation = MathF.Atan2(v1.Y - v2.Y, v1.X - v2.X);
		float delta = Rotation - prevRot;
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
				Vector2 _delta = newCen - prevCen;
				if (Math.Abs(delta) > 1e-5f)
				{
					float ori = MathF.Atan2(s.Centre.Y - Centre.Y, s.Centre.X - Centre.X);
					float length = GetDistance(Centre, s.Centre);
					_delta += Centre + GetVector2(length, (ori + delta) / PI * 180) - s.Centre;
				}
				if (sticky)
					s.Centre += _delta;
				else
				{
					Vector2 v_ = new(MathF.Cos(NormalRotation), MathF.Sin(NormalRotation));
					if (_delta.Length() > 0.001f)
						s.Centre += v_ * Cos(v_, _delta) * _delta.Length();
				}
			});
		}
		collidePlayers.Clear();
	}
	/// <summary>
	/// Sets the width of the line
	/// </summary>
	/// <param name="width">The width to set</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetWidth(float width) => this.width = width;
	/// <summary>
	/// Creates a gravity line
	/// </summary>
	/// <param name="v1">The first end of the line</param>
	/// <param name="v2">The other end of the line</param>
	public GravityLine(Vector2 v1, Vector2 v2)
	{
		this.v1 = v1;
		this.v2 = v2;
		_ = GravityLines.Add(this);
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

	private float A, B, C, width = 0;
	private float Length => Vector2.Distance(v1, v2);
	private Vector2 Centre;
	/// <summary>
	/// The rotation of the line
	/// </summary>
	public float Rotation { get; private set; } = 0;
	/// <summary>
	/// The rotation of the normal of the line
	/// </summary>
	public float NormalRotation => Rotation - PI / 2f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float Distance(Player.Heart heart) => (A * heart.Centre.X + B * heart.Centre.Y + C) / float.Sqrt(A * A + B * B);

	private bool isCollide;
	/// <summary>
	/// Whether the line is carries the soul or not
	/// </summary>
	public bool sticky = true;
	private readonly List<Player.Heart> collidePlayers = [];
	/// <summary>
	/// Whether the line is colliding with the heart
	/// </summary>
	/// <param name="player">The heart to check</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsCollideWith(Player.Heart player)
	{
		if (!IsEnable || !(isCollide = Math.Abs(Distance(player)) <= 8.01f + width && GetDistance(player.Centre, Centre) <= (Length / 2 + 6)))
			return false;
		if (Vector2.Dot(player.Centre - Centre, GetVector2(1, player.Rotation - 90)) < 0)
			return isCollide = false;
		collidePlayers.Add(player);
		return true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Vector2 CorrectPosition(Player.Heart player)
	{
		Vector2 _v = new(MathF.Cos(NormalRotation), MathF.Sin(NormalRotation));
		Vector2 change = _v * (8 + width - Cos(player.Centre - Centre, _v) * GetDistance(player.Centre, Centre)) * 0.25f;
		return change;
	}
	/// <summary>
	/// Draws the line for collision
	/// </summary>
	public void Draw()
	{
		if (!DebugState.ShowIntendedHitbox)
			return;
		if (!IsEnable)
			DrawLine(v1, v2, 2, Color.Red, 0.999f);
		else if (!isCollide)
			DrawLine(v1, v2, 2, Color.Green, 0.999f);
		else
			DrawLine(v1, v2, 2, Color.Gold, 0.999f);
		DrawVector(Centre, NormalRotation);
	}
}
/// <summary>
/// A rectangle with collision
/// </summary>
/// <param name="X">The x coordinate of the top left corner of the rectangle</param>
/// <param name="Y">The y coordinate of the top left corner of the rectangle</param>
/// <param name="Width">The width of the rectangle</param>
/// <param name="Height">The height of the rectangle</param>
public struct CollideRect(float X, float Y, float Width, float Height)
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
	/// The x coordinate of the top left corner of the rectangle
	/// </summary>
	public float X { get; set; } = X;
	/// <summary>
	/// The y coordinate of the top left corner of the rectangle
	/// </summary>
	public float Y { get; set; } = Y;
	/// <summary>
	/// Gets the <see cref="Vector2"/> coordinates of the vertices
	/// </summary>
	/// <returns>The array of vertices</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Vector2[] GetVertices() => [new(Left, Up), new(Right, Up), new(Right, Down), new(Left, Down)];
	/// <summary>
	/// Creates a rectangle with collision with the given position and size
	/// </summary>
	/// <param name="pos">The position of the rectangle</param>
	/// <param name="size">The dimensions of the rectangle</param>
	public CollideRect(Vector2 pos, Vector2 size) : this(pos.X, pos.Y, size.X, size.Y) { }
	/// <summary>
	/// Creates a rectangle with collision from a <see cref="Rectangle"/>
	/// </summary>
	/// <param name="rec">The <see cref="Rectangle"/> to create from</param>
	public CollideRect(Rectangle rec) : this(rec.X, rec.Y, rec.Width, rec.Height) { }
	/// <summary>
	/// Creates a rectangle with collision from a <see cref="System.Drawing.RectangleF"/>
	/// </summary>
	/// <param name="rec">The <see cref="System.Drawing.RectangleF"/> to create from</param>
	public CollideRect(System.Drawing.RectangleF rec) : this(rec.X, rec.Y, rec.Width, rec.Height) { }
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
	/// <param name="X">The x coordinate of the center of the rectangle</param>
	/// <param name="Y">The y coordinate of the center of the rectangle</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetCentre(float X, float Y) => Offset(new Vector2(X, Y) - GetCentre());
	/// <summary>
	/// The centre of the rectangle
	/// </summary>
	public Vector2 Centre { readonly get => GetCentre(); set => SetCentre(value); }
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
	/// Converts a <see cref="CollideRect"/> to <see cref="Rectangle"/> (Without rotation)
	/// </summary>
	/// <returns>The <see cref="Rectangle"/></returns>
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
	/// <summary>
	/// Converts the rectangle collision to a <see cref="Rectangle"/> (No rotation)
	/// </summary>
	/// <param name="rect"></param>
	public static implicit operator Rectangle(CollideRect rect) => rect.ToRectangle();
	/// <summary>
	/// Converts the rectangle into a CollideRect
	/// </summary>
	/// <param name="rect">The rectangle to convert</param>
	public static implicit operator CollideRect(Rectangle rect) => new(rect);
	/// <summary>
	/// Converts a <see cref="System.Drawing.RectangleF"/> into a CollideRect
	/// </summary>
	/// <param name="rect">The RectangleF to convert</param>
	public static implicit operator CollideRect(System.Drawing.RectangleF rect) => new(rect);
	/// <summary>
	/// The y coordinate of the upper side of the rectangle
	/// </summary>
	public readonly float Up => Y;
	/// <summary>
	/// The y coordinate of the lower side of the rectangle
	/// </summary>
	public readonly float Down => Y + Height;
	/// <summary>
	/// The x coordinate of the right side of the rectangle
	/// </summary>
	public readonly float Right => X + Width;
	/// <summary>
	/// The x coordinate of the left side of the rectangle
	/// </summary>
	public readonly float Left => X;
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
	/// <param name="right">The scalar to multiply</param>
	/// <returns>The scaled rectangle</returns>
	public static CollideRect operator *(CollideRect left, float right)
	{
		Vector2 vect = left.GetCentre();
		return new(vect.X - left.Width * right / 2, vect.Y - left.Height * right / 2, left.Width * right, left.Height * right);
	}
}