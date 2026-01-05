using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

/// <summary>
/// Simplified easing functions
/// </summary>
public static class SimplifiedEasing
{
	/// <summary>
	/// A virtual easing object that simulates the values extracted from <see cref="CentrePosition"/> and <see cref="Rotation"/> based on the given routes
	/// </summary>
	internal class VirtualEasingObject : GameObject, ICustomMotion
	{
		public VirtualEasingObject() => UpdateIn120 = true;
		public Func<ICustomMotion, Vector2> PositionRoute { get; set; }
		public Func<ICustomMotion, float> RotationRoute { get; set; }
		public float[] RotationRouteParam { get; set; }
		public float[] PositionRouteParam { get; set; }

		public float AppearTime { get; set; } = 0;
		public Vector2 CentrePosition { get; set; }
		public float Rotation { get; set; } = 0;

		public override void Update()
		{
			AppearTime += 0.5f;
			CentrePosition = PositionRoute?.Invoke(this) ?? CentrePosition;
			Rotation = RotationRoute?.Invoke(this) ?? Rotation;
		}
	}
	#region Ease exeuction
	/// <summary>
	/// Runs an action with a <see cref="Vector2"/> as it's variable with the given easing functions
	/// </summary>
	/// <param name="action">The action to apply the easing to</param>
	/// <param name="isAdjust">Whether the value will automatically start from the previous easing result</param>
	/// <param name="funcs">The easing functions</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RunEase(Action<Vector2> action, bool isAdjust, params EaseUnit<Vector2>[] funcs) => EasingUtil.Processor.PushProcess(isAdjust, action, funcs);
	/// <summary>
	/// Runs an action with a <see cref="Vector2"/> as it's variable with the given easing functions
	/// </summary>
	/// <param name="action">The action to apply the easing to</param>
	/// <param name="funcs">The easing functions</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RunEase(Action<Vector2> action, params EaseUnit<Vector2>[] funcs) => RunEase(action, true, funcs);
	/// <summary>
	/// Runs an action with a <see cref="float"/> as it's variable with the given easing functions
	/// </summary>
	/// <param name="action">The action to apply the easing to</param>
	/// <param name="isAdjust">Whether the value will automatically start from the previous easing result</param>
	/// <param name="funcs">The easing functions</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RunEase(Action<float> action, bool isAdjust, params EaseUnit<float>[] funcs) => EasingUtil.Processor.PushProcess(isAdjust, action, funcs);
	/// <summary>
	/// Runs an action with a <see cref="float"/> as it's variable with the given easing functions
	/// </summary>
	/// <param name="action">The action to apply the easing to</param>
	/// <param name="funcs">The easing functions</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RunEase(Action<float> action, params EaseUnit<float>[] funcs) => RunEase(action, true, funcs);
	/// <summary>
	/// Links multiple easing functions into an <see cref="EaseUnit{T}"/> where T is float
	/// </summary>
	/// <param name="isAdjust">Whether the value will automatically start from the previous easing result</param>
	/// <param name="funcs">The easing functions to link</param>
	/// <returns>The linked easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static EaseUnit<float> LinkEase(bool isAdjust, params EaseUnit<float>[] funcs)
	{
		float time = funcs[0].Time;
		VirtualEasingObject easingObject = new();

		int len = funcs.Length;
		float[] startTimes = new float[len];
		startTimes[0] = 0;
		for (int i = 1; i < len; i++)
		{
			startTimes[i] = startTimes[i - 1] + funcs[i - 1].Time;
			time += funcs[i].Time;
		}
		float[] basis = new float[len + 1];
		basis[0] = funcs[0].Start;
		if (isAdjust)
		{
			for (int i = 0; i <= len - 1; i++)
				basis[i + 1] = basis[i] + funcs[i].End - funcs[i].Start;
		}
		else
			basis[^1] = funcs[^1].End;
		int curProgress = 0;
		float baseTime = 0;
		float easeResult(ICustomMotion s)
		{
			easingObject.AppearTime += s.AppearTime - baseTime;
			baseTime = s.AppearTime;
			while (curProgress < len && easingObject.AppearTime >= funcs[curProgress].Time)
				easingObject.AppearTime -= funcs[curProgress++].Time;
			while (curProgress > 0 && easingObject.AppearTime < 0)
				easingObject.AppearTime += funcs[--curProgress].Time;
			return curProgress >= len ? basis[^1]
				: isAdjust ? funcs[curProgress].Easing(easingObject) - funcs[curProgress].Start + basis[curProgress]
				: funcs[curProgress].Easing(easingObject);
		}
		return new(funcs[0].Start, basis[^1], time, easeResult);
	}
	/// <summary>
	/// Links multiple easing functions into an <see cref="EaseUnit{T}"/> where T is float
	/// </summary>
	/// <param name="funcs">The easing functions to link</param>
	/// <returns>The linked easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> LinkEase(params EaseUnit<float>[] funcs) => LinkEase(true, funcs);
	/// <summary>
	/// Links multiple easing functions into an <see cref="EaseUnit{Vector2}"/>
	/// </summary>
	/// <param name="isAdjust">Whether the value will automatically start from the previous easing result</param>
	/// <param name="funcs">The easing functions to link</param>
	/// <returns>The linked easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static EaseUnit<Vector2> LinkEase(bool isAdjust = true, params EaseUnit<Vector2>[] funcs)
	{
		float time = funcs[0].Time;
		VirtualEasingObject easingObject = new();

		int len = funcs.Length;
		float[] startTimes = new float[len];
		startTimes[0] = 0;
		for (int i = 1; i < len; i++)
		{
			startTimes[i] = startTimes[i - 1] + funcs[i - 1].Time;
			time += funcs[i].Time;
		}
		Vector2[] basis = new Vector2[len + 1];
		basis[0] = funcs[0].Start;
		if (isAdjust)
		{
			for (int i = 0; i <= len - 1; i++)
				basis[i + 1] = basis[i] + funcs[i].End - funcs[i].Start;
		}
		else
			basis[^1] = funcs[^1].End;
		int curProgress = 0;
		float baseTime = 0;
		vec2 easeResult(ICustomMotion s)
		{
			easingObject.AppearTime += s.AppearTime - baseTime;
			baseTime = s.AppearTime;
			while (curProgress < len && easingObject.AppearTime >= funcs[curProgress].Time)
				easingObject.AppearTime -= funcs[curProgress++].Time;
			while (curProgress > 0 && easingObject.AppearTime < 0)
				easingObject.AppearTime += funcs[--curProgress].Time;
			return curProgress >= len ? basis[^1]
				: isAdjust ? funcs[curProgress].Easing(easingObject) - funcs[curProgress].Start + basis[curProgress]
				: funcs[curProgress].Easing(easingObject);
		}
		return new(funcs[0].Start, basis[^1], time, easeResult);
	}
	/// <summary>
	/// Links multiple easing functions into an <see cref="EaseUnit{Vector2}"/>
	/// </summary>
	/// <param name="funcs">The easing functions to link</param>
	/// <returns>The linked easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> LinkEase(params EaseUnit<Vector2>[] funcs) => LinkEase(true, funcs);
	#endregion
	/// <summary>
	/// Modes of easing to apply on easing functions, see <see href="https://easings.net/"/> for more information
	/// </summary>
	public enum EaseState
	{
		/// <summary>
		/// Linear easing
		/// </summary>
		Linear = 0,
		/// <summary>
		/// Quadratic easing
		/// </summary>
		Quad = 1,
		/// <summary>
		/// Cubic easing
		/// </summary>
		Cubic = 2,
		/// <summary>
		/// Quartic easing
		/// </summary>
		Quart = 3,
		/// <summary>
		/// Quintic easing
		/// </summary>
		Quint = 4,
		/// <summary>
		/// Circular easing
		/// </summary>
		Circ = 5,
		/// <summary>
		/// Sine easing
		/// </summary>
		Sine = 6,
		/// <summary>
		/// Oscillation-like easing
		/// </summary>
		Elastic = 7,
		/// <summary>
		/// Exponential easing
		/// </summary>
		Expo = 8,
		/// <summary>
		/// Easing where it sightly retracts before moving to the target
		/// </summary>
		Back = 9,
		/// <summary>
		/// Easing where it bounces near the end of the easing
		/// </summary>
		Bounce = 10
	}
	#region Linear easing
	//Note: Do not convert 99999.0f into float.maxValue
	/// <summary>
	/// Returns an infinite linear movement
	/// </summary>
	/// <param name="start">The initial value</param>
	/// <param name="speed">The speed of the increment of the value</param>
	/// <returns>The easing result</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> InfLinear(float start, float speed) => new(start, start, 99999.0f, (s) => start + s.AppearTime * speed);
	/// <summary>
	/// Returns an infinite linear movement
	/// </summary>
	/// <param name="start">The initial position</param>
	/// <param name="speed">The speed of the movement</param>
	/// <returns>The easing result</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> InfLinear(Vector2 start, Vector2 speed) => new(start, start, 99999.0f, (s) => start + s.AppearTime * speed);
	/// <summary>
	/// Returns an infinite linear movement (Default position (0, 0))
	/// </summary>
	/// <param name="speed">The speed of the movement</param>
	/// <returns>The easing result</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> InfLinear(Vector2 speed) => new(Vector2.Zero, Vector2.Zero, 99999.0f, (s) => s.AppearTime * speed);
	/// <summary>
	/// Returns a linear motion
	/// </summary>
	/// <param name="time">The duration of the linear movement</param>
	/// <param name="start">The initial position of the movement</param>
	/// <param name="end">The target position of the movement</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Linear(float time, Vector2 start, Vector2 end) => new(start, end, time, (x) => Vector2.Lerp(start, end, x.AppearTime / time));
	/// <summary>
	/// Returns a linear motion <br/>
	/// Note that this will increment <paramref name="end"/> based on the previous easing
	/// </summary>
	/// <param name="time">The duration of the linear movement</param>
	/// <param name="end">The target position of the movement</param>
	/// <returns>The easing result</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Linear(float time, Vector2 end) => Linear(time, Vector2.Zero, end);
	/// <summary>
	/// Returns a linear easing
	/// </summary>
	/// <param name="time">The duration of the linear easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Linear(float time, float start, float end) => new(start, end, time, (x) => float.Lerp(start, end, x.AppearTime / time));
	/// <summary>
	/// Returns a linear easing <br/>
	/// Note that this will increment <paramref name="end"/> based on the previous easing
	/// </summary>
	/// <param name="time">The duration of the linear easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Linear(float time, float end) => Linear(time, 0, end);
	#endregion
	#region Vector2 Easing
	/// <summary>
	/// Returns an easing function that starts slow and ends fast
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<Vector2> EaseIn(float time, Vector2 start, Vector2 end, EaseState state) => new(start, end, time, (x) => Vector2.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, state, EaseLibrary.EaseMode.In)));
	/// <summary>
	/// Returns an easing function that starts fast and ends slow
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<Vector2> EaseOut(float time, Vector2 start, Vector2 end, EaseState state) => new(start, end, time, (x) => Vector2.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, state, EaseLibrary.EaseMode.Out)));
	/// <summary>
	/// Returns an easing function that starts slow and ends fast <br/>
	/// Note that this will increment <paramref name="end"/> based on the previous easing
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> EaseIn(float time, Vector2 end, EaseState state) => EaseIn(time, Vector2.Zero, end, state);
	/// <summary>
	/// Returns an easing function that starts fast and ends slow <br/>
	/// Note that this will increment <paramref name="end"/> based on the previous easing
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> EaseOut(float time, Vector2 end, EaseState state) => EaseOut(time, Vector2.Zero, end, state);
	/// <summary>
	/// Returns an easing function that starts slow, increases in speed, then becomes slow again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="amount">The proportion of the distance between the starting point and the midpoint<br/>For example: 0.4f means that the starting point easing distance accounts for 40% and the midpoint easing distance accounts for 60%<br/>But the easing time ratio is still 1:1</param>
	/// <param name="Astate">Initial easing type</param>
	/// <param name="Bstate">Ending easing type (Default <paramref name="Astate"/>)</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<Vector2> EaseInOut(float time, Vector2 start, Vector2 end, float amount, EaseState Astate, EaseState? Bstate = null) => new(start, end, time, (x) => Vector2.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, x.AppearTime / time < amount ? Astate : Bstate ?? Astate, EaseLibrary.EaseMode.InOut)));
	/// <summary>
	/// Returns an easing function that starts fast, decreases in speed, then becomes fast again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="amount">The proportion of the distance between the starting point and the midpoint<br/>For example: 0.4f means that the starting point easing distance accounts for 40% and the midpoint easing distance accounts for 60%<br/>But the easing time ratio is still 1:1</param>
	/// <param name="Astate">Initial easing type</param>
	/// <param name="Bstate">Ending easing type (Default <paramref name="Astate"/>)</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<Vector2> EaseOutIn(float time, Vector2 start, Vector2 end, float amount, EaseState Astate, EaseState? Bstate = null) => new(start, end, time, (x) => Vector2.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, x.AppearTime / time < amount ? Astate : Bstate ?? Astate, EaseLibrary.EaseMode.OutIn)));
	/// <summary>
	/// Returns an easing function that starts slow, increases in speed, then becomes slow again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used in the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> EaseInOut(float time, Vector2 start, Vector2 end, EaseState state) => EaseInOut(time, start, end, 0.5f, state, state);
	/// <summary>
	/// Returns an easing function that starts fast, decreases in speed, then becomes fast again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used in the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> EaseOutIn(float time, Vector2 start, Vector2 end, EaseState state) => EaseOutIn(time, start, end, 0.5f, state, state);
	#endregion
	#region Float Easing
	/// <summary>
	/// Returns an easing function that starts slow and ends fast
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<float> EaseIn(float time, float start, float end, EaseState state) => new(start, end, time, (x) => float.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, state, EaseLibrary.EaseMode.In)));
	/// <summary>
	/// Returns an easing function that starts fast and ends slow
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<float> EaseOut(float time, float start, float end, EaseState state) => new(start, end, time, (x) => float.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, state, EaseLibrary.EaseMode.Out)));
	/// <summary>
	/// Returns an easing function that starts slow and ends fast <br/>
	/// Note that this will increment <paramref name="end"/> based on the previous easing
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> EaseIn(float time, float end, EaseState state) => EaseIn(time, 0, end, state);
	/// <summary>
	/// Returns an easing function that starts fast and ends slow <br/>
	/// Note that this will increment <paramref name="end"/> based on the previous easing
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> EaseOut(float time, float end, EaseState state) => EaseOut(time, 0, end, state);
	/// <summary>
	/// Returns an easing function that starts slow, increases in speed, then becomes slow again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="amount">The proportion of the distance between the starting point and the midpoint<br/>For example: 0.4f means that the starting point easing distance accounts for 40% and the midpoint easing distance accounts for 60%<br/>But the easing time ratio is still 1:1</param>
	/// <param name="Astate">Initial easing type</param>
	/// <param name="Bstate">Ending easing type (Default <paramref name="Astate"/>)</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<float> EaseInOut(float time, float start, float end, float amount, EaseState Astate, EaseState? Bstate = null) => new(start, end, time, (x) => float.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, x.AppearTime / time < amount ? Astate : Bstate ?? Astate, EaseLibrary.EaseMode.InOut)));
	/// <summary>
	/// Returns an easing function that starts fast, decreases in speed, then becomes fast again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="amount">The proportion of the distance between the starting point and the midpoint<br/>For example: 0.4f means that the starting point easing distance accounts for 40% and the midpoint easing distance accounts for 60%<br/>But the easing time ratio is still 1:1</param>
	/// <param name="Astate">Initial easing type</param>
	/// <param name="Bstate">Ending easing type (Default <paramref name="Astate"/>)</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<float> EaseOutIn(float time, float start, float end, float amount, EaseState Astate, EaseState? Bstate = null) => new(start, end, time, (x) => float.Lerp(start, end, EaseLibrary.GetValue(x.AppearTime / time, x.AppearTime / time < amount ? Astate : Bstate ?? Astate, EaseLibrary.EaseMode.OutIn)));
	/// <summary>
	/// Returns an easing function that starts slow, increases in speed, then becomes slow again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> EaseInOut(float time, float start, float end, EaseState state) => EaseInOut(time, start, end, 0.5f, state, state);
	/// <summary>
	/// Returns an easing function that starts fast, decreases in speed, then becomes fast again
	/// </summary>
	/// <param name="time">The duration of the easing</param>
	/// <param name="start">The initial value of the easing</param>
	/// <param name="end">The target value of the easing</param>
	/// <param name="state">The type of easing used for the easing function</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> EaseOutIn(float time, float start, float end, EaseState state) => EaseOutIn(time, start, end, 0.5f, state, state);
	/// <summary>
	/// Returns a stable value for the specified amount of time
	/// </summary>
	/// <param name="time">The duration of the stability of the value (Use 0 for instantly setting the value)</param>
	/// <param name="value">The value to set to</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Stable(float time, Vector2 value) => new(value, value, time, (s) => value);
	/// <summary>
	/// Returns a stable value for the specified amount of time, essentially the same with <see cref="Stable(float, Vector2)"/>
	/// </summary>
	/// <param name="time">The duration of the stability of the value (Use 0 for instantly setting the value)</param>
	/// <param name="xvalue">The first value to set to</param>
	/// <param name="yvalue">The second value to set to</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Stable(float time, float xvalue, float yvalue) => new(new Vector2(xvalue, yvalue), new Vector2(xvalue, yvalue), time, (s) => new Vector2(xvalue, yvalue));
	/// <summary>
	/// Returns a stable value for the specified amount of time
	/// </summary>
	/// <param name="time">The duration of the stability of the value (Use 0 for instantly setting the value)</param>
	/// <param name="value">The value to set to (Default 0)</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Stable(float time, float value = 0) => new(value, value, time, (s) => value);
	#endregion
	#region Misc. Easing Functions
	/// <summary>
	/// Returns a copy of the specified easing for the given amount of times
	/// </summary>
	/// <param name="ease">The easing function to copy</param>
	/// <param name="times">The amount of times needed to copy</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Copy(EaseUnit<Vector2> ease, int times)
	{
		EaseUnit<Vector2>[] easeUnits = new EaseUnit<Vector2>[times];
		for (int i = 0; i < times; i++)
			easeUnits[i] = new(ease.Start, ease.End, ease.Time, ease.Easing);
		return LinkEase(false, easeUnits);
	}
	/// <summary>
	/// Returns a copy of the specified easing for the given amount of times
	/// </summary>
	/// <param name="ease">The easing function to copy</param>
	/// <param name="times">The amount of times needed to copy</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Copy(EaseUnit<float> ease, int times)
	{
		EaseUnit<float>[] easeUnits = new EaseUnit<float>[times];
		for (int i = 0; i < times; i++)
			easeUnits[i] = new(ease.Start, ease.End, ease.Time, ease.Easing);
		return LinkEase(false, easeUnits);
	}
	/// <summary>
	/// Alternates between the given easing functions
	/// </summary>
	/// <param name="interval">The interval between each easing function</param>
	/// <param name="main">The main easing function</param>
	/// <param name="addons">The additional easing functions to alternate</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Alternate(float interval, EaseUnit<Vector2> main, params EaseUnit<Vector2>[] addons)
	{
		float curTime = 0;
		int curProgress = -1;
		return new(main.Start, main.End, main.Time, (s) =>
		{
			curTime += 0.5f;
			if (curTime > interval)
			{
				curTime -= interval;
				curProgress++;
			}
			if (curProgress == addons.Length)
				curProgress = -1;
			return curProgress == -1 ? main.Easing(s) : addons[curProgress].Easing(s);
		});
	}
	/// <summary>
	/// Alternates between the given easing functions
	/// </summary>
	/// <param name="interval">The interval between each easing function</param>
	/// <param name="main">The main easing function</param>
	/// <param name="addons">The additional easing functions to alternate</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Alternate(float interval, EaseUnit<float> main, params EaseUnit<float>[] addons)
	{
		float curTime = 0;
		int curProgress = -1;
		return new(main.Start, main.End, main.Time, (s) =>
		{
			curTime += 0.5f;
			if (curTime > interval)
			{
				curTime -= interval;
				curProgress++;
			}
			if (curProgress == addons.Length)
				curProgress = -1;
			return curProgress == -1 ? main.Easing(s) : addons[curProgress].Easing(s);
		});
	}
	/// <summary>
	/// Returns the result of two <see cref="Vector2"/> easing functions
	/// </summary>
	/// <param name="main">The original easing function</param>
	/// <param name="addon">The easing function to add</param>
	/// <returns>The sum of the two easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Add(EaseUnit<Vector2> main, EaseUnit<Vector2> addon) => new(main.Start + addon.Start, main.End + addon.End, main.Time, (s) => main.Easing(s) + addon.Easing(s));
	/// <summary>
	/// Returns the result of a <see cref="Vector2"/> easing function and a <see cref="Vector2"/>
	/// </summary>
	/// <param name="main">The original easing function</param>
	/// <param name="addon">The displacement to add</param>
	/// <returns>The sum of the easing function and vector</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Add(EaseUnit<Vector2> main, Vector2 addon) => new(main.Start + addon, main.End + addon, main.Time, (s) => main.Easing(s) + addon);
	/// <summary>
	/// Scales the <see cref="Vector2"/> easing by a <see cref="float"/> easing function
	/// </summary>
	/// <param name="origin">The vector easing to scale to</param>
	/// <param name="scalar">The float easing to scale</param>
	/// <returns>The result of the easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Scale(EaseUnit<Vector2> origin, EaseUnit<float> scalar) => new(origin.Start * scalar.Start, origin.End * scalar.End, origin.Time, (s) => origin.Easing(s) * scalar.Easing(s));
	/// <summary>
	/// Scales the <see cref="Vector2"/> easing by a scalar float value
	/// </summary>
	/// <param name="origin">The vector easing to scale to</param>
	/// <param name="scalar">The float to scale</param>
	/// <returns>The result of the scaled easing function</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Scale(EaseUnit<Vector2> origin, float scalar) => new(origin.Start * scalar, origin.End * scalar, origin.Time, (s) => origin.Easing(s) * scalar);
	/// <summary>
	/// Scales the <see cref="float"/> easing by a <see cref="float"/> easing function
	/// </summary>
	/// <param name="origin">The float easing to scale to</param>
	/// <param name="scalar">The float easing to scale</param>
	/// <returns>The result of the easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Scale(EaseUnit<float> origin, EaseUnit<float> scalar) => new(origin.Start * scalar.Start, origin.End * scalar.End, origin.Time, (s) => origin.Easing(s) * scalar.Easing(s));
	/// <summary>
	/// Scales the <see cref="float"/> easing by a scalar float value
	/// </summary>
	/// <param name="origin">The float easing to scale to</param>
	/// <param name="scalar">The float to scale</param>
	/// <returns>The result of the scaled easing function</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Scale(EaseUnit<float> origin, float scalar) => new(origin.Start * scalar, origin.End * scalar, origin.Time, (s) => origin.Easing(s) * scalar);
	/// <summary>
	/// Returns a easing of a rotating <see cref="Vector2"/> easing
	/// </summary>
	/// <param name="main">The vector easing to rotate</param>
	/// <param name="rotate">The easing of the rotation</param>
	/// <returns>The result of the easing function</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Polar(EaseUnit<Vector2> main, EaseUnit<float> rotate) => new(Rotate(main.Start, rotate.Start), Rotate(main.End, rotate.End), main.Time, (s) => Rotate(main.Easing(s), rotate.Easing(s)));
	/// <summary>
	/// Returns a easing of a rotating <see cref="float"/> easing
	/// </summary>
	/// <param name="main">The float easing to rotate</param>
	/// <param name="rotate">The easing of the rotation</param>
	/// <returns>The result of the easing function</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Polar(EaseUnit<float> main, EaseUnit<float> rotate) => new(GetVector2(main.Start, rotate.Start), GetVector2(main.End, rotate.End), main.Time, (s) => GetVector2(main.Easing(s), rotate.Easing(s)));
	/// <summary>
	/// Returns a easing of a rotating <see cref="Vector2"/> easing
	/// </summary>
	/// <param name="main">The vector easing to rotate</param>
	/// <param name="rotate">The rotation to set to</param>
	/// <returns>The result of the easing function</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Polar(EaseUnit<Vector2> main, float rotate) => new(Rotate(main.Start, rotate), Rotate(main.End, rotate), main.Time, (s) => Rotate(main.Start, rotate));
	/// <summary>
	/// Returns the result of two <see cref="float"/> easing functions
	/// </summary>
	/// <param name="main">The original easing function</param>
	/// <param name="addon">The easing function to add</param>
	/// <returns>The sum of the two easing functions</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Add(EaseUnit<float> main, EaseUnit<float> addon) => new(main.Start + addon.Start, main.End + addon.End, main.Time, (s) => main.Easing(s) + addon.Easing(s));
	/// <summary>
	/// Returns the result of a <see cref="float"/> easing function and a float value
	/// </summary>
	/// <param name="main">The original easing function</param>
	/// <param name="addon">The float value to add</param>
	/// <returns>The sum of the easing function and the float value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> Add(EaseUnit<float> main, float addon) => new(main.Start + addon, main.End + addon, main.Time, (s) => main.Easing(s) + addon);
	/// <summary>
	/// Returns a <see cref="Vector2"/> easing with the given <paramref name="xEase"/> and <paramref name="yEase"/> as the x and y components of the <see cref="Vector2"/> easing function
	/// </summary>
	/// <param name="xEase">The easing of the x value (Use <see cref="Stable(float, float)"/> for stable value</param>
	/// <param name="yEase">The easing of the y value (Use <see cref="Stable(float, float)"/> for stable value</param>
	/// <returns>The combined easing function</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> Combine(EaseUnit<float> xEase, EaseUnit<float> yEase) =>
		new(new(xEase.Start, yEase.Start), new(xEase.End, yEase.End), MathF.Max(xEase.Time, yEase.Time), (s) => new(xEase.Easing(s), yEase.Easing(s)));
	/// <summary>
	/// Returns a sine wave easing
	/// </summary>
	/// <param name="start">The minimal value of the wave</param>
	/// <param name="end">The maximum value of the wave</param>
	/// <param name="T">The period of the wave</param>
	/// <param name="waveCount">The amount of times the wave will run (Default 99999)</param>
	/// <param name="phase">The initial position (Default 0)</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> SineWave(Vector2 start, Vector2 end, float T, float waveCount = 99999, float phase = 0)
	{
		vec2 sine(float t) => Vector2.Lerp(start, end, MathF.Sin((t / T + phase) * MathF.PI * 2) * 0.5f + 0.5f);
		return new((start + end) / 2f, sine(waveCount), waveCount * T, (s) => sine(s.AppearTime));
	}
	/// <summary>
	/// Returns a sine wave easing
	/// </summary>
	/// <param name="impact">The magnitude of the wave</param>
	/// <param name="period">The period of the wave</param>
	/// <param name="waveCount">The amount of times the wave will run (Default 99999)</param>
	/// <param name="phase">The initial position (Default 0)</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<Vector2> SineWave(Vector2 impact, float period, float waveCount = 99999, float phase = 0)
	{
		vec2 sine(float time) => Vector2.Lerp(-impact, impact, MathF.Sin((time / period + phase) * MathF.PI * 2) * 0.5f + 0.5f);
		return new(Vector2.Zero, sine(waveCount), waveCount * period, (s) => sine(s.AppearTime));
	}
	/// <summary>
	/// Returns a sine wave easing
	/// </summary>
	/// <param name="amplitude">The amplitude of the wave</param>
	/// <param name="period">The period of the wave</param>
	/// <param name="waveCount">The amount of times the wave will run (Default 99999)</param>
	/// <param name="phase">The initial position (Default 0)</param>
	/// <returns>The result of the easing</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EaseUnit<float> SineWave(float amplitude, float period, float waveCount = 99999, float phase = 0)
	{
		float sine(float time) => float.Lerp(-amplitude, amplitude, MathF.Sin((time / period + phase) * MathF.PI * 2) * 0.5f + 0.5f);
		return new(0, sine(waveCount), waveCount * period, (s) => sine(s.AppearTime));
	}
	/// <summary>
	/// An accelerating easing
	/// </summary>
	/// <param name="time">The time of the acceleration</param>
	/// <param name="speed">The speed of the value change</param>
	/// <param name="acceleration">The acceleration of the speed</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<float> Accelerating(float time, float speed, float acceleration) => new(0, speed * time + acceleration * 0.5f * time * time, time, (s) => speed * s.AppearTime + acceleration * 0.5f * s.AppearTime * s.AppearTime);
	/// <summary>
	/// An accelerating easing
	/// </summary>
	/// <param name="time">The time of the acceleration</param>
	/// <param name="start">The initial value</param>
	/// <param name="speed">The speed of the value change</param>
	/// <param name="acceleration">The acceleration of the speed</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<float> Accelerating(float time, float start, float speed, float acceleration) => new(start, start + speed * time + acceleration * 0.5f * time * time, time, (s) => start + speed * s.AppearTime + acceleration * 0.5f * s.AppearTime * s.AppearTime);
	/// <summary>
	/// An accelerating easing
	/// </summary>
	/// <param name="time">The time of the acceleration</param>
	/// <param name="speed">The speed of the value change</param>
	/// <param name="acceleration">The acceleration of the speed</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<Vector2> Accelerating(float time, Vector2 speed, Vector2 acceleration) => new(Vector2.Zero, speed * time + acceleration * 0.5f * time * time, time, (s) => speed * s.AppearTime + acceleration * 0.5f * s.AppearTime * s.AppearTime);
	/// <summary>
	/// An accelerating easing
	/// </summary>
	/// <param name="time">The time of the acceleration</param>
	/// <param name="start">The initial value</param>
	/// <param name="speed">The speed of the value change</param>
	/// <param name="acceleration">The acceleration of the speed</param>
	/// <returns>The result of the easing</returns>
	public static EaseUnit<Vector2> Accelerating(float time, Vector2 start, Vector2 speed, Vector2 acceleration) => new(start, start + speed * time + acceleration * 0.5f * time * time, time, (s) => start + speed * s.AppearTime + acceleration * 0.5f * s.AppearTime * s.AppearTime);
	#endregion
}
/// <summary>
/// A unit of easing motion between two values over a specified duration.
/// </summary>
/// <typeparam name="T">The type of the values being eased between. (It should either be a <see cref="float"/> or <see cref="Vector2"/></typeparam>
/// <param name="start">The initial value of the easing</param>
/// <param name="end">The destination value of the easing</param>
/// <param name="time">The duration of the easing</param>
/// <param name="easing">The easing function</param>
public struct EaseUnit<T>(T start, T end, float time, Func<ICustomMotion, T> easing)
{
	/// <summary>
	/// The duration of the easing
	/// </summary>
	public float Time = time;
	/// <summary>
	/// The initial value of the easing
	/// </summary>
	public T Start { get; init; } = start;
	/// <summary>
	/// The destination value of the easing
	/// </summary>
	public T End { get; init; } = end;
	/// <summary>
	/// The easing function
	/// </summary>
	public Func<ICustomMotion, T> Easing = easing;
	/// <summary>
	/// Implicit conversion to the easing function
	/// </summary>
	/// <param name="u"></param>

	public static implicit operator Func<ICustomMotion, T>(EaseUnit<T> u) => u.Easing;
	/// <inheritdoc/>
	public override readonly string ToString() => $"EaseUnit<{typeof(T).Name}>({Start} -> {End} in {Time}f)";
}
/// <summary>
/// Easing utilities that support <see cref="SimplifiedEasing"/>, however it is better to use <see cref="SimplifiedEasing"/> for better readability
/// </summary>
public static class EasingUtil
{
	/// <summary>
	/// Easing library, note that they run at 125fps<br/>
	/// The functions are not documented as they are replaced by <see cref="SimplifiedEasing"/> functions, or are not useful except for a few occasions.
	/// </summary>
	/// <remarks>This class will be removed in UF-Ex once it has been completely removed in Rhythm Recall</remarks>
	[Obsolete("SimplifiedEasing is better")]
	public static class CentreEasing
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Stable(Vector2 position) => (s) => position;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Stable(float x, float y) => (s) => new(x, y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Linear(Vector2 speed) =>
			(s) => s.AppearTime * speed;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Linear(float Xspeed) =>
			(s) => s.AppearTime * new Vector2(Xspeed, 0);
		public static Func<ICustomMotion, Vector2> FromDown(float distance, float time) =>
			(s) =>
			{
				float cur = Math.Max(0, time - s.AppearTime) / time;
				return new Vector2(0, cur * cur) * distance;
			};
		public static Func<ICustomMotion, Vector2> FromUp(float distance, float time) =>
			(s) =>
			{
				float cur = Math.Max(0, time - s.AppearTime) / time;
				return new Vector2(0, -cur * cur) * distance;
			};
		public static Func<ICustomMotion, Vector2> FromRight(float distance, float time) =>
			(s) =>
			{
				float cur = Math.Max(0, time - s.AppearTime) / time;
				return new Vector2(cur * cur, 0) * distance;
			};
		public static Func<ICustomMotion, Vector2> FromLeft(float distance, float time) =>
			(s) =>
			{
				float cur = Math.Max(0, time - s.AppearTime) / time;
				return new Vector2(-cur * cur, 0) * distance;
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Circle(Vector2 centre, float radius, float roundTime, float startingRotation) => (s) => centre + GetVector2(radius, s.AppearTime / roundTime * 360f + startingRotation);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Convert(Func<float, Vector2> timeParamEase) => (s) => timeParamEase(s.AppearTime);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Circle(Func<ICustomMotion, Vector2> easing, float radius, float roundTime, float startingRotation) => (s) => easing(s) + GetVector2(radius, s.AppearTime / roundTime * 360f + startingRotation);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Circle(Func<ICustomMotion, Vector2> easing, Func<ICustomMotion, float> radius, float roundTime, float startingRotation) => (s) => easing(s) + GetVector2(radius(s), s.AppearTime / roundTime * 360f + startingRotation);

		/// <summary>
		/// 构建一个摆动的正弦波的缓动
		/// </summary>
		/// <param name="intensity">振幅</param>
		/// <param name="cycleTime">每个波占的时间，即周期</param>
		/// <param name="startPhase">初始位置在第一个半波里面的比例位置。例如写0.5即从第一个半波的一半位置开始。</param>
		/// <param name="rotation">摆动方向</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> SinWave(float intensity, float cycleTime, float startPhase, float rotation) => (s) => GetVector2(Sin01(s.AppearTime * 2 / cycleTime + startPhase) * intensity, rotation);
		/// <summary>
		/// 构建一个上下摆动的正弦波的缓动
		/// </summary>
		/// <param name="intensity">振幅</param>
		/// <param name="cycleTime">每个波占的时间，即周期</param>
		/// <param name="startPhase">初始位置在第一个半波里面的比例位置。例如写0.5即从第一个半波的一半位置开始。</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> XSinWave(float intensity, float cycleTime, float startPhase) => (s) => new(Sin01(s.AppearTime * 2 / cycleTime + startPhase) * intensity, 0);
		/// <summary>
		/// 构建一个左右摆动的正弦波的缓动
		/// </summary>
		/// <param name="intensity">振幅</param>
		/// <param name="cycleTime">每个波占的时间，即周期</param>
		/// <param name="startPhase">初始位置在第一个半波里面的比例位置。例如写0.5即从第一个半波的一半位置开始。</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> YSinWave(float intensity, float cycleTime, float startPhase) => (s) => new(0, Sin01(s.AppearTime * 2 / cycleTime + startPhase) * intensity);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Accelerating(Vector2 speed, Vector2 acceleration) => (s) => speed * s.AppearTime + acceleration * (0.5f * s.AppearTime * s.AppearTime);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Linear(Vector2 v1, Vector2 v2, float time) => (s) => Vector2.Lerp(v1, v2, s.AppearTime / time);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> EaseInSine(Vector2 v1, Vector2 v2, float time) => (s) => Vector2.Lerp(v1, v2, 1 - MathF.Cos(s.AppearTime / time * MathF.PI / 2));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> EaseOutSine(Vector2 v1, Vector2 v2, float time) => (s) => Vector2.Lerp(v1, v2, MathF.Sin(s.AppearTime / time * MathF.PI / 2));
		public static Func<ICustomMotion, Vector2> EaseInQuad(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, scale * scale);
			};
		public static Func<ICustomMotion, Vector2> EaseOutQuad(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale));
			};
		public static Func<ICustomMotion, Vector2> EaseInCubic(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, scale * scale * scale);
			};
		public static Func<ICustomMotion, Vector2> EaseOutCubic(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale) * (1 - scale));
			};
		public static Func<ICustomMotion, Vector2> EaseInQuart(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, scale * scale * scale * scale);
			};
		public static Func<ICustomMotion, Vector2> EaseInOutQuart(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;

				Vector2 V1 = v2 * 0.5f;
				return scale <= 0.49f ? EaseInQuart(v1, V1, time * 0.5f).Invoke(s) : EaseOutQuart(V1, v2, time * 0.5f).Invoke(s);
			};
		public static Func<ICustomMotion, Vector2> EaseInOutQuad(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;

				Vector2 V1 = v2 * 0.5f;
				return scale <= 0.49f ? EaseInQuad(v1, V1, time * 0.5f).Invoke(s) : EaseOutQuad(V1, v2, time * 0.5f).Invoke(s);
			};
		public static Func<ICustomMotion, Vector2> EaseOutQuart(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale) * (1 - scale) * (1 - scale));
			};
		public static Func<ICustomMotion, Vector2> EaseInQuint(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, scale * scale * scale * scale * scale);
			};
		public static Func<ICustomMotion, Vector2> EaseOutQuint(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale) * (1 - scale) * (1 - scale) * (1 - scale));
			};
		public static Func<ICustomMotion, Vector2> EaseInExpo(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, x == 0 ? 0 : MathF.Pow(2, 10 * x - 10));
			};
		public static Func<ICustomMotion, Vector2> EaseOutExpo(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, x == 1 ? 1 : 1 - MathF.Pow(2, -10 * x));
			};
		public static Func<ICustomMotion, Vector2> EaseInCirc(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, 1 - MathF.Sqrt(1 - x * x));
			};
		public static Func<ICustomMotion, Vector2> EaseOutCirc(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return Vector2.Lerp(v1, v2, MathF.Sqrt(1 - (1 - x) * (1 - x)));
			};
		public static Func<ICustomMotion, Vector2> EaseInBack(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				float c1 = 1.70158f;
				float c3 = c1 + 1;
				float value = c3 * x * x * x - c1 * x * x;
				return Vector2.Lerp(v1, v2, value);
			};
		public static Func<ICustomMotion, Vector2> EaseOutBack(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time - 1;
				float c1 = 1.70158f;
				float c3 = c1 + 1;
				float value = 1 + c3 * x * x * x + c1 * x * x;
				return Vector2.Lerp(v1, v2, value);
			};
		public static Func<ICustomMotion, Vector2> EaseInElastic(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				float c4 = 2 * MathF.PI / 3;
				float value = x == 0 ? 0 : (x == 1 ? 1 :
					-MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * c4)
				);

				return Vector2.Lerp(v1, v2, value);
			};
		public static Func<ICustomMotion, Vector2> EaseOutElastic(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = 1 - s.AppearTime / time;
				float c4 = 2 * MathF.PI / 3;
				float value = x == 0 ? 0 : (x == 1 ? 1 :
					-MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * c4)
				);
				return Vector2.Lerp(v1, v2, 1 - value);
			};
		public static Func<ICustomMotion, Vector2> EaseInBounce(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = 1 - s.AppearTime / time;
				float n1 = 7.5625f;
				float d1 = 2.75f;
				float value = x < 1 / d1
					? n1 * x * x
					: x < 2 / d1
						? n1 * (float)Math.Pow(x - 1.5f / d1, 2) + 0.75f
						: x < 2.5 / d1 ? n1 * (float)Math.Pow(x - 2.25f / d1, 2) + 0.9375f : n1 * (float)Math.Pow(x - 2.625f / d1, 2) * x + 0.984375f;
				return Vector2.Lerp(v1, v2, 1 - value);
			};
		public static Func<ICustomMotion, Vector2> EaseOutBounce(Vector2 v1, Vector2 v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				float n1 = 7.5625f;
				float d1 = 2.75f;

				float value = x < 1 / d1
					? n1 * x * x
					: x < 2 / d1
						? n1 * (float)Math.Pow(x - 1.5f / d1, 2) + 0.75f
						: x < 2.5 / d1 ? n1 * (float)Math.Pow(x - 2.25f / d1, 2) + 0.9375f : n1 * (float)Math.Pow(x - 2.625f / d1, 2) + 0.984375f;
				return Vector2.Lerp(v1, v2, value);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> LerpTo(Vector2 start, float scale, Func<ICustomMotion, Vector2> origin)
		{
			Vector2 curPos = start;
			return (s) =>
			{
				curPos = Vector2.Lerp(curPos, origin(s), scale);
				return curPos;
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> LerpTo(Vector2 start, float scale, Vector2 origin)
		{
			Vector2 curPos = start;
			return (s) =>
			{
				curPos = Vector2.Lerp(curPos, origin, scale);
				return curPos;
			};
		}
		public static Func<ICustomMotion, Vector2> Alternate(float time, params Func<ICustomMotion, Vector2>[] easings)
		{
			int curPhase = 0;
			float timer = 0;
			time *= 2;
			return (s) =>
			{
				timer++;
				if (timer >= time)
				{
					timer -= time;
					curPhase++;
				}
				if (curPhase >= easings.Length)
					curPhase = 0;
				return easings[curPhase](s);
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Alternate(params Func<ICustomMotion, Vector2>[] easings) => Alternate(1, easings);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Intensify(Func<ICustomMotion, Vector2> easing, Func<ICustomMotion, float> scale) => (s) => easing(s) * scale(s);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Combine(Func<ICustomMotion, Vector2> ease1, Func<ICustomMotion, Vector2> ease2) => (s) => ease1(s) + ease2(s);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Combine(Func<ICustomMotion, Vector2> ease1, Vector2 centre) => (s) => ease1(s) + centre;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Combine(Vector2 centre, Func<ICustomMotion, Vector2> ease1) => (s) => ease1(s) + centre;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> Combine(Func<ICustomMotion, float> xEase, Func<ICustomMotion, float> yEase) => (s) => new(xEase(s), yEase(s));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> PolarCombine(Func<ICustomMotion, Vector2> centreEase, Func<ICustomMotion, float> rotationEase) => (s) => Rotate(centreEase(s), rotationEase(s));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, Vector2> PolarCombine(Func<ICustomMotion, float> lengthEase, Func<ICustomMotion, float> rotationEase) => (s) => GetVector2(lengthEase(s), rotationEase(s));

		public class EaseBuilder
		{
			public static implicit operator Func<ICustomMotion, Vector2>(EaseBuilder val) => val.GetResult();
			public Vector2 OffsetPosition { get; set; }
			public bool Adjust { get; set; } = true;
			private readonly List<(float Time, Func <ICustomMotion, Vector2> Easing)> motionPairs = [];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Insert(float time, Func<ICustomMotion, Vector2> function) => motionPairs.Add(new(time, function));
			public void Run(Action<Vector2> action)
			{
				VirtualEasingObject easer = new();
				AddInstance(easer);
				easer.PositionRoute = GetResult();
				AddInstance(new TimeRangedEvent(_totalTime, () => action(easer.CentrePosition)) { UpdateIn120 = true });
				AddInstance(new InstantEvent(_totalTime, easer.Dispose));
			}
			private float _totalTime;
			public Func<ICustomMotion, Vector2> GetResult()
			{
				VirtualEasingObject obj = new(), objEnd = new();

				(float Time, Func <ICustomMotion, Vector2> Easing)[] pairs = [.. motionPairs];
				int len = pairs.Length;
				float[] timeZone = new float[len];
				float totalTime = 0;
				Vector2[] startings = new Vector2[len];
				Vector2[] endings = new Vector2[len];
				for (int i = 0; i < len; i++)
				{
					totalTime += pairs[i].Time;
					timeZone[i] = totalTime;
					startings[i] = pairs[i].Easing(obj);
					objEnd.AppearTime = pairs[i].Time;
					endings[i] = pairs[i].Easing(objEnd);
				}
				int curPhase = 0;
				Vector2 basis = Vector2.Zero;
				startings[0] = Vector2.Zero;
				_totalTime = totalTime;

				return !Adjust
					? ((s) =>
					{
						obj.AppearTime += 0.5f;
						if (curPhase >= len)
							return s.CentrePosition;
						while (s.AppearTime >= timeZone[curPhase])
						{
							curPhase++;
							obj.AppearTime = 0.5f;
							if (curPhase >= len)
								return s.CentrePosition;
						}
						return pairs[curPhase].Easing(obj) + OffsetPosition;
					})
					: ((s) =>
					{
						obj.AppearTime += 0.5f;
						if (curPhase >= len)
							return s.CentrePosition;
						while (s.AppearTime >= timeZone[curPhase])
						{
							basis = endings[curPhase] + basis - startings[curPhase];
							curPhase++;
							obj.AppearTime = 0.5f;
							if (curPhase >= len)
								return s.CentrePosition;
						}
						Vector2 result = pairs[curPhase].Easing(obj) + basis - startings[curPhase] + OffsetPosition;
						return result;
					});
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Stable(float time, Vector2 val) => Insert(time, CentreEasing.Stable(val));
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Stable(float time, float val1, float val2) => Insert(time, CentreEasing.Stable(val1, val2));
		}
	}

	/// <summary>
	/// Easing library, note that they run at 125fps<br/>
	/// The functions are not documented as they are replaced by <see cref="SimplifiedEasing"/> functions, or are not useful except for a few occasions
	/// </summary>
	/// <remarks>This class will be removed in UF-Ex once it has been completely removed in Rhythm Recall</remarks>
	[Obsolete("SimplifiedEasing is better")]
	public static class ValueEasing
	{
		#region Functions
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Convert(Func<float, float> timeParamEase) => (s) => timeParamEase(s.AppearTime);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Stable(float position) => (s) => position;

		/// <summary>
		/// 构建一个正弦波的缓动
		/// </summary>
		/// <param name="intensity">振幅</param>
		/// <param name="cycleTime">每个波占的时间，即周期</param>
		/// <param name="startPhase">初始位置在第一个半波里面的比例位置。例如写0.5即从第一个半波的一半位置开始。</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> SinWave(float intensity, float cycleTime, float startPhase) => (s) => Sin(s.AppearTime * 2 * PI / cycleTime + startPhase) * intensity;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Accelerating(float speed, float acceleration) => (s) => speed * s.AppearTime + acceleration * (0.5f * s.AppearTime * s.AppearTime);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Linear(float v1, float v2, float time) => (s) => float.Lerp(v1, v2, s.AppearTime / time);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Linear(float Xspeed) =>
			(s) => s.AppearTime * Xspeed;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInSine(float v1, float v2, float time) => (s) => float.Lerp(v1, v2, 1 - MathF.Cos(s.AppearTime / time * MathF.PI / 2));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutSine(float v1, float v2, float time) =>
			(s) => float.Lerp(v1, v2, MathF.Sin(s.AppearTime / time * MathF.PI / 2));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInQuad(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, scale * scale);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutQuad(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInCubic(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, scale * scale * scale);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutCubic(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale) * (1 - scale));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInQuart(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, scale * scale * scale * scale);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutQuart(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale) * (1 - scale) * (1 - scale));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInQuint(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, scale * scale * scale * scale * scale);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutQuint(float v1, float v2, float time) =>
			(s) =>
			{
				float scale = s.AppearTime / time;
				return float.Lerp(v1, v2, 1 - (1 - scale) * (1 - scale) * (1 - scale) * (1 - scale) * (1 - scale));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInExpo(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return float.Lerp(v1, v2, x == 0 ? 0 : MathF.Pow(2, 10 * x - 10));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutExpo(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return float.Lerp(v1, v2, x == 1 ? 1 : 1 - MathF.Pow(2, -10 * x));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInCirc(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return float.Lerp(v1, v2, 1 - MathF.Sqrt(1 - x * x));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutCirc(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				return float.Lerp(v1, v2, MathF.Sqrt(1 - (1 - x) * (1 - x)));
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInBack(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				float c1 = 1.70158f;
				float c3 = c1 + 1;
				float value = c3 * x * x * x - c1 * x * x;
				return float.Lerp(v1, v2, value);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutBack(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time - 1;
				float c1 = 1.70158f;
				float c3 = c1 + 1;
				float value = 1 + c3 * x * x * x + c1 * x * x;
				return float.Lerp(v1, v2, value);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInElastic(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				float c4 = 2 * MathF.PI / 3;
				float value = x == 0 ? 0 : (x == 1 ? 1 :
					-MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * c4)
				);
				return float.Lerp(v1, v2, value);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutElastic(float v1, float v2, float time) =>
			(s) =>
			{
				float x = 1 - s.AppearTime / time;
				float c4 = 2 * MathF.PI / 3;
				float value = x == 0 ? 0 : (x == 1 ? 1 :
					-MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * c4)
				);
				return float.Lerp(v1, v2, 1 - value);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseInBounce(float v1, float v2, float time) =>
			(s) =>
			{
				float x = 1 - s.AppearTime / time;
				float n1 = 7.5625f;
				float d1 = 2.75f;
				float value = x < 1 / d1
					? n1 * x * x
					: x < 2 / d1
						? n1 * (float)Math.Pow(x - 1.5f / d1, 2) + 0.75f
						: x < 2.5 / d1 ? n1 * (float)Math.Pow(x - 2.25f / d1, 2) + 0.9375f : n1 * (float)Math.Pow(x - 2.625f / d1, 2) + 0.984375f;
				return float.Lerp(v1, v2, 1 - value);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> EaseOutBounce(float v1, float v2, float time) =>
			(s) =>
			{
				float x = s.AppearTime / time;
				float n1 = 7.5625f;
				float d1 = 2.75f;
				float value = x < 1 / d1
					? n1 * x * x
					: x < 2 / d1
						? n1 * (float)Math.Pow(x - 1.5f / d1, 2) + 0.75f
						: x < 2.5 / d1 ? n1 * (float)Math.Pow(x - 2.25f / d1, 2) + 0.9375f : n1 * (float)Math.Pow(x - 2.625f / d1, 2) + 0.984375f;
				return float.Lerp(v1, v2, value);
			};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> LerpTo(float start, float scale, Func<ICustomMotion, float> origin) => (s) => float.Lerp(start, origin(s), scale);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> LerpTo(float start, float origin, float scale) => (s) => float.Lerp(start, origin, scale);
		public static Func<ICustomMotion, float> Alternate(float time, params Func<ICustomMotion, float>[] easings)
		{
			int curPhase = 0;
			float timer = 0;
			time *= 2;
			return (s) =>
			{
				timer++;
				if (timer >= time)
				{
					timer -= time;
					curPhase++;
				}
				if (curPhase >= easings.Length)
					curPhase = 0;
				return easings[curPhase](s);
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Alternate(params Func<ICustomMotion, float>[] easings) => Alternate(1, easings);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Combine(Func<ICustomMotion, float> ease1, Func<ICustomMotion, float> ease2) => (s) => ease1(s) + ease2(s);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Func<ICustomMotion, float> Combine(Func<ICustomMotion, float> ease1, float basis) => (s) => ease1(s) + basis;
		#endregion
		public class EaseBuilder
		{
			public float OffsetPosition { get; set; }
			public bool Adjust { get; set; } = true;
			private readonly List<Tuple<float, Func<ICustomMotion, float>>> motionPairs = [];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Insert(float time, Func<ICustomMotion, float> function) => motionPairs.Add(new(time, function));
			public static implicit operator Func<ICustomMotion, float>(EaseBuilder val) => val.GetResult();
			public void Run(Action<float> action)
			{
				VirtualEasingObject easer = new();
				AddInstance(easer);
				easer.RotationRoute = GetResult();
				AddInstance(new TimeRangedEvent(_totalTime, () => action(easer.Rotation)) { UpdateIn120 = true });
				AddInstance(new InstantEvent(_totalTime, easer.Dispose));
			}
			private float _totalTime;
			public Func<ICustomMotion, float> GetResult()
			{
				VirtualEasingObject obj = new(), objEnd = new();

				Tuple<float, Func<ICustomMotion, float>>[] pairs = [.. motionPairs];
				int len = pairs.Length;
				float[] timeZone = new float[len];
				float totalTime = 0;
				float[] startings = new float[len], endings = new float[len];
				for (int i = 0; i < len; i++)
				{
					totalTime += pairs[i].Item1;
					timeZone[i] = totalTime;
					if (Adjust)
						startings[i] = pairs[i].Item2(obj);
					objEnd.AppearTime = pairs[i].Item1;
					endings[i] = pairs[i].Item2(objEnd);
				}
				int curPhase = 0;
				float basis = 0;
				startings[0] = 0;
				_totalTime = totalTime;

				return !Adjust
					? ((s) =>
					{
						obj.AppearTime += 0.5f;
						if (curPhase >= len)
							return s.Rotation;
						while (s.AppearTime >= timeZone[curPhase])
						{
							curPhase++;
							obj.AppearTime = 0.5f;
							if (curPhase >= len)
								return s.Rotation;
						}
						return pairs[curPhase].Item2(obj) + OffsetPosition;
					})
					: ((s) =>
					{
						obj.AppearTime += 0.5f;
						if (s.AppearTime <= 0.5f)
						{
							basis = 0;
							curPhase = 0;
						}
						if (curPhase >= len)
							return basis;
						while (s.AppearTime >= timeZone[curPhase])
						{
							basis = endings[curPhase] + basis - startings[curPhase];
							curPhase++;
							obj.AppearTime = 0.5f;
							if (curPhase >= len)
								return basis;
						}
						return pairs[curPhase].Item2(obj) + basis - startings[curPhase] + OffsetPosition;
					});
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Stable(float time, float val) => Insert(time, ValueEasing.Stable(val));
		}
	}
	internal static class Processor
	{
		/// <summary>
		/// The proxy object to apply the easing to
		/// </summary>
		private static readonly VirtualEasingObject _virtualEasingObject = new();
		private struct EaseProcess<T>(bool adjust, Action<T> easeAction)
		{
			/// <summary>
			/// The current time of the easing process
			/// </summary>
			public float Time = 0;
			/// <summary>
			/// Whether to ease only the delta of the easing or ease the values as is
			/// </summary>
			public bool Adjust = adjust;
			/// <summary>
			/// The index of the current ease in the process
			/// </summary>
			public int EaseIndex = 0;
			/// <summary>
			/// The array of eases, containing the duration of the ease and the function of the ease
			/// </summary>
			public (float Duration, EaseUnit<T> EasingFunctions)[] Eases;
			/// <summary>
			/// The action to apply with the eased value
			/// </summary>
			public Action<T> EaseAction = easeAction;
			/// <summary>
			/// The adjustment value of the easing if <see cref="Adjust"/> is true"/>
			/// </summary>
			public T AdjustValue;
		}
		/// <summary>
		/// Adds a float easing process for the processor to handle
		/// </summary>
		/// <param name="Adjust">Whether to ease only the delta of the easing or ease the values as is</param>
		/// <param name="easeAction">The action to apply with the eased value</param>
		/// <param name="eases">The ease functions</param>
		public static void PushProcess(bool Adjust, Action<float> easeAction, params EaseUnit<float>[] eases)
		{
			EaseProcess<float> process = new(Adjust, easeAction);
			(float, EaseUnit<float>)[] easeArray = new (float, EaseUnit<float>)[eases.Length];
			for (int i = 0; i < eases.Length; i++)
				easeArray[i] = (eases[i].Time, eases[i]);
			process.Eases = easeArray;
			_floatEasingProcesses.Add(process);
		}
		/// <summary>
		/// Adds a Vector2 easing process for the processor to handle
		/// </summary>
		/// <param name="Adjust">Whether to ease only the delta of the easing or ease the values as is</param>
		/// <param name="easeAction">The action to apply with the eased value</param>
		/// <param name="eases">The ease functions</param>
		public static void PushProcess(bool Adjust, Action<Vector2> easeAction, params EaseUnit<Vector2>[] eases)
		{
			EaseProcess<Vector2> process = new(Adjust, easeAction);
			(float, EaseUnit<Vector2>)[] easeArray = new (float, EaseUnit<Vector2>)[eases.Length];
			for (int i = 0; i < eases.Length; i++)
				easeArray[i] = (eases[i].Time, eases[i]);
			process.Eases = easeArray;
			_vec2EasingProcesses.Add(process);
		}
		/// <summary>
		/// The processes for easing of float values
		/// </summary>
		private static readonly List<EaseProcess<float>> _floatEasingProcesses = [];
		/// <summary>
		/// The processes for easing of Vector2 values
		/// </summary>
		private static readonly List<EaseProcess<Vector2>> _vec2EasingProcesses = [];
		/// <summary>
		/// The indexes of completed easing processes
		/// </summary>
		private static readonly List<int> _EasingFreeIndices = [];
		/// <summary>
		/// Removes all easing processes
		/// </summary>
		public static void ClearEase()
		{
			_floatEasingProcesses.Clear();
			_vec2EasingProcesses.Clear();
			_EasingFreeIndices.Clear();
		}
		/// <summary>
		/// Processes all easing processes
		/// </summary>
		public static void ProcessEase()
		{
			//Process float easing
			for (int i = 0; i < _floatEasingProcesses.Count; i++)
			{
				EaseProcess<float> process = _floatEasingProcesses[i];
				//Advance time
				process.Time += 0.5f;
				//Move to next ease if current ease ended
				if (process.Time >= process.Eases[process.EaseIndex].Duration)
				{
					//If all eases are done, mark for removal
					if (++process.EaseIndex >= process.Eases.Length)
					{
						_EasingFreeIndices.Add(i);
						continue;
					}
					//Adjust delta value if needed
					if (process.Adjust)
						process.AdjustValue += process.Eases[process.EaseIndex - 1].EasingFunctions.End - (process.EaseIndex == 1 ? 0 : process.Eases[process.EaseIndex - 1].EasingFunctions.Start);
					//Reset process time
					process.Time = 0.5f;
				}
				//Setup the virtual object for easing
				_virtualEasingObject.AppearTime = process.Time;
				//Perform the ease on the virtual object and apply the eased value to the action
				process.EaseAction(process.Eases[process.EaseIndex].EasingFunctions.Easing(_virtualEasingObject) + (process.Adjust ? process.AdjustValue - (process.EaseIndex == 0 ? 0 : process.Eases[process.EaseIndex].EasingFunctions.Start) : 0));
				//Apply changes to the source process
				_floatEasingProcesses[i] = process;
			}
			//Remove completed float easing functions
			for (int i = _EasingFreeIndices.Count - 1; i >= 0; i--)
				_floatEasingProcesses.RemoveAt(_EasingFreeIndices[i]);
			//Clear the free indices cache
			_EasingFreeIndices.Clear();

			//Process Vector2 easing
			for (int i = 0; i < _vec2EasingProcesses.Count; i++)
			{
				EaseProcess<Vector2> process = _vec2EasingProcesses[i];
				//Advance time
				process.Time += 0.5f;
				//Move to next ease if current ease ended
				if (process.Time >= process.Eases[process.EaseIndex].Duration)
				{
					//If all eases are done, mark for removal
					if (++process.EaseIndex >= process.Eases.Length)
					{
						_EasingFreeIndices.Add(i);
						continue;
					}
					//Adjust delta value if needed
					if (process.Adjust)
						process.AdjustValue += process.Eases[process.EaseIndex - 1].EasingFunctions.End - (process.EaseIndex == 1 ? Vector2.Zero : process.Eases[process.EaseIndex - 1].EasingFunctions.Start);
					//Reset process time
					process.Time = 0.5f;
				}
				//Setup the virtual object for easing
				_virtualEasingObject.AppearTime = process.Time;
				//Perform the ease on the virtual object and apply the eased value to the action
				process.EaseAction(process.Eases[process.EaseIndex].EasingFunctions.Easing(_virtualEasingObject) + (process.Adjust ? process.AdjustValue - (process.EaseIndex == 0 ? Vector2.Zero : process.Eases[process.EaseIndex].EasingFunctions.Start) : Vector2.Zero));
				//Apply changes to the source process
				_vec2EasingProcesses[i] = process;
			}
			//Remove completed Vector2 easing functions
			for (int i = _EasingFreeIndices.Count - 1; i >= 0; i--)
				_vec2EasingProcesses.RemoveAt(_EasingFreeIndices[i]);
			//Clear the free indices cache
			_EasingFreeIndices.Clear();
		}
	}
}
/// <summary>
/// The library for ease functions
/// </summary>
public static class EaseLibrary
{
	#region Enum definitions
	/// <summary>
	/// Modes of easing
	/// </summary>
	public enum EaseMode
	{
		/// <summary>
		/// The easing is slower at the start and faster at the end
		/// </summary>
		In,
		/// <summary>
		/// The easing is faster at the start and slower at the end
		/// </summary>
		Out,
		/// <summary>
		/// The easing is slower at the start and end, and faster in the middle
		/// </summary>
		InOut,
		/// <summary>
		/// The easing is faster at the start and end, and slower in the middle
		/// </summary>
		OutIn
	}
	#endregion
	/// <summary>
	/// Easing library (All are EaseIn functions as EaseOut functions are "1 - EaseIn" and others are combinations of both)
	/// </summary>
	private static readonly Dictionary<string, Func<float, float>> _EasingFunctions = new()
	{
		[nameof(EaseState.Linear)] = (x) => x,
		[nameof(EaseState.Quad)] = (x) => x * x,
		[nameof(EaseState.Cubic)] = (x) => x * x * x,
		[nameof(EaseState.Quart)] = (x) => x * x * x * x,
		[nameof(EaseState.Quint)] = (x) => x * x * x * x * x,
		[nameof(EaseState.Circ)] = (x) => 1 - MathF.Sqrt(1 - x * x),
		[nameof(EaseState.Sine)] = (x) => 1 - MathF.Cos(x * MathF.PI / 2),
		[nameof(EaseState.Elastic)] = (x) => x switch
		{
			0 or 1 => x, //Ensure exact values at edges
			_ => -MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * (2 * MathF.PI / 3))
		},
		[nameof(EaseState.Expo)] = (x) => x == 0 ? 0 : MathF.Pow(2, 10 * x - 10),
		[nameof(EaseState.Back)] = (x) => 2.70158f * x * x * x - 1.70158f * x * x,
		[nameof(EaseState.Bounce)] = (x) =>
			x = 1 - x switch //Magic numbers, I know
			{
				< 1 / 2.75f => 1 - 7.5625f * x * x,
				< 2 / 2.75f => 1.75f - 7.5625f * (x - 1.5f / 2.75f) * (x - 1.5f / 2.75f),
				< 2.5f / 2.75f => 1.9375f - 7.5625f * (x - 2.25f / 2.75f) * (x - 2.25f / 2.75f),
				_ => 1.984375f - 7.5625f * (x - 2.625f / 2.75f) * (x - 2.625f / 2.75f),
			}
	};
	/// <summary>
	/// Defines a custom ease function
	/// </summary>
	/// <param name="state">The name of the state</param>
	/// <param name="function">The easing function (A map from [0,1] -> [0,1])</param>
	public static void RegisterEaseFunction(string state, Func<float, float> function) => _EasingFunctions[state] = function;
	/// <summary>
	/// Gets the lerp value of the easing at a given time
	/// </summary>
	/// <param name="timeProgress">The progress of the easing, value should be [0, 1]</param>
	/// <param name="state">The name of the easing state</param>
	/// <param name="mode">The mode of easing</param>
	/// <returns>The lerp progress</returns>
	internal static float GetValue(float timeProgress, EaseState state, EaseMode mode) => GetValue(timeProgress, state.ToString(), mode);
	/// <summary>
	/// Gets the lerp value of the easing at a given time
	/// </summary>
	/// <param name="timeProgress">The progress of the easing, value should be [0, 1]</param>
	/// <param name="state">The name of the easing state</param>
	/// <param name="mode">The mode of easing</param>
	/// <returns>The lerp progress</returns>
	/// <exception cref="ArgumentException">An unknown <see cref="EaseMode"/> was provided</exception>
	public static float GetValue(float timeProgress, string state, EaseMode mode) => mode switch
	{
		EaseMode.In => _EasingFunctions[state](timeProgress),
		EaseMode.Out => 1 - _EasingFunctions[state](1 - timeProgress),
		EaseMode.InOut => timeProgress < 0.5f
			? _EasingFunctions[state](timeProgress * 2) / 2
			: 1 - _EasingFunctions[state]((1 - timeProgress) * 2) / 2,
		EaseMode.OutIn => timeProgress < 0.5f
			? (1 - _EasingFunctions[state](1 - timeProgress * 2)) / 2
			: (_EasingFunctions[state]((timeProgress - 0.5f) * 2) + 1) / 2,
		_ => throw new ArgumentException("Unknown Ease Mode")
	};
}