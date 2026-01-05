global using UndyneFight_Ex;
namespace UndyneFight_Ex.Entities;
/// <summary>
/// A barrage that is shootable by a yellow soul bullet
/// </summary>
public abstract class BulletShootable : PerfectCollisionBarrage
{
	/// <summary>
	/// Event to occur when the barrage is shot
	/// </summary>
	/// <param name="bullet">The soul bullet that shot it</param>
	protected abstract void OnShot(SoulBullet bullet);

	private readonly List<SoulBullet> detects = [];
	/// <inheritdoc/>
	public override void Update()
	{
		base.Update();
		(bool EventExists, GameEventArgs EventArguments) = TryDetect("Bullet");
		if (EventExists)
		{
			GameEventArgs args = EventArguments;
			SoulBullet newBullet = args.Source as SoulBullet;
			detects.Add(newBullet);
			EventArguments.Dispose();
		}
		_ = detects.RemoveAll(s => s.Disposed);
		detects.ForEach(bullet =>
		{
			if (bullet.BeingUpdated)
			{
				//Get vertices of this box
				float sqrt = MathF.Sqrt(MathF.Pow(Image.Width, 2) + MathF.Pow(Image.Height, 2)) * Scale / 2f;
				Vector2[] thisVertices = new Vector2[4];
				for (int i = 0; i < 4; i++)
					thisVertices[i] = Centre + MathUtil.GetVector2(sqrt, 45 + i * 90 + Rotation);
				//Get vertices of the soul bullet
				sqrt = MathF.Sqrt(MathF.Pow(bullet.Image.Width, 2) + MathF.Pow(bullet.Image.Height, 2)) * Scale / 2f + 2;
				Vector2[] bulletVertices = new Vector2[4];
				float theta = MathUtil.GetAngle(MathF.Atan(bullet.Image.Width / (float)bullet.Image.Height));
				float[] verticesAngles = [theta, 180 - theta, 180 + theta, -theta];
				for (int i = 0; i < 4; i++)
					bulletVertices[i] = bullet.Centre + MathUtil.GetVector2(sqrt, verticesAngles[i] + bullet.Rotation);
				if (MathUtil.PolygonCollide(thisVertices, bulletVertices))
					OnShot(bullet);
			}
		});
		(EventExists, EventArguments) = TryDetect("Explode");
		if (EventExists)
		{
			GameEventArgs args = EventArguments;
			Bomb et = args.Source as Bomb;
			float dis = 6;
			args.Dispose();

			if (MathF.Abs(et.Centre.X - Centre.X) <= dis || MathF.Abs(et.Centre.Y - Centre.Y) <= dis)
			{
				if (this is Bomb b)
				{
					if (b.AbleLink)
						b.Explode();
				}
				else
				{
					if (et.Destructive)
						Dispose();
				}
			}
		}
	}
}