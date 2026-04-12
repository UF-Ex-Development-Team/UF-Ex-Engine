using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Shapes;

/// <summary>
/// Interface for shapes
/// </summary>
public interface IShape
{
	/// <summary>
	/// The list of vertices of the shape
	/// </summary>
	Vector2[] Vertices { get; set; }
	/// <summary>
	/// The rotation of the shape
	/// </summary>
	float Rotation { get; set; }
	/// <summary>
	/// The data for drawing the shape
	/// </summary>
	DrawingData DrawData { get; set; }

	/// <summary>
	/// Whether this shape is intersecting with another shape
	/// </summary>
	/// <param name="shape">The other shape</param>
	/// <returns></returns>
	bool IntersectsWith(IShape shape);
	/// <summary>
	/// Whether this shape contains another shape
	/// </summary>
	/// <param name="shape">The other shape</param>
	/// <returns></returns>
	bool Contains(IShape shape);
	/// <summary>
	/// The drawing method of the shape
	/// </summary>
	void Draw();
}
/// <summary>
/// The data for drawing the shape
/// </summary>
public struct DrawingData
{
	/// <summary>
	/// The mode to draw the shape in
	/// </summary>
	public DrawMode DrawingMode { get; set; }
	/// <summary>
	/// The array of blends of the shape
	/// </summary>
	public Color[] BlendArray { get; set; }
	/// <summary>
	/// Sets the entire blend array into the given blend
	/// </summary>
	public readonly Color Blend
	{
		set
		{
			for (int i = 0; i < BlendArray.Length; i++)
				BlendArray[i] = value;
		}
	}
	/// <summary>
	/// The mode of drawing of the shape
	/// </summary>
	public enum DrawMode
	{
		/// <summary>
		/// Does not draw
		/// </summary>
		None,
		/// <summary>
		/// Draws the outline
		/// </summary>
		Outline,
		/// <summary>
		/// Draws a filled shape
		/// </summary>
		Filled
	}
	/// <summary>
	/// The depth of the shape
	/// </summary>
	public float Depth { get; set; }
	/// <summary>
	/// The width of the outline
	/// </summary>
	public float Width { get; set; }
}
/// <summary>
/// A triangle
/// </summary>
/// <param name="v1">The first vertex of the triangle</param>
/// <param name="v2">The second vertex of the triangle</param>
/// <param name="v3">The third vertex of the triangle</param>
public struct Triangle(Vector2 v1, Vector2 v2, Vector2 v3) : IShape
{
	/// <inheritdoc/>
	public Vector2[] Vertices { get; set; } = [v1, v2, v3];
	private float _rotation;
	/// <inheritdoc/>
	public float Rotation
	{
		readonly get => _rotation;
		set
		{
			_rotation = value;
			Vertices[0].RotateAround(Incenter, _rotation);
			Vertices[1].RotateAround(Incenter, _rotation);
			Vertices[2].RotateAround(Incenter, _rotation);
		}
	}
	/// <inheritdoc/>
	public DrawingData DrawData { get; set; } = new();
	/// <inheritdoc/>
	public readonly bool Contains(IShape shape)
	{
		if (shape is Triangle or Rectangle)
		{
			for (int i = 0; i < 3; i++)
				foreach (Vector2 vertex in shape.Vertices)
					if (!InTriangle(Vertices[0], Vertices[1], Vertices[2], vertex))
						return false;
			return true;
		}
		else if (shape is Circle circ)
		{
			/*
				Cases where it is contained:
				1:
					Target circle is the inscribed circle of the triangle, then it is contained
				2:
					Target circle has a smaller radius than the inscribed circle, then compensate it via delta
			*/
			return (circ.Center - Incenter).Length() + circ.Radius <= Inradius;
		}
		return false;
	}
	//The length of the sides of the triangle denoted by triangle ABC
	private readonly float A => (Vertices[1] - Vertices[0]).Length();
	private readonly float B => (Vertices[2] - Vertices[1]).Length();
	private readonly float C => (Vertices[0] - Vertices[2]).Length();
	/// <summary>
	/// Radius of the inscribed circle
	/// <see href="https://en.wikipedia.org/wiki/Incircle_and_excircles"/>
	/// </summary>
	public readonly float Inradius => float.Sqrt((B + C - A) * (A + C - B) * (A + B - C) / (A + B + C) / 4);
	/// <inheritdoc/>
	public readonly void Draw()
	{
		if (DrawData.DrawingMode == DrawingData.DrawMode.Outline)
		{
			for (int i = 0; i < 3; i++)
				DrawingLab.DrawLineColors(Vertices[i], Vertices[(i + 1) % 3], DrawData.Width, [DrawData.BlendArray[(i + 1) % 3], DrawData.BlendArray[i], DrawData.BlendArray[i], DrawData.BlendArray[(i + 1) % 3]], DrawData.Depth);
		}
		else if (DrawData.DrawingMode == DrawingData.DrawMode.Filled)
			DrawingLab.DrawTriangle(Vertices[0], Vertices[1], Vertices[2], DrawData.BlendArray, DrawData.Depth);
	}

	/// <inheritdoc/>
	public readonly bool IntersectsWith(IShape shape)
	{
		if (shape is Circle circ)
		{
			//If any of the lines are within range of the circle, it is intersecting
			//For it to intersect, the line must be not contained by the circle, so distance < radius and both ends not simultaneously inscribed
			for (int i = 0; i < 3; i++)
				if (DistanceToLine(circ.Center, Vertices[i], Vertices[(i + 1) % 3]) < circ.Radius && ((Vertices[i] - circ.Center).Length() > circ.Radius || (Vertices[(i + 1) % 3] - circ.Center).Length() > circ.Radius))
					return true;
			//If the circle is the inscribed circle, it is intersecting. If not, then there are no more cases to compare
			return circ.Center == Incenter && circ.Radius == Inradius;
		}
		else //If any lines are intersecting, it is, well, intersecting
			for (int i = 0; i < 3; i++)
				for (int k = 0; k < shape.Vertices.Length; k++)
					if (LineIntersect((Vertices[i], Vertices[(i + 1) % 3]), (shape.Vertices[k], shape.Vertices[(k + 1) % shape.Vertices.Length])))
						return true;
		return false;
	}
	/// <summary>
	/// Gets the centroid of the triangle
	/// </summary>
	public readonly Vector2 Centroid => (Vertices[0] + Vertices[1] + Vertices[2]) / 3;
	/// <summary>
	/// Gets the incenter of the triangle
	/// </summary>
	public readonly Vector2 Incenter
	{
		get
		{
			float A = (Vertices[1] - Vertices[0]).Length(), B = (Vertices[2] - Vertices[1]).Length(), C = (Vertices[0] - Vertices[2]).Length();
			return (A * Vertices[0] + B * Vertices[1] + C * Vertices[2]) / (A + B + C);
		}
	}
	#region Operators
	/// <summary>
	/// Offsets the triangle by the given vector
	/// </summary>
	/// <param name="tri">The triangle to offset</param>
	/// <param name="vec">The amount to offset</param>
	/// <returns>The triangle after the offset</returns>
	public static Triangle operator +(Triangle tri, Vector2 vec)
	{
		for (int i = 0; i < 3; i++)
			tri.Vertices[i] += vec;
		return tri;
	}
	/// <summary>
	/// Offsets the triangle by the given vector
	/// </summary>
	/// <param name="tri">The triangle to offset</param>
	/// <param name="vec">The amount to offset</param>
	/// <returns>The triangle after the offset</returns>
	public static Triangle operator -(Triangle tri, Vector2 vec)
	{
		for (int i = 0; i < 3; i++)
			tri.Vertices[i] -= vec;
		return tri;
	}
	/// <summary>
	/// Scales the triangle by the given scalar
	/// </summary>
	/// <param name="tri">The triangle to scale</param>
	/// <param name="scale">The amount to scale</param>
	/// <returns>The scaled triangle</returns>
	public static Triangle operator *(Triangle tri, float scale)
	{
		for (int i = 0; i < 3; i++)
			tri.Vertices[i] = tri.Incenter + GetVector2((tri.Vertices[i] - tri.Incenter).Length() * scale, Direction(tri.Incenter, tri.Vertices[i]));
		return tri;
	}
	/// <summary>
	/// Scales the triangle by the given scalar
	/// </summary>
	/// <param name="tri">The triangle to scale</param>
	/// <param name="scale">The amount to scale</param>
	/// <returns>The scaled triangle</returns>
	public static Triangle operator /(Triangle tri, float scale)
	{
		for (int i = 0; i < 3; i++)
			tri.Vertices[i] = tri.Incenter + GetVector2((tri.Vertices[i] - tri.Incenter).Length() / scale, Direction(tri.Incenter, tri.Vertices[i]));
		return tri;
	}
	#endregion
}
/// <summary>
/// A rectangle
/// </summary>
public struct Rectangle : IShape
{
	/// <summary>
	/// Creates a rectangle with the given center and size
	/// </summary>
	/// <param name="center">The center of the rectangle</param>
	/// <param name="size">The width and height of the rectangle</param>
	public Rectangle(Vector2 center, Vector2 size) => Vertices = [center - (size /= 2), center + new Vector2(size.X, -size.Y), center + size, center + new Vector2(-size.X, size.Y)];
	/// <summary>
	/// Creates a rectangle with the given vertices
	/// </summary>
	/// <param name="TopLeft">The top left corner of the rectangle</param>
	/// <param name="TopRight">The top right corner of the rectangle</param>
	/// <param name="BottomRight">The bottom right corner of the rectangle</param>
	/// <param name="BottomLeft">The bottom left corner of the rectangle</param>
	public Rectangle(Vector2 TopLeft, Vector2 TopRight, Vector2 BottomRight, Vector2 BottomLeft) => Vertices = [TopLeft, TopRight, BottomRight, BottomLeft];
	/// <inheritdoc/>
	public Vector2[] Vertices { get; set; }
	private float _rotation;
	/// <inheritdoc/>
	public float Rotation
	{
		readonly get => _rotation; set
		{
			_rotation = value;
			Vertices[0].RotateAround(Center, _rotation);
			Vertices[1].RotateAround(Center, _rotation);
			Vertices[2].RotateAround(Center, _rotation);
			Vertices[3].RotateAround(Center, _rotation);
		}
	}
	/// <inheritdoc/>
	public DrawingData DrawData { get; set; } = new();
	/// <inheritdoc/>
	public readonly bool Contains(IShape shape)
	{
		if (shape is Triangle or Rectangle)
		{
			for (int i = 0; i < 4; i++)
				foreach (Vector2 vertex in shape.Vertices)
					if (!InTriangle(Vertices[0], Vertices[1], Vertices[2], vertex) && !InTriangle(Vertices[1], Vertices[2], Vertices[0], vertex))
						return false;
			return true;
		}
		else if (shape is Circle circ)
		{
			return //If the center is within the rectangle
				InTriangle(Vertices[0], Vertices[1], Vertices[2], circ.Center) && InTriangle(Vertices[1], Vertices[2], Vertices[0], circ.Center) &&
				//and the distance to all the lines are more than the radius, it is contained
				float.Min(
					float.Min(DistanceToLine(circ.Center, Vertices[0], Vertices[1]), DistanceToLine(circ.Center, Vertices[1], Vertices[2])),
					float.Min(DistanceToLine(circ.Center, Vertices[2], Vertices[3]), DistanceToLine(circ.Center, Vertices[3], Vertices[0]))
				) < circ.Radius;
		}
		return false;
	}
	/// <inheritdoc/>
	public readonly void Draw()
	{
		if (DrawData.DrawingMode == DrawingData.DrawMode.Outline)
		{
			for (int i = 0; i < 4; i++)
				DrawingLab.DrawLineColors(Vertices[i], Vertices[(i + 1) % 4], DrawData.Width, [DrawData.BlendArray[(i + 1) % 4], DrawData.BlendArray[i], DrawData.BlendArray[i], DrawData.BlendArray[(i + 1) % 4]], DrawData.Depth);
		}
		else if (DrawData.DrawingMode == DrawingData.DrawMode.Filled)
			DrawingLab.DrawLineColors(Center, Rotation, (Vertices[1] - Vertices[0]).Length(), (Vertices[2] - Vertices[1]).Length(), DrawData.BlendArray, DrawData.Depth);
	}
	/// <inheritdoc/>
	public readonly bool IntersectsWith(IShape shape)
	{
		if (shape is Triangle or Rectangle)
		{
			for (int i = 0; i < 4; i++)
				for (int k = 0; k < shape.Vertices.Length; k++)
					if (LineIntersect((Vertices[i], Vertices[(i + 1) % 4]), (shape.Vertices[k], shape.Vertices[(k + 1) % shape.Vertices.Length])))
						return true;
		}
		else if (shape is Circle circ)
		{
			//For it to intersect, the line must be not contained by the circle, so distance < radius and both ends not simultaneously inscribed
			for (int i = 0; i < 4; i++)
				if (DistanceToLine(circ.Center, Vertices[i], Vertices[(i + 1) % 4]) < circ.Radius && ((Vertices[i] - circ.Center).Length() > circ.Radius || (Vertices[(i + 1) % 4] - circ.Center).Length() > circ.Radius))
					return true;
		}
		return false;
	}
	/// <summary>
	/// The center of the rectangle
	/// </summary>
	public Vector2 Center { readonly get => (Vertices[0] + Vertices[1] + Vertices[2] + Vertices[3]) / 4; set => this += value - Center; }
	#region Operators
	/// <summary>
	/// Offsets the rectangle by the given vector
	/// </summary>
	/// <param name="rect">The rectangle to offset</param>
	/// <param name="vec">The amount to offset</param>
	/// <returns>The rectangle after offset</returns>
	public static Rectangle operator +(Rectangle rect, Vector2 vec)
	{
		for (int i = 0; i < 4; i++)
			rect.Vertices[i] += vec;
		return rect;
	}
	/// <summary>
	/// Offsets the rectangle by the given vector
	/// </summary>
	/// <param name="rect">The rectangle to offset</param>
	/// <param name="vec">The amount to offset</param>
	/// <returns>The rectangle after offset</returns>
	public static Rectangle operator -(Rectangle rect, Vector2 vec)
	{
		for (int i = 0; i < 4; i++)
			rect.Vertices[i] -= vec;
		return rect;
	}
	/// <summary>
	/// Scales the rectangle by the given vector
	/// </summary>
	/// <param name="rect">The rectangle to scale</param>
	/// <param name="scale">The amount to scale</param>
	/// <returns>The rectangle after scaling</returns>
	public static Rectangle operator *(Rectangle rect, float scale)
	{
		for (int i = 0; i < 4; i++)
			rect.Vertices[i] = GetVector2((rect.Vertices[i] - rect.Center).Length() * scale, Direction(rect.Center, rect.Vertices[i]));
		return rect;
	}
	/// <summary>
	/// Scales the rectangle by the given vector
	/// </summary>
	/// <param name="rect">The rectangle to scale</param>
	/// <param name="scale">The amount to scale</param>
	/// <returns>The rectangle after scaling</returns>
	public static Rectangle operator /(Rectangle rect, float scale)
	{
		for (int i = 0; i < 4; i++)
			rect.Vertices[i] = GetVector2((rect.Vertices[i] - rect.Center).Length() / scale, Direction(rect.Center, rect.Vertices[i]));
		return rect;
	}
	#endregion
	#region Conversion
	/// <summary>
	/// Converts the rectangle into a <see cref="System.Drawing.Rectangle"/> without taking in the rotation
	/// </summary>
	/// <param name="rect">The rectangle to convert</param>
	public static implicit operator System.Drawing.Rectangle(Rectangle rect)
	{
		rect.Rotation = 0;
		return new((int)rect.Vertices[0].X, (int)rect.Vertices[0].Y, (int)(rect.Vertices[1] - rect.Vertices[0]).Length(), (int)(rect.Vertices[2] - rect.Vertices[1]).Length());
	}
	#endregion
}
/// <summary>
/// A circle
/// </summary>
public struct Circle : IShape
{
	/// <summary>
	/// Creates a circle with the given center and radius
	/// </summary>
	/// <param name="center">The center of the circle</param>
	/// <param name="radius">The radius of the circle</param>
	public Circle(Vector2 center, float radius)
	{
		Center = center;
		Radius = radius;
		Resolution = 64;
	}
	/// <inheritdoc/>
	public vec2[] Vertices { get; set; }
	/// <inheritdoc/>
	public float Rotation { readonly get; set; } = 0;
	private int _resolution;
	/// <summary>
	/// The rendering resolution of the circle
	/// </summary>
	public int Resolution
	{
		readonly get => _resolution; set
		{
			//If it is an increase of resolution, create new information
			if (value > Resolution)
			{
				//Set array size if null
				if (DrawData.BlendArray is null)
				{
					DrawingData TempDat = DrawData;
					TempDat.BlendArray = new Color[value];
					DrawData = TempDat;
				}
				Color[] OldBlend = DrawData.BlendArray;
				Vertices = new Vector2[value];
				Color?[] TempBlend = new Color?[value];
				int coloredIndex = (int)float.Ceiling((float)value / (Resolution == 0 ? value : Resolution));
				int indexDelta = coloredIndex;
				int prevColoredIndex = 0;
				for (int i = 0; i < value; i++)
				{
					Vertices[i] = Center + GetVector2(Radius, i * 360f / value);
					//'Expands' the array such that the old blend info are still as evenly spaced as possible
					if (i < OldBlend.Length)
						TempBlend[(int)float.Ceiling(i * (float)value / (Resolution == 0 ? value : Resolution))] = OldBlend[i];
					//Lerp the empty values
					if (i > prevColoredIndex)
					{
						for (int k = prevColoredIndex + 1; k < coloredIndex; k++)
							TempBlend[k] = Color.Lerp(TempBlend[prevColoredIndex] ?? Color.White, TempBlend[coloredIndex % value] ?? Color.White, (float)(k - prevColoredIndex) / (coloredIndex - prevColoredIndex));
						prevColoredIndex += indexDelta;
						coloredIndex += indexDelta;
					}
				}
				//Update array size if not null
				if (DrawData.BlendArray is not null)
				{
					DrawingData TempDat = DrawData;
					TempDat.BlendArray = new Color[value];
					DrawData = TempDat;
				}
				for (int i = 0; i < value; i++)
					DrawData.BlendArray[i] = TempBlend[i] ?? Color.White; //The value will never be null
			}
			//Remove information if it is a decrease of resolution
			else if (value < Resolution)
			{
				(Vector2[] OldVertices, Color[] OldBlend) = (Vertices, DrawData.BlendArray);
				Vertices = new Vector2[value];
				DrawingData TempDat = DrawData;
				TempDat.BlendArray = new Color[value];
				DrawData = TempDat;
				for (int i = 0; i < value; i++)
				{
					Vertices[i] = OldVertices[(int)(i * (float)Resolution / value)];
					DrawData.BlendArray[i] = OldBlend[(int)(i * (float)Resolution / value)];
				}
			}
			_resolution = value;
		}
	}
	/// <inheritdoc/>
	public DrawingData DrawData { get; set; } = new();
	/// <summary>
	/// The radius of the circle
	/// </summary>
	public float Radius;
	/// <summary>
	/// The center of the circle
	/// </summary>
	public Vector2 Center;
	/// <inheritdoc/>
	public readonly bool Contains(IShape shape)
	{
		if (shape is Triangle or Rectangle)
		{
			for (int i = 0; i < shape.Vertices.Length; i++)
				if ((shape.Vertices[i] - Center).Length() >= Radius)
					return false;
		}
		else if (shape is Circle circ)
		{
			return (circ.Center - Center).Length() < Radius + circ.Radius;
		}
		return true;
	}
	/// <inheritdoc/>
	public readonly void Draw()
	{
		if (DrawData.DrawingMode == DrawingData.DrawMode.Outline)
		{
			for (int i = 0; i < Resolution; i++)
			{
				DrawingLab.DrawLineColors(Center + GetVector2(Radius, Rotation + i * 360 / Resolution), Center + GetVector2(Radius, Rotation + (i + 1) * 360 / Resolution), DrawData.Width, [DrawData.BlendArray[(i + 1) % Resolution], DrawData.BlendArray[i], DrawData.BlendArray[i], DrawData.BlendArray[(i + 1) % Resolution]], DrawData.Depth);
			}
		}
		else if (DrawData.DrawingMode == DrawingData.DrawMode.Filled)
		{
			for (int i = 0; i < Resolution; i++)
			{
				DrawingLab.DrawTriangle(Center + GetVector2(Radius, Rotation + i * 360 / Resolution), Center + GetVector2(Radius, Rotation + (i + 1) * 360 / Resolution), Center, [DrawData.BlendArray[i], DrawData.BlendArray[(i + 1) % Resolution], Color.Lerp(DrawData.BlendArray[i], DrawData.BlendArray[(i + 1) % Resolution], 0.5f)], DrawData.Depth);
			}
		}
	}
	/// <inheritdoc/>
	public readonly bool IntersectsWith(IShape shape)
	{
		if (shape is Triangle tri)
			return (Center - tri.Incenter).Length() + Radius <= tri.Inradius;
		else if (shape is Rectangle rect)
		{
			//For it to intersect, the line must be not contained by the circle, so distance < radius and both ends not simultaneously inscribed
			for (int i = 0; i < 4; i++)
				if (DistanceToLine(Center, rect.Vertices[i], rect.Vertices[(i + 1) % 4]) < Radius && ((rect.Vertices[i] - Center).Length() > Radius || (rect.Vertices[(i + 1) % 4] - Center).Length() > Radius))
					return true;
		}
		else if (shape is Circle circ)
		{
			float dist = (Center - circ.Center).Length();
			return float.Abs(Radius - circ.Radius) < dist && dist < Radius + circ.Radius;
		}
		return false;
	}
	#region Operators
	/// <summary>
	/// Offsets the circle by the given vector
	/// </summary>
	/// <param name="circ">The circle to offset</param>
	/// <param name="vec">The amount to offset</param>
	/// <returns>The circle after offset</returns>
	public static Circle operator +(Circle circ, Vector2 vec)
	{
		circ.Center += vec;
		return circ;
	}
	/// <summary>
	/// Offsets the circle by the given vector
	/// </summary>
	/// <param name="circ">The circle to offset</param>
	/// <param name="vec">The amount to offset</param>
	/// <returns>The circle after offset</returns>
	public static Circle operator -(Circle circ, Vector2 vec)
	{
		circ.Center -= vec;
		return circ;
	}
	/// <summary>
	/// Scales the circle by the given scale
	/// </summary>
	/// <param name="circ">The circle to scale</param>
	/// <param name="scale">The amount to scale</param>
	/// <returns>The scaled circle</returns>
	public static Circle operator *(Circle circ, float scale)
	{
		circ.Radius *= scale;
		return circ;
	}
	/// <summary>
	/// Scales the circle by the given scale
	/// </summary>
	/// <param name="circ">The circle to scale</param>
	/// <param name="scale">The amount to scale</param>
	/// <returns>The scaled circle</returns>
	public static Circle operator /(Circle circ, float scale)
	{
		circ.Radius /= scale;
		return circ;
	}
	#endregion
}