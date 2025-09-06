using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.FightResources.Sprites;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// A platform
	/// </summary>
	public class Platform : Entity, ICustomMotion, ICustomLength
	{
		protected bool autoDispose = true;
		private bool hasBeenInside = false;
		private static CollideRect screen = new(-550, -550, 1740, 1580);

		private float length;
		private Vector2 startPos;
		/// <inheritdoc/>
		public Func<ICustomMotion, Vector2> PositionRoute { get; set; }
		/// <inheritdoc/>
		public Func<ICustomLength, float> LengthRoute { get; set; }
		/// <inheritdoc/>
		public Func<ICustomMotion, float> RotationRoute { get; set; }
		/// <inheritdoc/>
		public Vector2 CentrePosition => delta;
		/// <inheritdoc/>
		public float[] PositionRouteParam { get; set; }
		/// <inheritdoc/>
		public float[] LengthRouteParam { get; set; }
		/// <inheritdoc/>
		public float[] RotationRouteParam { get; set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetStartPosition(Vector2 vector2) => startPos = vector2;
		/// <summary>
		/// The time elapsed after the platform was created
		/// </summary>
		public float AppearTime => appearTime;
		protected int platformType;
		protected float appearTime = 0;
		/// <summary>
		/// Whether the platform is masked inside the box
		/// </summary>
		public bool isMasked = true;
		/// <summary>
		/// Whether the platform will have a scaling animation when created
		/// </summary>
		public bool createWithScaling = false;
		/// <summary>
		/// The default scale of the platform
		/// </summary>
		public float scale = 0.0f;
		private Vector2 delta;
		/// <summary>
		/// Creates a platform with fixed size
		/// </summary>
		/// <param name="platformType">The type of platform, 0-> Green, 1-> Purple</param>
		/// <param name="startPos">The initial position of the platform</param>
		/// <param name="positionRoute">The position route of the platform (Delta positioning, therefore the position of the platform will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
		/// <param name="rotation">The rotation of the platform</param>
		/// <param name="length">The length of the platform</param>
		public Platform(int platformType, Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, float rotation, float length) : this(platformType, startPos, positionRoute, Motions.LengthRoute.stableValue, Motions.RotationRoute.stableValue)
		{
			RotationRouteParam = [rotation];
			LengthRouteParam = [length];
		}
		/// <summary>
		/// Creates a platform with fixed size that lasts for a given duration before folding itself
		/// </summary>
		/// <param name="platformType">The type of platform, 0-> Green, 1-> Purple</param>
		/// <param name="startPos">The initial position of the platform</param>
		/// <param name="positionRoute">The position route of the platform (Delta positioning, therefore the position of the platform will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
		/// <param name="rotation">The angle of the platform</param>
		/// <param name="length">The length of the platform</param>
		/// <param name="duration">The duration of the platform</param>
		public Platform(int platformType, Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, float rotation, float length, float duration) : this(platformType, startPos, positionRoute, Motions.LengthRoute.autoFold, Motions.RotationRoute.stableValue)
		{
			RotationRouteParam = [rotation];
			LengthRouteParam = [length, duration];
		}
		/// <summary>
		/// Creates a platform
		/// </summary>
		/// <param name="platformType">The type of platform, 0-> Green, 1-> Purple</param>
		/// <param name="startPos">The initial position of the platform</param>
		/// <param name="positionRoute">The position route of the platform (Delta positioning, therefore the position of the platform will be the sum of <paramref name="startPos"/> and <paramref name="positionRoute"/></param>
		/// <param name="lengthRoute">The easing of the size of the platform</param>
		/// <param name="rotationRoute">The easing of the rotation of the platform</param>
		public Platform(int platformType, Vector2 startPos, Func<ICustomMotion, Vector2> positionRoute, Func<ICustomLength, float> lengthRoute, Func<ICustomMotion, float> rotationRoute)
		{
			gravityLine = new GravityLine(Vector2.Zero, Vector2.Zero);
			this.platformType = platformType;
			Centre = startPos;
			this.startPos = startPos;
			PositionRoute = positionRoute;
			LengthRoute = lengthRoute;
			RotationRoute = rotationRoute;
			Image = platform[platformType];
			gravityLine.SetWidth(4);
			gravityLine.sticky = platformType == 0;
			UpdateIn120 = true;
		}

		public override void Draw()
		{
			Vector2 delta = GetVector2(length / 2, Rotation);
			Texture2D side = platformSide[platformType];
			GameMain.MissionSpriteBatch.Draw(Image, Centre, new Rectangle(0, 0, (int)length, 12), Color.White, GetRadian(Rotation), new Vector2(length / 2, 6), 1.0f, SpriteEffects.None, 0.35f);
			Depth = 0.35F;
			FormalDraw(side, Centre - delta, Color.White, GetRadian(Rotation), new Vector2(0, 6));
			FormalDraw(side, Centre + delta, Color.White, GetRadian(Rotation), new Vector2(0, 6));
		}

		private readonly GravityLine gravityLine;

		public override void Update()
		{
			controlLayer = isMasked ? Surface.Hidden : Surface.Normal;
			if (autoDispose)
			{
				if (length < 0)
					Dispose();
				bool ins = screen.Contain(Centre);
				if (ins && (!hasBeenInside))
					hasBeenInside = true;
				if (hasBeenInside && (!ins))
					Dispose();
			}
			scale = createWithScaling ? scale * 0.85f + 0.15f : 1.0f;
			appearTime += 0.5f;
			Vector2 v = PositionRoute(this);
			this.delta = v;
			Centre = startPos + v;
			Rotation = RotationRoute(this);
			length = LengthRoute(this) * scale;

			Vector2 delta = GetVector2(length / 2, Rotation);

			gravityLine.SetPosition(Centre + delta, Centre - delta);
			gravityLine.SetLength(length);
		}
		public override void Dispose()
		{
			gravityLine?.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// 获取一组有关此平台的数据。数据值分别为: length, platformType
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Vector2 GetData() => new(length, platformType);
		/// <summary>
		/// Changes the type of the platform (Purple becomes Green and Green becomes Purple)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ChangeType()
		{
			platformType = 1 - platformType;
			Image = platform[platformType];
			gravityLine.sticky ^= true;
		}
		/// <summary>
		/// Resets the <see cref="AppearTime"/> to 0
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetTime() => appearTime = 0;
	}
}