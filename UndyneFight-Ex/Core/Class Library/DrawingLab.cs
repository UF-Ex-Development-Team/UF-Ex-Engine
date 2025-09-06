using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Fight;
using static UndyneFight_Ex.GameMain;
using static UndyneFight_Ex.MathUtil;
using Color = Microsoft.Xna.Framework.Color;

namespace UndyneFight_Ex
{
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MaskDraw(Texture2D tex, Vector2 centre, Color color, float rotation, float depth, CollideRect mask)
		{
			float h = tex.Height;
			float w = tex.Width;
			MaskDraw(tex, new CollideRect(centre.X - w / 2f, centre.Y - h / 2f, w, h), color, rotation, Vector2.Zero, depth, mask);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MaskDraw(Texture2D tex, CollideRect drawArea, Color color, float rotation, Vector2 rotateCentre, float depth, CollideRect mask)
		{
			color *= Surface.Normal.drawingAlpha;
			Vector2 samplerPlace = Vector2.Zero;
			Vector2 size = drawArea.Size;
			if (mask.TopLeft.X > drawArea.X)
			{
				float delta = mask.TopLeft.X - drawArea.X;
				samplerPlace.X += delta;
				drawArea.X += delta;
				drawArea.Width -= delta;
				size.X -= delta;
			}
			if (mask.TopLeft.Y > drawArea.Y)
			{
				float delta2 = mask.TopLeft.Y - drawArea.Y;
				samplerPlace.Y += delta2;
				drawArea.Y += delta2;
				drawArea.Height -= delta2;
				size.Y -= delta2;
			}
			if (drawArea.Right > mask.Right)
			{
				float delta3 = drawArea.Right - mask.Right;
				size.X -= delta3;
				drawArea.Width -= delta3;
			}
			if (drawArea.Down > mask.Down)
			{
				float delta4 = drawArea.Down - mask.Down;
				size.Y -= delta4;
				drawArea.Height -= delta4;
			}
			if (!(size.X < 0f || size.Y < 0f))
				MissionSpriteBatch.Draw(tex, drawArea, new CollideRect(samplerPlace, size), color, rotation, rotateCentre, SpriteEffects.None, depth);
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
			float proX = Project(dirX, target), proY = Project(dirY, target);
			return new Vector2(proX / dirX.Length(), proY / dirY.Length());
		}
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
		/// Loads a file (Cross-platform)
		/// </summary>
		/// <typeparam name="T">Content type</typeparam>
		/// <param name="path">Path to file</param>
		/// <param name="cm">Content manager to use</param>
		/// <returns>The loaded content</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T LoadContent<T>(string path, ContentManager cm = null) => (cm ??= Scene.Loader).Load<T>(Path.Combine($"{AppContext.BaseDirectory}{cm.RootDirectory}\\{(path.StartsWith(cm.RootDirectory) ? path[cm.RootDirectory.Length..] : path)}".Split('\\')));
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
	}
	/// <summary>
	/// The shader class
	/// </summary>
	public class Shader()
	{
		/// <summary>
		/// Creates a shader using an existing <see cref="Effect"/>
		/// </summary>
		/// <param name="effect"></param>
		public Shader(Effect effect) : this() => this.effect = effect;
		/// <summary>
		/// Loads a shader in the given path
		/// </summary>
		/// <param name="path">The path of the shader</param>
		public Shader(string path) : this() => effect = GlobalResources.LoadContent<Effect>(path, Scene.Loader);
		private readonly Effect effect;
		private string effectName = "NormalDrawing";
		public string EffectName { get => effectName; set { effectName = value; effect.CurrentTechnique = effect.Techniques[value]; } }
		/// <summary>
		/// The parameters of the shader
		/// </summary>
		public EffectParameterCollection Parameters => effect.Parameters;
		public Dictionary<string, Action<Effect>> PartEvents { private get; set; }
		public Action<Effect> StableEvents { private get; set; }

		public bool LateApply { get; set; } = false;
		/// <summary>
		/// Applies the given texture to the shader
		/// </summary>
		/// <param name="tex">The texture to import to the shader</param>
		/// <param name="index">The index of the texture to import (Range should be [1, inf))</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterTexture(Texture2D tex, int index) => RegisterTextures[index - 1] = tex;
		/// <summary>
		/// Sets multiple parameters to the shader
		/// </summary>
		/// <param name="vals">A KeyValuePair of the name of the parameter and the value</param>
		public void SetParameters(params KeyValuePair<string, object>[] vals)
		{
			foreach (KeyValuePair<string, object> kvp in vals)
			{
				//A table is all I can think of to automatically convert all available
				//SetValue types
				if (kvp.Value is float flt)
					effect.Parameters[kvp.Key].SetValue(flt);
				else if (kvp.Value is float[] fltarr)
					effect.Parameters[kvp.Key].SetValue(fltarr);
				else if (kvp.Value is int inte)
					effect.Parameters[kvp.Key].SetValue(inte);
				else if (kvp.Value is int[] intearr)
					effect.Parameters[kvp.Key].SetValue(intearr);
				else if (kvp.Value is bool bl)
					effect.Parameters[kvp.Key].SetValue(bl);
				else if (kvp.Value is Matrix mtx)
					effect.Parameters[kvp.Key].SetValue(mtx);
				else if (kvp.Value is Matrix[] mtxarr)
					effect.Parameters[kvp.Key].SetValue(mtxarr);
				else if (kvp.Value is Quaternion quat)
					effect.Parameters[kvp.Key].SetValue(quat);
				else if (kvp.Value is Texture tex)
					effect.Parameters[kvp.Key].SetValue(tex);
				else if (kvp.Value is Vector2 vec2)
					effect.Parameters[kvp.Key].SetValue(vec2);
				else if (kvp.Value is Vector2[] vec2arr)
					effect.Parameters[kvp.Key].SetValue(vec2arr);
				else if (kvp.Value is Vector3 vec3)
					effect.Parameters[kvp.Key].SetValue(vec3);
				else if (kvp.Value is Vector3[] vec3arr)
					effect.Parameters[kvp.Key].SetValue(vec3arr);
				else if (kvp.Value is Vector4 vec4)
					effect.Parameters[kvp.Key].SetValue(vec4);
				else if (kvp.Value is Vector4[] vec4arr)
					effect.Parameters[kvp.Key].SetValue(vec4arr);
			}
		}
		public void SetParameter(KeyValuePair<string, object> vals) => SetParameters([vals]);
		public void Update()
		{
			Shader shader = this;
			shader.StableEvents?.Invoke(shader);
			if (shader.PartEvents?.TryGetValue(shader.effectName, out Action<Effect> value) ?? false)
				value(shader.effect);
		}

		public static implicit operator Effect(Shader shader) => shader.effect;

	}
	public class GLFont
	{
		public SpriteFont SFX;
		private readonly Dictionary<char, Vector2> __storedGlyphSizes = [];
		private readonly Dictionary<char, int> charIndex = [];
		public GLFont(string path, ContentManager cm)
		{
			SFX = DrawingLab.LoadContent<SpriteFont>(path, cm);
			for (int i = 0; i < SFX.Glyphs.Length; i++)
				charIndex[SFX.Glyphs[i].Character] = i;
		}
		/// <summary>
		/// Draws text
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the top left corner of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="sb">The <see cref="SpriteBatchEX"/> used to render the text (Default default renderer)</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(string texts, Vector2 location, Color color, SpriteBatchEX sb = null) => (sb ?? MissionSpriteBatch).DrawString(this, texts, location, color * Surface.Normal.drawingAlpha);
		/// <summary>
		/// Draws text
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the top left corner of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(string texts, Vector2 location, Color color, float scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
		/// <summary>
		/// Draws text
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the top left corner of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(string texts, Vector2 location, Color color, Vector2 scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
		/// <summary>
		/// Draws text
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the top left corner of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="rotation">The rotation of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="anchor">The anchor of rotation</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(string texts, Vector2 location, Color color, float rotation, float scale, Vector2 anchor, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, rotation, anchor, scale, SpriteEffects.None, depth);
		/// <summary>
		/// Draws text
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the top left corner of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="rotation">The rotation of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(string texts, Vector2 location, Color color, float rotation, float scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
		/// <summary>
		/// Draws text
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the top left corner of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="rotation">The rotation of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(string texts, Vector2 location, Color color, float rotation, vec2 scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
		/// <summary>
		/// Draws text that is aligned to the center
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the center of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="sb">The <see cref="SpriteBatchEX"/> used to render the text (Default default renderer)</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CentreDraw(string texts, Vector2 location, Color color, SpriteBatchEX sb = null)
		{
			sb ??= MissionSpriteBatch;
			string[] lines = texts.Split('\n');
			vec2 Size = SFX.MeasureString(texts);
			float initY = -Size.Y / 2;
			for (int i = 0; i < lines.Length; i++)
				sb.DrawString(this, lines[i], location + new vec2(-SFX.MeasureString(lines[i]).X / 2, initY + i * SFX.MeasureString(lines[i]).Y), color * Surface.Normal.drawingAlpha);
		}
		/// <summary>
		/// Draws text that is aligned to the center
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the center of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CentreDraw(string texts, Vector2 location, Color color, float scale, float depth) => CentreDraw(texts, location, color, new vec2(scale), depth);
		/// <summary>
		/// Draws text that is aligned to the center
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the center of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CentreDraw(string texts, Vector2 location, Color color, vec2 scale, float depth)
		{
			string[] lines = texts.Split('\n');
			vec2 Size = SFX.MeasureString(texts);
			float initY = -Size.Y / 2 * scale.Y;
			for (int i = 0; i < lines.Length; i++)
				MissionSpriteBatch.DrawString(this, lines[i], location + new vec2(0, initY + (i + 0.8f) * SFX.MeasureString(lines[i]).Y * scale.Y), color * Surface.Normal.drawingAlpha, 0, SFX.MeasureString(lines[i]) / 2, scale, SpriteEffects.None, depth);
		}
		/// <summary>
		/// Draws text that is aligned to the center
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the center of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="rotation">The rotation of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CentreDraw(string texts, Vector2 location, Color color, float scale, float rotation, float depth) => CentreDraw(texts, location, color, new vec2(scale), rotation, depth);
		/// <summary>
		/// Draws text that is aligned to the center
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The location of the center of the text</param>
		/// <param name="color">The color of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="rotation">The rotation of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CentreDraw(string texts, Vector2 location, Color color, vec2 scale, float rotation, float depth)
		{
			string[] lines = texts.Split('\n');
			vec2 Size = SFX.MeasureString(texts);
			float initY = -Size.Y / 2 * scale.Y;
			for (int i = 0; i < lines.Length; i++)
				MissionSpriteBatch.DrawString(this, lines[i], location + new vec2(0, initY + (i + 0.5f) * SFX.MeasureString(lines[i]).Y * scale.Y), color * Surface.Normal.drawingAlpha, rotation, SFX.MeasureString(lines[i]) / 2, scale, SpriteEffects.None, depth);
		}
		/// <summary>
		/// Draws a piece of text that will break to a new line when the given limit is reached
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The top left corner to draw the text with</param>
		/// <param name="color">The color of the text</param>
		/// <param name="lineLength">The maximum width of a line in pixels</param>
		/// <param name="lineDistance">The vertical distance between lines</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="depth">The depth of the text</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void LimitDraw(string texts, Vector2 location, Color color, float lineLength, float lineDistance, float scale, float depth)
		{
			Vector2[] sizes = new Vector2[texts.Length];
			for (int i = 0; i < texts.Length; i++)
				sizes[i] = SFX.MeasureString(texts[i].ToString());

			string curLine = string.Empty;
			float cur = 0;
			List<string> strings = [];
			int accuSpace = 0;
			for (int i = 0; i < texts.Length; i++)
			{
				float v;
				bool u;
				cur += v = sizes[i].X * scale;
				if ((u = texts[i] is '\r' or '\n') || cur > lineLength)
				{
					if (cur > lineLength)
					{
						int lastSpace = curLine.LastIndexOf(' ');
						if (lastSpace != -1)
						{
							accuSpace += lastSpace;
							curLine = curLine[..lastSpace];
							i = accuSpace + 1;
							v = sizes[i].X * scale;
						}
					}
					strings.Add(curLine);
					curLine = string.Empty;
					cur = v;
					if (u)
						continue;
				}
				curLine += texts[i];
			}
			strings.Add(curLine);
			foreach (string s in strings)
			{
				string finalText = s;
				if (s.StartsWith(' '))
					finalText = s[1..];
				MissionSpriteBatch.DrawString(this, finalText, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
				location.Y += lineDistance;
			}
		}
		/// <summary>
		/// Draws a piece of text that will break to a new line when the given limit is reached
		/// </summary>
		/// <param name="texts">The text to draw</param>
		/// <param name="location">The top left corner to draw the text with</param>
		/// <param name="color">The color of the text</param>
		/// <param name="size">The area to restrict the text in</param>
		/// <param name="lineDistance">The vertical distance between lines</param>
		/// <param name="scale">The scale of the text (Note that the size may shrink because <paramref name="size"/> is too small</param>
		/// <param name="depth">The depth of the text</param>
		/// <param name="by_word">Whether the line break will consider the spaces</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void LimitDraw(string texts, Vector2 location, Color color, Vector2 size, float lineDistance, float scale, float depth, bool by_word = true)
		{
			Vector2[] sizes = new Vector2[texts.Length];
			for (int i = 0; i < texts.Length; i++)
				sizes[i] = SFX.MeasureString(texts[i].ToString());

			string curLine = string.Empty;
			float cur = 0;
			int accuSpace = 0;
			List<string> strings = [];
			for (int i = 0; i < texts.Length; i++)
			{
				float v;
				bool u;
				cur += v = sizes[i].X * scale;
				if ((u = texts[i] is '\r' or '\n') || cur > size.X)
				{
					if (cur > size.X)
					{
						int lastSpace = curLine.LastIndexOf(' ');
						if (lastSpace != -1)
						{
							accuSpace += lastSpace;
							curLine = curLine[..lastSpace];
							i = accuSpace + 1;
							v = sizes[i].X * scale;
						}
					}
					strings.Add(curLine);
					curLine = string.Empty;
					cur = v;
					if (u)
						continue;
				}
				curLine += texts[i];
			}
			strings.Add(curLine);
			float originalLineDist = lineDistance;
			while (lineDistance * (strings.Count + 1) * scale > size.Y)
			{
				scale -= 0.1f;
				lineDistance = originalLineDist * scale;
			}
			foreach (string s in strings)
			{
				string finalText = s;
				if (s.StartsWith(' '))
					finalText = s[1..];
				MissionSpriteBatch.DrawString(this, finalText, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
				location.Y += lineDistance;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe int GetGlyphIndexOrDefault(char c) => charIndex[c];
		/// <summary>
		/// Gets the size of a specified <see cref="char"/>
		/// </summary>
		/// <param name="ch">The character to measure the size of</param>
		/// <returns>The size of the given character</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2 MeasureChar(char ch)
		{
			if (!__storedGlyphSizes.TryGetValue(ch, out Vector2 size))
				return size;
			else
			{
				_ = __storedGlyphSizes.TryAdd(ch, size);
				return SFX.Glyphs[GetGlyphIndexOrDefault(ch)].Cropping.Size.ToVector2();
			}
		}
	}
}