using static UndyneFight_Ex.DrawingLab;

namespace UndyneFight_Ex.Entities
{
    public partial class Line
    {
        private class StateStorer : GameObject
        {
            Line follow;
            public StateStorer() => UpdateIn120 = true;
            public override void Start() => follow = FatherObject as Line;
            public override void Update()
            {
                LineState state;
                state.p1 = follow.vec1.CentrePosition;
                state.p2 = follow.vec2.CentrePosition;
                state.time = follow.AppearTime;
                state.alpha = follow.Alpha;
                state.color = follow.DrawingColor;
                state.verticalMirror = follow.VerticalMirror;
                state.transverseMirror = follow.TransverseMirror;
                state.obliqueMirror = follow.ObliqueMirror;
                state.verticalline = follow.VerticalLine;
                DataStore.Add(follow.AppearTime, state);
            }
            public Dictionary<float, LineState> DataStore = [];
        }
        StateStorer storer;
        /// <summary>
        /// Inserts a retention effect
        /// </summary>
        /// <param name="effect">The retention effect</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRetention(RetentionEffect effect)
        {
            if (storer == null)
                AddChild(storer = new());
            AddChild(effect);
        }
        /// <summary>
        /// Disposes itself after the given amount of time
        /// </summary>
        /// <param name="v">The delay before disposing</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelayDispose(float v) => AddChild(new InstantEvent(v, Dispose));
        /// <summary>
        /// The retention effect of the line
        /// </summary>
        public new class RetentionEffect : Entity
        {
            float timeLag;
            readonly Func<float, float> alphaGenerator;

            Line follow;
            private RetentionEffect() => UpdateIn120 = true;
            public override void Start()
            {
                follow = FatherObject as Line;

                //process timeLag to a number which can be divided by 0.5f
                timeLag = MathF.Round(timeLag * 2) / 2f;

                base.Start();
            }
            /// <summary>
            /// Creates a retention effect
            /// </summary>
            /// <param name="timeLag">The delay before it spawns</param>
            /// <param name="alphaFactor">The alpha scale of the line</param>
            public RetentionEffect(float timeLag, float alphaFactor = 1) : this()
            {
                this.timeLag = timeLag;
                alphaGenerator = (s) => s * alphaFactor;
            }
            /// <summary>
            /// Creates a retention effect
            /// </summary>
            /// <param name="timeLag">The delay before it spawns</param>
            /// <param name="alphaGenerator">The easing of the alpha multiplication of the retention effect</param>
            public RetentionEffect(float timeLag, Func<float, float> alphaGenerator) : this()
            {
                this.timeLag = timeLag;
                this.alphaGenerator = alphaGenerator;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void DrawTargetLine(Vector2 Start, Vector2 End) => DrawLine(Start, End, follow.Width, follow.DrawingColor * alpha, Depth);
            public override void Draw()
            {
                if (!available || alpha <= 0)
                    return;
                DrawTargetLine(vec1, vec2);
                if (verticalMirror)
                    DrawTargetLine(new Vector2(vec1.X, 480 - vec1.Y), new Vector2(vec2.X, 480 - vec2.Y));
                if (transverseMirror)
                    DrawTargetLine(new Vector2(640 - vec1.X, vec1.Y), new Vector2(640 - vec2.X, vec2.Y));
                if (obliqueMirror)
                    DrawTargetLine(new Vector2(640 - vec1.X, 480 - vec1.Y), new Vector2(640 - vec2.X, 480 - vec2.Y));
                if (verticalline)
                {
                    DrawTargetLine(new Vector2(560 - vec1.Y, vec1.X - 80), new Vector2(560 - vec2.Y, vec2.X - 80));
                    if (transverseMirror)
                        DrawTargetLine(new Vector2(640 - (560 - vec1.Y), vec1.X - 80), new Vector2(640 - (560 - vec2.Y), vec2.X - 80));
                    if (verticalMirror)
                        DrawTargetLine(new Vector2(560 - vec1.Y, 480 - (vec1.X - 80)), new Vector2(560 - vec2.Y, 480 - (vec2.X - 80)));
                    if (obliqueMirror)
                        DrawTargetLine(new Vector2(640 - (560 - vec1.Y), 480 - (vec1.X - 80)), new Vector2(640 - (560 - vec2.Y), 480 - (vec2.X - 80)));
                }
            }

            Vector2 vec1, vec2;
            float alpha;
            Color color;
            bool transverseMirror, verticalMirror, obliqueMirror, verticalline;

            bool available = false;
            public override void Update()
            {
                float key = follow.AppearTime - timeLag;
                if (!follow.storer.DataStore.TryGetValue(key, out LineState value))
                {
                    available = false;
                    return;
                }
                available = true;
                vec1 = value.p1;
                vec2 = value.p2;
                alpha = alphaGenerator(value.alpha);
                color = value.color;
                transverseMirror = value.transverseMirror;
                verticalMirror = value.verticalMirror;
                obliqueMirror = value.obliqueMirror;
                verticalline = value.verticalline;
            }
        }
    }
}