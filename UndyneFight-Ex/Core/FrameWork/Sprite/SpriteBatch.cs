using Microsoft.Xna.Framework.Graphics;

namespace UndyneFight_Ex
{
	public partial class SpriteBatchEX
	{
		private readonly SpriteBatcherEX _batcher;

		private SpriteSortMode _sortMode;
		private BlendState _blendState;
		private SamplerState _samplerState;
		private DepthStencilState _depthStencilState;
		private RasterizerState _rasterizerState;
		private Effect _effect;
		protected GraphicsDevice _graphicDevice;
		public GraphicsDevice GraphicsDevice => _graphicDevice;
		private readonly SpriteEffect _spriteEffect;
		private readonly EffectPass _spritePass;
		public static SamplerState NearestSample { get; set; }
		public SamplerState DefaultState { get; set; }
		private static readonly Dictionary<GLFont, float> MinFontHeight = [];
		public SpriteBatchEX(GraphicsDevice graphicsDevice)
		{
			_graphicDevice = graphicsDevice;
			_spriteEffect = GameMain.SpriteEffect;
			_spritePass = GameMain.SpritePass;
			_beginCalled = false;
			_batcher = new(graphicsDevice);

			if (NearestSample == null)
			{
				SamplerState state = new()
				{
					AddressU = TextureAddressMode.Clamp,
					AddressV = TextureAddressMode.Clamp,
					AddressW = TextureAddressMode.Clamp,
					MaxMipLevel = 0,
					MipMapLevelOfDetailBias = 0,
					MaxAnisotropy = 0,

					ComparisonFunction = CompareFunction.Never,
					Filter = TextureFilter.Point
				};
				DefaultState = state;
				NearestSample = DefaultState;
			}
		}
		private bool _beginCalled = false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckValid(Texture2D texture)
		{
			ArgumentNullException.ThrowIfNull(texture);

			if (!_beginCalled)
			{
				throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckValid(SpriteFont spriteFont, string text)
		{
			ArgumentNullException.ThrowIfNull(spriteFont);

			ArgumentNullException.ThrowIfNull(text);

			if (!_beginCalled)
			{
				throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
			}
		}

		private float _depthBuffer = 0.0000f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Begin(
			SpriteSortMode sortMode = SpriteSortMode.Deferred,
			BlendState blendState = null,
			SamplerState samplerState = null,
			DepthStencilState depthStencilState = null,
			RasterizerState rasterizerState = null,
			Effect effect = null,
			Matrix? transform = null
		)
		{
			if (_beginCalled)
			{
				throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
			}

			_sortMode = sortMode;
			_blendState = blendState ?? BlendState.AlphaBlend;
			_samplerState = samplerState ?? DefaultState;
			_depthStencilState = depthStencilState ?? DepthStencilState.None;
			_rasterizerState = rasterizerState ?? RasterizerState.CullNone;
			_effect = effect;
			_spriteEffect.TransformMatrix = transform;

			if (sortMode == SpriteSortMode.Immediate)
			{
				Setup();
			}

			_beginCalled = true;
			_depthBuffer = 0.0000f;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void End()
		{
			if (!_beginCalled)
			{
				throw new InvalidOperationException("Begin must be called before calling End.");
			}

			_beginCalled = false;
			if (_sortMode != SpriteSortMode.Immediate)
			{
				Setup();
			}

			_batcher.DrawBatch(_sortMode, _effect);

			for (int i = 0; i < 3; i++)
			{
				if (GameMain.RegisterTextures[i] == null)
					break;
				GameMain.RegisterTextures[i] = null;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Setup()
		{
			GraphicsDevice obj = _graphicDevice;
			obj.BlendState = _blendState;
			obj.DepthStencilState = _depthStencilState;
			obj.RasterizerState = _rasterizerState;
			obj.SamplerStates[0] = _samplerState;
			_spritePass.Apply();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float TextureSortKey(float depth) => _sortMode switch
		{
			SpriteSortMode.Texture => throw new Exception("SB monogame"),
			SpriteSortMode.FrontToBack => depth * 10,
			SpriteSortMode.BackToFront => depth * -10,
			_ => 0
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(Texture2D texture, CollideRect origin, CollideRect? source, Color color, float rotation, Vector2 anchor, Vector2 scale, SpriteEffects effects, float depth)
		{
			float sortKey = TextureSortKey(depth) + _depthBuffer;
			_depthBuffer += 0.00001f;
			Vector2 realAnchor = anchor + origin.TopLeft;
			Vector2[] pos = [
				-anchor,                                // TL
                new Vector2(origin.Width, 0) - anchor,  // TR
                new Vector2(0, origin.Height) - anchor, // BL
                origin.Size - anchor                    // BR
            ];
			if ((effects & SpriteEffects.FlipVertically) != 0)
			{
				scale.Y *= -1;
			}

			if ((effects & SpriteEffects.FlipHorizontally) != 0)
			{
				scale.X *= -1;
			}
			if (scale != Vector2.One)
			{
				for (int i = 0; i < 4; i++)
					pos[i] *= scale;
			}
			if (MathF.Abs((MathUtil.GetAngle(rotation) + 0.005f) % 360) > 0.01f) // need for rotating
			{
				for (int i = 0; i < 4; i++)
					pos[i] = MathUtil.RotateRadian(pos[i], rotation);
			}
			for (int i = 0; i < 4; i++)
				pos[i] += realAnchor;
			VertexPositionColorTexture vTL, vTR, vBL, vBR;
			vTL.Position = new(pos[0], 0);
			vTR.Position = new(pos[1], 0);
			vBL.Position = new(pos[2], 0);
			vBR.Position = new(pos[3], 0);
			vTL.Color = vTR.Color = vBL.Color = vBR.Color = color;

			if (source.HasValue)
			{
				CollideRect area = source.Value;
				Rectangle full = texture.Bounds;
				float l = area.X / full.Width;
				float r = area.Right / full.Width;
				float t = area.Y / full.Height;
				float b = area.Down / full.Height;

				vTL.TextureCoordinate = new(l, t);
				vTR.TextureCoordinate = new(r, t);
				vBL.TextureCoordinate = new(l, b);
				vBR.TextureCoordinate = new(r, b);
			}
			else
			{
				vTL.TextureCoordinate = Vector2.Zero;
				vTR.TextureCoordinate = Vector2.UnitX;
				vBL.TextureCoordinate = Vector2.UnitY;
				vBR.TextureCoordinate = Vector2.One;
			}
			_batcher.Insert(new RectangleItem(vTL, vTR, vBL, vBR, texture, sortKey));
			FlushIfNeeded();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(Texture2D texture, Vector2 position, CollideRect? sourceRectangle, Color color, float rotation, Vector2 anchor, Vector2 scale, SpriteEffects effects, float depth)
		{
			CheckValid(texture);
			Vector2 topLeft = position - anchor;
			CollideRect rect = new(topLeft, sourceRectangle.HasValue ?
				sourceRectangle.Value.Size :
				texture.Bounds.Size.ToVector2());

			Draw(texture, rect, sourceRectangle, color, rotation, anchor, scale, effects, depth);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(Texture2D texture, Vector2 centre, CollideRect? sourceRectangle, Color color, float rotation, Vector2 anchor, float scale, SpriteEffects effects, float depth) =>
			Draw(texture, centre, sourceRectangle, color, rotation, anchor, new Vector2(scale), effects, depth);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(Texture2D texture, Vector2 centre, Color color, float rotation, Vector2 anchor, float scale, float depth) =>
			Draw(texture, centre, null, color, rotation, anchor, new Vector2(scale, scale), SpriteEffects.None, depth);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Draw(Texture2D texture, Vector2 centre, Color color) =>
			Draw(texture, centre, null, color, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
		public void Draw(Texture2D texture, Vector2 centre, CollideRect? sourceRectangle, Color color, float rotation, Vector2 anchor, float scale, float depth) =>
			Draw(texture, centre, sourceRectangle, color, rotation, anchor, new Vector2(scale, scale), SpriteEffects.None, depth);
		public void Draw(Texture2D texture, CollideRect rect1, CollideRect? rect2, Color color, float rotation, Vector2 anchor, SpriteEffects se, float depth) =>
			Draw(texture, rect1, rect2, color, rotation, anchor, Vector2.One, se, depth);
		public void Draw(Texture2D texture, CollideRect rect1, CollideRect? rect2, Color color) =>
			Draw(texture, rect1, rect2, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);

		public void Draw(Texture2D texture, CollideRect bounds, Color color) =>
			Draw(texture, bounds, null, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);

		/// <summary>
		/// Give the Vertices information of sprite to draw on the current RenderTarget
		/// </summary>
		/// <param name="texture">the texture sprite</param>
		/// <param name="Vertices">The Vertices given. Make them in the order of clockwise! </param>
		/// <param name="depth">The depth to sort</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawVertex(Texture2D texture, float depth, params VertexPositionColorTexture[] Vertices) => _batcher.Insert(new VertexItem(texture, TextureSortKey(depth), Vertices));
		/// <summary>
		/// Give the Vertices information of sprite to draw on the current RenderTarget
		/// </summary>
		/// <param name="Vertices">The Vertices given. Make them in the order of clockwise! </param>
		/// <param name="depth">The depth to sort</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawVertex(float depth, params VertexPositionColor[] Vertices) => _batcher.Insert(new PrimitiveItem(TextureSortKey(depth), Vertices));
		/// <summary>
		/// Give the Vertices information of sprite to draw on the current RenderTarget
		/// </summary>
		/// <param name="vertices">The Vertices given. Make them in the order of clockwise! </param>
		/// <param name="depth">The depth to sort</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawSortedVertex(float depth, params VertexPositionColor[] vertices)
		{
			Vector2[] list = new Vector2[vertices.Length];
			for (int i = 0; i < list.Length; i++)
				list[i] = new(vertices[i].Position.X, vertices[i].Position.Y);
			List<Tuple<int, int, int>> raw = DrawingLab.GetIndices(list);
			int[] indices = new int[raw.Count * 3];
			for (int i = 0; i < raw.Count; i++)
			{
				indices[i * 3] = raw[i].Item1;
				indices[i * 3 + 1] = raw[i].Item2;
				indices[i * 3 + 2] = raw[i].Item3;
			}
			_batcher.Insert(new PrimitiveItem(TextureSortKey(depth), indices, vertices));
		}
		/// <summary>
		/// Give the Vertices information of sprite to draw on the current RenderTarget
		/// </summary>
		/// <param name="texture">the texture sprite</param>
		/// <param name="vertices">The Vertices given. Make them in the order of clockwise! </param>
		/// <param name="indices">The list of indices of the texture</param>
		/// <param name="depth">The depth to sort</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawVertex(Texture2D texture, float depth, int[] indices, params VertexPositionColorTexture[] vertices) => _batcher.Insert(new VertexItem(texture, TextureSortKey(depth), indices, vertices));
		/// <summary>
		/// Give the Vertices information of sprite to draw on the current RenderTarget
		/// </summary>
		/// <param name="texture">the texture sprite</param>
		/// <param name="vertices">The Vertices given. Make them in the order of clockwise! </param>
		/// <param name="depth">The depth to sort</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawVertex(Texture2D texture, float depth, params VertexPositionColor[] vertices)
		{
			VertexPositionColorTexture[] vpctVertices = new VertexPositionColorTexture[vertices.Length];
			float t = 99999f, b = -99999f, l = 99999f, r = -99999f;
			for (int i = 0; i < vertices.Length; i++)
			{
				t = MathF.Min(t, vertices[i].Position.Y);
				b = MathF.Max(b, vertices[i].Position.Y);
				l = MathF.Min(l, vertices[i].Position.X);
				r = MathF.Max(r, vertices[i].Position.X);
			}

			for (int i = 0; i < vertices.Length; i++)
			{
				float u = (vertices[i].Position.X - l) / (r - l);
				float v = (vertices[i].Position.Y - t) / (b - t);
				vpctVertices[i] = new(vertices[i].Position, vertices[i].Color, new(u, v));
			}

			_batcher.Insert(new VertexItem(texture, TextureSortKey(depth), vpctVertices));
		}
		/// <summary>
		/// Give the Vertices information of sprite to draw on the current RenderTarget
		/// </summary>
		/// <param name="texture">the texture sprite</param>
		/// <param name="vertices">The Vertices given. Make them in the order of clockwise! </param>
		/// <param name="depth">The depth to sort</param>
		/// <param name="indices"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawVertex(Texture2D texture, float depth, int[] indices, params VertexPositionColor[] vertices)
		{
			VertexPositionColorTexture[] vpctVertices = new VertexPositionColorTexture[vertices.Length];
			float t = 99999f, b = -99999f, l = 99999f, r = -99999f;
			for (int i = 0; i < vertices.Length; i++)
			{
				t = MathF.Min(t, vertices[i].Position.Y);
				b = MathF.Max(b, vertices[i].Position.Y);
				l = MathF.Min(l, vertices[i].Position.X);
				r = MathF.Max(r, vertices[i].Position.X);
			}

			for (int i = 0; i < vertices.Length; i++)
			{
				float u = (vertices[i].Position.X - l) / (r - l);
				float v = (vertices[i].Position.Y - t) / (b - t);
				vpctVertices[i] = new(vertices[i].Position, vertices[i].Color, new(u, v));
			}

			_batcher.Insert(new VertexItem(texture, TextureSortKey(depth), indices, vpctVertices));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawString(GLFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) => DrawString(spriteFont, text, position, color, rotation, origin, new vec2(scale), effects, layerDepth);
		private static void ParseMinFontHeight(GLFont font)
		{
			float minHeight = float.MaxValue;
			foreach (char item in font.SFX.Characters)
			{
				if (item is >= 'a' and <= 'z')
				{
					SpriteFont.Glyph curGlyph = font.SFX.Glyphs[font.GetGlyphIndexOrDefault(item)];
					minHeight = MathF.Min(minHeight, curGlyph.BoundsInTexture.Height);
				}
			}
			MinFontHeight.Add(font, minHeight);
		}
		public void DrawString(GLFont font, string text, Vector2 position, Color color, float rotation = 0, Vector2? originN = null, Vector2? scaleN = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0) => DrawString(font, text, position, [color, color, color, color], rotation, originN, scaleN, effects, layerDepth);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void DrawString(GLFont font, string text, Vector2 position, Color[] color, float rotation = 0, Vector2? originN = null, Vector2? scaleN = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
		{
			vec2 origin = originN ?? Vector2.Zero;
			vec2 scale = scaleN ?? Vector2.One;
			SpriteFont spriteFont = font.SFX;
			CheckValid(spriteFont, text);
			float sortKey = TextureSortKey(layerDepth);
			Vector2 zero = Vector2.Zero;
			bool flipV = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			bool flipH = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
			if (flipV || flipH)
			{
				Vector2 size = spriteFont.MeasureString(text);
				if (flipH)
				{
					origin.X *= -1f;
					zero.X = -size.X;
					scale.X *= -1f;
				}

				if (flipV)
				{
					origin.Y *= -1f;
					zero.Y = spriteFont.LineSpacing - size.Y;
					scale.Y *= -1f;
				}
			}

			Matrix matrix = Matrix.Identity;
			float num = 0f, num2 = 0f;
			if (rotation == 0f)
			{
				matrix.M11 = flipH ? -scale.X : scale.X;
				matrix.M22 = flipV ? -scale.Y : scale.Y;
				matrix.M41 = (zero.X - origin.X) * matrix.M11 + position.X;
				matrix.M42 = (zero.Y - origin.Y) * matrix.M22 + position.Y;
			}
			else
			{
				num = MathF.Cos(rotation);
				num2 = MathF.Sin(rotation);
				matrix.M11 = (flipH ? -scale.X : scale.X) * num;
				matrix.M12 = (flipH ? -scale.X : scale.X) * num2;
				matrix.M21 = (flipV ? -scale.Y : scale.Y) * -num2;
				matrix.M22 = (flipV ? -scale.Y : scale.Y) * num;
				matrix.M41 = (zero.X - origin.X) * matrix.M11 + (zero.Y - origin.Y) * matrix.M21 + position.X;
				matrix.M42 = (zero.X - origin.X) * matrix.M12 + (zero.Y - origin.Y) * matrix.M22 + position.Y;
			}

			Vector2 zero2 = Vector2.Zero;
			bool newLine = true;
			char? prevChar = null;

			//Parse min font height for displacement apply on text
			if (!MinFontHeight.ContainsKey(font))
				ParseMinFontHeight(font);

			fixed (SpriteFont.Glyph* ptr = spriteFont.Glyphs)
			{
				foreach (char c in text)
				{
					switch (c)
					{
						case '\n':
							zero2.X = 0f;
							zero2.Y += spriteFont.LineSpacing;
							newLine = true;
							continue;
						case '\r':
							continue;
					}
					//Fixing incorrect spacing for certain characters
					if (prevChar != null)
						switch (prevChar)
						{
							case '(':
							case '\'':
								zero2.X -= 5 * scale.X;
								break;
						}

					SpriteFont.Glyph* ptr2 = ptr + font.GetGlyphIndexOrDefault(c);
					if (newLine)
					{
						zero2.X = Math.Max(ptr2->LeftSideBearing, 0f);
						newLine = false;
					}
					else
						zero2.X += spriteFont.Spacing + ptr2->LeftSideBearing;

					Vector2 position2 = zero2;
					if (flipH)
						position2.X += ptr2->BoundsInTexture.Width;

					position2.X += ptr2->Cropping.X;
					if (flipV)
						position2.Y += ptr2->BoundsInTexture.Height - spriteFont.LineSpacing;
					//Characters are downwards extending instead of upwards extending, no displacement required
					if (c is not ('q' or 'y' or 'p' or 'g' or '\'' or '"' or '`' or '+'))
						position2.Y -= ptr2->BoundsInTexture.Height - MinFontHeight[font];
					//Special chars
					if (c is 'j' or 'Q' or '-' or '*' or '\'' or '~' or '=')
						position2.Y += (ptr2->BoundsInTexture.Height - MinFontHeight[font]) * 0.5f;

					Vector2.Transform(ref position2, ref matrix, out position2);

					Vector2 _texCoordTL, _texCoordBR;
					_texCoordTL.X = ptr2->BoundsInTexture.X;
					_texCoordTL.Y = ptr2->BoundsInTexture.Y;
					_texCoordBR.X = ptr2->BoundsInTexture.X + ptr2->BoundsInTexture.Width;
					_texCoordBR.Y = ptr2->BoundsInTexture.Y + ptr2->BoundsInTexture.Height;
					Vector2 _uvTL, _uvBR;
					_uvTL.X = _texCoordTL.X / spriteFont.Texture.Width;
					_uvTL.Y = _texCoordTL.Y / spriteFont.Texture.Height;
					_uvBR.X = _texCoordBR.X / spriteFont.Texture.Width;
					_uvBR.Y = _texCoordBR.Y / spriteFont.Texture.Height;
					//position2.Y -= (ptr2->Cropping.Height - ptr2->BoundsInTexture.Height);
					SpriteBatchItem spriteBatchItem = rotation == 0f
						? new RectangleItem(position2.X, position2.Y, ptr2->BoundsInTexture.Width * scale.X, ptr2->BoundsInTexture.Height * scale.Y, color, _uvTL, _uvBR, layerDepth, spriteFont.Texture, sortKey)
						: new RectangleItem(position2.X, position2.Y, 0f, 0f, ptr2->BoundsInTexture.Width * scale.X, ptr2->BoundsInTexture.Height * scale.Y, num2, num, color, _uvTL, _uvBR, layerDepth, spriteFont.Texture, sortKey);
					_batcher.Insert(spriteBatchItem);
					zero2.X += ptr2->Width + ptr2->RightSideBearing;
					prevChar = c;
				}
			}

			FlushIfNeeded();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void FlushIfNeeded()
		{
			if (_sortMode == SpriteSortMode.Immediate)
				_batcher.DrawBatch(_sortMode, _effect);
		}
	}
}