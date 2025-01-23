namespace UndyneFight_Ex.Entities
{
    public partial class Arrow
    {
        private class DelayControl : GameObject
        {
            public enum DelayType
            {
                Pull = 0,
                Stop = 1
            }
            private float delay = 0;
            private readonly DelayType type;
            public DelayControl(float delay, DelayType delayType)
            {
                UpdateIn120 = true;
                type = delayType;
                this.delay = delay;
            }
            public override void Update()
            {
                Arrow control = FatherObject as Arrow;
                float del = type == DelayType.Pull
                    ? Math.Max(0.8f, MathF.Min(2.5f, delay * 0.1f))
                    : Math.Max(0.7f, MathF.Min(1, (delay > 10 ? 10 : MathF.Sqrt(delay * 2)) * 0.3f));
                del /= 2;
                if (delay < del)
                    del = delay;
                control.BlockTime += del;
                delay -= del;
                if (delay <= 0.01f)
                    Dispose();
            }
            public override void Dispose() => base.Dispose();
        }
        /// <summary>
        /// Delay the arrow by the given amount of time
        /// </summary>
        /// <param name="delay">The time to delay</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delay(float delay) => AddChild(new DelayControl(delay, DelayControl.DelayType.Pull));
        /// <summary>
        /// Stops the arrow for the gievn amount of time
        /// </summary>
        /// <param name="delay">The time to pstop the arrow for</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop(float delay) => AddChild(new DelayControl(delay, DelayControl.DelayType.Stop));
    }
}