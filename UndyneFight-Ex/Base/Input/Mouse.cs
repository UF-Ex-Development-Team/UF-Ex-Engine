using Microsoft.Xna.Framework.Input;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex;
/// <summary>
/// The mouse input system
/// </summary>
public static class MouseSystem
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Initialize() => ScreenSize = CurrentWindow.ClientBounds.Size.ToVector2();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Update()
	{
		lastState = currentState;
		currentState = Mouse.GetState();

		MouseWheelDelta = currentState.ScrollWheelValue - lastState.ScrollWheelValue;
		Moved = (PositionMoved = lastState.Position != currentState.Position) || MathF.Abs(MouseWheelDelta) > 0.1f || IsLeftClick() || IsRightClick();

		if (GameOnFocus)
		{
			Vector2 real = CurrentState.Position.ToVector2();

			Vector2 centre = new Vector2(240 * Aspect, 240) * SurfaceScale;
			Vector2 delta = (real - ScreenSize / 2f) / float.Min(ScreenSize.X / (480f * Aspect * SurfaceScale), ScreenSize.Y / (480f * SurfaceScale));
			TransferredPosition = centre + delta;
		}
	}
	private static MouseState currentState, lastState;
	/// <summary>
	/// Whether the cursor was used (Moved, Wheel scrolled, Clicked)
	/// </summary>
	public static bool Moved { get; private set; }
	/// <summary>
	/// Whether the cursor was moved
	/// </summary>
	public static bool PositionMoved { get; private set; }

	private static MouseState CurrentState => currentState;
	/// <summary>
	/// The position of the cursor with respect to <see cref="ScreenSize"/>
	/// </summary>
	public static Vector2 TransferredPosition { get; private set; }
	/// <summary>
	/// The size of the screen
	/// </summary>
	public static Vector2 ScreenSize { private get; set; } = new Vector2(640, 480);
	/// <summary>
	/// The delta value of mouse scroll wheel
	/// </summary>
	public static float MouseWheelDelta { get; private set; } = 0f;
	/// <summary>
	/// Whether the left mouse button is clicked
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLeftClick() => GameOnFocus && TransferredPosition.Y > 0 && (lastState.LeftButton == ButtonState.Released) && (currentState.LeftButton == ButtonState.Pressed);
	/// <summary>
	/// Whether the left mouse button was held
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLeftDown() => GameOnFocus && TransferredPosition.Y > 0 && currentState.LeftButton == ButtonState.Pressed;
	/// <summary>
	/// Whether the left mouse button is just released
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLeftReleasing() => GameOnFocus && (currentState.LeftButton == ButtonState.Released) && (lastState.LeftButton == ButtonState.Pressed);
	/// <summary>
	/// Whether the right mouse button is clicked
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRightClick() => GameOnFocus && (lastState.RightButton == ButtonState.Released) && (currentState.RightButton == ButtonState.Pressed);
	/// <summary>
	/// Whether the right mouse button was held
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRightDown() => GameOnFocus && currentState.RightButton == ButtonState.Pressed;
	/// <summary>
	/// Whether the right mouse button is just released
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRightReleasing() => GameOnFocus && (currentState.RightButton == ButtonState.Released) && (lastState.RightButton == ButtonState.Pressed);
}