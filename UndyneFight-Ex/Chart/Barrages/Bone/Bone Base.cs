using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

/// <summary>
/// The base class for a bone, you should not call this
/// </summary>
public class Bone : LineCollisionBarrage
{
	private protected FightBox controllingBox;
	/// <inheritdoc/>
	public FightBox ControllingBox => controllingBox;

	private protected bool autoDispose = true;
	private bool hasBeenInside = false;
	private static CollideRect screen = new(-50, -50, 740, 580);
	/// <summary>
	/// The length of the bone
	/// </summary>
	public float Length { get; set; }
	/// <summary>
	/// Whether the bone is masked inside of the box
	/// </summary>
	public bool IsMasked { get; set; } = true;

	private protected Color drawingColor;

	private int colorType = 0;
	/// <summary>
	/// The color of the bone, 0-> White, 1-> Blue, 2-> Orange
	/// </summary>
	public new float ColorType
	{
		set
		{
			base.ColorType = (int)value;
			switch (value)
			{
				case 0:
					drawingColor = Color.White;
					colorType = 0;
					break;
				case 1:
					drawingColor = new Color(110, 203, 255, 255);
					colorType = 1;
					break;
				case 2:
					drawingColor = Color.Orange;
					colorType = 2;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, "The value can only be 0, 1 or 2");
			}
		}
		get => colorType;
	}
	/// <summary>
	/// Sets the drawing color of the bone
	/// </summary>
	/// <param name="color"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ResetColor(Color color) => drawingColor = color;
	/// <summary>
	/// Whether the depth will be automatically sorted by their color type (Will override the original depth)
	/// </summary>
	public bool AutoDepth = true;
	/// <summary>
	/// The sprite of the bone's head
	/// </summary>
	public Texture2D boneHead = Sprites.boneHead;
	/// <summary>
	/// The sprite of the bone's tail
	/// </summary>
	public Texture2D boneTail = Sprites.boneHead;
	/// <summary>
	/// The width of the bone
	/// </summary>
	public float boneWidth = 6;
	/// <inheritdoc/>
	public override void Draw()
	{
		if (Length < 0)
			return;
		if (AutoDepth)
			Depth = 0.5f - colorType * 0.02f;
		Vector2 delta = GetVector2(Length / 2f, Rotation + 90);
		col Col = Color.Lerp(Color.Transparent, drawingColor, Alpha);
		GeneralDraw(Sprites.pixUnit, Centre, Col, new Vector2(Length - 6, boneWidth), GetRadian(Rotation + 90));
		GeneralDraw(boneHead, Centre + delta, Col, Vector2.One, GetRadian(Rotation + 180));
		GeneralDraw(boneTail, Centre - delta, Col, Vector2.One, GetRadian(Rotation));
		if (DebugState.ShowIntendedHitbox)
			DrawingLab.DrawLine(Points.Start, Points.End, Thickness, Color.Red, Depth + 0.1f);
	}
	/// <inheritdoc/>
	public override void Update()
	{
		controlLayer = IsMasked ? Surface.Hidden : Surface.Normal;
		Points.Start = Centre + GetVector2(Length / 2, Rotation + 90);
		Points.End = Centre - GetVector2(Length / 2, Rotation + 90);
		if (autoDispose)
		{
			bool ins = this is CustomBone Cbone ? Cbone.screenC.Contain(Centre) : screen.Contain(Centre);
			if (ins && !hasBeenInside)
				hasBeenInside = true;
			if (hasBeenInside && !ins)
				Dispose();
		}
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void GetCollide(Player.Heart Heart)
	{
		if (Alpha <= 0.9f)
			return;
		base.GetCollide(Heart);
	}
	/// <inheritdoc/>
	public Bone() : base(3.5f)
	{
		drawingColor = GameMain.CurrentDrawingSettings.themeColor;
		UpdateIn120 = true;
		if ((controllingBox = FightBox.instance as RectangleBox) == null)
			throw new NotImplementedException();
	}
}