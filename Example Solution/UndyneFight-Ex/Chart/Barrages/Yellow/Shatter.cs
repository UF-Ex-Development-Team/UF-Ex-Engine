namespace UndyneFight_Ex.Entities;
/// <summary>
/// A shattered box
/// </summary>
public class Shattered : Entity
{
	/// <summary>
	/// Creates a shattered box
	/// </summary>
	/// <param name="origin">The origin entity</param>
	public Shattered(Entity origin)
	{
		AngleMode = origin.AngleMode;
		Image = origin.Image;
		Centre = origin.Centre;
		Rotation = origin.Rotation;
		Scale = origin.Scale;
		UpdateIn120 = true;
		rect = [
			new CollideRect(ImageCentre, ImageCentre),
			new CollideRect(new vec2(0, ImageCentre.Y), ImageCentre),
			new CollideRect(vec2.Zero, ImageCentre),
			new CollideRect(new vec2(ImageCentre.X, 0), ImageCentre)
		];
		distance = ImageCentre / 2f;
	}

	/// <inheritdoc/>
	public override void Draw()
	{
		for (int i = 0; i < 4; i++)
			FormalDraw(Image, pos[i], rect[i], col.White * alpha);
	}
	private vec2 distance = vec2.Zero;
	private float alpha = 1.0f;
	private readonly CollideRect[] pos = new CollideRect[4], rect = new CollideRect[4];

	/// <inheritdoc/>
	public override void Update()
	{
		if ((alpha -= 0.035f) < 0.0f)
			Dispose();
		distance += ImageCentre * Scale * 0.035f;
		for (int i = 0; i < 4; i++)
		{
			pos[i].Size = ImageCentre;
			pos[i].SetCentre(Centre + MathUtil.GetVector2(MathF.Sqrt(2), i * 90 + 45) * distance);
		}

	}
}