using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;

namespace UndyneFight_Ex.Entities;
/// <summary>
/// A yellow soul bullet
/// </summary>
public class SoulBullet : AutoEntity, ICustomMotion
{
	/// <summary>
	/// Creates a yellow soul bullet
	/// </summary>
	/// <param name="heart">The heart to create the bullet from</param>
	public SoulBullet(Player.Heart heart)
	{
		UpdateIn120 = true;
		_origin = heart.Centre + MathUtil.GetVector2(14, heart.Rotation + 90);
		PositionRoute = InfLinear(MathUtil.GetVector2(22, heart.Rotation + 90));
		Image = Sprites.SoulShoot;
		PlaySound(Sounds.YellowShoot);
		Rotation = heart.Rotation + 180;
		AngleMode = true;
		Broadcast("Bullet");
	}

	/// <inheritdoc/>
	public Func<ICustomMotion, Vector2> PositionRoute { get; set; }
	/// <inheritdoc/>
	public Func<ICustomMotion, float> RotationRoute { get; set; }
	/// <inheritdoc/>
	public float[] RotationRouteParam { get; set; }
	/// <inheritdoc/>
	public float[] PositionRouteParam { get; set; }

	/// <inheritdoc/>
	public float AppearTime { get; set; } = 0.0f;

	/// <inheritdoc/>
	public Vector2 CentrePosition => _delta;

	private Vector2 _delta;
	private Vector2 _origin;

	/// <inheritdoc/>
	public override void Update()
	{
		_delta = PositionRoute(this);
		Centre = _delta + _origin;
		AppearTime += 0.5f;
		Depth = 0.5f;
		Alpha = 1.0f;

		if (!new Rectangle(0, 0, 640, 480).Contains(Centre.ToPoint()))
			Dispose();
	}
}