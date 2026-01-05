using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

/// <summary>
/// Base class of a spear (You should not create this)
/// </summary>
public class Spear : LineCollisionBarrage
{
	/// <summary>
	/// Whether the spear will be drawn exclusively inside the box or not
	/// </summary>
	public bool IsHidden { set => Hidden = value; private protected get => Hidden; }
	/// <summary>
	/// Forces the spear to dispose when offscreen
	/// </summary>
	private bool ForceDispose { set; get; } = false;
	/// <summary>
	/// The drawing color of the spear
	/// </summary>
	public Color DrawingColor = Color.White;
	/// <inheritdoc/>
	public Spear() : base(1.5f)
	{
		Depth = 0.5f;
		Image = FightResources.Sprites.spear;
	}
	/// <inheritdoc/>
	public override void Draw() => FormalDraw(Image, Centre, DrawingColor * Alpha, GetRadian(Rotation), ImageCentre);
	/// <inheritdoc/>
	public override void Dispose()
	{
		if (!hasHit && MarkScore)
			PushScore(score);
		base.Dispose();
	}

	private static CollideRect screen = new(-50, -50, 740, 580);
	/// <inheritdoc/>
	public override void Update()
	{
		controlLayer = IsHidden ? Surface.Hidden : Surface.Normal;
		if (AutoDispose)
		{
			bool ins = screen.Contain(Centre);
			if (ins && !ForceDispose)
				ForceDispose = true;
			if (ForceDispose && !ins)
			{
				if (this is NormalSpear NSpear)
				{
					if (NSpear.Rebound && NSpear.ReboundCount > -1)
					{
						int Normal = 0;
						//Left
						if (Centre.X <= 30)
							Normal = 270;
						//Right
						else if (Centre.X >= 610)
							Normal = 90;
						//Top
						if (Centre.Y <= 30)
							Normal = 0;
						//Down
						else if (Centre.Y >= 450)
							Normal = 180;

						Rotation = 2 * Normal - Rotation;
						NSpear.ReboundCount--;
					}
					else
						Dispose();
				}
				else
					Dispose();
			}
		}
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void GetCollide(Player.Heart heart)
	{
		if (Alpha <= 0.9f)
			return;
		Points.Start = Centre + GetVector2(29, Rotation);
		Points.End = Centre - GetVector2(29, Rotation);
		base.GetCollide(heart);
	}
}