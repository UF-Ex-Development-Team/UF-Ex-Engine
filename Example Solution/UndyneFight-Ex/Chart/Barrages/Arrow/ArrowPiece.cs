using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

public partial class Arrow
{
	/// <summary>
	/// A broken shard of an arrow
	/// </summary>
	protected class ArrowPiece : Entity
	{
		/// <inheritdoc/>
		public override void Draw() => FormalDraw(Image, Centre, Color.White * 0.5f * alp, Scale, GetRadian(Rotation), ImageCentre);

		/// <inheritdoc/>
		public override void Update()
		{
			Centre += speed *= 0.999f;
			Rotation += rotateSpeed;
			alp -= fadeSpeed;
			if (alp < 0)
				Dispose();
		}

		private Vector2 speed;
		private readonly float rotateSpeed, fadeSpeed;
		private float alp = 1f;
		/// <summary>
		/// Creates a broken shard of an arrow
		/// </summary>
		/// <param name="speed">The speed of the shard</param>
		/// <param name="pos">The position of the shard</param>
		/// <param name="rotation">The rotation of the shard</param>
		/// <param name="image">The sprite of the shard</param>
		/// <param name="scale">The size of the shard</param>
		public ArrowPiece(Vector2 speed, Vector2 pos, float rotation, Texture2D image, float scale)
		{
			Scale = scale;
			UpdateIn120 = true;
			Depth = 0.5f;
			fadeSpeed = Rand(0.04f, 0.09f);
			Rotation = rotation;
			Image = image;
			Centre = pos;
			this.speed = speed;
			rotateSpeed = Rand(2.5f, 4.5f) * RandSignal();
		}
	}
}