using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Fight;
using static UndyneFight_Ex.Entities.EasingUtil;
using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.FightResources;

namespace UndyneFight_Ex.Entities;
/// <summary>
/// A mettaton robot with parasol
/// </summary>
public class ParasolMett : BulletShootable
{
	/// <summary>
	/// The rotation easing duration
	/// </summary>
	public float WaveTime { get; set; } = 50.0f;
	/// <summary>
	/// The depth of the thrown bullet
	/// </summary>
	public float BulletDepth { get; set; } = 0.5f;

	private class MettBullet : PerfectCollisionBarrage
	{
		public MettBullet(Vector2 pos, float waveTime)
		{
			Image = Sprites.MettBullet;
			Centre = pos;
			AngleMode = true;

			PositionRoute = InfLinear(Centre, MathUtil.GetVector2(4.0f, MathUtil.Direction(Centre, Functions.Heart.Centre)));
			RotationRoute = SineWave(10, waveTime);
		}
		public override void Update()
		{
			base.Update();
			if (Alpha < 1.0f)
				Alpha += 0.05f;
		}
	}
	private ParasolMett()
	{
		images = Sprites.ParasolMett;
		Image = images[0];
		shootQueue = new();
		Alpha = 1.0f;
	}
	/// <summary>
	/// Creates a mettaton with parasol
	/// </summary>
	/// <param name="centreEasing">The easing of the position</param>
	public ParasolMett(EaseUnit<Vector2> centreEasing) : this() => PositionRoute = centreEasing.Easing;
	private readonly Texture2D[] images;

	private readonly Queue<float> shootQueue;
	/// <summary>
	/// Adds the amount of heart fired with the given delay
	/// </summary>
	/// <param name="val">The amount of hearts to throw</param>
	/// <param name="time">The delay between each throw</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void InsertShot(int val, float time)
	{
		for (int a = 0; a < val; a++)
			shootQueue.Enqueue(time * a);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override void OnShot(SoulBullet bullet)
	{
		bullet.Dispose();
		Dispose();
		Functions.PlaySound(Sounds.TargetBurst);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void MakeShoot() => index = 0;
	private int index = -1;

	private float appearTime = 0;
	/// <inheritdoc/>
	public override void Update()
	{
		base.Update();
		appearTime += 0.5f;
		while (shootQueue.Count > 0 && shootQueue.Peek() <= appearTime)
		{
			_ = shootQueue.Dequeue();
			MakeShoot();
		}
		if (index != -1 && appearTime % 1f < 0.5f)
		{
			if (++index >= images.Length)
			{
				index = -1;
				GameStates.InstanceCreate(new MettBullet(Centre, WaveTime) { Depth = BulletDepth });
			}
		}
		if (index != -1)
			Image = images[index];
	}
}