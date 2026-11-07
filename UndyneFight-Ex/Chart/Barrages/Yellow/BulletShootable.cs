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
		GameEventArgs args = null;
		Bomb et = null;
		if (EventExists)
		{
			args = EventArguments;
			et = args.Source as Bomb;
			SoulBullet newBullet = args.Source as SoulBullet;
			detects.Add(newBullet);
			EventArguments.Dispose();
		}
		_ = detects.RemoveAll(s => s.Disposed);
		//Get vertices of this box
		float imgWidth = Image.Width, imgHeight = Image.Height;
		float sqrt = MathF.Sqrt(imgWidth * imgWidth + imgHeight * imgHeight) * Scale / 2f;
		Vector2[] thisVertices = new Vector2[4];
		for (int i = 0; i < 4; i++)
			thisVertices[i] = Centre + MathUtil.GetVector2(sqrt, 45 + i * 90 + Rotation);
		detects.ForEach(bullet =>
		{
			if (!bullet.BeingUpdated)
				return;
			//Get vertices of the soul bullet
			float bulWidth = bullet.Image.Width, bulHeight = bullet.Image.Height;
			sqrt = MathF.Sqrt(bulWidth * bulWidth + bulHeight * bulHeight) * Scale / 2f + 2;
			Vector2[] bulletVertices = new Vector2[4];
			float theta = MathUtil.GetAngle(MathF.Atan(bulWidth / (float)bulHeight));
			float[] verticesAngles = [theta, 180 - theta, 180 + theta, -theta];
			for (int i = 0; i < 4; i++)
				bulletVertices[i] = bullet.Centre + MathUtil.GetVector2(sqrt, verticesAngles[i] + bullet.Rotation);
			if (MathUtil.PolygonCollide(thisVertices, bulletVertices))
				OnShot(bullet);
		});
		(EventExists, EventArguments) = TryDetect("Explode");
		//Early exit if no event exists
		if (!EventExists || et is null)
			return;
		float dis = 6;
		args.Dispose();
		//Early exit if not in range
		if (MathF.Abs(et.Centre.X - Centre.X) > dis && MathF.Abs(et.Centre.Y - Centre.Y) > dis)
			return;
		if (this is Bomb b)
		{
			if (b.AbleLink)
				b.Explode();
		}
		else if (et.Destructive)
			Dispose();
	}
}