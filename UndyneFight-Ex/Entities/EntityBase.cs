using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UndyneFight_Ex.Entities;

namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// Interface for entities that has motion control
	/// </summary>
	public interface ICustomMotion
	{
		/// <summary>
		/// The position function of the entity, use <see cref="Motions.PositionRoute"/> or <see cref="SimplifiedEasing"/>
		/// </summary>
		Func<ICustomMotion, Vector2> PositionRoute { get; set; }
		/// <summary>
		/// The rotation function of the entity, use <see cref="Motions.RotationRoute"/> or <see cref="SimplifiedEasing"/>
		/// </summary>
		Func<ICustomMotion, float> RotationRoute { get; set; }
		/// <summary>
		/// The parameters for the rotation route
		/// </summary>
		float[] RotationRouteParam { get; set; }
		/// <summary>
		/// The parameters for the position route
		/// </summary>
		float[] PositionRouteParam { get; set; }
		/// <summary>
		/// The frames elapsed after being created
		/// </summary>
		float AppearTime { get; }
		/// <summary>
		/// The rotation of the entity
		/// </summary>
		float Rotation { get; }
		/// <summary>
		/// The centre position of the entity
		/// </summary>
		Vector2 CentrePosition { get; }
	}
	/// <summary>
	/// Interface for entities that has length control (Bones)
	/// </summary>
	public interface ICustomLength
	{
		/// <summary>
		/// The frames elapsed after being created
		/// </summary>
		float AppearTime { get; }
		/// <summary>
		/// The length easing function of the entity, use <see cref="Motions.LengthRoute"/> or <see cref="SimplifiedEasing"/>
		/// </summary>
		Func<ICustomLength, float> LengthRoute { get; set; }
		/// <summary>
		/// The parameters for the length route
		/// </summary>
		float[] LengthRouteParam { get; set; }
	}
	/// <summary>
	/// Set motions for entities
	/// </summary>
	public static class Motions
	{
		/// <summary>
		/// Rotation route for entities
		/// </summary>
		public static class RotationRoute
		{
			/// <summary>
			/// Sinusoidal rotation motion, [Intensity, Wavelength, Initial Time, Constant]<br></br>
			/// The formula is: Intensity * Sin((AppearTime + Initial Time) / Wavelength * PI * 2) + Constant
			/// </summary>
			public static readonly Func<ICustomMotion, float> sin = (s) => s.RotationRouteParam[3] + (float)(s.RotationRouteParam[0] * Math.Sin((s.AppearTime + s.RotationRouteParam[2]) / s.RotationRouteParam[1] * Math.PI * 2));
			/// <summary>
			/// Linear rotation, [Rotation speed, Initial Angle]
			/// </summary>
			public static readonly Func<ICustomMotion, float> linear = (s) => s.RotationRouteParam[0] * s.AppearTime + s.RotationRouteParam[1];
			/// <summary>
			/// Stable value, [Value]
			/// </summary>
			public static readonly Func<ICustomMotion, float> stableValue = (s) => s.RotationRouteParam[0];
		}
		/// <summary>
		/// Length route for entities
		/// </summary>
		public static class LengthRoute
		{
			/// <summary>
			/// Will automatically fold when duration has been reached, [Length, Duration]
			/// </summary>
			public static readonly Func<ICustomLength, float> autoFold = (s) =>
			{
				float dec = Math.Max(0, s.AppearTime - s.LengthRouteParam[1]);
				return s.LengthRouteParam[0] - dec * dec / 12f;
			};
			/// <summary>
			/// Cubic Sinusoidal motion, [Intensity, Wavelength, Initial Time, Constant]
			/// The formula is: Intensity * Sin((AppearTime + Initial Time) / Wavelength * PI * 2) ^ 3 + Constant
			/// </summary>
			public static readonly Func<ICustomLength, float> sin3 = (s) => s.LengthRouteParam[3] + s.LengthRouteParam[0] * MathF.Pow(MathF.Sin((s.AppearTime + s.LengthRouteParam[2]) / s.LengthRouteParam[1] * MathF.PI * 2), 3);
			/// <summary>
			/// Stable value, [Value]
			/// </summary>
			public static readonly Func<ICustomLength, float> stableValue = (s) => s.LengthRouteParam[0];
			/// <summary>
			/// Sinusoidal motion, [Intensity, Wavelength, Initial Time, Constant]
			/// /// The formula is: Intensity * Sin((AppearTime + Initial Time) / Wavelength * PI * 2) + Constant
			/// </summary>
			public static readonly Func<ICustomLength, float> sin = (s) => s.LengthRouteParam[3] + s.LengthRouteParam[0] * MathF.Sin((s.AppearTime + s.LengthRouteParam[2]) / s.LengthRouteParam[1] * MathF.PI * 2);
		}
		/// <summary>
		/// Position route for entities
		/// </summary>
		public static class PositionRoute
		{
			/// <summary>
			/// Static position
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> stableValue = (s) => Vector2.Zero;
			/// <summary>
			/// Begins at the top, extends to the lower side
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> cameFromUp = (s) => new Vector2(0, -MathF.Pow(0.85f, s.AppearTime) * 600);
			/// <summary>
			/// Begins at the bottom, extends to the upper side
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> cameFromDown = (s) => new Vector2(0, MathF.Pow(0.85f, s.AppearTime) * 600);
			/// <summary>
			/// Begins at the left, extends to the right side
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> cameFromLeft = (s) => new Vector2(-MathF.Pow(0.85f, s.AppearTime) * 600, 0);
			/// <summary>
			/// Begins at the right, extends to the left side
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> cameFromRight = (s) => new Vector2(MathF.Pow(0.85f, s.AppearTime) * 600, 0);
			/// <summary>
			/// Linear motion, [x speed, y speed]
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> linear = (s) => new Vector2(s.PositionRouteParam[0], s.PositionRouteParam[1]) * s.AppearTime;
			/// <summary>
			/// Linear Horizontal movement + Sinusoidal Vertical Movement, [X speed, Intensity, Wavelength, Initial Time]
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> XAxisSin = (s) => new Vector2(s.PositionRouteParam[0] * s.AppearTime, s.PositionRouteParam[1] * MathF.Sin((s.AppearTime + s.PositionRouteParam[3]) / s.PositionRouteParam[2] * MathF.PI * 2));
			/// <summary>
			/// Linear Vertical movement + Sinusoidal Horizontal Movement, [Y speed, Intensity, Wavelength, Initial Time]
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> YAxisSin = (s) => new Vector2(s.PositionRouteParam[1] * MathF.Sin((s.AppearTime + s.PositionRouteParam[3]) / s.PositionRouteParam[2] * MathF.PI * 2), s.PositionRouteParam[0] * s.AppearTime);
			/// <summary>
			/// Linear Circular motion, [Distance, Angular Speed, Initial Angle]
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> circle = (s) =>
			{
				float alpha = s.AppearTime * s.PositionRouteParam[1] + s.PositionRouteParam[2];
				return new Vector2(s.PositionRouteParam[0] * Fight.Functions.Cos(alpha), s.PositionRouteParam[0] * Fight.Functions.Sin(alpha));
			};
			/// <summary>
			/// Linear Horizontal Accelerative movement + Sinusoidal Vertical Movement, [X speed, X acceleration, Intensity, Wavelength, Initial Time]
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> XAccAxisSin = (s) => new Vector2(s.AppearTime * s.AppearTime / 2 * s.PositionRouteParam[1] + s.PositionRouteParam[0] * s.AppearTime, s.PositionRouteParam[2] * MathF.Sin((s.AppearTime + s.PositionRouteParam[4]) / s.PositionRouteParam[3] * MathF.PI * 2));
			/// <summary>
			/// Linear Horizontal Accelerative movement + Linear Vertical movement, [X speed, X acceleration, Y speed]
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> XAccYLinear = (s) => new Vector2(s.AppearTime * s.AppearTime / 2 * s.PositionRouteParam[1] + s.PositionRouteParam[0] * s.AppearTime, s.PositionRouteParam[2] * s.AppearTime);

			/// <summary>
			/// Sinusoidal Vertical Movement + Sinusoidal Horizontal Movement, [Y Intensity, Y Wavelength, Y Initial Time, X Intensity, X Wavelength, X Initial Time]
			/// </summary>
			public static readonly Func<ICustomMotion, Vector2> XYAxisSin = (s) => new Vector2((float)(s.PositionRouteParam[3] * Math.Sin((s.AppearTime + s.PositionRouteParam[5]) / s.PositionRouteParam[4] * Math.PI * 2)), (float)(s.PositionRouteParam[0] * Math.Sin((s.AppearTime + s.PositionRouteParam[2]) / s.PositionRouteParam[1] * Math.PI * 2)));
		}
	}
	/// <summary>
	/// Invokes an action after a given delay
	/// </summary>
	public class InstantEvent : GameObject
	{
		private readonly Action _action;
		private float _timeDelay;
		/// <summary>
		/// Invoke an action after the given delay
		/// </summary>
		/// <param name="timeDelay">The delay to invoke the action</param>
		/// <param name="action">The action to invoke</param>
		public InstantEvent(float timeDelay, Action action)
		{
			_timeDelay = (int)timeDelay;
			_action = action;
			UpdateIn120 = true;
		}
		public override void Update()
		{
			if (_timeDelay <= 0)
			{
				_action();
				Dispose();
			}
			_timeDelay -= 0.5f;
		}
	}
	/// <summary>
	/// Invoke an action that lasts for a duration after a delay
	/// </summary>
	public class TimeRangedEvent : GameObject
	{
		private readonly Action _action;
		private float _timeDelay;
		private readonly float _duration;
		/// <summary>
		/// Invoke an action that lasts for the given duration after the given delay
		/// </summary>
		/// <param name="timeDelay">The delay to invoke the action</param>
		/// <param name="duration">The duration of the action to invoke</param>
		/// <param name="action">The action to invoke</param>
		public TimeRangedEvent(float timeDelay, float duration, Action action)
		{
			_duration = (int)duration;
			_timeDelay = (int)timeDelay;
			_action = action;
		}
		/// <summary>
		/// Invoke an action that lasts for the given duration
		/// </summary>
		/// <param name="duration">The duration of the action to invoke</param>
		/// <param name="action">The action to invoke</param>
		public TimeRangedEvent(float duration, Action action)
		{
			_duration = (int)duration;
			_timeDelay = 0;
			_action = action;
		}
		public override void Update()
		{
			if (_timeDelay <= 0)
			{
				if (_timeDelay <= -_duration)
				{
					Dispose();
					return;
				}
				_action();
			}
			_timeDelay -= UpdateIn120 ? 0.5f : 1;
		}
	}
	/// <summary>
	/// Background for legacy engine
	/// </summary>
	internal class BackGround : Entity
	{
		private readonly Entity camera;
		private Vector2 centrePos;

		public float Alpha { get; set; }
		public BackGround(Texture2D tex, Entity camera, Vector2 centrePosition)
		{
			UpdateIn120 = true;
			Depth = 0f;
			centrePos = centrePosition;
			Image = tex;
			this.camera = camera;
		}

		public override void Draw() => FormalDraw(Image, Centre, Color.White * Alpha, new Vector2(640 / Image.Width, 480 / Image.Height), Rotation, ImageCentre);

		public override void Update()
		{
			Centre = centrePos + ImageCentre - camera.Centre;
			Rotation = -camera.Rotation;
			if (camera.Disposed)
				Dispose();
		}
	}
}
namespace UndyneFight_Ex
{
	/// <summary>
	/// An entity that draws an image
	/// </summary>
	/// <param name="image">The image to draw</param>
	public class ImageEntity(Texture2D image) : AutoEntity
	{
		public override void Update() { }
		/// <summary>
		/// Overrides the draw event
		/// </summary>
		public event Action OnDraw;

		public override void Draw()
		{
			Image = image;
			if (OnDraw != null)
				OnDraw();
			else
				base.Draw();
		}
	}
	/// <summary>
	/// Draws an image with color and alpha
	/// </summary>
	public abstract class AutoEntity : Entity
	{
		/// <summary>
		/// The color of the image to draw
		/// </summary>
		public Color BlendColor { set; private get; } = Color.White;
		/// <summary>
		/// The alpha of the image
		/// </summary>
		public float Alpha { get; set; }
		/// <summary>
		/// Whether to enable pre-multiply alpha
		/// </summary>
		public bool PreMultiplyAlpha { get; set; } = false;

		private Vector2 _anchor;
		private bool _anchorEnabled = false;
		/// <summary>
		/// The anchor of the image
		/// </summary>
		public Vector2 Anchor
		{
			get => _anchorEnabled ? _anchor : ImageCentre;
			set { _anchor = value; _anchorEnabled = true; }
		}

		public override void Draw()
		{
			if (Alpha <= 0 || Image == null)
				return;

			FormalDraw(Image, Centre, PreMultiplyAlpha ? Color.Lerp(Color.Transparent, BlendColor, Alpha) : BlendColor * Alpha, Scale, Rotation, Anchor);
		}
	}
	/// <summary>
	/// Draws a piece of static text
	/// </summary>
	/// <param name="text">The text to draw</param>
	/// <param name="centre">The centre of the text</param>
	public class TextEntity(string text, Vector2 centre) : Entity
	{
		/// <summary>
		/// The color of the text
		/// </summary>
		public Color BlendColor { set; private get; } = Color.White;
		/// <summary>
		/// The alpha of the text
		/// </summary>
		public float Alpha { get; set; } = 1f;
		/// <summary>
		/// The text to display
		/// </summary>
		public string Text { private get; set; } = text;
		/// <summary>
		/// The font of the text
		/// </summary>
		public GLFont Font { get; set; } = FightResources.Font.NormalFont;
		/// <summary>
		/// Draws the entity
		/// </summary>
		public override void Draw() => Font.CentreDraw(Text, centre, BlendColor * Alpha, Scale, AngleMode ? MathUtil.GetRadian(Rotation) : Rotation, Depth);
		/// <summary>
		/// The update logic of the entity
		/// </summary>
		public override void Update() { }
	}
	/// <summary>
	/// An entity base
	/// </summary>
	public abstract class Entity() : GameObject
	{
		/// <summary>
		/// Whether the entity is visible
		/// </summary>
		public bool Visible { get; set; } = true;
		/// <summary>
		/// Whether to use Radians (true) or Degrees (false) for the rotation angle
		/// </summary>
		public bool AngleMode { set; get; } = false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float DrawingRotation(float rotation) => AngleMode ? MathUtil.GetRadian(rotation) : rotation;
		/// <summary>
		/// The controlling surface of the entity (Default <see cref="Surface.Normal"/>)
		/// </summary>

		public Surface controlLayer = Surface.Normal;
		/// <summary>
		/// The sprite batch that is used for drawing
		/// </summary>
		protected static SpriteBatchEX SpriteBatch => GameMain.MissionSpriteBatch;
		/// <summary>
		/// Draws the given texture
		/// </summary>
		/// <param name="tex">The texture to draw</param>
		/// <param name="centre">The position to draw</param>
		/// <param name="color">The color of the texture</param>
		/// <param name="rotation">The rotation of the texture</param>
		/// <param name="rotateCentre">The center of rotation</param>
		public void FormalDraw(Texture2D tex, Vector2 centre, Color color, float rotation, Vector2 rotateCentre)
		{
			rotation = DrawingRotation(rotation);
			if (!NotInScene(tex, centre, vec2.One, rotation, rotateCentre))
				GameMain.MissionSpriteBatch.Draw(tex, centre, null, color * controlLayer.drawingAlpha, rotation, rotateCentre, 1.0f, SpriteEffects.None, Depth);

		}
		/// <summary>
		/// Draws the given texture
		/// </summary>
		/// <param name="tex">The texture to draw</param>
		/// <param name="centre">The position to draw</param>
		/// <param name="texArea">The rectangle area to draw the texture</param>
		/// <param name="color">The color of the texture</param>
		/// <param name="rotation">The rotation of the texture</param>
		/// <param name="rotateCentre">The center of rotation</param>
		public void FormalDraw(Texture2D tex, Vector2 centre, Rectangle? texArea, Color color, float rotation, Vector2 rotateCentre)
		{
			rotation = DrawingRotation(rotation);
			if (!NotInScene(tex, centre, vec2.One, rotation, rotateCentre))
				GameMain.MissionSpriteBatch.Draw(tex, centre, texArea, color * controlLayer.drawingAlpha, rotation, rotateCentre, 1.0f, SpriteEffects.None, Depth);
		}
		/// <summary>
		/// Draws the given texture
		/// </summary>
		/// <param name="tex">The texture to draw</param>
		/// <param name="centre">The position to draw</param>
		/// <param name="texArea">The rectangle area to draw the texture</param>
		/// <param name="color">The color of the texture</param>
		/// <param name="drawingScale">The scale of drawing</param>
		/// <param name="rotation">The rotation of the texture</param>
		/// <param name="rotateCentre">The center of rotation</param>
		/// <param name="spriteEffects">The sprite effect to apply on it</param>
		public void FormalDraw(Texture2D tex, Vector2 centre, Rectangle? texArea, Color color, Vector2 drawingScale, float rotation, Vector2 rotateCentre, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			rotation = DrawingRotation(rotation);
			if (!NotInScene(tex, centre, drawingScale, rotation, rotateCentre))
				GameMain.MissionSpriteBatch.Draw(tex, centre, texArea, color * controlLayer.drawingAlpha, rotation, rotateCentre, drawingScale, spriteEffects, Depth);

		}
		/// <summary>
		/// Draws the given texture
		/// </summary>
		/// <param name="tex">The texture to draw</param>
		/// <param name="area">The area to draw the texture in</param>
		/// <param name="color">The color of the texture to draw</param>
		public void FormalDraw(Texture2D tex, CollideRect area, Color color) => GameMain.MissionSpriteBatch.Draw(tex, area, null, color * controlLayer.drawingAlpha, 0, Vector2.Zero, SpriteEffects.None, Depth);
		/// <summary>
		/// Draws the given texture in a restricted area
		/// </summary>
		/// <param name="tex">The texture to draw</param>
		/// <param name="area">The area of the texture to draw</param>
		/// <param name="restrict">The area the texture can be drawn in</param>
		/// <param name="color">The color of the texture to draw</param>
		public void FormalDraw(Texture2D tex, Rectangle area, Rectangle restrict, Color color) => GameMain.MissionSpriteBatch.Draw(tex, area, restrict, color * controlLayer.drawingAlpha, 0, Vector2.Zero, SpriteEffects.None, Depth);
		/// <summary>
		/// Draws the given texture
		/// </summary>
		/// <param name="tex">The texture to draw</param>
		/// <param name="centre">The position to draw the texture</param>
		/// <param name="color">The color of the texture to draw</param>
		/// <param name="drawingScale">The scale of the drawn texture</param>
		/// <param name="rotation">The rotation of the texture</param>
		/// <param name="rotateCentre">The center of rotation</param>
		public void FormalDraw(Texture2D tex, Vector2 centre, Color color, float drawingScale, float rotation, Vector2 rotateCentre) => FormalDraw(tex, centre, color, new Vector2(drawingScale), rotation, rotateCentre);
		/// <summary>
		/// Draws the given texture
		/// </summary>
		/// <param name="tex">The texture to draw</param>
		/// <param name="centre">The position to draw the texture</param>
		/// <param name="color">The color of the texture to draw</param>
		/// <param name="drawingScale">The scale of the drawn texture</param>
		/// <param name="rotation">The rotation of the texture</param>
		/// <param name="rotateCentre">The center of rotation</param>
		public void FormalDraw(Texture2D tex, Vector2 centre, Color color, Vector2 drawingScale, float rotation, Vector2 rotateCentre)
		{
			rotation = DrawingRotation(rotation);
			if (!NotInScene(tex, centre, drawingScale, rotation, rotateCentre))
				GameMain.MissionSpriteBatch.Draw(tex, centre, null, color * controlLayer.drawingAlpha, rotation, rotateCentre, drawingScale, SpriteEffects.None, Depth);
		}
		/// <summary>
		/// A general texture drawing function that integrates all functionalities from all FormalDraw functions
		/// </summary>
		/// <param name="texture">The texture to draw</param>
		/// <param name="position">The position to draw the texture</param>
		/// <param name="color">The color of the texture to draw (Default white)</param>
		/// <param name="scale">The scale of the texture to draw (Default 1)</param>
		/// <param name="rotation">The rotation of the texture to draw in radians (Default 0)</param>
		/// <param name="spriteOrigin">The origin of the texture to draw (Default center of texture)</param>
		/// <param name="texArea">The bounds of drawing on the screen (Default null for normal drawing)</param>
		/// <param name="sourceRect">The region of the texture to render (Default null for full texture)</param>
		/// <param name="depth">The depth of the texture to draw (Default current depth)</param>
		public void GeneralDraw(Texture2D texture, Vector2 position, Color? color = null, Vector2? scale = null, float rotation = 0, Vector2? spriteOrigin = null, CollideRect? texArea = null, CollideRect? sourceRect = null, float? depth = null)
		{
			if (texture is null)
			{
				Debug.WriteLine("The texture you are trying to draw is not a texture or is not loaded");
				return;
			}
			Vector2 GetRotCen = spriteOrigin ?? new(texture.Width / 2f, texture.Height / 2f);
			Vector2 drawingScale = scale ?? Vector2.One;
			CollideRect rect = new(position - GetRotCen, texArea.HasValue ? texArea.Value.Size : texture.Bounds.Size.ToVector2());
			if (!NotInScene(texture, position, drawingScale, rotation, GetRotCen))
				GameMain.MissionSpriteBatch.Draw(texture, rect, sourceRect, (color ?? Color.White) * controlLayer.drawingAlpha, rotation, GetRotCen, drawingScale, SpriteEffects.None, depth ?? Depth);
		}
		private static float SqrtTwo => MathF.Sqrt(2);
		/// <summary>
		/// Check if the texture is inside the current view
		/// </summary>
		/// <param name="tex">The texture to check</param>
		/// <param name="centre">The position of the texture</param>
		/// <param name="drawingScale">The drawing scale of the texture</param>
		/// <param name="rotation">The rotation of the texture</param>
		/// <param name="rotateCentre">The center of rotation</param>
		/// <returns>Whether the texture is inside the current view</returns>
		private static bool NotInScene(Texture2D tex, Vector2 centre, Vector2 drawingScale, float rotation, Vector2 rotateCentre)
		{
			if (!DrawOptimize)
				return false;
			Scene.DrawingSettings drawingSettings = CurrentScene.CurrentDrawingSettings;
			float scale = 1 / MathF.Abs(drawingSettings.screenScale) * (MathF.Abs(MathF.Sin(drawingSettings.screenAngle * 2)) * (SqrtTwo - 1) + 1) * 1.212f;
			Vector4 extend = drawingSettings.Extending;
			float scrWidth = drawingSettings.defaultWidth;
			float scrHeight = scrWidth / GameStates.Aspect;
			CollideRect cur = new(0, -scrHeight * extend.W, scrWidth * scale * GameStates.SurfaceScale, scrHeight * (scale + extend.W) * GameStates.SurfaceScale);
			cur.SetCentre(new Vector2(scrWidth / 2f, (1 - extend.W) * 0.5f * scrHeight) * GameStates.SurfaceScale);
			cur.Offset(-drawingSettings.screenDelta / drawingSettings.screenScale);

			if (cur.Contain(centre))
				return false;

			Vector2 size = tex.Bounds.Size.ToVector2() * drawingScale;
			rotateCentre *= drawingScale;
			Vector2[] points = [Vector2.Zero, new(size.X, 0), new(0, size.Y), size];
			for (int i = 0; i < points.Length; i++)
				points[i] -= rotateCentre;
			Vector2[] reals = new Vector2[4];
			for (int i = 0; i < reals.Length; i++)
				reals[i] = MathUtil.Rotate(points[i], MathUtil.GetAngle(rotation)) + centre;
			float[] dirs =
			[
				reals.Min(dir => dir.X),
				reals.Max(dir => dir.X),
				reals.Min(dir => dir.Y),
				reals.Max(dir => dir.Y),
			];
			CollideRect bounding = new(dirs[0], dirs[2], dirs[1] - dirs[0], dirs[3] - dirs[2]);

			return !bounding.Intersects(cur);
		}
		/// <summary>
		/// The depth of the entity (The higher the value is, the less shallow it is)
		/// </summary>
		public float Depth { get; set; }
		private Texture2D image;
		/// <summary>
		/// The centre of the image
		/// </summary>
		protected Vector2 ImageCentre { get; set; }
		/// <summary>
		/// The image of the entity to draw
		/// </summary>
		public Texture2D Image
		{
			get => image;
			set
			{
				if (value == null)
					return;
				image = value;
				ImageCentre = new Vector2(value.Width, value.Height) / 2.0f;
			}
		}
		/// <summary>
		/// Do not use this variable
		/// </summary>
		protected CollideRect collidingBox;
		/// <summary>
		/// The rotation of the entity
		/// </summary>
		public float Rotation { get; set; }
		/// <summary>
		/// The scale of the entity
		/// </summary>
		public float Scale { get; set; } = 1.0f;
		/// <summary>
		/// The colliding box of the entity
		/// </summary>
		public CollideRect CollidingBox
		{
			get => collidingBox;
			init => collidingBox = value;
		}
		/// <summary>
		/// The centre of the entity
		/// </summary>
		public Vector2 Centre
		{
			get => collidingBox.GetCentre();
			set => collidingBox.SetCentre(value);
		}
		/// <summary>
		/// Whether the entity will be drawn regardless whether it is inside of the current view (false -> Drawn regardless, true -> Check if inside screen)
		/// </summary>
		public static bool DrawOptimize { get; set; } = true;
		/// <summary>
		/// The rendering logic of the entity
		/// </summary>
		public abstract void Draw();
		/// <summary>
		/// Creates an expanding fade out effect of this entity
		/// </summary>
		/// <param name="color">The color of the effect</param>
		/// <param name="image">The image of the effect</param>
		/// <returns>The created effect</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected ShinyEffect CreateShinyEffect(Color? color = null, Texture2D image = null)
		{
			ShinyEffect effect = new(this, color, image);
			GameStates.InstanceCreate(effect);
			return effect;
		}
		/// <summary>
		/// Creates a drag effect of this entity
		/// </summary>
		/// <param name="time">The duration of the drag</param>
		/// <param name="color">The color of the effect</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void CreateRetentionEffect(float time, Color? color = null) => GameStates.InstanceCreate(new RetentionEffect(this, time, color ?? Color.White));

		protected class ShinyEffect : Entity
		{
			private readonly Entity attracter;
			private float baseScale = 1.0f;
			private float drawingScale = 1.0f;
			private float darkerSpeed = 3.5f;
			public ShinyEffect(Entity original, Color? color = null, Texture2D shinyImage = null)
			{
				controlLayer = original.controlLayer;
				attracter = original;
				Image = shinyImage ?? original.image;
				drawingColor = color ?? Color.White;
			}

			public float DarkerSpeed { set => darkerSpeed = value; }
			public Vector2 MissionSize { set => missionSize = value; }

			private Color drawingColor;
			private Vector2 missionSize = new(3);

			public override void Update()
			{
				if (attracter != null)
				{
					controlLayer = attracter.controlLayer;
					Centre = attracter.Centre;
					Rotation = attracter.Rotation;
					Depth = attracter.Depth + 0.001f;
					baseScale = attracter.Scale;
				}
				drawingScale += darkerSpeed / 100f;
				if (drawingScale >= 2f)
					Dispose();
			}

			public override void Draw() => FormalDraw(image, Centre, drawingColor * (2 - drawingScale), Vector2.Lerp(Vector2.One, missionSize * baseScale, drawingScale - 1), MathUtil.GetRadian(Rotation), ImageCentre);
		}

		protected class RetentionEffect : Entity
		{
			private readonly float totalTime = 30;
			private Color color;

			public RetentionEffect(Entity original, float time, Color? color = null)
			{
				controlLayer = original.controlLayer;
				time += 5f;
				Depth = original.Depth - 0.01f;
				this.color = color ?? Color.White;
				totalTime = time;
				Rotation = original.Rotation;
				Centre = original.Centre;
				Image = original.image;
			}
			public override void Draw() => FormalDraw(image, Centre, color * alpha, Rotation, ImageCentre);

			private float alpha = 1f;
			public override void Update()
			{
				alpha -= 1 / totalTime;
				alpha *= 0.95f;
				Depth -= 0.0005f;
				if (alpha < 0)
					Dispose();
			}
		}
	}
	/// <summary>
	/// A game object
	/// </summary>
	public abstract class GameObject
	{
		/// <summary>
		/// Whether the <see cref="ChildObjects"/> will be updated as well
		/// </summary>
		protected bool UpdateChildren { get; set; } = true;
		/// <summary>
		/// Whether the <see cref="ChildObjects"/> updates before this object updates
		/// </summary>
		protected bool ChildrenUpdateFirst { private get; set; } = false;
		/// <summary>
		/// The current scene
		/// </summary>
		protected static Scene CurrentScene => GameStates.CurrentScene;
		/// <summary>
		/// The current <see cref="SongFightingScene"/>
		/// </summary>
		protected static SongFightingScene CurrentFightingScene => GameStates.CurrentScene as SongFightingScene;
		/// <summary>
		/// Whether the object will be updated every 120 frames or 60 frames (It is better to set this as true)
		/// </summary>
		public bool UpdateIn120 { get; init; } = false;
		/// <summary>
		/// Whether the object is being updated
		/// </summary>
		public bool BeingUpdated { get; protected internal set; } = false;
		/// <summary>
		/// Whether the object will be updated
		/// </summary>
		public bool UpdateEnabled { get; set; } = true;
		/// <summary>
		/// Whether the object will persist after changing to another scene
		/// </summary>
		public bool CrossScene { get; protected set; } = false;
		/// <summary>
		/// Whether the object contains the given tag
		/// </summary>
		/// <param name="tagName">The tag to check</param>
		/// <returns>Whether the object contains the given tag</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainTag(string tagName)
		{
			if (tags == null)
				return false;
			foreach (Tag v in tags)
			{
				if (v.tagName == tagName)
					return true;
			}
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Broadcast(string info) => GameStates.Broadcast(new GameEventArgs(this, info));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Tuple<bool, GameEventArgs> TryDetect(string tagName) => GameStates.DetectEvent(tagName) is null || GameStates.DetectEvent(tagName).Count == 0 ? new(false, null) : new(true, GameStates.DetectEvent(tagName)[0]);
		/// <summary>
		/// The tags of the game object
		/// </summary>
		public string[] Tags
		{
			set
			{
				tags = new Tag[value.Length];
				for (int i = 0; i < value.Length; i++)
					tags[i] = new Tag(value[i]);
			}
			get
			{
				string[] str = new string[tags.Length];
				for (int i = 0; i < str.Length; i++)
					str[i] = tags[i].tagName;
				return str;
			}
		}
		/// <summary>
		/// Whether the game object has any tags
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasTag() => tags != null;
		private Tag[] tags;
		/// <summary>
		/// An extra variable for manipulation
		/// </summary>
		public object Extras { get; set; }
		/// <summary>
		/// The list of child game objects
		/// </summary>
		public List<GameObject> ChildObjects { get; private set; } = [];
		/// <summary>
		/// The logic to run each frame
		/// </summary>
		public abstract void Update();
		/// <summary>
		/// The initialization of the object
		/// </summary>
		public virtual void Start() { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TreeUpdate()
		{
			if (!UpdateEnabled || Disposed)
				return;
			if (ChildrenUpdateFirst && UpdateChildren)
				ChildObjects.ForEach(s => s.TreeUpdate());
			if (Update120F || UpdateIn120)
			{
				if (!BeingUpdated)
					Start();
				Update();
				BeingUpdated = true;
			}
			if (UpdateChildren)
			{
				_ = ChildObjects.RemoveAll(s => s.Disposed);
				if (!ChildrenUpdateFirst)
					ChildObjects.ForEach(s => s.TreeUpdate());
			}
		}

		private static bool Update120F => GameMain.Update120F;
		/// <summary>
		/// Whether the game object is disposed
		/// </summary>
		public bool Disposed { get; private set; } = false;
		/// <summary>
		/// Disposes the object
		/// </summary>
		public virtual void Dispose()
		{
			OnDispose?.Invoke();
			Disposed = true;
			ChildObjects.ForEach(s => s.Dispose());
		}
		/// <summary>
		/// This does not invoke the <see cref="Dispose"/> event
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Kill() => Disposed = true;
		/// <summary>
		/// The event to execute when the object is disposed
		/// </summary>
		public event Action OnDispose;
		/// <summary>
		/// The father game object of this child game object (if any)
		/// </summary>
		public GameObject FatherObject { get; private set; }
		/// <summary>
		/// Adds a child to the current game object
		/// </summary>
		/// <param name="obj"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddChild(GameObject obj)
		{
			ChildObjects.Add(obj);
			obj.FatherObject = this;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void Reverse()
		{
			ChildObjects.ForEach(s => s.Reverse());
			Disposed = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<Entity> GetDrawableTree()
		{
			List<Entity> list = [];
			if (BeingUpdated && this is Entity && (this as Entity).Visible)
				list.Add(this as Entity);
			foreach (GameObject child in ChildObjects)
				list.AddRange(child.GetDrawableTree());
			return list;
		}
	}
	/// <summary>
	/// The Tag class
	/// </summary>
	/// <param name="name"></param>
	public class Tag(string name = null)
	{
		/// <summary>
		/// The name of the tag
		/// </summary>
		public string tagName = name;
	}
	/// <summary>
	/// An entity that has gravitational motion
	/// </summary>
	public class GravityEntity() : Entity
	{
		/// <summary>
		/// The gravitational force
		/// </summary>
		public float Gravity = 0;
		/// <summary>
		/// The direction of the gravity (Default downwards)
		/// </summary>
		public float GravityDirection = 90;
		/// <summary>
		/// The speed of the entity
		/// </summary>
		public vec2 Speed = vec2.Zero;
		public override void Update() => Centre += Speed += MathUtil.GetVector2(Gravity, GravityDirection);
		public override void Draw() { }
	}
}