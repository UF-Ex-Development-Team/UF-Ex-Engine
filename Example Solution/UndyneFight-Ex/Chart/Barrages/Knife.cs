using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;

namespace UndyneFight_Ex.Entities;
/// <summary>
/// A knife beam
/// </summary>
public class Knife : Barrage
{
	private readonly float delay;
	/// <summary>
	/// Creates a knife beam
	/// </summary>
	/// <param name="delay">The duration of the warning</param>
	/// <param name="vecease">The easing for the position</param>
	/// <param name="rotease">The easing for the rotation</param>
	public Knife(float delay, Func<ICustomMotion, Vector2> vecease, Func<ICustomMotion, float> rotease)
	{
		Image = Sprites.knife;
		this.delay = delay;
		PositionRoute = vecease;
		RotationRoute = rotease;
		AngleMode = true;
		UpdateIn120 = true;

		PlaySound(Sounds.Warning);

		Line l = new((s) => Centre, (s) => Rotation);
		CreateEntity(l);
		DelayEventProcessor.AddInstantEvent(delay, () =>
		{
			PlaySound(Sounds.largeKnife, 0.7f);
			l.Dispose();
		});
	}
	/// <summary>
	/// Creates a knife beam
	/// </summary>
	/// <param name="delay">The duration of the warning</param>
	/// <param name="centre">The centre of the beam</param>
	/// <param name="rot">The rotation of the beam</param>
	public Knife(float delay, Vector2 centre, float rot) : this(delay, (s) => centre, (s) => rot) { }
	/// <summary>
	/// The appear time of the knife beam
	/// </summary>
	public new float AppearTime { get; private set; } = 0;
	/// <summary>
	/// The color of the beam
	/// </summary>
	public Color DrawColor { get; set; } = Color.Purple;
	/// <inheritdoc/>
	public override void Draw()
	{
		Depth = 0.99f;
		float alpha = 1 - AppearTime / delay * 2f;
		if (AppearTime > delay)
			FormalDraw(Image, Centre, DrawColor * rayAlpha, new Vector2(scale * 0.5f, 2), Rotation - 90, ImageCentre);
		else if (alpha > 0)
			FormalDraw(Sprites.KnifeWarn, Centre, DrawColor * alpha, Rotation - 90, Sprites.KnifeWarn.Bounds.Size.ToVector2() / 2);
	}
	/// <inheritdoc/>
	public override void GetCollide(Player.Heart player)
	{
		if (AppearTime < delay)
			return;
		bool needAP = ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0;
		float dist = Rotation % 90 is < 0.1f or > 89.9f
			? Centre.X - player.Centre.X //Compute directly if is parallel
			: MathUtil.ScalarProject(new(float.Tan(MathUtil.GetRadian(Rotation)), -1), player.Centre - Centre);

		float res = Math.Abs(dist) - 2 - 8.5f * scale;

		int offset = 3 - (int)JudgeState;

		if (res < 0)
			goto TakeDamage;
		else if (res <= 1.6f - offset * 0.4f)
			OkayCollision();
		else if (res <= 4.2f - offset * 1.2f)
			NiceCollision();
		if (score == 3 || !needAP || !MarkScore)
			return;
		goto TakeDamage;

	TakeDamage:
		if (!hasHit)
			PushScore(0);
		LoseHP(player);
		hasHit = true;
	}
	private float scale = 0;
	private float rayAlpha = 1;
	/// <inheritdoc/>
	public override void Update()
	{
		AppearTime += 0.5f;
		Centre = PositionRoute(this);
		Rotation = RotationRoute(this);
		if (AppearTime > delay)
		{
			scale = scale * 0.9f + 1 * 0.1f;
			rayAlpha -= 0.015f;
		}
		if (rayAlpha < 0)
			Dispose();
	}
	/// <inheritdoc/>
	public override void Dispose()
	{
		if (!hasHit && MarkScore)
			PushScore(score);
		base.Dispose();
	}
}