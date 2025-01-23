using static UndyneFight_Ex.DrawingLab;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// The class for a line effect
    /// </summary>
    public partial class Line : Entity
    {
        private class LinePoint : GameObject, ICustomMotion
        {
            public Func<ICustomMotion, Vector2> PositionRoute { get; set; }
            public Func<ICustomMotion, float> RotationRoute { get; set; }
            public float[] RotationRouteParam { get; set; }
            public float[] PositionRouteParam { get; set; }

            public float AppearTime { get; private set; }

            public Vector2 CentrePosition { get; private set; }
            public float Rotation => 0;

            public override void Update()
            {
                AppearTime += 0.5f;
                CentrePosition = PositionRoute.Invoke(this);
            }
            public LinePoint(Func<ICustomMotion, Vector2> positionRoute)
            {
                PositionRoute = positionRoute;
                UpdateIn120 = true;
            }
        }

        readonly LinePoint vec1, vec2;
        /// <summary>
        /// The alpha of the line
        /// </summary>
        public float Alpha { get; set; } = 1.0f;
        /// <summary>
        /// The width of the line (Default 3 pixels)
        /// </summary>
        public float Width { private get; set; } = 3.0f;
        /// <summary>
        /// The color of the line
        /// </summary>
        public Color DrawingColor { get; set; } = Color.White;
        /// <summary>
        /// Whether the line will be reflected vertically
        /// </summary>
        public bool VerticalMirror { private get; set; } = false;
        /// <summary>
        /// Whether the line will be reflected horizontally
        /// </summary>
        public bool TransverseMirror { private get; set; } = false;
        /// <summary>
        /// Whether the line will be reflected diagonally
        /// </summary>
        public bool ObliqueMirror { private get; set; } = false;
        /// <summary>
        /// Whether the line is a vertical line
        /// </summary>
        public bool VerticalLine { private get; set; } = false;
        /// <summary>
        /// Whether the line will have pre-multipied alpha
        /// </summary>
        public bool PreMultiplyAlpha { private get; set; } = false;
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="vec1">The position of the first vertex</param>
        /// <param name="vec2">The position of the second vertex</param>
        public Line(Vector2 vec1, Vector2 vec2) : this(SimplifiedEasing.Stable(0, vec1), SimplifiedEasing.Stable(0, vec2)) { }
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="centre">The center of the line</param>
        /// <param name="rotation">The rotation of the line</param>
        public Line(Vector2 centre, float rotation) : this(centre, SimplifiedEasing.Stable(0, rotation)) { }
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="Xcentre">The x coordinate of the line</param>
        /// <param name="rotation">The rotation of the line</param>
        public Line(float Xcentre, float rotation) : this(SimplifiedEasing.Stable(0, Xcentre, 240), SimplifiedEasing.Stable(0, rotation)) { }
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="easing1">The easing of the first vertex</param>
        /// <param name="easing2">The easing of the second vertex</param>
        public Line(Func<ICustomMotion, Vector2> easing1, Func<ICustomMotion, Vector2> easing2)
        {
            UpdateIn120 = true;
            AddChild(vec1 = new(easing1));
            AddChild(vec2 = new(easing2));
        }
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="centre">The center of the line</param>
        /// <param name="rotationEasing">The easing of the rotation of the line</param>
        public Line(Vector2 centre, Func<ICustomMotion, float> rotationEasing) : this(SimplifiedEasing.Stable(0, centre), rotationEasing) { }
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="centreEasing">The easing of the center of the line</param>
        /// <param name="rotationEasing">The easing of the rotation of the line</param>
        public Line(Func<ICustomMotion, Vector2> centreEasing, Func<ICustomMotion, float> rotationEasing)
        {
            UpdateIn120 = true;

            bool xCalc = false;
            float rotation = 0;
            Vector2 centre = new();
            vec2 easing1(ICustomMotion s)
            {
                rotation = rotationEasing.Invoke(s);
                centre = centreEasing.Invoke(s);
                float jr = rotation;
                jr = MathUtil.Posmod(jr, 180);
                Vector2 result;
                xCalc = jr < 45 || jr > 135f;
                if (xCalc)
                {
                    float dist = centre.X + 640;
                    result = new(-640, centre.Y - Tan(rotation) * dist);
                }
                else
                {
                    float dist = centre.Y + 480;
                    result = new(centre.X - dist / Tan(rotation), -480);
                }
                return result;
            }
            vec2 easing2(ICustomMotion s)
            {
                Vector2 result;
                if (xCalc)
                {
                    float dist = 1280 - centre.X;
                    result = new(1280, centre.Y + Tan(rotation) * dist);
                }
                else
                {
                    float dist = 960 - centre.Y;
                    result = new(centre.X + dist / Tan(rotation), 960);
                }
                return result;
            }
            AddChild(vec1 = new(easing1));
            AddChild(vec2 = new(easing2));
        }
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="centreEasing">The easing of the center of the line</param>
        /// <param name="rotationEasing">The easing of the rotation of the line</param>
        /// <param name="lengthEasing">The easing of the length of the line from its center</param>
        public Line(Func<ICustomMotion, Vector2> centreEasing, Func<ICustomMotion, float> rotationEasing, Func<ICustomMotion, float> lengthEasing)
        {
            UpdateIn120 = true;
            float rotation = 0;

            LinePoint centre = new(centreEasing);
            AddChild(centre);

            vec2 easing1(ICustomMotion s) => centre.CentrePosition + MathUtil.GetVector2(lengthEasing.Invoke(s) / 2, rotation = rotationEasing.Invoke(s));
            vec2 easing2(ICustomMotion s) => centre.CentrePosition - MathUtil.GetVector2(lengthEasing.Invoke(s) / 2, rotation);
            AddChild(vec1 = new(easing1));
            AddChild(vec2 = new(easing2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DrawTargetLine(Vector2 Start, Vector2 End) => DrawLine(Start, End, Width, PreMultiplyAlpha ? Color.Lerp(DrawingColor, ScreenDrawing.BackGroundColor, Alpha) : (DrawingColor * Alpha), Depth, Image);
        public override void Draw()
        {
            if (Alpha <= 0)
                return;
            DrawTargetLine(vec1.CentrePosition, vec2.CentrePosition);
            if (VerticalMirror)
                DrawTargetLine(new Vector2(vec1.CentrePosition.X, 480 - vec1.CentrePosition.Y), new Vector2(vec2.CentrePosition.X, 480 - vec2.CentrePosition.Y));
            if (TransverseMirror)
                DrawTargetLine(new Vector2(640 - vec1.CentrePosition.X, vec1.CentrePosition.Y), new Vector2(640 - vec2.CentrePosition.X, vec2.CentrePosition.Y));
            if (ObliqueMirror)
                DrawTargetLine(new Vector2(640 - vec1.CentrePosition.X, 480 - vec1.CentrePosition.Y), new Vector2(640 - vec2.CentrePosition.X, 480 - vec2.CentrePosition.Y));
            if (VerticalLine)
            {
                DrawTargetLine(new Vector2(-vec1.CentrePosition.Y - 80, vec1.CentrePosition.X - 80), new Vector2(-vec2.CentrePosition.Y - 80, vec2.CentrePosition.X - 80));
                if (TransverseMirror)
                    DrawTargetLine(new Vector2(640 - (560 - vec1.CentrePosition.Y), vec1.CentrePosition.X - 80), new Vector2(640 - (560 - vec2.CentrePosition.Y), vec2.CentrePosition.X - 80));
                if (VerticalMirror)
                    DrawTargetLine(new Vector2(560 - vec1.CentrePosition.Y, 480 - (vec1.CentrePosition.X - 80)), new Vector2(560 - vec2.CentrePosition.Y, 480 - (vec2.CentrePosition.X - 80)));
                if (ObliqueMirror)
                    DrawTargetLine(new Vector2(640 - (560 - vec1.CentrePosition.Y), 480 - (vec1.CentrePosition.X - 80)), new Vector2(640 - (560 - vec2.CentrePosition.Y), 480 - (vec2.CentrePosition.X - 80)));
            }
        }
        /// <summary>
        /// The frames elapsed after the line was created
        /// </summary>
        public float AppearTime => vec1.AppearTime;
        /// <summary>
        /// The centre position of the line
        /// </summary>
        public new Vector2 Centre => (vec1.CentrePosition + vec2.CentrePosition) / 2;
        /// <summary>
        /// The rotation of the line
        /// </summary>
        public new float Rotation => MathUtil.Direction(vec1.CentrePosition, vec2.CentrePosition);

        public override void Update() { }
        /// <summary>
        /// Fades out the line for the given duration
        /// </summary>
        /// <param name="time">The time taken for the line to fade out</param>
        /// <param name="val">The amount of alpha to decrease (Default entirely)</param>
        /// <param name="willDispose">Whether the line will automatically dispose when the alpha reaches 0</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlphaDecrease(float time, float? val = null, bool? willDispose = true)
        {
            float total = val ??= Alpha, once = total / time;
            InstanceCreate(new TimeRangedEvent(time, () => Alpha -= once));
            if (val == Alpha && (willDispose ?? true))
                InstanceCreate(new InstantEvent(time, Dispose));
        }
        /// <summary>
        /// Fades out the line by the given amount for the given duration after the given delay
        /// </summary>
        /// <param name="delay">The delay before the line to fade</param>
        /// <param name="time">The time taken for the line to fade</param>
        /// <param name="val">The amount to fade out (Default entirely)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelayAlphaDecrease(float delay, float time, float? val = null) => InstanceCreate(new InstantEvent(delay, () =>
            {
                float total = val ?? Alpha, once = total / time;
                InstanceCreate(new TimeRangedEvent(time + 5, () => Alpha -= once));
            }));
        /// <summary>
        /// Fades in the line by the given duration by the given value
        /// </summary>
        /// <param name="time">The time taken for the line to fade in</param>
        /// <param name="val">The amount to fade in (Default 1)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlphaIncrease(float time, float val = 1)
        {
            float total = val, once = total / time;
            InstanceCreate(new TimeRangedEvent(time, () => Alpha += once));
        }
        /// <summary>
        /// Fades in the line by the given amount for the given duration after the given delay
        /// </summary>
        /// <param name="delay">The delay before the line to fade</param>
        /// <param name="time">The time taken for the line to fade</param>
        /// <param name="val">The amount to fade in (Default 1)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelayAlphaIncrease(float delay, float time, float val = 1) => InstanceCreate(new InstantEvent(delay, () =>
            {
                float total = val, once = total / time;
                InstanceCreate(new TimeRangedEvent(time, () => Alpha += once));
            }));
        /// <summary>
        /// Decreases the alpha of the line and then increases it
        /// </summary>
        /// <param name="time">The time taken to complete the entire animation</param>
        /// <param name="val">The amount alpha to fade (Default 1)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlphaDecreaseAndIncrease(float time, float val = 1)
        {
            float total = val, once = total / time;
            InstanceCreate(new TimeRangedEvent(time / 4, () => Alpha -= once * 4));
            InstanceCreate(new TimeRangedEvent(time / 4, time / 4 * 3, () => Alpha += once * 4 / 3));
            InstanceCreate(new InstantEvent(time, () => { if (Alpha <= once) Dispose(); }));
        }
        /// <summary>
        /// Increases the alpha of the line and then decreases it
        /// </summary>
        /// <param name="time">The time taken to complete the entire animation</param>
        /// <param name="val">The amount alpha to fade (Default 1)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlphaIncreaseAndDecrease(float time, float val = 1)
        {
            float total = val, once = total / time;
            InstanceCreate(new TimeRangedEvent(time / 4, () => Alpha += once * 4));
            InstanceCreate(new TimeRangedEvent(time / 4, time / 4 * 3, () => Alpha -= once * 4 / 3));
            InstanceCreate(new InstantEvent(time, () => { if (Alpha <= once) Dispose(); }));
        }
        /// <summary>
        /// Splits the line
        /// </summary>
        /// <param name="clear">Whether to return the original line or the splitted line</param>
        /// <returns>The split line</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Line Split(bool clear) => clear
                ? this
                : new Line(vec1.CentrePosition, vec2.CentrePosition) { Alpha = Alpha, Depth = Depth, DrawingColor = DrawingColor, };
        /// <summary>
        /// Inserts a retention effect, the exact same as <see cref="InsertRetention(RetentionEffect)"/>
        /// </summary>
        /// <param name="timeLag">The delay before the effect spawns</param>
        /// <param name="alphaFactor">The alpha of the effect</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddShadow(float timeLag, float alphaFactor) => InsertRetention(new RetentionEffect(timeLag, alphaFactor));
        /// <summary>
        /// Inserts a retention effect, the exact same as <see cref="InsertRetention(RetentionEffect)"/>
        /// </summary>
        /// <param name="r">The retention effect</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddShadow(RetentionEffect r) => InsertRetention(r);
        /// <summary>
        /// Inserts retention effects, the exact same as <see cref="InsertRetention(RetentionEffect)"/>
        /// </summary>
        /// <param name="r">The retention effects</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddShadow(params RetentionEffect[] r)
        {
            foreach (RetentionEffect retention in r)
                InsertRetention(retention);
        }
        private struct LineState
        {
            public Vector2 p1, p2;
            public float alpha;
            public float time;
            public Color color;
            public bool transverseMirror, verticalMirror, obliqueMirror, verticalline;
        }
    }
}