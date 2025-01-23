using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public partial class SpriteBatchEX
    {
        private abstract class SpriteBatchItem(Texture2D tex, float key) : IComparable<SpriteBatchItem>
        {
            public float SortKey { get; init; } = key;
            public Texture2D Texture { get; init; } = tex;

            public int[] Indices { get; protected set; }
            public VertexPositionColorTexture[] Vertices { get; protected set; }
            public int PrimitiveCount { get; protected set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(SpriteBatchItem other) => SortKey.CompareTo(other.SortKey);
        }

        private class PrimitiveItem : SpriteBatchItem
        {
            public PrimitiveItem(float key, VertexPositionColor[] vertices) : base(FightResources.Sprites.pixUnit, key)
            {
                Vertices = new VertexPositionColorTexture[vertices.Length];
                for (int i = 0; i < Vertices.Length; i++)
                    Vertices[i] = new(Vertices[i].Position, vertices[i].Color, Vector2.Zero);
                int count = vertices.Length - 2;
                Indices = new int[count * 3];

                for (int i = 0; i < count; i++)
                {
                    Indices[i * 3] = 0;
                    Indices[i * 3 + 1] = i + 1;
                    Indices[i * 3 + 2] = i + 2;
                }
                PrimitiveCount = count;
            }
            public PrimitiveItem(float key, int[] indices, VertexPositionColor[] vertices) : base(FightResources.Sprites.pixUnit, key)
            {
                Vertices = new VertexPositionColorTexture[vertices.Length];
                for (int i = 0; i < Vertices.Length; i++)
                    Vertices[i] = new(Vertices[i].Position, vertices[i].Color, Vector2.Zero);
                int count = vertices.Length - 2;
                Indices = indices;
                PrimitiveCount = count;
            }
        }
        private class VertexItem : SpriteBatchItem
        {
            public VertexItem(Texture2D tex, float key, VertexPositionColorTexture[] vertices) : base(tex, key)
            {
                Vertices = vertices;
                int count = vertices.Length - 2;
                Indices = new int[count * 3];

                for (int i = 0; i < count; i++)
                {
                    Indices[i * 3] = 0;
                    Indices[i * 3 + 1] = i + 1;
                    Indices[i * 3 + 2] = i + 2;
                }
                PrimitiveCount = count;
            }
            public VertexItem(Texture2D tex, float key, int[] indices, VertexPositionColorTexture[] vertices) : base(tex, key)
            {
                Vertices = vertices;
                int count = vertices.Length - 2;
                Indices = indices;
                PrimitiveCount = count;
            }
        }

        private class RectangleItem : SpriteBatchItem
        {
            public static readonly int[] _indices = [0, 1, 2, 1, 3, 2];
            public RectangleItem(
                VertexPositionColorTexture topLeft,
                VertexPositionColorTexture topRight,
                VertexPositionColorTexture bottomLeft,
                VertexPositionColorTexture bottomRight,
                Texture2D tex, float key) : base(tex, key)
            {
                Indices = _indices;
                Vertices = [topLeft, topRight, bottomLeft, bottomRight];
                PrimitiveCount = 2;
            }
            public RectangleItem(float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth,
                Texture2D tex, float key
                ) : base(tex, key)
            {
                VertexPositionColorTexture vertexTL, vertexTR, vertexBL, vertexBR;
                vertexTL.Position.X = x + dx * cos - dy * sin;
                vertexTL.Position.Y = y + dx * sin + dy * cos;
                vertexTL.Position.Z = depth;
                vertexTL.Color = color;
                vertexTL.TextureCoordinate.X = texCoordTL.X;
                vertexTL.TextureCoordinate.Y = texCoordTL.Y;
                vertexTR.Position.X = x + (dx + w) * cos - dy * sin;
                vertexTR.Position.Y = y + (dx + w) * sin + dy * cos;
                vertexTR.Position.Z = depth;
                vertexTR.Color = color;
                vertexTR.TextureCoordinate.X = texCoordBR.X;
                vertexTR.TextureCoordinate.Y = texCoordTL.Y;
                vertexBL.Position.X = x + dx * cos - (dy + h) * sin;
                vertexBL.Position.Y = y + dx * sin + (dy + h) * cos;
                vertexBL.Position.Z = depth;
                vertexBL.Color = color;
                vertexBL.TextureCoordinate.X = texCoordTL.X;
                vertexBL.TextureCoordinate.Y = texCoordBR.Y;
                vertexBR.Position.X = x + (dx + w) * cos - (dy + h) * sin;
                vertexBR.Position.Y = y + (dx + w) * sin + (dy + h) * cos;
                vertexBR.Position.Z = depth;
                vertexBR.Color = color;
                vertexBR.TextureCoordinate.X = texCoordBR.X;
                vertexBR.TextureCoordinate.Y = texCoordBR.Y;
                Indices = _indices;
                Vertices = [vertexTL, vertexTR, vertexBL, vertexBR];
                PrimitiveCount = 2;
            }
            public RectangleItem(float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color[] color, Vector2 texCoordTL, Vector2 texCoordBR, float depth,
                Texture2D tex, float key
                ) : base(tex, key)
            {
                VertexPositionColorTexture vertexTL, vertexTR, vertexBL, vertexBR;
                vertexTL.Position.X = x + dx * cos - dy * sin;
                vertexTL.Position.Y = y + dx * sin + dy * cos;
                vertexTL.Position.Z = depth;
                vertexTL.Color = color[0];
                vertexTL.TextureCoordinate.X = texCoordTL.X;
                vertexTL.TextureCoordinate.Y = texCoordTL.Y;
                vertexTR.Position.X = x + (dx + w) * cos - dy * sin;
                vertexTR.Position.Y = y + (dx + w) * sin + dy * cos;
                vertexTR.Position.Z = depth;
                vertexTR.Color = color[1];
                vertexTR.TextureCoordinate.X = texCoordBR.X;
                vertexTR.TextureCoordinate.Y = texCoordTL.Y;
                vertexBL.Position.X = x + dx * cos - (dy + h) * sin;
                vertexBL.Position.Y = y + dx * sin + (dy + h) * cos;
                vertexBL.Position.Z = depth;
                vertexBL.Color = color[2];
                vertexBL.TextureCoordinate.X = texCoordTL.X;
                vertexBL.TextureCoordinate.Y = texCoordBR.Y;
                vertexBR.Position.X = x + (dx + w) * cos - (dy + h) * sin;
                vertexBR.Position.Y = y + (dx + w) * sin + (dy + h) * cos;
                vertexBR.Position.Z = depth;
                vertexBR.Color = color[3];
                vertexBR.TextureCoordinate.X = texCoordBR.X;
                vertexBR.TextureCoordinate.Y = texCoordBR.Y;
                Indices = _indices;
                Vertices = [vertexTL, vertexTR, vertexBL, vertexBR];
                PrimitiveCount = 2;
            }
            public RectangleItem(float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth,
                       Texture2D tex, float key
                ) : base(tex, key)
            {
                VertexPositionColorTexture vertexTL, vertexTR, vertexBL, vertexBR;
                vertexTL.Position.X = x;
                vertexTL.Position.Y = y;
                vertexTL.Position.Z = depth;
                vertexTL.Color = color;
                vertexTL.TextureCoordinate.X = texCoordTL.X;
                vertexTL.TextureCoordinate.Y = texCoordTL.Y;
                vertexTR.Position.X = x + w;
                vertexTR.Position.Y = y;
                vertexTR.Position.Z = depth;
                vertexTR.Color = color;
                vertexTR.TextureCoordinate.X = texCoordBR.X;
                vertexTR.TextureCoordinate.Y = texCoordTL.Y;
                vertexBL.Position.X = x;
                vertexBL.Position.Y = y + h;
                vertexBL.Position.Z = depth;
                vertexBL.Color = color;
                vertexBL.TextureCoordinate.X = texCoordTL.X;
                vertexBL.TextureCoordinate.Y = texCoordBR.Y;
                vertexBR.Position.X = x + w;
                vertexBR.Position.Y = y + h;
                vertexBR.Position.Z = depth;
                vertexBR.Color = color;
                vertexBR.TextureCoordinate.X = texCoordBR.X;
                vertexBR.TextureCoordinate.Y = texCoordBR.Y;
                Indices = _indices;
                Vertices = [vertexTL, vertexTR, vertexBL, vertexBR];
                PrimitiveCount = 2;
            }
        }
    }
}