namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// A custom box vertex
	/// </summary>
	public class BoxVertex
	{
		/// <summary>
		/// The current position of the vertex
		/// </summary>
		public vec2 CurrentPosition { get; set; }
		/// <summary>
		/// The target position of the vertex
		/// </summary>
		public vec2 MissionPosition { get; set; }
		/// <summary>
		/// Distance towards the target position
		/// </summary>
		internal float ToMissionDistance => (CurrentPosition - MissionPosition).Length();
		/// <summary>
		/// The ID of the vertex
		/// </summary>
		public int ID
		{
			get
			{
				if (_id == -1)
					_id = Previous == null ? 0 : Previous.ID + 1;
				return _id;
			}
		}
		private int _id = -1;

		/// <summary>
		/// Creates a box vertex with a given position
		/// </summary>
		/// <param name="pos">The position of the vertex</param>
		public BoxVertex(vec2 pos) => CurrentPosition = MissionPosition = pos;
		/// <summary>
		/// Creates a box vertex
		/// </summary>
		public BoxVertex() { }
		/// <summary>
		/// The previous box vertex
		/// </summary>
		public BoxVertex Previous { get; set; }
		/// <summary>
		/// Moves the box position by 1 frame in the given lerp scale
		/// </summary>
		/// <param name="scale">The lerp scale to move</param>

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Move(float scale) => CurrentPosition = CurrentPosition * (1 - scale) + MissionPosition * scale;
		/// <summary>
		/// Sets the box vertex to it's <see cref="MissionPosition"/>
		/// </summary>
		/// <param name="position">The <see cref="MissionPosition"/> of the vertex</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InstantMove(vec2 position) => CurrentPosition = MissionPosition = position;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private LinkedList<BoxVertex> GetAll(LinkedList<BoxVertex> prev)
		{
			if (prev.First.Value != this)
			{
				_ = prev.AddLast(this);
				_ = GetAll(prev);
			}
			return prev;
		}
		public BoxVertex[] GetAll() => [.. GetAll(new())];
		public static implicit operator Vector2(BoxVertex v) => v.CurrentPosition;
		public static implicit operator BoxVertex(Vector2 v) => new(v);
	}
	/// <summary>
	/// The box during fight
	/// </summary>
	public abstract class FightBox : Entity
	{
		/// <summary>
		/// The list of vertices of the box
		/// </summary>
		public BoxVertex[] Vertices { get; set; }
		/// <summary>
		/// The <see cref="FightBox"/> instance
		/// </summary>
		public static FightBox instance { get; set; }
		/// <summary>
		/// The list of Fight Boxes
		/// </summary>
		public static List<FightBox> boxes { get; set; } = [];
		/// <summary>
		/// Moves the box to the given position
		/// </summary>
		/// <param name="v"></param>
		public abstract void MoveTo(object v);
		/// <summary>
		/// Immediately moves the box to the given position
		/// </summary>
		/// <param name="v"></param>
		public abstract void InstanceMove(object v);

		protected readonly Player.Heart detect;
		public Player.Heart Detect => detect;
		/// <summary>
		/// The lerp value of the box (Default 0.15f)
		/// </summary>
		public float MovingScale { get; set; } = 0.15f;
		/// <summary>
		/// The alpha of the box in green soul mode
		/// </summary>
		public float GreenSoulAlpha { get; set; } = 0.5f;
		/// <summary>
		/// Instantly sets the alpha of the box
		/// </summary>
		/// <param name="alpha">The alpha of the box</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InstantSetAlpha(float alpha) => GreenSoulAlpha = curAlpha = alpha;
		private float curAlpha = 1.0f;

		private bool _doDraw = false;

		public override void Update()
		{
			_doDraw = false;
			curAlpha = curAlpha * 0.9f + (detect?.SoulType == 1 ? GreenSoulAlpha : 1) * 0.1f;
			if (Vertices == null)
				return;
			float scale = MovingScale * 0.6f;
			for (int i = 0; i < Vertices.Length; i++)
			{
				Vertices[i].Move(scale + MathF.Max(0, 1 - scale - Vertices[i].ToMissionDistance));
			}
		}
		public override void Draw()
		{
			if (_doDraw || !Visible)
				return;
			_doDraw = true;
			vec2 gravity = vec2.Zero;
			foreach (BoxVertex item in Vertices)
				gravity += item.CurrentPosition / Vertices.Length;
			vec2[] positions = new vec2[Vertices.Length];
			for (int i = 0; i < Vertices.Length; i++)
			{
				vec2 delta = Vertices[i].CurrentPosition - gravity;
				delta = MathUtil.GetVector2(delta.Length() + 2f, MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI);
				positions[i] = delta + gravity;
			}
			for (int i = 0; i < Vertices.Length; i++)
			{
				DrawingLab.DrawLine(positions[i], positions[(i + 1) % Vertices.Length], 4.2f, new(col.Lerp(GameMain.CurrentDrawingSettings.backGroundColor, GameMain.CurrentDrawingSettings.themeColor, curAlpha), curAlpha * 255), 0.4f);
			}
		}

		public FightBox()
		{
			UpdateIn120 = true;
			boxes.Add(this);
			instance = this;
		}
		public FightBox(Player.Heart p) : this() => detect = p;
	}

	public class VertexBox : FightBox
	{
		public VertexBox(Player.Heart heart, RectangleBox rectangleBox) : base(heart) => Vertices = [new(rectangleBox.CollidingBox.TopRight),
						new(rectangleBox.CollidingBox.BottomRight),
						new(rectangleBox.CollidingBox.BottomLeft),
						new(rectangleBox.CollidingBox.TopLeft)];

		public override void Draw() => base.Draw();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InstanceMove(object v)
		{
			if (v is not vec2[])
				throw new ArgumentException($"{nameof(v)} has to be an vector array");

			vec2[] temp = v as vec2[];
			if (temp.Length != Vertices.Length)
				throw new ArgumentOutOfRangeException($"{nameof(v)} must be in same length with vertex count");

			for (int i = 0; i < temp.Length; i++)
				Vertices[i].InstantMove(temp[i]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void MoveTo(object v)
		{
			if (v is not vec2[])
				throw new ArgumentException($"nameof(v) has to be an vector array");

			vec2[] temp = v as vec2[];
			if (temp.Length != Vertices.Length)
				throw new ArgumentOutOfRangeException($"{nameof(v)} must be in same length with vertex count");

			int i = 0;
			foreach (vec2 val in temp)
				Vertices[i++].MissionPosition = val;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Split(int originID, float scale)
		{
			if (scale is < 0 or > 1)
				throw new ArgumentOutOfRangeException($"{nameof(scale)} has to be in [0, 1]");
			if (originID != Vertices.Length - 1)
			{
				BoxVertex a = Vertices[originID], b = Vertices[originID + 1];
				vec2 pos = vec2.Lerp(a.CurrentPosition, b.CurrentPosition, scale);

				BoxVertex[] temp = new BoxVertex[Vertices.Length + 1];
				Array.Copy(Vertices, 0, temp, 0, originID + 1);
				temp[originID + 1] = new(pos);
				Array.Copy(Vertices, originID + 1, temp, originID + 2, Vertices.Length - originID - 1);

				Vertices = temp;
			}
			else
			{
				BoxVertex a = Vertices[originID], b = Vertices[0];
				vec2 pos = vec2.Lerp(a.CurrentPosition, b.CurrentPosition, scale);

				BoxVertex[] temp = new BoxVertex[Vertices.Length + 1];
				Array.Copy(Vertices, temp, Vertices.Length);
				temp[originID + 1] = new(pos);

				Vertices = temp;
			}

			return originID + 1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Split(int originID, float[] scales)
		{
			if (scales == null || scales.Length <= 0)
				throw new ArgumentOutOfRangeException($"{nameof(scales)} should include elements");

			BoxVertex a = Vertices[originID], b = Vertices[originID + 1];
			vec2[] pos = new vec2[scales.Length];

			int i;
			for (i = 0; i < pos.Length; i++)
				pos[i] = vec2.Lerp(a.CurrentPosition, b.CurrentPosition, scales[i]);

			BoxVertex[] temp = new BoxVertex[Vertices.Length + scales.Length];

			i = 0;
			int j = 0, k = 0;
			for (; i <= originID; i++, j++)
				temp[j] = Vertices[i];
			for (; k < scales.Length; k++, j++)
				temp[j] = new(pos[k]);
			for (; i < Vertices.Length; i++, j++)
				temp[j] = Vertices[i];

			Vertices = temp;

			return originID + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPosition(int originID, vec2 position) => Vertices[originID].MissionPosition = position;

		public override void Update()
		{
			base.Update();
			float x1 = Vertices[0].CurrentPosition.X, x2 = Vertices[0].CurrentPosition.X,
				  y1 = Vertices[0].CurrentPosition.Y, y2 = Vertices[0].CurrentPosition.Y;
			for (int i = 1; i < Vertices.Length; i++)
			{
				x1 = MathF.Min(x1, Vertices[i].CurrentPosition.X);
				x2 = MathF.Max(x2, Vertices[i].CurrentPosition.X);
				y1 = MathF.Min(y1, Vertices[i].CurrentPosition.Y);
				y2 = MathF.Max(y2, Vertices[i].CurrentPosition.Y);
			}
			collidingBox = new(x1, y1, x2 - x1, y2 - y1);
		}
	}
	/// <summary>
	/// A rectangle box
	/// </summary>
	public class RectangleBox : FightBox
	{
		public override void Dispose()
		{
			foreach (GravityLine v in gravityLines)
				v?.Dispose();
			_ = boxes.Remove(this);
			base.Dispose();
		}
		/// <summary>
		/// Creates a rectangle box using the given <see cref="CollideRect"/>
		/// </summary>
		/// <param name="Area">The area of the rectangle</param>
		public RectangleBox(CollideRect Area)
		{
			Vertices = [new(), new(), new(), new()];
			InstanceMove(Area);
			collidingBox = Area;
		}
		public RectangleBox(Player.Heart p, CollideRect? area = null) : base(p)
		{
			Vertices = [new(), new(), new(), new()];
			collidingBox = area ??= new(0, 0, 640, 480);
			InstanceMove(area);
			gravityLines = [right, down, left, up];
		}

		public override void Update()
		{
			vec2 v1 = collidingBox.TopLeft,
				v2 = collidingBox.TopRight,
				v3 = collidingBox.BottomLeft,
				v4 = collidingBox.BottomRight;
			up.SetPosition(v1, v2);
			right.SetPosition(v2, v4);
			left.SetPosition(v3, v1);
			down.SetPosition(v4, v3);
			Vertices[0].CurrentPosition = v1;
			Vertices[1].CurrentPosition = v2;
			Vertices[3].CurrentPosition = v3;
			Vertices[2].CurrentPosition = v4;

			base.Update();
			collidingBox = new CollideRect(Vertices[0].CurrentPosition, Vertices[2].CurrentPosition - Vertices[0].CurrentPosition);

			if (detect == null)
				return;
			bool[] enabled = [false, false, false, false];
			if (detect.SoulType is 2 or 5)
			{
				enabled[detect.YFacing] = true;
			}
			else
				enabled = [true, true, true, true];
			for (int i = 0; i < 4; i++)
				gravityLines[i].enabled = enabled[i];
		}

		private readonly GravityLine[] gravityLines = new GravityLine[4];
		private readonly GravityLine up = new(vec2.Zero, vec2.Zero);
		private readonly GravityLine down = new(vec2.Zero, vec2.Zero);
		private readonly GravityLine left = new(vec2.Zero, vec2.Zero);
		private readonly GravityLine right = new(vec2.Zero, vec2.Zero);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void MoveTo(object cl)
		{
			vec2[] temp = ((CollideRect)cl).GetVertices();
			int i = 0;
			foreach (vec2 val in temp)
				Vertices[i++].MissionPosition = val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void InstanceMove(object cl)
		{
			vec2[] temp = ((CollideRect)cl).GetVertices();
			int i = 0;
			foreach (vec2 val in temp)
				Vertices[i++].InstantMove(val);
			collidingBox = (CollideRect)cl;
		}
		/// <summary>
		/// The x coordinate of the left side of the box
		/// </summary>
		public float Left => collidingBox.X;
		/// <summary>
		/// The y coordinate of the top side of the box
		/// </summary>
		public float Up { get => collidingBox.Y; set => InstanceMove(new CollideRect(Left, value, Width, Down - value)); }
		/// <summary>
		/// The x coordinate of the right side of the box
		/// </summary>
		public float Right => collidingBox.Right;
		/// <summary>
		/// The Y coordinate of the down side of the box
		/// </summary>
		public float Down { get => collidingBox.Down; set => InstanceMove(new CollideRect(Left, Up, Width, value - Up)); }
		/// <summary>
		/// The width of the box
		/// </summary>
		public float Width => Right - Left;
		/// <summary>
		/// The height of the box
		/// </summary>
		public float Height => Down - Up;
	}
}