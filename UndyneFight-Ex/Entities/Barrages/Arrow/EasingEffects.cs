using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.Fight.Functions;

namespace UndyneFight_Ex.Entities
{
	public partial class Arrow : Entity
	{
		public abstract class ArrowEasing : GameObject
		{
			private readonly List<Arrow> arrows = [];

			protected internal bool RotateOffset { get; set; } = false;
			public ArrowEasing() => UpdateIn120 = true;
			/// <summary>
			/// Applies the easing functions to the arrows with the given tag
			/// </summary>
			/// <param name="tagName">The tag of the arrows to apply to</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void TagApply(string tagName)
			{
				if (CurrentScene is SongFightingScene)
					AddInstance(new InstantEvent(1.2f, () =>
					{
						if ((CurrentScene as SongFightingScene).Accuracy.TaggedArrows.TryGetValue(tagName, out List<Arrow> value))
							SetArrowSet(value);
					}));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetArrowSet(List<Arrow> arrows) => this.arrows.AddRange(arrows);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected abstract void ApplyArrowEasing(Arrow arr);

			private bool _lastRotateOffsetState = false;
			public override void Update()
			{
				_ = arrows.RemoveAll(s => s.Disposed);
				arrows.ForEach(ApplyArrowEasing);
				if (RotateOffset ^ _lastRotateOffsetState)
				{
					arrows.ForEach(s => s.RotateOffset = RotateOffset);
					_lastRotateOffsetState = RotateOffset;
				}
			}

			public float Intensity { get; set; } = 1.0f;
		}
		public class EnsembleEasing() : ArrowEasing
		{
			private Vector2 _deltaEasing = Vector2.Zero;
			private float _revolutionEasing = 0;
			private float _rotationEasing = 0;
			private float _distanceEasing = 0;
			/// <summary>
			/// Eases the coordinate displacement of the arrow
			/// </summary>
			/// <param name="deltaEases">The easing function of the coordinate</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void DeltaEase(params EaseUnit<Vector2>[] deltaEases) => RunEase((s) => _deltaEasing = s, false, deltaEases);
			/// <summary>
			/// Eases the angle of the arrow from its target
			/// </summary>
			/// <param name="rotationEases">The easing function of the angle</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RevolutionEase(params EaseUnit<float>[] rotationEases) => RunEase((s) => _revolutionEasing = s, rotationEases);
			/// <summary>
			/// Eases the arrow's own angle
			/// </summary>
			/// <param name="rotationEases">The easing function of the angle</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SelfRotationEase(params EaseUnit<float>[] rotationEases) => RunEase((s) => _rotationEasing = s, rotationEases);
			/// <summary>
			/// Eases the arrow's distance
			/// </summary>
			/// <param name="distanceEases">The easing function of the distance of the arrow</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void DistanceEase(params EaseUnit<float>[] distanceEases) => RunEase((s) => _distanceEasing = s, distanceEases);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override void ApplyArrowEasing(Arrow arr)
			{
				arr.Offset = _deltaEasing * Intensity;
				arr.CentreRotationOffset = _revolutionEasing * Intensity;
				arr.SelfRotationOffset = _rotationEasing * Intensity;
				arr.additiveDistance = _distanceEasing * Intensity;
			}
		}
		public class UnitEasing() : ArrowEasing, ICustomMotion
		{
			public override void Start()
			{
				Reset();
				base.Start();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void Reset()
			{
				_easingTimeMax = 0;
				if (positionEaseEnabled)
					_easingTimeMax = MathF.Max(_easingTimeMax, positionEase.Time);
				if (rotationEaseEnabled)
					_easingTimeMax = MathF.Max(_easingTimeMax, rotationEase.Time);
				if (distanceEaseEnabled)
					_easingTimeMax = MathF.Max(_easingTimeMax, distanceEase.Time);
				if (alphaEaseEnabled)
					_easingTimeMax = MathF.Max(_easingTimeMax, alphaEase.Time);

				maxIndex = ToArrayIndex(_easingTimeMax) + 1;
				if (maxIndex > 0)
				{
					if (positionEaseEnabled)
						if (positionBuffer == null || maxIndex > positionBuffer.Length)
							positionBuffer = new Vector2[maxIndex];
					if (rotationEaseEnabled)
						if (rotationBuffer == null || maxIndex > rotationBuffer.Length)
							rotationBuffer = new float[maxIndex];
					if (distanceEaseEnabled)
						if (distanceBuffer == null || maxIndex > distanceBuffer.Length)
							distanceBuffer = new float[maxIndex];
					if (alphaEaseEnabled)
						if (alphaBuffer == null || maxIndex > alphaBuffer.Length)
							alphaBuffer = new float[maxIndex];
				}
			}
			/// <summary>
			/// The total time of the easing
			/// </summary>
			public float ApplyTime { get; set; } = 60;
			private float _easingTimeMax = 0;

			private EaseUnit<Vector2> positionEase;
			private EaseUnit<float> rotationEase, distanceEase, alphaEase;

			private bool positionEaseEnabled = false;
			private bool rotationEaseEnabled = false;
			private bool distanceEaseEnabled = false;
			private bool alphaEaseEnabled = false;
			/// <summary>
			/// The easing of the position of the arrow
			/// </summary>
			public EaseUnit<Vector2> PositionEase { set { positionEase = value; positionEaseEnabled = true; arrayIndex = -1; Reset(); } }
			/// <summary>
			/// The easing of the rotation of the arrow
			/// </summary>
			public EaseUnit<float> RotationEase { set { rotationEase = value; rotationEaseEnabled = true; arrayIndex = -1; Reset(); } }
			/// <summary>
			/// The easing of the distance of the arrow
			/// </summary>
			public EaseUnit<float> DistanceEase { set { distanceEase = value; distanceEaseEnabled = true; arrayIndex = -1; Reset(); } }
			/// <summary>
			/// The easing of the alpha of the arrow
			/// </summary>
			public EaseUnit<float> AlphaEase { set { alphaEase = value; alphaEaseEnabled = true; arrayIndex = -1; Reset(); } }

			private Vector2[] positionBuffer;
			private float[] rotationBuffer, distanceBuffer, alphaBuffer;
			/// <inheritdoc/>
			public Func<ICustomMotion, Vector2> PositionRoute { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			/// <inheritdoc/>
			public Func<ICustomMotion, float> RotationRoute { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			/// <inheritdoc/>
			public float[] RotationRouteParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			/// <inheritdoc/>
			public float[] PositionRouteParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			/// <summary>
			/// Time elapsed after spawning
			/// </summary>
			public float AppearTime { get; set; } = 0f;
			/// <inheritdoc/>
			public float Rotation { get; set; } = 0f;
			/// <summary>
			/// The distance between the arrow and the target
			/// </summary>
			public float Distance { get; set; } = 0f;
			/// <inheritdoc/>
			public Vector2 CentrePosition { get; set; }
			/// <summary>
			/// The rotation of the arrow itself
			/// </summary>
			public float SelfRotation { get; set; } = 0f;
			public bool AutoDispose { get; internal set; }

			private int maxIndex = 0;
			private int arrayIndex = -1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int ToArrayIndex(float x) => (int)((x - 0.5f) * 2f);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override void ApplyArrowEasing(Arrow arr)
			{
				float time = ApplyTime - arr.TimeDelta;
				if (time < 0.5f)
					return;
				if (MathF.Abs(SelfRotation) > 1)
					arr.SelfRotationOffset = SelfRotation;
				int l = ToArrayIndex(time), r = l + 1;

				if (l >= maxIndex + 2)
					return;

				if (l >= maxIndex)
					l = maxIndex - 1;
				if (r >= maxIndex)
					r = maxIndex - 1;

				while (r > arrayIndex)
					UpdateEase();

				float add = (time - 0.5f) * 2f - l;

				Vector2 realPos = Vector2.Zero;
				float realRot = 0, realDis = 0, realAlp = 0.0f;
				if (l == r)
				{
					if (positionEaseEnabled)
						realPos = positionBuffer[l] * Intensity;
					if (rotationEaseEnabled)
						realRot = rotationBuffer[l] * Intensity;
					if (distanceEaseEnabled)
						realDis = distanceBuffer[l] * Intensity;
					if (alphaEaseEnabled)
						realAlp = alphaBuffer[l] * Intensity;
				}
				else
				{
					if (positionEaseEnabled)
						realPos = Vector2.Lerp(positionBuffer[l], positionBuffer[r], add) * Intensity;
					if (rotationEaseEnabled)
						realRot = float.Lerp(rotationBuffer[l], rotationBuffer[r], add) * Intensity;
					if (distanceEaseEnabled)
						realDis = float.Lerp(distanceBuffer[l], distanceBuffer[r], add) * Intensity;
					if (alphaEaseEnabled)
						realAlp = float.Lerp(alphaBuffer[l], alphaBuffer[r], add) * 1.0f;
				}

				if (positionEaseEnabled)
					arr.Offset = realPos;
				if (rotationEaseEnabled)
					arr.CentreRotationOffset = realRot;
				if (distanceEaseEnabled)
					arr.additiveDistance = realDis;
				if (alphaEaseEnabled)
					arr.Alpha = realAlp;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void UpdateEase()
			{
				AppearTime += 0.5f;

				arrayIndex++;

				if (positionEaseEnabled)
					positionBuffer[arrayIndex] = CentrePosition = positionEase.Easing(this);
				if (rotationEaseEnabled)
					rotationBuffer[arrayIndex] = Rotation = rotationEase.Easing(this);
				if (distanceEaseEnabled)
					distanceBuffer[arrayIndex] = Distance = distanceEase.Easing(this);
				if (alphaEaseEnabled)
					alphaBuffer[arrayIndex] = alphaEase.Easing(this);

				if (AppearTime > 20 && AutoDispose && arrows.Count == 0)
						Dispose();
			}
		}
		public class ClassicApplier : ArrowEasing
		{
			/// <summary>
			/// Applies an arrow delay effect
			/// </summary>
			/// <param name="delay">The amount to delay</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ApplyDelay(float delay) => tuples.Add(new(delay, DelayControl.DelayType.Pull));
			/// <summary>
			/// Applies an arrow stop effect
			/// </summary>
			/// <param name="stopTime">The duration of time to stop</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ApplyStop(float stopTime) => tuples.Add(new(stopTime, DelayControl.DelayType.Stop));
			private readonly List<Tuple<float, DelayControl.DelayType>> tuples = [];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override void ApplyArrowEasing(Arrow arr)
			{
				foreach (Tuple<float, DelayControl.DelayType> pair in tuples)
				{
					if (pair.Item2 == DelayControl.DelayType.Pull)
						arr.Delay(pair.Item1);
					else
						arr.Stop(pair.Item1);
				}
			}
			public override void Update()
			{
				base.Update();
				tuples.Clear();
			}
		}
	}
}