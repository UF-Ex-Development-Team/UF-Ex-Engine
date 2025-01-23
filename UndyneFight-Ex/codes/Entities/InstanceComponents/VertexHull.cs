using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities.Advanced
{
    /// <summary>
    /// 3D processing logic, used in Dream Battle and Maware Setsugekka
    /// </summary>
    /// <param name="vectors">The location of the 8 vertices in (x, y, z) with respect to the centre</param>
    /// <param name="rotateSpeed">The rotation speed of (x, y, z)</param>
    public class VertexHull(Vector3[] vectors, Vector3 rotateSpeed) : GameObject
    {
        private Vector3 rotateSpeed = rotateSpeed;
        private readonly Vector3[] positions = vectors;
        public Vector2[] Translated { get; private set; } = new Vector2[vectors.Length];
        public Vector2[] Axises { get; private set; } = [new(1, 0), new(0, -1), new(-0.5f, 1.732f / 2f)];

        public Vector3 rotation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 Reflect(Vector3 rot, Vector3 pos, Vector2 xAxis, Vector2 yAxis, Vector2 zAxis)
        {
            Matrix v = Matrix.CreateRotationX(GetRadian(rot.X)) * Matrix.CreateRotationY(GetRadian(rot.Y)) * Matrix.CreateRotationZ(GetRadian(rot.Z));
            Vector3 s = (new Matrix(Vector4.Zero, Vector4.Zero, Vector4.Zero, new(pos, 0)) * v).Translation;
            return s.X * xAxis + s.Y * yAxis + s.Z * zAxis;
        }

        public override void Update()
        {
            rotation += rotateSpeed;
            for (int i = 0; i < Translated.Length; i++)
                Translated[i] = Reflect(rotation, positions[i], Axises[0], Axises[1], Axises[2]);
        }
    }
}