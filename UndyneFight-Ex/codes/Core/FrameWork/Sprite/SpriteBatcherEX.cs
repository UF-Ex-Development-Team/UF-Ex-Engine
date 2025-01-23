using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
    public partial class SpriteBatchEX
    {
        private class SpriteBatcherEX(GraphicsDevice graphicsDevice)
        {
            private readonly GraphicsDevice _graphicsDevice = graphicsDevice;

            private readonly List<SpriteBatchItem> _items = new(256);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Insert(SpriteBatchItem item) => _items.Add(item);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void DrawBatch(SpriteSortMode sortMode, Effect effect)
            {
                ObjectDisposedException.ThrowIf(effect != null && effect.IsDisposed, "effect");

                SpriteBatchItem[] _current;
                _current = [.. _items];
                if (_current.Length == 0)
                {
                    return;
                }
                if ((uint)(sortMode - 2) <= 2u)
                {
                    Array.Sort(_current);
                }
                if (GameMain.RegisterTextures[0] == null)
                    for (int i = 0; i < _current.Length; i++)
                    {
                        if (effect == null)
                            DrawItem(_current[i]);
                        else
                            DrawItem(effect, _current[i]);
                    }
                else
                {
                    for (int i = 0; i < _current.Length; i++)
                        DrawItemSampler(effect, _current[i]);
                }
                _items.Clear();
            }

            readonly VertexPositionColorTexture[] buffer = new VertexPositionColorTexture[3];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DrawItem(SpriteBatchItem item)
            {
                _graphicsDevice.Textures[0] = item.Texture;
                _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, item.Vertices, 0, item.Vertices.Length, item.Indices, 0, item.PrimitiveCount, VertexPositionColorTexture.VertexDeclaration);

            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DrawItem(Effect effect, SpriteBatchItem item)
            {
                for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
                {
                    effect.CurrentTechnique.Passes[i].Apply();
                    _graphicsDevice.Textures[0] = item.Texture;
                    _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, item.Vertices, 0, item.Vertices.Length, item.Indices, 0, item.PrimitiveCount, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DrawItemSampler(Effect effect, SpriteBatchItem item)
            {
                for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
                {
                    effect.CurrentTechnique.Passes[i].Apply();
                    _graphicsDevice.Textures[0] = item.Texture;
                    for (int k = 0; k < 3; k++)
                    {
                        if (GameMain.RegisterTextures[k] == null)
                            break;
                        _graphicsDevice.Textures[k + 1] = GameMain.RegisterTextures[k];
                    }

                    _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, item.Vertices, 0, item.Vertices.Length, item.Indices, 0, item.PrimitiveCount, VertexPositionColorTexture.VertexDeclaration);
                }
            }
        }
    }
}