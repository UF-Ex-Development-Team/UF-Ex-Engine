using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;

namespace UndyneFight_Ex.Entities;
/// <summary>
/// A shootable bomb
/// </summary>
public class Bomb : BulletShootable
{
	private class BombBlast : PerfectCollisionBarrage
	{
		private readonly Texture2D[] allStates;
		public BombBlast(Vector2 position, Texture2D[] mode)
		{
			allStates = mode;
			Centre = position;
			Image = allStates[0];
			Alpha = 1.0f;
			Depth = 0.53f;
			Hidden = false;
		}
		private int index = 0, count = 0;

		public override void Update()
		{
			if (++count > 4)
			{
				count -= 2;
				if (++index >= allStates.Length)
				{
					Dispose();
					return;
				}
				Image = allStates[index];
			}
		}
	}
	private readonly float _explodeDelay;
	/// <summary>
	/// Creates a shootable bomb
	/// </summary>
	/// <param name="explodeDelay">The delay before exploding when shot</param>
	/// <param name="ease">The easing of the position</param>
	public Bomb(float explodeDelay, Func<ICustomMotion, Vector2> ease)
	{
		_explodeDelay = explodeDelay;
		Image = Sprites.MettBomb[0];
		Alpha = 1.0f;
		PositionRoute = ease;
		Depth = 0.501f;
	}
	private readonly List<Entity> canDestroy = [];
	private bool isShot = false;
	/// <inheritdoc/>
	protected override void OnShot(SoulBullet bullet)
	{
		bullet.Dispose();
		Trigger();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Trigger()
	{
		PlaySound(Sounds.Warning);
		isShot = true;
	}

	private int count = 0;
	/// <summary>
	/// Whether the bomb can be triggered by other explosions
	/// </summary>
	public bool AbleLink { get; set; } = true;
	/// <summary>
	/// Whether the bomb can destroy other <see cref="BulletShootable"/> objects
	/// </summary>
	public bool Destructive { get; set; } = true;
	/// <inheritdoc/>
	public override void Update()
	{
		base.Update();
		if (isShot)
		{
			count++;
			Image = Sprites.MettBomb[count % 8 < 4 ? 1 : 0];
			if (count / 2f >= _explodeDelay)
			{
				PlaySound(Sounds.Bomb);
				Explode();
			}
		}
	}
	/// <summary>
	/// Explode the bomb
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Explode()
	{
		Broadcast("Explode");
		GenerateBlast();
		Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GenerateBlast()
	{
		canDestroy.ForEach(c => c.Dispose());
		Vector2 centre = Centre;
		GameStates.InstanceCreate(new BombBlast(centre, Sprites.MettBombCoreBlast) { controlLayer = controlLayer, Depth = Depth });
		for (int i = 0; i < 4; i++)
		{
			float rotation = i * 90;
			centre = Centre;
			while (Screen.Contain(centre))
			{
				centre += MathUtil.GetVector2(20, rotation);
				GameStates.InstanceCreate(new BombBlast(centre, Sprites.MettBombBlast)
				{ controlLayer = controlLayer, Depth = Depth, Rotation = i % 2 == 0 ? 0 : 90 });
			}
		}
	}
	private readonly CollideRect Screen = new(0, 0, 640, 480);
}