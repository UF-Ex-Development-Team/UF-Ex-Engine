using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Fight;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Entities.Player;

namespace UndyneFight_Ex.Entities;

/// <summary>
/// The interface for a player collidable instance
/// </summary>
public interface ICollideAble
{
	/// <summary>
	/// The function to check collision with the player
	/// </summary>
	/// <param name="player">The player to check</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	void GetCollide(Heart player);
}
/// <summary>
/// A barrage with a line collision
/// </summary>
/// <param name="thickness">The thickness of the collision</param>
public abstract class LineCollisionBarrage(float thickness = 1) : Barrage
{
	/// <summary>
	/// The thickness of the line
	/// </summary>
	public float Thickness = thickness;
	/// <summary>
	/// The points of the line
	/// </summary>
	public (Vector2 Start, Vector2 End) Points;
	/// <summary>
	/// The distance threshold for okay and nice
	/// </summary>
	public (float Okay, float Nice) DistanceThreshold = (1.6f, 3.9f);
	private float GetDistance(Heart heart) => MathUtil.DistanceToLine(heart.Centre, Points.Start, Points.End);
	/// <inheritdoc/>
	public override void GetCollide(Heart player)
	{
		if ((ColorType == 1 && player.IsStable) || (ColorType == 2 && player.IsMoved) || (Functions.PlayerInstance.hpControl.ScoreProtected && Functions.PlayerInstance.hpControl.protectTime > 0))
			return;

		float res = Math.Max(Math.Abs(GetDistance(player)) - 8 + Thickness, MathUtil.GetDistance(player.Centre, Centre) - (Points.End - Points.Start).Length() / 2) - Thickness;
		//Debug.WriteLine(res);
		int offset = 3 - (int)JudgeState;
		if (res < 0)
			MissCollision(player);
		else if (res <= DistanceThreshold.Okay - offset * 0.4f)
			OkayCollision();
		else if (res <= DistanceThreshold.Nice - offset * 1.2f)
			NiceCollision();
		bool needAP = (CurrentFightingScene.Mode & GameMode.PerfectOnly) != 0;
		if (score != 3 && needAP && MarkScore)
			MissCollision(player);
	}
}
/// <summary>
/// A barrage with circle collision
/// </summary>
public abstract class CircleCollisionBarrage(float Radius) : Barrage
{
	/// <summary>
	/// The distance threshold for okay and nice
	/// </summary>
	public (float Okay, float Nice) DistanceThreshold = (1.6f, 4.2f);
	private float GetDistance(Heart heart) => Vector2.Distance(heart.Centre, Centre);
	/// <inheritdoc/>
	public override void GetCollide(Heart player)
	{
		if ((ColorType == 1 && player.IsStable) ||
			(ColorType == 2 && player.IsMoved) ||
			Functions.PlayerInstance.hpControl.protectTime > 0)
			return;

		float res = GetDistance(player) - Radius * Scale - 6;
		int offset = 3 - (int)JudgeState;
		if (res < 0)
			MissCollision(player);
		else if (res <= DistanceThreshold.Okay - offset * 0.4f)
			OkayCollision();
		else if (res <= DistanceThreshold.Nice - offset * 1.2f)
			NiceCollision();
		bool needAP = (CurrentFightingScene.Mode & GameMode.PerfectOnly) != 0;
		if (score != 3 && needAP && MarkScore)
			MissCollision(player);
	}
}
/// <summary>
/// A barrage with pixel perfect collision
/// </summary>
public abstract class PerfectCollisionBarrage() : Barrage
{
	//I love Linear Algebra :D
	/// <summary>
	/// The minimum alpha of the barrage for the collision to occur
	/// </summary>
	public float AlphaThreshold = 0.5f;
	/// <summary>
	/// The color bits for precise collision
	/// </summary>
	private Color[] ColorBits = [];
	/// <summary>
	/// The collision mask of the barrage
	/// </summary>
	public Texture2D Mask;
	/// <summary>
	/// The bounding box of the barrage
	/// </summary>
	private Rectangle BoundingBox => GetBoundingBox(BaseRect, Transform);
	/// <summary>
	/// The base rectangle of the mask
	/// </summary>
	private Rectangle BaseRect => new(0, 0, (Mask ??= Image).Width, Mask.Height);
	/// <summary>
	/// Local matrix based on position and rotation
	/// </summary>
	protected Matrix Transform => Matrix.CreateTranslation(new(-ImageCentre, 0)) * Matrix.CreateScale(Scale) * Matrix.CreateRotationZ(Rotation / 180 * float.Pi) * Matrix.CreateTranslation(new(Centre, 0));
	/// <summary>
	/// Mask data to read from to prevent read per frame
	/// </summary>
	private static readonly Dictionary<Texture2D, Color[]> MaskData = [];
	/// <summary>
	/// Updates the collision mask of the barrage (Typically when changing the <see cref="Entity.Image"/> of the barrage)
	/// </summary>
	/// <param name="NewMask">The new texture for the mask (Default the current <see cref="Entity.Image"/>)</param>
	public void UpdateMask(Texture2D NewMask = null)
	{
		//Sets new collision mask
		Mask = NewMask ?? Image;
		//Sets new size of the color bit array
		ColorBits = new Color[Mask.Width * Mask.Height];
		//Copy data of new mask
		if (!MaskData.TryGetValue(Mask, out col[] value))
		{
			Mask.GetData(ColorBits);
			MaskData.Add(Mask, ColorBits);
		}
		else
			ColorBits = value;
	}
	private static Rectangle GetBoundingBox(Rectangle rect, Matrix transform)
	{
		//Coordinates of the 4 corners in local space
		Vector2 TopLeft = new(rect.Left, rect.Top),
				TopRight = new(rect.Right, rect.Top),
				BottomLeft = new(rect.Left, rect.Bottom),
				BottomRight = new(rect.Right, rect.Bottom);
		//Transform them into the rotated space
		Vector2.Transform(ref TopLeft, ref transform, out TopLeft);
		Vector2.Transform(ref TopRight, ref transform, out TopRight);
		Vector2.Transform(ref BottomLeft, ref transform, out BottomLeft);
		Vector2.Transform(ref BottomRight, ref transform, out BottomRight);
		//Find the new top left and bottom right corners
		Vector2 NewTopLeft = Vector2.Min(Vector2.Min(TopLeft, TopRight), Vector2.Min(BottomLeft, BottomRight));
		Vector2 NewBottomRight = Vector2.Max(Vector2.Max(TopLeft, TopRight), Vector2.Max(BottomLeft, BottomRight));
		//Return the resulted rectangle
		return new((int)NewTopLeft.X, (int)NewTopLeft.Y, (int)(NewBottomRight.X - NewTopLeft.X), (int)(NewBottomRight.Y - NewTopLeft.Y));
	}
	/// <summary>
	/// Whether the current mask is colliding with another mask
	/// </summary>
	/// <param name="SourceTransform">The transform matrix of the current barrage</param>
	/// <param name="DestTransform">The transform matrix of the target</param>
	/// <param name="DestBitData">The color array of the texture of the target</param>
	/// <returns>Whether the two masks are colliding</returns>
	protected bool IsColliding(Matrix SourceTransform, Matrix DestTransform, Color[] DestBitData)
	{
		//Transform the source matrix to world space then to the destination (Soul)
		Matrix Transformed = SourceTransform * Matrix.Invert(DestTransform);
		//Get the unit axes after transformation
		Vector2 UnitX = Vector2.TransformNormal(Vector2.UnitX, Transformed),
				UnitY = Vector2.TransformNormal(Vector2.UnitY, Transformed);
		//Gets the respective coordinate of first pixel to check (Top Left of source)
		Vector2 CheckPos = Vector2.Transform(Vector2.Zero, Transformed),
				//The displacement vector for the check
				Displacement = Vector2.Zero;
		//Oh look, big loop
		for (int curY = 0; curY < Mask.Height; curY++)
		{
			for (int curX = 0; curX < Mask.Width; curX++)
			{
				Vector2 roundedPos = CheckPos + Displacement;
				//If the pixel is within the destination texture (Array is zero based, therefore the end is not inclusive)
				//Since we know that the soul is a 16x16 sprite, we can simplify the calculations
				if (roundedPos == Vector2.Clamp(roundedPos, Vector2.Zero, new(15)))
				{
					//Get the colors of the overlapped pixels
					Color SourcePixel = ColorBits[curX + curY * Mask.Width],
							DestPixel = DestBitData[(int)roundedPos.X + (int)roundedPos.Y * 16];
					//If source pixel has reached the alpha threshold and the destination pixel is not transparent,
					if (SourcePixel.A > AlphaThreshold && DestPixel.A > 0)
					{
						//collision exists
						return true;
					}
				}
				Displacement += UnitX;
			}
			//Reset the displacement on the x axis and move on to the next row
			Displacement = Vector2.Zero;
			CheckPos += UnitY;
		}
		//No collision
		return false;
	}
	/// <inheritdoc/>
	public override void GetCollide(Heart player)
	{
		if ((ColorType == 1 && player.IsStable) ||
			(ColorType == 2 && player.IsMoved) ||
			Functions.PlayerInstance.hpControl.protectTime > 0 || Disposed)
			return;
		//Prevent empty color bitmask for whatever reason
		if (ColorBits.Length == 0)
			UpdateMask();
		if (BoundingBox.Intersects(player.BoundingBox) || BoundingBox.Contains(player.BoundingBox))
		{
			//Check for damage collision
			if (IsColliding(Transform, player.GetTransform(0.5f), Heart.ColorBitData))
				MissCollision(player);
			//Check for okay
			else if (IsColliding(Transform, player.GetTransform(0.75f), Heart.ColorBitData))
				OkayCollision();
			//Set to nice
			else if (IsColliding(Transform, player.GetTransform(), Heart.ColorBitData))
				NiceCollision();
		}
		if (score != 3 && ((CurrentFightingScene.Mode & GameMode.PerfectOnly) != 0) && MarkScore)
			MissCollision(player);
	}
}
/// <summary>
/// <para>A parent class for barrage making, contains commonly used variables and functions</para>
/// This class is not the parent class for <see cref="Arrow"/>
/// </summary>
public abstract class Barrage : Entity, ICollideAble, ICustomMotion
{
	private protected int score = 3;
	private protected bool hasHit = false;
	/// <summary>
	/// Whether the barrage count towards the score
	/// </summary>
	public bool MarkScore { get; set; } = true;
	/// <summary>
	/// The color type of the barrage
	/// </summary>
	public int ColorType { get; set; }
	/// <summary>
	/// <br>The colors for each green soul shield</br>
	/// <br>0-> Blue, 1 -> Red etc</br>
	/// </summary>
	public Color[] ShieldColorTypes { get; set; } = [Color.LightBlue, Color.LightCoral, new(255, 255, 0, 128), new(255, 128, 255, 1)];
	/// <summary>
	/// The colors for normal barrages
	/// </summary>
	public Color[] BarrageColorTypes { get; set; } = [Color.White, Color.LightBlue, Color.Orange];
	/// <summary>
	/// Whether the barrage will automatically dispose itself when it goes offscreen after entering the screen
	/// </summary>
	public bool AutoDispose { get; set; } = true;
	/// <summary>
	/// Screen bounds
	/// </summary>
	private static readonly CollideRect screen = new CollideRect(-80, -80, 720, 560) * (1 / Functions.ScreenDrawing.ScreenScale);
	private bool _hasBeenInside = false;
	/// <summary>
	/// Whether the barrage will only be displayed inside the box
	/// </summary>
	public bool Hidden { private protected get; set; } = false;
	/// <summary>
	/// The current <see cref="JudgementState"/> of the chart
	/// </summary>
	public static JudgementState JudgeState => CurrentFightingScene.JudgeState;
	/// <inheritdoc/>
	public Func<ICustomMotion, vec2> PositionRoute { get; set; }
	/// <inheritdoc/>
	public Func<ICustomMotion, float> RotationRoute { get; set; }
	/// <inheritdoc/>
	public float[] RotationRouteParam { get; set; }
	/// <inheritdoc/>
	public float[] PositionRouteParam { get; set; }
	/// <inheritdoc/>
	public float AppearTime { get; set; } = 0;
	/// <summary>
	/// The opacity of the barrage
	/// </summary>
	public float Alpha = 1;
	/// <inheritdoc/>
	public vec2 CentrePosition { get; }
	/// <inheritdoc/>
	public abstract void GetCollide(Heart player);
	/// <summary>
	/// Change bullet score to nice
	/// </summary>
	protected void NiceCollision()
	{
		if (score >= 3)
		{
			score = 2;
			CreateCollideEffect(Color.LightBlue, 6f);
		}
	}
	/// <summary>
	/// Change bullet score to okay
	/// </summary>
	protected void OkayCollision()
	{
		if (score >= 2)
		{
			score = 1;
			CreateCollideEffect(Color.LawnGreen, 3f);
		}
	}
	/// <summary>
	/// Change bullet score to miss and damage the player
	/// </summary>
	protected void MissCollision(Heart player)
	{
		if (!hasHit)
			AdvanceFunctions.PushScore(0);
		Functions.LoseHP(player);
		hasHit = true;
	}
	/// <inheritdoc/>
	public override void Update()
	{
		AppearTime += UpdateIn120 ? 0.5f : 1;
		Centre = PositionRoute?.Invoke(this) ?? Centre;
		Rotation = RotationRoute?.Invoke(this) ?? Rotation;
		controlLayer = Hidden ? Surface.Hidden : Surface.Normal;
		if (AutoDispose)
		{
			bool inside = screen.Contain(Centre);
			if (inside && (!_hasBeenInside))
				_hasBeenInside = true;
			if (_hasBeenInside && (!inside))
				Dispose();
		}
	}
	/// <inheritdoc/>
	public override void Draw()
	{
		if (Alpha <= 0 || Image == null)
			return;
		GeneralDraw(Image, Centre, BarrageColorTypes[ColorType] * Alpha, new(Scale), Rotation * float.Pi / 180, depth: Depth);
	}
	/// <inheritdoc/>
	public override void Dispose()
	{
		if (!hasHit && MarkScore)
			AdvanceFunctions.PushScore(score);
		base.Dispose();
	}
	/// <summary>
	/// Creates an expanding fade effect of the barrage
	/// </summary>
	public void CreateShinyEffect() => base.CreateShinyEffect().Depth = Depth + 0.001f;
}