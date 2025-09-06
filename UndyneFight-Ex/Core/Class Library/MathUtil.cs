using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using static System.Math;
using static System.MathF;

namespace UndyneFight_Ex
{
	public static class MathUtil
	{
		internal static Random rander = new();
		public const float PI = 3.141592f;
		#region Collision
		/// <summary>
		/// Checks if two lines intersect each other
		/// </summary>
		/// <param name="lineA">The source line</param>
		/// <param name="lineB">The destination line</param>
		/// <returns>Whether two lines intersect</returns>
		static bool LineIntersect((Vector2 StartPoint, Vector2 EndPoint) lineA, (Vector2 StartPoint, Vector2 EndPoint) lineB)
		{
			Vector2 A = lineA.EndPoint - lineA.StartPoint, B = lineB.EndPoint - lineB.StartPoint;
			float determinant = A.Cross(B);

			return determinant != 0 &&
				InRange((B.X * (lineA.StartPoint.Y - lineB.StartPoint.Y) - B.Y * (lineA.StartPoint.X - lineB.StartPoint.X)) / determinant, 0, 1) &&
				InRange((A.X * (lineA.StartPoint.Y - lineB.StartPoint.Y) - A.Y * (lineA.StartPoint.X - lineB.StartPoint.X)) / determinant, 0, 1);
		}
		/// <summary>
		/// Checks whether the point is inside of a triangle
		/// </summary>
		/// <param name="v1">First vertex of the triangle</param>
		/// <param name="v2">Second vertex of the triangle</param>
		/// <param name="v3">Third vertex of the triangle</param>
		/// <param name="target">The point to check</param>
		/// <returns>Whether the point is inside the triangle</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 target)
		{
			Vector2 l1 = v2 - v1, l2 = v3 - v2, l3 = v1 - v3;
			Vector2 d1 = target - v1, d2 = target - v2, d3 = target - v3;
			float re1 = l1.Cross(d1);
			return !((re1 > 0) ^ (l2.Cross(d2) > 0)) && !((re1 > 0) ^ l3.Cross(d3) > 0);
		}
		/// <summary>
		/// Checks if a rectangle is colliding with a triangle
		/// </summary>
		/// <param name="sourceRectTL">The coordinate of the top left corner of the source rectangle</param>
		/// <param name="sourceRectBR">The coordinate of the bottom right corner of the source rectangle</param>
		/// <param name="destTriV1">The coordinate of the first vertex of the triangle</param>
		/// <param name="destTriV2">The coordinate of the second vertex of the triangle</param>
		/// <param name="destTriV3">The coordinate of the third vertex of the triangle</param>
		/// <returns>0 -> Does not intersect or collide; 1 -> Rectangle is inside of Triangle; 2 -> Rectangle intersects the Triangle</returns>
		public static int RectangleInTriangle(Vector2 sourceRectTL, Vector2 sourceRectBR, Vector2 destTriV1, Vector2 destTriV2, Vector2 destTriV3)
		{
			(Vector2 Start, Vector2 End)[] RectLines = [(sourceRectTL, new(sourceRectTL.X, sourceRectBR.Y)), (new(sourceRectTL.X, sourceRectBR.Y), sourceRectBR), (sourceRectBR, new(sourceRectBR.X, sourceRectTL.Y)), (new(sourceRectBR.X, sourceRectTL.Y), sourceRectTL)];
			(Vector2 Start, Vector2 End)[] TriLines = [(destTriV1, destTriV2), (destTriV2, destTriV3), (destTriV3, destTriV1)];
			for (int i = 0; i < 4; i++)
				for(int ii = 0; ii < 3; ii++)
				{
					if (LineIntersect(RectLines[i], TriLines[ii]))
						return 2;
				}
			return InTriangle(destTriV1, destTriV2, destTriV3, sourceRectTL) && InTriangle(destTriV1, destTriV2, destTriV3, sourceRectBR) && InTriangle(destTriV1, destTriV2, destTriV3, new Vector2(sourceRectTL.X, sourceRectBR.Y)) && InTriangle(destTriV1, destTriV2, destTriV3, new Vector2(sourceRectBR.X, sourceRectTL.Y)) ? 1 : 0;
		}
		/// <summary>
		/// Checks if two rectangles collide or intersect
		/// </summary>
		/// <param name="sourceTL">The coordinate of the top left corner of the source rectangle</param>
		/// <param name="sourceBR">The coordinate of the bottom right corner of the source rectangle</param>
		/// <param name="destTL">The coordinate of the top left corner of the destination rectangle</param>
		/// <param name="destBR">The coordinate of the bottom right corner of the destination rectangle</param>
		/// <returns>0 -> Does not intersect or collide; 1 -> Source rectangle is inside Destination rectangle; 2 -> Source rectangle intersects with Destination rectangle</returns>
		public static int RectangleInRectangle(Vector2 sourceTL, Vector2 sourceBR, Vector2 destTL, Vector2 destBR)
		{
			RectangleF source = new(new(sourceTL.X, sourceTL.Y), new(sourceBR.X, sourceBR.Y)),
						dest = new(new(destTL.X, destTL.Y), new(destBR.X, destBR.Y));
			return dest.Contains(source) ? 1 : dest.IntersectsWith(source) ? 2 : 0;
		}
		/// <summary>
		/// Checks whether two polygons are colliding
		/// </summary>
		/// <param name="polygonA">The list of vertices of the first polygon (In clockwise order)</param>
		/// <param name="polygonB">The list of vertices of the second polygon (In clockwise order)</param>
		/// <returns></returns>
		public static bool PolygonCollide(Vector2[] polygonA, Vector2[] polygonB)
		{
			//Convert polygon to list of lines
			static List<(Vector2, Vector2)> ConvertToLine(Vector2[] polygon)
			{
				List<(Vector2, Vector2)> list = [];
				for (int i = 0; i < polygon.Length; i++)
					list.Add(new(polygon[i], polygon[(i + 1) % polygon.Length]));
				return list;
			}
			List<(Vector2 StartPoint, Vector2 EndPoint)> LinesA = ConvertToLine(polygonA), LinesB = ConvertToLine(polygonB);
			static bool PointOverlap(Vector2 Point, List<(Vector2, Vector2)> Polygon)
			{
				int PointIntersectCount = 0;
				(Vector2, Vector2) test = new(Point, new Vector2(Point.X + 10000, Point.Y));
				foreach ((Vector2, Vector2) L in Polygon)
					if (LineIntersect(test, L))
						PointIntersectCount++;
				return PointIntersectCount % 2 != 0;
			}

			//Check if any two pairs of lines intersect
			foreach ((Vector2, Vector2) lineA in LinesA)
				foreach ((Vector2, Vector2) lineB in LinesB)
					if (LineIntersect(lineA, lineB))
						return true;
			//Check if two points collide with respect to polygonA
			foreach ((Vector2 StartPoint, _) in LinesA)
				if (PointOverlap(StartPoint, LinesB))
					return true;
			//Check if two points collide with respect to polygonB
			foreach ((Vector2 StartPoint, _) in LinesB)
				if (PointOverlap(StartPoint, LinesA))
					return true;
			return false;
		}
		#endregion
		/// <summary>
		/// Converts a float to a string, regardless of decimal separator
		/// </summary>
		/// <param name="val">The value to convert to string</param>
		/// <param name="digits">The rounding digit of the string</param>
		/// <returns>The string of the float</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string FloatToString(float val, int digits = 0)
		{
			string padMeth = "0.";
			for (int i = 0; i < digits; i++)
				padMeth += "0";
			return Round(val, digits).ToString(padMeth).Replace(',', '.');
		}
		/// <summary>
		/// Converts a float from a string, regardless of decimal separator
		/// </summary>
		/// <param name="str">The string to convert to a float</param>
		/// <returns>The float from string</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FloatFromString(string str)
		{
			//Uses , as separator
			if (str.Contains(','))
				str = str.Trim(',');
			string[] strs = str.Split('.');
			string BeforeDecimal = strs.First();
			string AfterDecimal = strs.Last();
			//Has multiple '.'
			if (strs.Length > 2)
				for (int i = 1; i < strs.Length - 2; i++)
					BeforeDecimal += strs[i];
			float fin = (float)Convert.ToDouble(BeforeDecimal) + (float)Convert.ToDouble(AfterDecimal) / Pow(10, AfterDecimal.Length);
			return str.Contains('.') ? fin : (float)Convert.ToDouble(BeforeDecimal);
		}
		/// <summary>
		/// Gets the distance of a point to a line
		/// </summary>
		/// <param name="Point">The point to evaluate</param>
		/// <param name="Start">The first end of the line</param>
		/// <param name="End">The second end of the line</param>
		/// <returns>The distance of the point to line</returns>
		public static float DistanceToLine(Vector2 Point, Vector2 Start, Vector2 End) => MathF.Abs((End.Y - Start.Y) * Point.X - (End.X - Start.X) * Point.Y + End.X * Start.Y - End.Y * Start.X) / (End - Start).Length();
		/// <summary>
		/// Returns the minimal angle difference between two angles
		/// </summary>
		/// <param name="rot1">The first angle</param>
		/// <param name="rot2">The second angle</param>
		/// <returns>The angle difference, range is [0, 180]</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float RotationDist(float rot1, float rot2) => MathF.Min((rot1 + 360 - rot2) % 360, (rot2 + 360 - rot1) % 360);
		/// <summary>
		/// Projects a vector onto the given vector
		/// </summary>
		/// <param name="origin">The vector to project</param>
		/// <param name="vec">The vector to project to</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Project(Vector2 origin, Vector2 vec) => Vector2.Dot(origin, vec) / origin.Length();
		/// <summary>
		/// Rotates the vector by the given angle in degrees
		/// </summary>
		/// <param name="origin">The original vector</param>
		/// <param name="rot">The amount of degrees to rotate</param>
		/// <returns>The rotated vector</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Rotate(Vector2 origin, float rot) => GetVector2(origin.Length(), origin.Direction() + rot);
		/// <summary>
		/// Rotates the vector by the given angle in radians
		/// </summary>
		/// <param name="origin">The original vector</param>
		/// <param name="rad">The amount of degrees to rotate in radians</param>
		/// <returns>The rotated vector</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 RotateRadian(Vector2 origin, float rad)
		{
			float angle = Atan2(origin.Y, origin.X) + rad;
			return new Vector2(MathF.Cos(angle), Sin(angle)) * origin.Length();
		}
		/// <summary>
		/// Returns the minimal angle difference between two angles
		/// </summary>
		/// <param name="rot1">The first angle</param>
		/// <param name="rot2">The second angle</param>
		/// <returns>The angle difference, [0, 180]</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MinRotate(float rot1, float rot2)
		{
			if (MathF.Abs(rot1 - rot2) <= 0.001f)
				return 0.0f;
			float a1 = (rot2 + 360 - rot1) % 360;
			return a1 <= 180 ? a1 : -((rot1 + 360 - rot2) % 360);
		}
		/// <summary>
		/// Returns the value raised to the specific amount of power, maintaining it's original sign
		/// </summary>
		/// <param name="val">The value to raise</param>
		/// <param name="pow">The power to raise to</param>
		/// <returns>The number raised to the power <paramref name="pow"/> maintaining the sign of <paramref name="val"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SignedPow(float val, float pow) => val >= 0 ? Pow(val, pow) : -Pow(-val, pow);

		/// <summary>
		/// Get the angle in degrees between the two vectors
		/// </summary>
		/// <param name="start">The starting vector</param>
		/// <param name="end">The ending vector</param>
		/// <returns>The angle between the two vectors</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Direction(Vector2 start, Vector2 end) => Atan2((end - start).Y, (end - start).X) / MathF.PI * 180;
		/// <summary>
		/// Gets the direction of the vector with respect to the origin
		/// </summary>
		/// <param name="vec">The vector to check</param>
		/// <returns>The angle of the vector</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Direction(this Vector2 vec) => Atan2(vec.Y, vec.X) / MathF.PI * 180;
		/// <summary>
		/// Adjusted <see cref="Tanh(float)"/> value, used for smooth transition. range and domain are both [0, 1]
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sigmoid01(float val) => (Tanh(val * 6 - 3) / 0.99505f + 1) / 2f;
		/// <summary>
		/// The determinant/cross product of two 2D vectors
		/// </summary>
		/// <param name="vec">The first vector</param>
		/// <param name="vec2">The second vector</param>
		/// <returns>The determinant/cross product</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cross(this Vector2 vec, Vector2 vec2) => vec.X * vec2.Y - vec.Y * vec2.X;
		/// <summary>
		/// The cross product of two 3D vectors
		/// </summary>
		/// <param name="vec">The first vector</param>
		/// <param name="vec2">The second vector</param>
		/// <returns>The cross product</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Cross(this Vector3 vec, Vector3 vec2) => new(vec.Y * vec2.Z - vec.Z * vec2.Y, vec.Z * vec2.X - vec.X * vec2.Z, vec.X * vec2.Y - vec.Y * vec2.X);
		/// <summary>
		/// Clamps the value between the two specified values
		/// </summary>
		/// <param name="min">The minimum value</param>
		/// <param name="val">The value to set</param>
		/// <param name="max">The maximum value</param>
		/// <returns>The clamped value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(int min, int val, int max) => val > max ? max : (val < min ? min : val);
		/// <summary>
		/// Clamps the value between the two specified values
		/// </summary>
		/// <param name="min">The minimum value</param>
		/// <param name="val">The value to set</param>
		/// <param name="max">The maximum value</param>
		/// <returns>The clamped value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(float min, float val, float max) => val > max ? max : (val < min ? min : val);
		/// <summary>
		/// Gets the radian equivalent of the angle in degrees
		/// </summary>
		/// <param name="angle">The angle to convert to radians</param>
		/// <returns>The angle in radians</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetRadian(float angle) => angle / 180 * MathF.PI;
		/// <summary>
		/// Gets the degree equivalent of the angle in radians
		/// </summary>
		/// <param name="angle">The angle to convert to degrees</param>
		/// <returns>The angle in degrees</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetAngle(float angle) => angle / MathF.PI * 180;
		/// <summary>
		/// Calculates the <see cref="Vector2"/> using the given <paramref name="length"/> and <paramref name="angle"/>.
		/// </summary>
		/// <param name="length">The specified length of the vector</param>
		/// <param name="angle">The specified angle (in degrees)</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 GetVector2(float length, float angle) => new Vector2(MathF.Cos(GetRadian(angle)), Sin(GetRadian(angle))) * length;
		/// <summary>
		/// Gets the distance between two vectors
		/// </summary>
		/// <param name="P1">The first vector</param>
		/// <param name="P2">The second vector</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetDistance(Vector2 P1, Vector2 P2) => Vector2.Distance(P1, P2);
		/// <summary>
		/// Gets a random value at [<paramref name="x"/>, <paramref name="y"/>]
		/// </summary>
		/// <param name="x">The minimum integer</param>
		/// <param name="y">The maximum integer</param>
		/// <returns>The random integer</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetRandom(int x, int y) => rander.Next(x, y + 1);
		/// <summary>
		/// Gets a random value at [<paramref name="x"/>, <paramref name="y"/>]
		/// </summary>
		/// <param name="x">The minimum value</param>
		/// <param name="y">The maximum value</param>
		/// <returns>The random value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetRandom(float x, float y) => float.Lerp(x, y, (float)rander.NextDouble());
		/// <summary>
		/// Returns the square root of the specified number
		/// </summary>
		/// <param name="v">The value to get the square root of</param>
		/// <returns>The square root of <paramref name="v"/></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(float v)
		{
			float l = 0.001f, r = v, mid = -1;
			while (r - l > 1e-5)
			{
				mid = (l + r) / 2;
				if (mid * mid > v)
					r = mid;
				else
					l = mid;
			}
			return mid;
		}
		/// <summary>
		/// Gets a random value from the specified values
		/// </summary>
		/// <typeparam name="T">Can be any type</typeparam>
		/// <param name="inputs">The values for getting</param>
		/// <returns>The random value from the specified values</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RandIn<T>(params T[] inputs) => inputs[GetRandom(0, inputs.Length - 1)];
		/// <summary>
		/// The cosine value from vector calculation, see <see href="https://en.wikipedia.org/wiki/Cosine_similarity"/> for more information
		/// </summary>
		/// <param name="a">The first vector</param>
		/// <param name="b">The second vector</param>
		/// <returns>The cosine value of the angle between the two vectors</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static float Cos(Vector2 a, Vector2 b) => Vector2.Dot(a, b) / (a.Length() * b.Length());
		/// <summary>
		/// Hashes the given string
		/// </summary>
		/// <param name="s">The string to hash</param>
		/// <returns>The hashed value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint StringHash(string s)
		{
			ulong partA = GetHashCode(s);
			ulong hashResult1 = QuickPow(97, partA);
			uint hashResult2 = Convert.ToUInt32(hashResult1 % (1u << 31));
			return hashResult2;
		}
		/// <summary>
		/// Gets the hash code of the string
		/// </summary>
		/// <param name="s">The string to hash</param>
		/// <returns>The hash code</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong GetHashCode(string s)
		{
			ulong res = 0;
			for (int i = 0; i < s.Length; i++)
				res = res * 131 + Convert.ToUInt64((int)s[i]);
			return res;
		}
		/// <summary>
		/// Raises <paramref name="a"/> by the power of <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The value to raise</param>
		/// <param name="b">The power to raise to</param>
		/// <returns>The value raised to the given power</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong QuickPow(ulong a, ulong b)
		{
			ulong ret = 1, pow = a;

			while (b != 0)
			{
				if ((b & 1) == 1)
					ret *= pow;
				pow *= pow;
				b /= 2;
			}
			return ret;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong QuickPow(ulong a, ulong b, ulong mod)
		{
			ulong ret = 1, pow = a;

			while (b != 0)
			{
				if ((b & 1) == 1)
					ret = pow * ret % mod;
				pow = pow * pow % mod;
				b /= 2;
			}
			return ret;
		}
		/// <summary>
		/// Value wrap-around of <paramref name="a"/> between [0, <paramref name="b"/>]
		/// </summary>
		/// <param name="a">The value to wrap around</param>
		/// <param name="b">The max value that can be attained by <paramref name="a"/></param>
		/// <returns>The wrapped value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Posmod(int a, int b)
		{
			int value = a % b;
			while ((value < 0 && b > 0) || (value > 0 && b < 0))
				value += b;
			return value;
		}
		/// <summary>
		/// Value wrap-around of <paramref name="a"/> between [0, <paramref name="b"/>]
		/// </summary>
		/// <param name="a">The value to wrap around</param>
		/// <param name="b">The max value that can be attained by <paramref name="a"/></param>
		/// <returns>The wrapped value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Posmod(float a, float b)
		{
			float value = a % b;
			while ((value < 0 && b > 0) || (value > 0 && b < 0))
				value += b;
			return value;
		}
		/// <summary>
		/// RSA encryption for a string
		/// </summary>
		/// <param name="password">The string to encrypt</param>
		/// <param name="rsaKeyPublic">The key of encryption</param>
		/// <returns>The encrypted string</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Encrypt(string password, string rsaKeyPublic)
		{
			RSACryptoServiceProvider rsa = new();
			rsa.FromXmlString(rsaKeyPublic);
			return Convert.ToBase64String(rsa.Encrypt(Encoding.ASCII.GetBytes(password), false));
		}
		/// <summary>
		/// RSA decryption of a string
		/// </summary>
		/// <param name="base64Origin">The string to decrypt</param>
		/// <param name="rsaKeyPrivate">The key of encryption</param>
		/// <returns>The decrypted string</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Decrypt(string base64Origin, string rsaKeyPrivate)
		{
			RSACryptoServiceProvider rsa = new();
			rsa.FromXmlString(rsaKeyPrivate);

			byte[] buffer = Convert.FromBase64String(base64Origin);
			byte[] DecryptBuffer = rsa.Decrypt(buffer, false); // 进行解密
			string str = Encoding.UTF8.GetString(DecryptBuffer); // 将byte[]转换为明文

			return str;
		}
		/// <summary>
		/// Check whether the value is inside of a range
		/// </summary>
		/// <typeparam name="T">Any type that is a <see cref="IComparable"/></typeparam>
		/// <param name="value">The value to check</param>
		/// <param name="min">The minimum range</param>
		/// <param name="max">The maximum range</param>
		/// <returns>Whether value is [<paramref name="min"/>,<paramref name="max"/>]</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InRange<T>(this T value, T min, T max) where T : IComparable => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
		/// <summary>
		/// Gets the sum of the arithmetic sequence
		/// </summary>
		/// <param name="start">The first term of the sequence</param>
		/// <param name="diff">The difference between each term in the sequence</param>
		/// <param name="items">The number of items in the sequence</param>
		/// <returns>The sum of the sequence</returns>
		public static float ArithmeticSum(float start, float diff, float items) => (start + start + diff * (items - 1)) * items / 2;
		/// <summary>
		/// Gets the sum of the geometric sequence
		/// </summary>
		/// <param name="start">The first term of the sequence</param>
		/// <param name="ratio">The ratio between each term in the sequence</param>
		/// <param name="items">The number of items in the sequence</param>
		/// <returns>The sum of the sequence</returns>
		public static float GeometricSum(float start, float ratio, float items) => start * (1 - Pow(ratio, items)) / (1 - ratio);
		/// <summary>
		/// Gets the amount of substring inside of a string
		/// </summary>
		/// <param name="text"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int CountSubstring(this string text, string substring)
		{
			int count = 0, minIndex = text.IndexOf(substring, 0);
			while (minIndex != -1)
			{
				minIndex = text.IndexOf(substring, minIndex + substring.Length);
				count++;
			}
			return count;
		}
	}
}