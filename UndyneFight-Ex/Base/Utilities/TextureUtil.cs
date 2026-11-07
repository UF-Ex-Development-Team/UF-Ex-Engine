using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex;
/// <summary>
/// Utilities for textures
/// </summary>
public static class TextureUtil
{
#if NET10_0_OR_GREATER
	extension(Texture2D tex)
	{
		public Vector2 Size() => new(tex.Width, tex.Height);
	}
#else
	public static Vector2 Size(this Texture2D tex) => new(tex.Width, tex.Height);
#endif
}