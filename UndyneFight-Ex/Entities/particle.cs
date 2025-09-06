using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// Gathers particles
	/// </summary>
	public class ParticleGather : Entity
	{
		private int appearTime = 0;
		/// <summary>
		/// Creates particles that gather to the center
		/// </summary>
		/// <param name="centre">The center position for the particles to gather to</param>
		/// <param name="count">The amount of particles to create</param>
		/// <param name="duration">The duration of the gathering</param>
		/// <param name="color">The color of the particles</param>
		/// <param name="speed_range">The range of the speed of the particles (Default [2, 5]x of the image)</param>
		/// <param name="size_range">The range of the sizes of the particles (Default [0.4, 0.9]x of the image)</param>
		public ParticleGather(Vector2 centre, int count, float duration, Color color, float[] speed_range = null, float[] size_range = null)
		{
			speed_range ??= [2, 5];
			size_range ??= [0.4f, 0.9f];
			Image ??= FightResources.Sprites.lightBall;
			Centre = centre;
			this.count = count;
			this.duration = duration;

			rotations = new float[count];
			sizes = new float[count];
			speeds = new float[count];
			for (int i = 0; i < count; i++)
			{
				rotations[i] = Rand(0, 359f);
				speeds[i] = Rand(speed_range[0], speed_range[1]);
				sizes[i] = Rand(size_range[0], size_range[1]);
			}

			drawingColor = color;
		}

		private Color drawingColor;
		private readonly float[] rotations, speeds, sizes;
		private readonly int count;
		private readonly float duration;
		private float timeLeft;

		public override void Update()
		{
			if ((timeLeft = duration - ++appearTime) < 0)
				Dispose();
		}

		public override void Draw()
		{
			for (int i = 0; i < count; i++)
			{
				FormalDraw(Image, Centre + GetVector2(speeds[i] * timeLeft, rotations[i]), drawingColor * MathHelper.Min(0.7f, appearTime / (duration / 1.3f)), sizes[i], 0, ImageCentre);
			}
		}
	}
	/// <summary>
	/// A particle entity
	/// </summary>
	public class Particle : Entity
	{
		/// <summary>
		/// Creates a particle
		/// </summary>
		/// <param name="color">The color of the particle</param>
		/// <param name="speed">The speed of the particle</param>
		/// <param name="size">The size of the particle (In Pixels) (Multiply by 20)</param>
		/// <param name="centre">The position to create the particle</param>
		/// <param name="image">The image of the particle (Default <see cref="FightResources.Sprites.lightBall"/>)</param>
		public Particle(Color color, Vector2 speed, float size, Vector2 centre, Texture2D image = null)
		{
			Image = image ?? FightResources.Sprites.lightBall;
			this.size = size / 20;
			Centre = centre;
			this.color = color;
			this.speed = speed;
			Depth = 0.45f;
		}

		/// <summary>
		/// The fading speed of the particle, default 3f (3f/255f)
		/// </summary>
		public float DarkingSpeed { private get; set; } = 3;
		/// <summary>
		/// The alpha of the particle
		/// </summary>
		public float Alpha { private get; set; } = 1;
		/// <summary>
		/// The rotation speed of the particle
		/// </summary>
		public float RotateSpeed { private get; set; } = 0.0f;

		private readonly float size;

		private bool autoRotate = false;
		/// <summary>
		/// Whether the particle automatically rotates
		/// </summary>
		public bool AutoRotate
		{
			get => autoRotate;
			set
			{
				autoRotate = value;
				RotateSpeed = value ? Rand(-42, 42) / 20f : 0f;
			}
		}
		/// <summary>
		/// The friction of the particle's motion
		/// </summary>
		public float SlowLerp { private get; set; }

		private Color color;
		private Vector2 speed;

		public override void Draw() => FormalDraw(Image, Centre, color * Alpha, size, Rotation, ImageCentre);

		public override void Update()
		{
			if (Alpha >= DarkingSpeed / 255f)
				Alpha -= DarkingSpeed / 255f;
			else
				Dispose();
			Centre += speed *= 1 - SlowLerp;
			if (autoRotate)
				Rotation += RotateSpeed / 180f * MathHelper.Pi;
		}
	}
}