using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;

namespace UndyneFight_Ex.Entities;
/// <summary>
/// A spider
/// </summary>
public class Spider : CircleCollisionBarrage
{
	/// <summary>
	/// Creates a spider with the given x and y easing
	/// </summary>
	/// <param name="x">The easing of the x coordinate of the spider</param>
	/// <param name="y">The easing of the y coordinate of the spider</param>
	public Spider(EaseUnit<float> x, EaseUnit<float> y) : this() => PositionRoute = Combine(x, y);
	/// <summary>
	/// Creates a spider with the given position easing and rotation
	/// </summary>
	/// <param name="_positionRoute">The easing of the position of the spider</param>
	/// <param name="Rotation">The rotation of the spider (Default the direction of the easing)</param>
	public Spider(EaseUnit<Vector2> _positionRoute, float? Rotation = null) : this()
	{
		PositionRoute = _positionRoute.Easing;
		this.Rotation = Rotation ?? MathUtil.Direction(_positionRoute.Start, _positionRoute.End);
	}
	/// <summary>
	/// Creates a spider with the given position easing and rotation easing
	/// </summary>
	/// <param name="_positionRoute">The easing of the position of the spider</param>
	/// <param name="_rotationRoute">The easing of the rotation of the spider</param>
	public Spider(Func<ICustomMotion, Vector2> _positionRoute, Func<ICustomMotion, float> _rotationRoute) : this()
	{
		PositionRoute = _positionRoute;
		RotationRoute = _rotationRoute;
	}
	/// <summary>
	/// Creates a spider with no attributes
	/// </summary>
	protected internal Spider() : base(5)
	{
		Image = Sprites.spider;
		Depth = 0.5f;
		Alpha = 1.0f;
		UpdateIn120 = true;
		Hidden = true;
		AngleMode = true;
	}
}
/// <summary>
/// A spider on a line
/// </summary>
public class LineSpider : Spider
{
	private readonly float count;
	/// <summary>
	/// The speed of the spider
	/// </summary>
	public float Speed;
	/// <summary>
	/// Creates a spider on a line
	/// </summary>
	/// <param name="CountLine">The line to create on</param>
	/// <param name="IsLeftOrRight">True for coming from the left, False for coming from the right</param>
	/// <param name="Speed">The speed of the spider</param>
	public LineSpider(int CountLine, bool IsLeftOrRight, float Speed)
	{
		collidingBox.X = IsLeftOrRight ? BoxStates.Centre.X - BoxStates.Width / 2f - Image.Width : BoxStates.Centre.X + BoxStates.Width / 2f + Image.Width;
		Rotation = IsLeftOrRight ? 0 : 180;
		this.Speed = Speed;
		movingWay = IsLeftOrRight;
		count = CountLine;
	}
	private readonly bool movingWay;
	/// <inheritdoc/>
	public override void Update()
	{
		collidingBox.X += Speed * 0.5f * (movingWay ? 1 : -1);
		collidingBox.Y = BoxStates.Centre.Y - BoxStates.Height / 2f + BoxStates.Height / (Heart.PurpleLineCount + 1f) * count;
		base.Update();
	}
}
/// <summary>
/// A spider that moves vertically
/// </summary>
public class VerticalSpider : Spider
{
	private readonly float count;
	/// <summary>
	/// The speed of the spider
	/// </summary>
	public float Speed;
	/// <summary>
	/// Creates a vertical spider
	/// </summary>
	/// <param name="X">The x coordinate of the spider</param>
	/// <param name="IsUpOrDown">True for coming from up, False for coming from down</param>
	/// <param name="Speed">The speed of the spider</param>
	public VerticalSpider(float X, bool IsUpOrDown, float Speed)
	{
		collidingBox.Y = IsUpOrDown ? BoxStates.Centre.Y - BoxStates.Height / 2 - 50 : BoxStates.Centre.Y + BoxStates.Height / 2 + 50;
		Rotation = IsUpOrDown ? 90 : 270;
		this.Speed = Speed;
		movingWay = IsUpOrDown;
		count = X;
	}
	private readonly bool movingWay;
	/// <inheritdoc/>
	public override void Update()
	{
		collidingBox.X = count;
		collidingBox.Y += Speed * 0.5f * (movingWay ? 1 : -1);
		base.Update();
	}
}