using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

/// <summary>
/// A boneslab (The bones on the side of the box)
/// </summary>
public class Boneslab : Barrage, ICustomLength
{
	private const int boneslabOuttime = 8;
	private const float quarterAngle = MathF.PI / 2;

	internal static Texture2D BoneSlabTexture => Sprites.boneSlab;
	internal static Texture2D WarningLine => Sprites.warningLine;
	private int appearTime, colorType = 0;
	private Vector2 renderPlace, _warningLine;
	private float currentHeight, missionHeight;
	private readonly int appearDelay, totalTime;
	private readonly float trueRotation;
	private Color drawingColor = GameMain.CurrentDrawingSettings.themeColor;
	/// <summary>
	/// The <see cref="Action"/> to execute when the boneslab is created (When the warning ends)
	/// </summary>
	public Action BoneProtruded { get; set; }
	/// <summary>
	/// The color of the boneslab, 0-> White, 1-> Blue (Aqua), 2-> Orange
	/// </summary>
	public new int ColorType
	{
		set
		{
			if (value is < 0 or > 2)
				throw new ArgumentOutOfRangeException(nameof(value), "ColorType must be between 0 and 2.");
			colorType = value;
			drawingColor = value switch
			{
				0 => Color.White,
				1 => new Color(110, 203, 255, 255),
				2 => Color.Orange,
				_ => drawingColor
			};
		}
	}
	/// <inheritdoc/>
	public new float AppearTime => appearTime - appearDelay;

	private FightBox controllingBox = FightBox.instance;
	/// <summary>
	/// The box assigned to the boneslab
	/// </summary>
	public FightBox ControllingBox { set => controllingBox = value; }
	/// <inheritdoc/>
	public Func<ICustomLength, float> LengthRoute { get; set; }
	/// <inheritdoc/>
	public float[] LengthRouteParam { get; set; }

	/// <summary>
	/// Creates a boneslab
	/// </summary>
	/// <param name="rotation">The rotation of the wall (Must be a multiple of 90)</param>
	/// <param name="appearDelay">The duration of the warning before spawning</param>
	/// <param name="totalTime">The duration of the boneslab</param>
	/// <param name="lengthRoute">The route of the height of the boneslab</param>
	/// <param name="lengthRouteParam">The parameters of the route</param>
	public Boneslab(float rotation, int appearDelay, int totalTime, Func<ICustomLength, float> lengthRoute, float[] lengthRouteParam)
	{
		LengthRoute = lengthRoute;
		LengthRouteParam = lengthRouteParam;
		rotation %= 360;
		trueRotation = rotation;
		Rotation = rotation;
		this.totalTime = totalTime;
		this.appearDelay = appearDelay;
	}
	/// <summary>
	/// Creates a boneslab
	/// </summary>
	/// <param name="rotation">The rotation of the wall (Must be a multiple of 90)</param>
	/// <param name="height">The height of the boneslab</param>
	/// <param name="appearDelay">The duration of the warning before spawning</param>
	/// <param name="totalTime">The duration of the boneslab</param>
	public Boneslab(float rotation, float height, float appearDelay, float totalTime)
	{
		controlLayer = Surface.Hidden;
		rotation %= 360;
		trueRotation = rotation;
		Rotation = rotation;
		missionHeight = height;
		this.totalTime = (int)totalTime;
		this.appearDelay = (int)appearDelay;
	}

	/// <inheritdoc/>
	public override void Draw()
	{
		RectangleBox controlRectBox = controllingBox as RectangleBox;
		if (trueRotation is 90 or 270)
			GameMain.MissionSpriteBatch.Draw(BoneSlabTexture, renderPlace, new System.Drawing.RectangleF(0, 320 - currentHeight, controlRectBox.Height, currentHeight),
				drawingColor, GetRadian(Rotation) + MathF.PI, new Vector2(controlRectBox.Height / 2, 0), 1.0f, SpriteEffects.None, 0.499f);
		if (trueRotation is 0 or 180)
			GameMain.MissionSpriteBatch.Draw(BoneSlabTexture, renderPlace, new System.Drawing.RectangleF(0, 320 - currentHeight, controlRectBox.Width, currentHeight),
				drawingColor, GetRadian(Rotation) + MathF.PI, new Vector2(controlRectBox.Width / 2, 0), 1.0f, SpriteEffects.None, 0.499f);

		if (appearTime >= appearDelay)
			return;

		if (trueRotation is 90 or 270)
			GameMain.MissionSpriteBatch.Draw(WarningLine, _warningLine,
			new Rectangle(0, 0, (int)controlRectBox.Height, 2),
			appearTime % 6 < 3 ? Color.Red : Color.Yellow,
			GetRadian(Rotation) + MathF.PI, new Vector2(controlRectBox.Height / 2, 0),
			1.0f, SpriteEffects.None, 0.3f);
		else
			GameMain.MissionSpriteBatch.Draw(WarningLine, _warningLine,
			new Rectangle(0, 0, (int)controlRectBox.Width, 2),
			appearTime % 6 < 3 ? Color.Red : Color.Yellow,
			GetRadian(Rotation) + MathF.PI, new Vector2(controlRectBox.Width / 2, 0),
			1.0f, SpriteEffects.None, 0.3f);
	}

	/// <inheritdoc/>
	public override void Update()
	{
		FightBox box = controllingBox;
		if (++appearTime >= appearDelay)
		{
			if (appearTime == appearDelay + 1)
				BoneProtruded?.Invoke();
			if (LengthRoute != null && LengthRouteParam != null)
			{
				if (appearTime <= appearDelay + boneslabOuttime * 2)
				{
					float d = (appearTime - appearDelay * 1.0f) / (boneslabOuttime * 2);
					float e = d * d * 0.85f + 0.15f;
					missionHeight = LengthRoute(this);
					currentHeight = missionHeight * e + currentHeight * (1 - e);
				}
				else if (appearTime <= appearDelay + totalTime)
					currentHeight = LengthRoute(this);
				else
					currentHeight -= ((appearTime - appearDelay - totalTime) / 1.2f + 0.5f) * MathF.Sqrt(missionHeight) / boneslabOuttime;
				goto DisposingCheck;
			}
			if (appearTime <= appearDelay + boneslabOuttime)
			{
				currentHeight += missionHeight / 20f;
				currentHeight = Math.Min(missionHeight * 0.22f + currentHeight * 0.78f, missionHeight);
			}
			else if (appearTime >= appearDelay + totalTime)
				currentHeight -= ((appearTime - appearDelay - totalTime) / 1.2f + 0.5f) * MathF.Sqrt(missionHeight) / boneslabOuttime;
			else
				currentHeight = missionHeight;

			DisposingCheck:
			if (currentHeight < -4)
				Dispose();
		}
		float angle = quarterAngle + GetRadian(Rotation);
		renderPlace.X = MathF.Cos(angle) * box.CollidingBox.Width / 2 + box.Centre.X;
		renderPlace.Y = MathF.Sin(angle) * box.CollidingBox.Height / 2 + box.Centre.Y;
		_warningLine.X = -MathF.Cos(angle) * missionHeight + renderPlace.X;
		_warningLine.Y = -MathF.Sin(angle) * missionHeight + renderPlace.Y;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void GetCollide(Player.Heart heart)
	{
		RectangleBox box = controllingBox as RectangleBox;
		//Early exit if not colliding, height insufficient, or color conditions
		if (!box.CollidingBox.Contain(heart.Centre) || currentHeight <= 1 || (colorType == 1 && heart.IsStable) || (colorType == 2 && heart.IsMoved) || appearTime <= appearDelay)
			return;

		float res = trueRotation switch
		{
			0 => box.Down - currentHeight - heart.Centre.Y,
			90 => -(box.Left + currentHeight - heart.Centre.X),
			180 => -(box.Up + currentHeight - heart.Centre.Y),
			270 => box.Right - currentHeight - heart.Centre.X,
			_ => 0x3f3f3f3f
		};

		if (res < 0.7f)
			MissCollision(heart);
		else if (res <= 2.1f)
			OkayCollision();
		else if (res <= 4.5f)
			NiceCollision();

		if (score != 3 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
			MissCollision(heart);
	}
}