using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

internal class Bullet : Barrage
{
	public Bullet(Vector2 centre, float rotation)
	{
		controlLayer = Surface.Hidden;
		Depth = 0.64f;
		missionCentre = centre;
		Rotation = rotation;

		rotation %= 360;

		CollideRect rect = FightBox.instance.CollidingBox;

		bool isDownEnabled = true, isUpEnabled = true, isLeftEnabled = true, isRightEnabled = true;
		float downCollideX = 0, upCollideX = 0, rightCollideY = 0, leftCollideY = 0;

		if (rotation is >= 0 and <= 180)
			isUpEnabled = false;
		else
			upCollideX = missionCentre.X + (rect.Up - missionCentre.Y) / Tan(rotation);
		if (rotation is >= 90 and <= 270)
			isRightEnabled = false;
		else
			rightCollideY = missionCentre.Y + (rect.Right - missionCentre.X) * Tan(rotation);
		if (rotation is >= 180 or 0)
			isDownEnabled = false;
		else
			downCollideX = missionCentre.X + (rect.Down - missionCentre.Y) / Tan(rotation);
		if (rotation is >= 270 or <= 90)
			isLeftEnabled = false;
		else
			leftCollideY = missionCentre.Y + (rect.Left - missionCentre.X) * Tan(rotation);

		if (isDownEnabled && downCollideX >= rect.Left && downCollideX <= rect.Right)
			distance = (new Vector2(downCollideX, rect.Down) - missionCentre).Length();
		else if (isUpEnabled && upCollideX >= rect.Left && upCollideX <= rect.Right)
			distance = (new Vector2(upCollideX, rect.Up) - missionCentre).Length();
		else if (isLeftEnabled && leftCollideY >= rect.Up && leftCollideY <= rect.Down)
			distance = (new Vector2(rect.Left, leftCollideY) - missionCentre).Length();
		else if (isRightEnabled && rightCollideY >= rect.Up && leftCollideY <= rect.Down)
			distance = (new Vector2(rect.Right, rightCollideY) - missionCentre).Length();
		distance += 20;
		Image = Sprites.bullet;
	}

	private Vector2 missionCentre;
	private float distance;

	private static CollideRect screen = new(-50, -50, 740, 580);

	public override void Update()
	{
		distance -= 12;
		Centre = missionCentre + GetVector2(distance, Rotation);

		if (!screen.Contain(Centre))
			Dispose();
	}

	public override void Draw() => FormalDraw(Image, Centre, Color.White, 0.8f, GetRadian(Rotation), ImageCentre);

	public override void Dispose()
	{
		if (!hasHit && MarkScore)
			PushScore(score);
		base.Dispose();
	}

	public override void GetCollide(Player.Heart player)
	{
		float res = (Centre - player.Centre).Length() - 11;

		if (res < 0)
		{
			if (!hasHit)
			{
				PushScore(0);
				GiveKR(0.6f);
			}
			goto TakeDamage;
		}
		else if (res <= 2)
			OkayCollision();
		else if (res <= 6)
			NiceCollision();
		if (score == 3 || ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) == 0 || !MarkScore)
			return;

		if (!hasHit)
			PushScore(0);
		goto TakeDamage;
	TakeDamage:
		LoseHP(Heart);
		hasHit = true;
	}
}
/// <summary>
/// A (Sudden Changes) bullet
/// </summary>
public class GunBullet : Entity
{
	/// <summary>
	/// Creates a (Sudden Changes) bullet
	/// </summary>
	/// <param name="targetCentre">The position of the target</param>
	/// <param name="delayTime">The time delay of the bullet to fire</param>
	/// <param name="rotation">The angle of the bullet with respect to the target</param>
	public GunBullet(Vector2 targetCentre, float delayTime, float rotation) : this(targetCentre, delayTime, [rotation]) { }
	/// <summary>
	/// Creates multiple (Sudden Changes) bullets
	/// </summary>
	/// <param name="targetCentre">The position of the target</param>
	/// <param name="delayTime">The time delay of the bullets to fire</param>
	/// <param name="rotations">The angles of the bullets with respect to the target</param>
	public GunBullet(Vector2 targetCentre, float delayTime, float[] rotations)
	{
		PlaySound(Sounds.gunTargeting);
		Image = Sprites.target;
		Centre = targetCentre;
		this.delayTime = delayTime;
		Depth = 0.41f;
		this.rotations = rotations;
	}

	private readonly float[] rotations;
	private const float distance = 190;
	private float currentDistance;

	private int appearTime = 0;
	private readonly float delayTime;
	//TK: What is this.
	private float lerp = 0f;
	private float alpha = 0f;

	/// <inheritdoc/>
	public override void Draw()
	{
		if (appearTime <= delayTime)
		{
			foreach (float item in rotations)
				FormalDraw(Image, Centre + GetVector2(currentDistance, item), Color.White * alpha, 0.55f, 0, ImageCentre);
			FormalDraw(Image, Centre, Color.White * 0.8f, 0.62f, 0, ImageCentre);
		}
		else
			FormalDraw(Image, Centre, Color.White * alpha, 0.62f * lerp, 0, ImageCentre);
	}

	/// <inheritdoc/>
	public override void Update()
	{
		switch (++appearTime)
		{
			case int x when x <= delayTime:
				lerp = AlphaLerp(appearTime / delayTime);
				currentDistance = (1 - lerp) * distance;
				alpha = lerp * 0.7f + 0.1f;
				break;
			case int x when x == (int)delayTime:
				PlaySound(Sounds.gunShot, 0.8f);
				for (int i = 0; i < rotations.Length; i++)
					GameStates.InstanceCreate(new Bullet(Centre, rotations[i]));
				break;
			default:
				if (alpha < 0)
					Dispose();
				lerp += 0.06f;
				alpha -= 0.05f;
				break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float AlphaLerp(float x) => x / (2 - x);
}