namespace UndyneFight_Ex.Entities;
/// <summary>
/// A fireball
/// </summary>
public class FireBall : CircleCollisionBarrage
{
	private float time = 0;
	/// <summary>
	/// Creates a fireball with the given position easing
	/// </summary>
	/// <param name="_positionRoute">The easing of the position of the fireball</param>
	public FireBall(EaseUnit<Vector2> _positionRoute) : base(4)
	{
		PositionRoute = _positionRoute.Easing;
		Depth = 0.5f;
		Alpha = 1.0f;
		UpdateIn120 = true;
	}
	/// <inheritdoc/>
	public override void Update()
	{
		base.Update();
		time += 0.5f;
	}
	/// <inheritdoc/>
	public override void Draw() => GeneralDraw(FightResources.Sprites.fireball, Centre, Color.White * Alpha, Scale * new Vector2((time % 10) < 5 ? -1 : 1, 1), Rotation);
}