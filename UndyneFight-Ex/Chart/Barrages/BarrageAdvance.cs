using static UndyneFight_Ex.Entities.Platform;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities.Advanced;

/// <summary>
/// Advanced barrage classes
/// </summary>
public static class BarrageExtend
{
	private class SplitBone : Bone
	{
		public const float rotateSpeed = -2f;
		private Vector2 speed;
		public SplitBone(Vector2 centre, Vector2 speed, float rotation, float length)
		{
			Rotation = rotation;
			this.speed = speed;
			Length = length;
			Alpha = 1;
			Centre = centre;
		}
		public override void Update()
		{
			Rotation += rotateSpeed;
			speed.Y += 0.02f;
			Centre += speed;
			base.Update();
		}
	}
	/// <summary>
	/// Creates a platform fade out effect
	/// </summary>
	/// <param name="pt">The platform to create the effect of</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CreateShinyEffect(this Platform pt) => GameStates.InstanceCreate(new PlatformShinyEffect(pt, pt.length, pt.platformType));
	/// <summary>
	/// Splits the bone
	/// </summary>
	/// <param name="bone">The bone to split</param>
	/// <returns>The split bones</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Bone[] Split(this Bone bone)
	{
		Vector2 c1 = bone.Centre;
		Vector2 delta = GetVector2(bone.Length / 4f, bone.Rotation + 90);
		bone.Dispose();
		return [ new SplitBone(c1 + delta, GetVector2(1, Rand(0, 359)), bone.Rotation, bone.Length / 2 - 1){ IsMasked = bone.IsMasked },
			new SplitBone(c1 - delta, GetVector2(1, Rand(0, 359)),bone.Rotation,  bone.Length / 2 - 1){ IsMasked = bone.IsMasked }];
	}
}