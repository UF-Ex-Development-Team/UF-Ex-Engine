using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities;
/// <summary>
/// A block that will be destroyed upon being shot at
/// </summary>
public class ShootableBlock : BulletShootable
{
	/// <summary>
	/// Creates a shootable block
	/// </summary>
	/// <param name="centreEasing">The easing of the position</param>
	public ShootableBlock(EaseUnit<Vector2> centreEasing)
	{
		AngleMode = true;
		Image = Sprites.MettBlockA;
		Depth = 0.45f;
		PositionRoute = centreEasing.Easing;
	}
	/// <inheritdoc/>
	protected override void OnShot(SoulBullet bullet)
	{
		bullet.Dispose();
		PlaySound(Sounds.TargetBurst);
		InstanceCreate(new Shattered(this));
		Dispose();
	}
}
/// <summary>
/// A block that cannot be destroyed upon being shot at
/// </summary>
public class ToughBlock : BulletShootable
{
	/// <summary>
	/// Creates a tough block
	/// </summary>
	/// <param name="centreEasing">The easing of the position</param>
	public ToughBlock(EaseUnit<Vector2> centreEasing)
	{
		AngleMode = true;
		Image = Sprites.MettBlockB;
		Depth = 0.45f;
		PositionRoute = centreEasing.Easing;
	}
	/// <inheritdoc/>
	protected override void OnShot(SoulBullet bullet)
	{
		bullet.Dispose();
		PlaySound(Sounds.Ding);
	}
}