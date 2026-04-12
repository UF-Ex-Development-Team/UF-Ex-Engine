using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.GameMain;
using Color = Microsoft.Xna.Framework.Color;

namespace UndyneFight_Ex;

/// <summary>
/// Graphics Library Font class
/// </summary>
public class GLFont
{
	/// <summary>
	/// The sprite font data
	/// </summary>
	public SpriteFont SFX;
	private readonly Dictionary<char, Vector2> _storedGlyphSizes = [];
	private readonly Dictionary<char, int> _charIndex = [];
	/// <summary>
	/// Creates a new GLFont
	/// </summary>
	/// <param name="path">Path to font</param>
	/// <param name="cm">Loader to load the font</param>
	public GLFont(string path, ContentManager cm)
	{
		SFX = DrawingLab.LoadContent<SpriteFont>(path, cm);
		for (int i = 0; i < SFX.Glyphs.Length; i++)
			_charIndex[SFX.Glyphs[i].Character] = i;
	}
	/// <summary>
	/// Draws text
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the top left corner of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="sb">The <see cref="SpriteBatchEX"/> used to render the text (Default default renderer)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Draw(string texts, Vector2 location, Color color, SpriteBatchEX sb = null) => (sb ?? MissionSpriteBatch).DrawString(this, texts, location, color * Surface.Normal.drawingAlpha);
	/// <summary>
	/// Draws text
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the top left corner of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Draw(string texts, Vector2 location, Color color, float scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
	/// <summary>
	/// Draws text
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the top left corner of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Draw(string texts, Vector2 location, Color color, Vector2 scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
	/// <summary>
	/// Draws text
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the top left corner of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="rotation">The rotation of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="anchor">The anchor of rotation</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Draw(string texts, Vector2 location, Color color, float rotation, float scale, Vector2 anchor, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, rotation, anchor, scale, SpriteEffects.None, depth);
	/// <summary>
	/// Draws text
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the top left corner of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="rotation">The rotation of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Draw(string texts, Vector2 location, Color color, float rotation, float scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
	/// <summary>
	/// Draws text
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the top left corner of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="rotation">The rotation of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Draw(string texts, Vector2 location, Color color, float rotation, vec2 scale, float depth) => MissionSpriteBatch.DrawString(this, texts, location, color * Surface.Normal.drawingAlpha, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
	/// <summary>
	/// Draws text that is aligned to the center
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the center of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="sb">The <see cref="SpriteBatchEX"/> used to render the text (Default default renderer)</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CentreDraw(string texts, Vector2 location, Color color, SpriteBatchEX sb = null)
	{
		sb ??= MissionSpriteBatch;
		string[] lines = texts.Split('\n');
		vec2 Size = SFX.MeasureString(texts);
		float initY = -Size.Y / 2;
		for (int i = 0; i < lines.Length; i++)
			sb.DrawString(this, lines[i], location + new vec2(-SFX.MeasureString(lines[i]).X / 2, initY + i * SFX.MeasureString(lines[i]).Y), color * Surface.Normal.drawingAlpha);
	}
	/// <summary>
	/// Draws text that is aligned to the center
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the center of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CentreDraw(string texts, Vector2 location, Color color, float scale, float depth) => CentreDraw(texts, location, color, new vec2(scale), depth);
	/// <summary>
	/// Draws text that is aligned to the center
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the center of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CentreDraw(string texts, Vector2 location, Color color, vec2 scale, float depth)
	{
		string[] lines = texts.Split('\n');
		vec2 Size = SFX.MeasureString(texts);
		float initY = -Size.Y / 2 * scale.Y;
		for (int i = 0; i < lines.Length; i++)
			MissionSpriteBatch.DrawString(this, lines[i], location + new vec2(0, initY + (i + 0.8f) * SFX.MeasureString(lines[i]).Y * scale.Y), color * Surface.Normal.drawingAlpha, 0, SFX.MeasureString(lines[i]) / 2, scale, SpriteEffects.None, depth);
	}
	/// <summary>
	/// Draws text that is aligned to the center
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the center of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="rotation">The rotation of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CentreDraw(string texts, Vector2 location, Color color, float scale, float rotation, float depth) => CentreDraw(texts, location, color, new vec2(scale), rotation, depth);
	/// <summary>
	/// Draws text that is aligned to the center
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The location of the center of the text</param>
	/// <param name="color">The color of the text</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="rotation">The rotation of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CentreDraw(string texts, Vector2 location, Color color, vec2 scale, float rotation, float depth)
	{
		string[] lines = texts.Split('\n');
		vec2 Size = SFX.MeasureString(texts);
		float initY = -Size.Y / 2 * scale.Y;
		for (int i = 0; i < lines.Length; i++)
			MissionSpriteBatch.DrawString(this, lines[i], location + new vec2(0, initY + (i + 0.5f) * SFX.MeasureString(lines[i]).Y * scale.Y), color * Surface.Normal.drawingAlpha, rotation, SFX.MeasureString(lines[i]) / 2, scale, SpriteEffects.None, depth);
	}
	/// <summary>
	/// Draws a piece of text that will break to a new line when the given limit is reached
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The top left corner to draw the text with</param>
	/// <param name="color">The color of the text</param>
	/// <param name="lineLength">The maximum width of a line in pixels</param>
	/// <param name="lineDistance">The vertical distance between lines</param>
	/// <param name="scale">The scale of the text</param>
	/// <param name="depth">The depth of the text</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void LimitDraw(string texts, Vector2 location, Color color, float lineLength, float lineDistance, float scale, float depth)
	{
		Vector2[] sizes = new Vector2[texts.Length];
		for (int i = 0; i < texts.Length; i++)
			sizes[i] = SFX.MeasureString(texts[i].ToString());

		string curLine = string.Empty;
		float cur = 0;
		List<string> strings = [];
		int accuSpace = 0;
		for (int i = 0; i < texts.Length; i++)
		{
			float v;
			bool u;
			cur += v = sizes[i].X * scale;
			if ((u = texts[i] is '\r' or '\n') || cur > lineLength)
			{
				if (cur > lineLength)
				{
					int lastSpace = curLine.LastIndexOf(' ');
					if (lastSpace != -1)
					{
						accuSpace += lastSpace;
						curLine = curLine[..lastSpace];
						i = accuSpace + 1;
						v = sizes[i].X * scale;
					}
				}
				strings.Add(curLine);
				curLine = string.Empty;
				cur = v;
				if (u)
					continue;
			}
			curLine += texts[i];
		}
		strings.Add(curLine);
		foreach (string s in strings)
		{
			string finalText = s;
			if (s.StartsWith(' '))
				finalText = s[1..];
			MissionSpriteBatch.DrawString(this, finalText, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
			location.Y += lineDistance;
		}
	}
	/// <summary>
	/// Draws a piece of text that will break to a new line when the given limit is reached
	/// </summary>
	/// <param name="texts">The text to draw</param>
	/// <param name="location">The top left corner to draw the text with</param>
	/// <param name="color">The color of the text</param>
	/// <param name="size">The area to restrict the text in</param>
	/// <param name="lineDistance">The vertical distance between lines</param>
	/// <param name="scale">The scale of the text (Note that the size may shrink because <paramref name="size"/> is too small</param>
	/// <param name="depth">The depth of the text</param>
	/// <param name="by_word">Whether the line break will consider the spaces</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void LimitDraw(string texts, Vector2 location, Color color, Vector2 size, float lineDistance, float scale, float depth, bool by_word = true)
	{
		Vector2[] sizes = new Vector2[texts.Length];
		for (int i = 0; i < texts.Length; i++)
			sizes[i] = SFX.MeasureString(texts[i].ToString());

		string curLine = string.Empty;
		float cur = 0;
		int accuSpace = 0;
		List<string> strings = [];
		for (int i = 0; i < texts.Length; i++)
		{
			float v;
			bool u;
			cur += v = sizes[i].X * scale;
			if ((u = texts[i] is '\r' or '\n') || cur > size.X)
			{
				if (cur > size.X)
				{
					int lastSpace = curLine.LastIndexOf(' ');
					if (lastSpace != -1)
					{
						accuSpace += lastSpace;
						curLine = curLine[..lastSpace];
						i = accuSpace + 1;
						v = sizes[i].X * scale;
					}
				}
				strings.Add(curLine);
				curLine = string.Empty;
				cur = v;
				if (u)
					continue;
			}
			curLine += texts[i];
		}
		strings.Add(curLine);
		float originalLineDist = lineDistance;
		while (lineDistance * (strings.Count + 1) * scale > size.Y)
		{
			scale -= 0.1f;
			lineDistance = originalLineDist * scale;
		}
		foreach (string s in strings)
		{
			string finalText = s;
			if (s.StartsWith(' ') && by_word)
				finalText = s[1..];
			MissionSpriteBatch.DrawString(this, finalText, location, color * Surface.Normal.drawingAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
			location.Y += lineDistance;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe int GetGlyphIndexOrDefault(char c) => _charIndex[c];
	/// <summary>
	/// Check whether a character exists in the font
	/// </summary>
	/// <param name="c">The character to check</param>
	/// <returns>Whether the character exists</returns>
	public bool CharExists(char c) => _charIndex.ContainsKey(c);
	/// <summary>
	/// Gets the size of a specified <see cref="char"/>
	/// </summary>
	/// <param name="ch">The character to measure the size of</param>
	/// <returns>The size of the given character</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2 MeasureChar(char ch)
	{
		if (_storedGlyphSizes.TryGetValue(ch, out Vector2 size))
			return size;
		else
		{
			_ = _storedGlyphSizes.TryAdd(ch, size);
			return SFX.Glyphs[GetGlyphIndexOrDefault(ch)].Cropping.Size.ToVector2();
		}
	}
}