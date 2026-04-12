using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using static UndyneFight_Ex.GameMain;
using static UndyneFight_Ex.MathUtil;
using Color = Microsoft.Xna.Framework.Color;

namespace UndyneFight_Ex;

/// <summary>
/// Drawing Utilities
/// </summary>
public static class DrawingLab
{
	#region Triangulation
	/// <summary>
	/// Enter a point sequence clockwise to obtain a set of triangulations of the point sequence.
	/// </summary>
	/// <param name="pointList"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int[] GetIndices(VertexPositionColor[] pointList)
	{
		int i;
		Vector2[] vector2s = new Vector2[pointList.Length];
		for (i = 0; i < pointList.Length; i++)
		{
			vector2s[i] = new(pointList[i].Position.X, pointList[i].Position.Y);
		}
		List<Tuple<int, int, int>> results = GetIndices(vector2s);
		int[] indices = new int[results.Count * 3];
		i = 0;
		foreach (Tuple<int, int, int> tuple in results)
		{
			indices[i++] = tuple.Item1;
			indices[i++] = tuple.Item2;
			indices[i++] = tuple.Item3;
		}
		return indices;
	}
	/// <summary>
	/// Enter a point sequence clockwise to obtain a set of triangulations of the point sequence.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<Tuple<int, int, int>> GetIndices(VertexPositionColorTexture[] pointList)
	{
		Vector2[] vector2s = new Vector2[pointList.Length];
		for (int i = 0; i < pointList.Length; i++)
		{
			vector2s[i] = new(pointList[i].Position.X, pointList[i].Position.Y);
		}
		return GetIndices(vector2s);
	}
	/// <summary>
	/// Enter a point sequence clockwise to obtain a set of triangulations of the point sequence.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<Tuple<int, int, int>> GetIndices(Vector2[] pointList)
	{
		Tuple<int, Vector2>[] arr = new Tuple<int, Vector2>[pointList.Length];
		for (int i = 0; i < arr.Length; i++)
			arr[i] = new(i, pointList[i]);
		return GetIndices(arr);
	}
	/// <summary>
	/// Enter a point sequence clockwise to obtain a set of triangulations of the point sequence.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<Tuple<int, int, int>> GetIndices(Tuple<int, Vector2>[] pointList)
	{
		//Line segment
		if (pointList.Length <= 2)
			return [];
		//Triangle
		if (pointList.Length == 3)
		{
			return [new Tuple<int, int, int>(pointList[0].Item1, pointList[1].Item1, pointList[2].Item1)];
		}
		List<Tuple<int, int, int>> result = [];

		List<int> reflexes = null;

		bool[] reflex = new bool[pointList.Length];
		bool existReflex = false;
		Vector2 last = pointList[0].Item2 - pointList[^1].Item2;
		for (int i = 0; i < pointList.Length; i++)
		{
			int i2 = i + 1;
			if (i2 == pointList.Length)
				i2 = 0;
			Vector2 cur = pointList[i2].Item2 - pointList[i].Item2;
			if (reflex[i] = last.Cross(cur) < 0)
			{
				if (!existReflex)
				{
					reflexes = [];
					existReflex = true;
				}
				reflexes.Add(i);
			}
			last = cur;
		}

		if (!existReflex) //凸多边形
		{
			for (int i = 2; i < pointList.Length; i++)
			{
				result.Add(new(pointList[0].Item1, pointList[i - 1].Item1, pointList[i].Item1));
			}
			return result;
		}
		// 凹多边形
		int length = pointList.Length;
		bool[] used = new bool[pointList.Length];
		for (int i = 0; i < pointList.Length; i++)
		{
			if (i == pointList.Length - 1 && used[0])
				break;
			if (!reflex[i]) // 可能是可以分割的顶点
			{
				int v1 = i, v0 = i - 1, v2 = i + 1;
				if (v0 < 0)
					v0 = pointList.Length - 1;
				if (v2 >= pointList.Length)
					v2 = 0;

				Vector2 pv1 = pointList[v1].Item2, pv0 = pointList[v0].Item2, pv2 = pointList[v2].Item2;

				bool flag = true;
				foreach (int j in reflexes) // 检验是否可以分割
				{
					if (j == v2 || j == v0)
						continue;
					if (InTriangle(pv1, pv0, pv2, pointList[j].Item2))
					{ // 在三角形内，不可分割
						flag = false;
						break;
					}
				}
				used[i] = flag;
				if (flag) // 添加一组三角
				{
					length -= 1;
					i++;
					result.Add(new(pointList[v1].Item1, pointList[v0].Item1, pointList[v2].Item1));
				}
			}
			else
				used[i] = false;
		}
		int k = 0;
		Tuple<int, Vector2>[] tuples = new Tuple<int, Vector2>[length];
		for (int i = 0; i < pointList.Length; i++)
		{
			if (!used[i])
			{
				tuples[k] = pointList[i];
				k++;
			}
		}
		result.AddRange(GetIndices(tuples));

		return result;
	}
	#endregion
	/// <summary>
	/// HSV value of a Color
	/// </summary>
	/// <param name="h">The Hue of the color</param>
	/// <param name="s">The Saturation of the color</param>
	/// <param name="v">The Value of the color</param>
	private struct HSV(int h, int s, int v)
	{
		public int H = h, S = s, V = v;
	}
	/// <summary>
	/// Converts a HSV color to RGB
	/// </summary>
	/// <param name="hue">The hue of the color</param>
	/// <param name="saturation">The saturation of the color</param>
	/// <param name="value">The value of the color</param>
	/// <param name="input_a">The alpha of the color</param>
	/// <returns>The color in RGBA form</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4 HsvToRgb(float hue, float saturation, float value, int input_a)
	{
		saturation /= 255f;
		value /= 255f;
		int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
		double f = hue / 60 - Math.Floor(hue / 60);

		value *= 255;
		int v = Convert.ToInt32(value);
		int p = Convert.ToInt32(value * (1 - saturation));
		int q = Convert.ToInt32(value * (1 - f * saturation));
		int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

		Color output = hi == 0
			? new(v, t, p)
			: hi == 1 ? new(q, v, p) : hi == 2 ? new(p, v, t) : hi == 3 ? new(p, q, v) : hi == 4 ? new(t, p, v) : new(v, p, q);
		//Apply alpha
		output = new(output, input_a);
		return output.ToVector4();
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="triangle">Three vertex information, first is (0, 0), second is (1, 0), third is (0, 1)</param>
	/// <param name="cur"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 UVPosition(Vector2[] triangle, Vector2 cur)
	{
		Vector2 dirX, dirY;
		dirX = triangle[1] - triangle[0];
		dirY = triangle[2] - triangle[0];
		Vector2 target = cur - triangle[0];
		float proX = dirX.ScalarProject(target), proY = dirY.ScalarProject(target);
		return new Vector2(proX / dirX.Length(), proY / dirY.Length());
	}
	#region Basic shape drawing
	/// <summary>
	/// Draws a line with the given width, color and depth in the given position
	/// </summary>
	/// <param name="P1">The first <see cref="Vector2"/> point fo the line</param>
	/// <param name="P2">The second <see cref="Vector2"/> point of the line</param>
	/// <param name="width">The width of the line</param>
	/// <param name="cl">The <see cref="Color"/> of the line</param>
	/// <param name="depth">The depth of the line</param>
	/// <param name="texture">The drawing texture of the line (Default <see cref="FightResources.Sprites.pixUnit"/>)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawLine(Vector2 P1, Vector2 P2, float width, Color cl, float depth, Texture2D texture = null) => DrawLine((P1 + P2) / 2, MathF.Atan2(P2.Y - P1.Y, P2.X - P1.X), GetDistance(P1, P2) + 2, width, cl, depth, texture);
	/// <summary>
	/// Draws a vector arrow
	/// </summary>
	/// <param name="centre">The centre of the vector</param>
	/// <param name="rotation">The rotation of the vector</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawVector(Vector2 centre, float rotation) =>
		MissionSpriteBatch.Draw(GlobalResources.Sprites.debugArrow, centre, null, Color.White * 0.5f, rotation, new Vector2(3, 3), 1.0f, SpriteEffects.None, 0.9999f);
	/// <summary>
	/// Draws a line
	/// </summary>
	/// <param name="Centre">The center of the line</param>
	/// <param name="angle">The rotation of the line(In radians)</param>
	/// <param name="length">The length of the line</param>
	/// <param name="width">The width of the line</param>
	/// <param name="cl">The <see cref="Color"/> of the line</param>
	/// <param name="depth">The depth of the line</param>
	/// <param name="texture">The texture of the line</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawLine(Vector2 Centre, float angle, float length, float width, Color cl, float depth, Texture2D texture = null)
	{
		texture ??= FightResources.Sprites.pixUnit;
		angle = GetAngle(angle);
		Vector2 v1 = GetVector2(length / 2f, angle), v2 = -v1;
		v1 += Centre;
		v2 += Centre;
		Vector2 del = GetVector2(width / 2f, angle + 90);
		Vector2 p1 = v1 + del, p2 = v2 + del;
		Vector2 p3 = v1 - del, p4 = v2 - del;
		MissionSpriteBatch.DrawVertex(texture, depth,
			new VertexPositionColorTexture(new(p1, depth), cl, Vector2.Zero),
			new VertexPositionColorTexture(new(p2, depth), cl, Vector2.UnitY),
			new VertexPositionColorTexture(new(p4, depth), cl, Vector2.One),
			new VertexPositionColorTexture(new(p3, depth), cl, Vector2.UnitX)
			);
	}

	/// <summary>
	/// Draws an outline of a rectangle
	/// </summary>
	/// <param name="rect">The perimeter of the rectangle</param>
	/// <param name="color">The color of the rectangle</param>
	/// <param name="width">The width of the outline of the rectangle</param>
	/// <param name="depth">The depth of the rectangle</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawRectangle(CollideRect rect, Color color, float width, float depth)
	{
		Vector2 V2 = rect.TopLeft + new Vector2(0, rect.Height);
		Vector2 V3 = rect.TopLeft + new Vector2(rect.Width, 0);
		Vector2 V4 = rect.TopLeft + new Vector2(rect.Width, rect.Height);
		DrawLine(rect.TopLeft, V2, width, color, depth);
		DrawLine(rect.TopLeft, V3, width, color, depth);
		DrawLine(V2, V4, width, color, depth);
		DrawLine(V3, V4, width, color, depth);
	}
	/// <summary>
	/// Draws a circle outline
	/// </summary>
	/// <param name="center">The center of the circle</param>
	/// <param name="radius">The radius of the circle</param>
	/// <param name="vertexnum">The amount of vertices used to draw the circle (Higher value would result in higher precision and more lag, range: [3, inf))</param>
	/// <param name="thickness">The thickness of the circle outline</param>
	/// <param name="col">The <see cref="Color"/> of the circle</param>
	/// <param name="depth">The depth of the circle</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawCircle(Vector2 center, float radius, int vertexnum, float thickness, Color col, float depth) => DrawCircleSections(center, radius, vertexnum, thickness, col, depth, 0, 360);
	/// <summary>
	/// Draws a section of a circle outline
	/// </summary>
	/// <param name="center">The center of the circle</param>
	/// <param name="radius">The radius of the circle</param>
	/// <param name="vertexnum">The amount of vertices used to draw the circle (Higher value would result in higher precision and more lag, range: [3, inf))</param>
	/// <param name="thickness">The thickness of the circle outline</param>
	/// <param name="col">The <see cref="Color"/> of the circle</param>
	/// <param name="depth">The depth of the circle</param>
	/// <param name="startang">The starting angle to draw from</param>
	/// <param name="endang">The ending angle to draw to</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawCircleSections(Vector2 center, float radius, int vertexnum, float thickness, Color col, float depth, float startang, float endang)
	{
		vertexnum = Math.Max(3, vertexnum);
		for (int i = 0; i < vertexnum; i++)
		{
			bool check = (i + 1) * 360 / vertexnum + startang > endang;
			DrawLine(center + GetVector2(radius, i * 360f / vertexnum + startang),
					check ? center + GetVector2(radius, endang) : center + GetVector2(radius, (i + 1) * 360f / vertexnum + startang),
					thickness, col, depth);
			if (check)
				break;
		}
	}
	/// <summary>
	/// Draws a filled circle
	/// </summary>
	/// <param name="center">The center of the circle</param>
	/// <param name="radius">The radius of the circle</param>
	/// <param name="vertexnum">The amount of vertices used to draw the circle (Higher value would result in higher precision and more lag, range: [3, inf))</param>
	/// <param name="col">The <see cref="Color"/> of the circle</param>
	/// <param name="depth">The depth of the circle</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawCircleFilled(Vector2 center, float radius, int vertexnum, Color col, float depth) => DrawCircleFilledSections(center, radius, vertexnum, col, depth, 0, 360);
	/// <summary>
	/// Draws a section of a filled circle
	/// </summary>
	/// <param name="center">The center of the circle</param>
	/// <param name="radius">The radius of the circle</param>
	/// <param name="vertexnum">The amount of vertices used to draw the circle (Higher value would result in higher precision and more lag, range: [3, inf))</param>
	/// <param name="col">The <see cref="Color"/> of the circle</param>
	/// <param name="depth">The depth of the circle</param>
	/// <param name="startang">The starting angle to draw from</param>
	/// <param name="endang">The ending angle to draw to</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawCircleFilledSections(Vector2 center, float radius, int vertexnum, Color col, float depth, float startang, float endang)
	{
		vertexnum = Math.Max(3, vertexnum);
		for (int i = 0; i < vertexnum; i++)
		{
			bool check = (i + 1) * 360 / vertexnum + startang > endang;
			DrawTriangle(center,
						center + GetVector2(radius, i * 360f / vertexnum + startang),
						check ? center + GetVector2(radius, endang) : center + GetVector2(radius, (i + 1) * 360f / vertexnum + startang),
						col, depth);
			if (check)
				break;
		}
	}
	/// <summary>
	/// Draws a triangle with given colors
	/// </summary>
	/// <param name="p1">The coordinate of the first vertex</param>
	/// <param name="p2">The coordinate of the second vertex</param>
	/// <param name="p3">The coordinate of the third vertex</param>
	/// <param name="colors">The colors for each vertex</param>
	/// <param name="depth">The depth of the triangle</param>
	public static void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color[] colors, float depth) =>
		MissionSpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, depth,
			new VertexPositionColorTexture(new(p1, depth), colors[0], Vector2.Zero),
			new VertexPositionColorTexture(new(p2, depth), colors[1], Vector2.UnitY),
			new VertexPositionColorTexture(new(p3, depth), colors[2], Vector2.One),
			new VertexPositionColorTexture(new(p3, depth), colors[2], Vector2.UnitX));
	/// <summary>
	/// Draws a triangle with given color
	/// </summary>
	/// <param name="p1">The coordinate of the first vertex</param>
	/// <param name="p2">The coordinate of the second vertex</param>
	/// <param name="p3">The coordinate of the third vertex</param>
	/// <param name="color">The color of the triangle</param>
	/// <param name="depth">The depth of the triangle</param>
	public static void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color color, float depth) => DrawTriangle(p1, p2, p3, [color, color, color], depth);
	/// <summary>
	/// Draws a line with different colors in each corner
	/// </summary>
	/// <param name="Centre">The center of the line</param>
	/// <param name="angle">The angle of the line</param>
	/// <param name="length">The length of the line</param>
	/// <param name="width">The width of the line</param>
	/// <param name="cl">The colors of the line (Top Left, Top Right, Bottom Right, Bottom Left)</param>
	/// <param name="depth">The depth of the line</param>
	/// <param name="texture">The drawing texture of the line (Default <see cref="FightResources.Sprites.pixUnit"/>)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawLineColors(Vector2 Centre, float angle, float length, float width, Color[] cl, float depth, Texture2D texture = null)
	{
		texture ??= FightResources.Sprites.pixUnit;
		Vector2 v1 = GetVector2(length / 2f, angle), v2 = -v1;
		v1 += Centre;
		v2 += Centre;
		Vector2 del = GetVector2(width / 2f, angle + 90),
				p1 = v1 + del, p2 = v2 + del, p3 = v1 - del, p4 = v2 - del;
		MissionSpriteBatch.DrawVertex(texture, depth,
			new VertexPositionColorTexture(new(p1, depth), cl[2], Vector2.One), //BR
			new VertexPositionColorTexture(new(p2, depth), cl[3], Vector2.UnitY), //BL
			new VertexPositionColorTexture(new(p4, depth), cl[0], Vector2.Zero), //TL
			new VertexPositionColorTexture(new(p3, depth), cl[1], Vector2.UnitX)); //TR
	}
	/// <summary>
	/// Draws a line with different colors in each corner
	/// </summary>
	/// <param name="Centre">The center of the line</param>
	/// <param name="angle">The angle of the line</param>
	/// <param name="length">The length of the line</param>
	/// <param name="width">The width of the line</param>
	/// <param name="cl">The colors of the line (Top Left, Top Right, Bottom Right, Bottom Left)</param>
	/// <param name="depth">The depth of the line</param>
	/// <param name="texture">The texture of the line (Default none)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawLineColors(Vector2 Centre, float angle, float length, float width, Color cl, float depth, Texture2D texture = null) => DrawLineColors(Centre, angle, length, width, [cl, cl, cl, cl], depth, texture);
	/// <summary>
	/// Draws a line with different colors in each corner
	/// </summary>
	/// <param name="v1">The first vertex</param>
	/// <param name="v2">The second vertex</param>
	/// <param name="width">The width of the line</param>
	/// <param name="cl">The colors of the line (Top Left, Top Right, Bottom Right, Bottom Left)</param>
	/// <param name="depth">The depth of the line</param>
	/// <param name="texture">The drawing texture of the line (Default <see cref="FightResources.Sprites.pixUnit"/>)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawLineColors(Vector2 v1, Vector2 v2, float width, Color[] cl, float depth, Texture2D texture = null)
	{
		texture ??= FightResources.Sprites.pixUnit;
		Vector2 del = GetVector2(width / 2f, (v2 - v1).Direction() + 90),
				p1 = v1 + del, p2 = v2 + del, p3 = v1 - del, p4 = v2 - del;
		MissionSpriteBatch.DrawVertex(texture, depth,
			new VertexPositionColorTexture(new(p1, depth), cl[2], Vector2.One), //BR
			new VertexPositionColorTexture(new(p2, depth), cl[3], Vector2.UnitY), //BL
			new VertexPositionColorTexture(new(p4, depth), cl[0], Vector2.Zero), //TL
			new VertexPositionColorTexture(new(p3, depth), cl[1], Vector2.UnitX)); //TR
	}
	/// <summary>
	/// Draws a line with different colors in each corner
	/// </summary>
	/// <param name="v1">The first vertex</param>
	/// <param name="v2">The second vertex</param>
	/// <param name="width">The width of the line</param>
	/// <param name="cl">The colors of the line (Top Left, Top Right, Bottom Right, Bottom Left)</param>
	/// <param name="depth">The depth of the line</param>
	/// <param name="texture">The texture of the line (Default none)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawLineColors(Vector2 v1, Vector2 v2, float width, Color cl, float depth, Texture2D texture = null) => DrawLineColors(v1, v2, width, [cl, cl, cl, cl], depth, texture);
	/// <summary>
	/// Draws a rectangle with rounded corners
	/// </summary>
	/// <param name="centre">The center of the rectangle</param>
	/// <param name="size">The size of the rectangle</param>
	/// <param name="radius">The radius of the rounded corners</param>
	/// <param name="color">The color of the rounded rectangle</param>
	/// <param name="depth">The depth of the rounded rectangle</param>
	public static void DrawRoundedRectangle(Vector2 centre, Vector2 size, float radius, Color color, float depth)
	{
		DrawLine(centre - new Vector2(size.X / 2 - radius, 0), centre + new Vector2(size.X / 2 - radius, 0), size.Y, color, depth);
		DrawLine(centre - new Vector2(size.X / 2 - 1, 0), centre + new Vector2(size.X / 2 - 1, 0), size.Y - radius * 2, color, depth);
		//Corner circles
		DrawCircleFilled(centre - size / 2 + new Vector2(radius), radius, 32, color, depth);
		DrawCircleFilled(centre + new Vector2(size.X / 2 - radius, -size.Y / 2 + radius), radius, 32, color, depth);
		DrawCircleFilled(centre + size / 2 - new Vector2(radius), radius, 32, color, depth);
		DrawCircleFilled(centre + new Vector2(-size.X / 2 + radius, size.Y / 2 - radius), radius, 32, color, depth);
	}
	/// <summary>
	/// A general texture drawing function that integrates all functionalities from all FormalDraw functions
	/// </summary>
	/// <param name="texture">The texture to draw</param>
	/// <param name="position">The position to draw the texture</param>
	/// <param name="color">The color of the texture to draw (Default white)</param>
	/// <param name="scale">The scale of the texture to draw (Default 1)</param>
	/// <param name="rotation">The rotation of the texture to draw in radians (Default 0)</param>
	/// <param name="spriteOrigin">The origin of the texture to draw (Default center of texture)</param>
	/// <param name="texArea">The bounds of drawing on the screen (Default null for normal drawing)</param>
	/// <param name="sourceRect">The region of the texture to render (Default null for full texture)</param>
	/// <param name="depth">The depth of the texture to draw (Default current depth)</param>
	public static void GeneralDraw(Texture2D texture, Vector2 position, Color? color = null, Vector2? scale = null, float rotation = 0, Vector2? spriteOrigin = null, CollideRect? texArea = null, CollideRect? sourceRect = null, float depth = 0)
	{
		if (texture is null)
		{
			Debug.WriteLine($"The texture is not a texture or is not loaded");
			return;
		}
		Vector2 GetRotCen = spriteOrigin ?? new(texture.Width / 2f, texture.Height / 2f);
		Vector2 drawingScale = scale ?? Vector2.One;
		CollideRect rect = new(position - GetRotCen, texArea.HasValue ? texArea.Value.Size : texture.Bounds.Size.ToVector2());
		MissionSpriteBatch.Draw(texture, rect, sourceRect, color ?? Color.White, rotation, GetRotCen, drawingScale, SpriteEffects.None, depth);
	}
	#endregion
	/// <summary>
	/// Loads a file (Cross-platform)
	/// </summary>
	/// <typeparam name="T">Content type</typeparam>
	/// <param name="path">Path to file</param>
	/// <param name="cm">Content manager to use</param>
	/// <returns>The loaded content</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T LoadContent<T>(string path, ContentManager cm = null) => (cm ??= Scene.Loader).Load<T>(Path.Combine($"{(path.StartsWith(cm.RootDirectory) && cm.RootDirectory != string.Empty ? path.Replace(cm.RootDirectory, null) : path)}".Split('\\')));
	/// <summary>
	/// Loads an image
	/// </summary>
	/// <param name="path">Path of the image</param>
	/// <returns>The loaded texture</returns>
	public static Texture2D LoadImage(string path) => Texture2D.FromFile(GameStates.SpriteBatch.GraphicsDevice, path);
	/// <summary>
	/// Draws a text
	/// </summary>
	/// <param name="font">The font to draw in</param>
	/// <param name="text">The text to draw</param>
	/// <param name="position">The position of the text</param>
	/// <param name="color">The color of the text (Default white)</param>
	/// <param name="scale">The scale of the text (Default 1)</param>
	/// <param name="rotation">The rotation of the text (Default 0)</param>
	/// <param name="rotateCenter">The rotation origin of the text (Default top left)</param>
	/// <param name="depth">The depth of the text (Default 1)</param>
	/// <param name="spriteBatch">The sprite batch to draw (Default <see cref="MissionSpriteBatch"/>)</param>
	public static void DrawText(GLFont font, string text, Vector2 position, Color? color = null, Vector2? scale = null, float? rotation = 0, Vector2? rotateCenter = null, float? depth = null, SpriteBatchEX spriteBatch = null) => (spriteBatch ?? MissionSpriteBatch).DrawString(font, text, position, (color ?? Color.White) * Surface.Normal.drawingAlpha, rotation ?? 0, rotateCenter ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, depth ?? 1);
	#region Utilities
	/// <summary>
	/// Converts a <see cref="Vector3"/> to a <see cref="Color"/>
	/// </summary>
	/// <param name="vec">The vector to convert</param>
	/// <returns>The converted color</returns>
	public static Color ToColor(this Vector3 vec) => new(vec);
	/// <summary>
	/// Converts a <see cref="Vector4"/> to a <see cref="Color"/>
	/// </summary>
	/// <param name="vec">The vector to convert</param>
	/// <returns>The converted color</returns>
	public static Color ToColor(this Vector4 vec) => new(vec);
	#endregion
}