namespace UndyneFight_Ex;

/// <summary>
/// Functions that utilize Vectors that the base framework did not provide
/// </summary>
public static class VectorsUtil
{
#if NET10_0_OR_GREATER
	/// <summary>
	/// Vector2 extension methods
	/// </summary>
	/// <param name="vector">The Vector2 to utilize</param>
	extension(vec2 vector)
	{
		/// <summary>
		/// Gets the distance of the given point to a line
		/// </summary>
		/// <param name="Point">The point to evaluate</param>
		/// <param name="Start">The first end of the line</param>
		/// <param name="End">The second end of the line</param>
		/// <returns>The distance of the point to line</returns>
		public float DistanceToLine(vec2 Start, vec2 End)
		{
			vec2 Delta = End - Start;
			return MathF.Abs(vector.Cross(Delta) + End.Cross(Start)) / Delta.Length();
		}
		/// <summary>
		/// Returns the scalar projection of this vector onto the given vector
		/// </summary>
		/// <param name="vec">The vector to project to</param>
		/// <returns>The scalar projection of a vector</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ScalarProject(vec2 vec) => vec2.Dot(vector, vec) / vector.Length();
		/// <summary>
		/// Projects this vector onto the given vector
		/// </summary>
		/// <param name="vec">The vector to project to</param>
		/// <returns>The projected vector</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public vec2 Project(vec2 vec) => vec2.Dot(vector, vec) / vector.LengthSquared() * vec;
		/// <summary>
		/// Rotates the vector by the given angle in degrees
		/// </summary>
		/// <param name="rot">The amount of degrees to rotate</param>
		/// <returns>The rotated vector</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public vec2 RotateDegrees(float rot) => MathUtil.GetVector2(vector.Length(), vector.Direction() + rot);
		/// <summary>
		/// Gets the direction of the vector with respect to the origin in degrees
		/// </summary>
		/// <returns>The angle of the vector in degrees</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Direction() => MathF.Atan2(vector.Y, vector.X) / MathF.PI * 180;
		/// <summary>
		/// The determinant/cross product of two 2D vectors
		/// </summary>
		/// <param name="vec2">The second vector</param>
		/// <returns>The determinant/cross product</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Cross(vec2 vec2) => vector.X * vec2.Y - vector.Y * vec2.X;
	}
	/// <summary>
	/// Vector3 extension methods
	/// </summary>
	/// <param name="vector">The Vector3 to utilize</param>
	extension(Vector3 vector)
	{
		/// <summary>
		/// The cross product of two 3D vectors
		/// </summary>
		/// <param name="vec2">The second vector</param>
		/// <returns>The cross product</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 Cross(Vector3 vec2) => new(vector.Y * vec2.Z - vector.Z * vec2.Y, vector.Z * vec2.X - vector.X * vec2.Z, vector.X * vec2.Y - vector.Y * vec2.X);
		/// <summary>
		/// Converts a <see cref="Vector3"/> to a <see cref="col"/>
		/// </summary>
		/// <returns>The converted color</returns>
		public col ToColor(float alpha = 1) => new col(vector) * alpha;
	}
	/// <summary>
	/// Vector4 extension methods
	/// </summary>
	/// <param name="vector">The Vector4 to utilize</param>
	extension(Vector4 vector)
	{
		/// <summary>
		/// Converts a <see cref="Vector4"/> to a <see cref="col"/>
		/// </summary>
		/// <returns>The converted color</returns>
		public col ToColor() => new(vector);
	}
#else
	/// <summary>
	/// Gets the distance of the given point to a line
	/// </summary>
	/// <param name="Point">The point to evaluate</param>
	/// <param name="Start">The first end of the line</param>
	/// <param name="End">The second end of the line</param>
	/// <returns>The distance of the point to line</returns>
	public static float DistanceToLine(this vec2 vector, vec2 Start, vec2 End)
	{
		vec2 Delta = End - Start;
		return MathF.Abs(vector.Cross(Delta) + End.Cross(Start)) / Delta.Length();
	}
	/// <summary>
	/// Returns the scalar projection of this vector onto the given vector
	/// </summary>
	/// <param name="vec">The vector to project to</param>
	/// <returns>The scalar projection of a vector</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float ScalarProject(this vec2 vector, vec2 vec) => vec2.Dot(vector, vec) / vector.Length();
	/// <summary>
	/// Projects this vector onto the given vector
	/// </summary>
	/// <param name="vec">The vector to project to</param>
	/// <returns>The projected vector</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static vec2 Project(this vec2 vector, vec2 vec) => vec2.Dot(vector, vec) / vector.LengthSquared() * vec;
	/// <summary>
	/// Rotates the vector by the given angle in degrees
	/// </summary>
	/// <param name="rot">The amount of degrees to rotate</param>
	/// <returns>The rotated vector</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static vec2 RotateDegrees(this vec2 vector, float rot) => MathUtil.GetVector2(vector.Length(), vector.Direction() + rot);
	/// <summary>
	/// Gets the direction of the vector with respect to the origin in degrees
	/// </summary>
	/// <returns>The angle of the vector in degrees</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Direction(this vec2 vector) => MathF.Atan2(vector.Y, vector.X) / MathF.PI * 180;
	/// <summary>
	/// The determinant/cross product of two 2D vectors
	/// </summary>
	/// <param name="vec2">The second vector</param>
	/// <returns>The determinant/cross product</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Cross(this vec2 vector, vec2 vec2) => vector.X * vec2.Y - vector.Y * vec2.X;
	/// <summary>
	/// The cross product of two 3D vectors
	/// </summary>
	/// <param name="vec2">The second vector</param>
	/// <returns>The cross product</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Cross(this Vector3 vector, Vector3 vec2) => new(vector.Y * vec2.Z - vector.Z * vec2.Y, vector.Z * vec2.X - vector.X * vec2.Z, vector.X * vec2.Y - vector.Y * vec2.X);
	/// <summary>
	/// Converts a <see cref="Vector3"/> to a <see cref="col"/>
	/// </summary>
	/// <returns>The converted color</returns>
	public static col ToColor(this Vector3 vector, float alpha = 1) => new col(vector) * alpha;
	/// <summary>
	/// Converts a <see cref="Vector4"/> to a <see cref="col"/>
	/// </summary>
	/// <returns>The converted color</returns>
	public static col ToColor(this Vector4 vector) => new(vector);
#endif
}