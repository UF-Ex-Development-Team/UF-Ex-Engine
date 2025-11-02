using Microsoft.Xna.Framework.Input;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex;

public static class MouseSystem
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Initialize() => ScreenSize = CurrentWindow.ClientBounds.Size.ToVector2();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Update()
	{
		lastState = currentState;
		currentState = Mouse.GetState();

		MouseWheelDelta = currentState.ScrollWheelValue - lastState.ScrollWheelValue;
		Moved = (PositionMoved = lastState.Position != currentState.Position) || MathF.Abs(MouseWheelDelta) > 0.1f || IsLeftClick() || IsRightClick();

		Vector2 real = CurrentState.Position.ToVector2();

		Vector2 centre = new Vector2(240 * Aspect, 240) * SurfaceScale;
		Vector2 delta = real - ScreenSize / 2f;

		delta /= MathF.Min(ScreenSize.X / (480f * Aspect * SurfaceScale), ScreenSize.Y / (480f * SurfaceScale));

		Vector2 result = centre + delta;
		if (GameOnFocus)
			TransferredPosition = result;
	}
	private static MouseState currentState, lastState;

	public static bool Moved { get; private set; }
	public static bool PositionMoved { get; private set; }

	public static MouseState CurrentState => currentState;

	public static Vector2 TransferredPosition { get; private set; }
	public static Vector2 ScreenSize { private get; set; } = new Vector2(640, 480);

	public static float MouseWheelDelta { get; private set; } = 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLeftClick() => GameOnFocus && TransferredPosition.Y > 0 && (lastState.LeftButton == ButtonState.Released) && (currentState.LeftButton == ButtonState.Pressed);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLeftDown() => GameOnFocus && TransferredPosition.Y > 0 && currentState.LeftButton == ButtonState.Pressed;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLeftReleasing() => GameOnFocus && (currentState.LeftButton == ButtonState.Released) && (lastState.LeftButton == ButtonState.Pressed);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRightClick() => GameOnFocus && (lastState.RightButton == ButtonState.Released) && (currentState.RightButton == ButtonState.Pressed);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRightDown() => GameOnFocus && currentState.RightButton == ButtonState.Pressed;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRightReleasing() => GameOnFocus && (currentState.RightButton == ButtonState.Released) && (lastState.RightButton == ButtonState.Pressed);
}