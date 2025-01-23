using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities.Advanced
{
    /// <summary>
    /// Shakes the screen
    /// </summary>
    public class ScreenShaker : GameObject
    {
        internal static Vector2 ScreenShakeDelta { get => CurrentSetting.shakings; set => CurrentSetting.shakings = value; }
        readonly Vector2[] shakeVector;
        /// <summary>
        /// Creates a screen shaker
        /// </summary>
        /// <param name="shakeCount">The amount of times to shake</param>
        /// <param name="shakeIndensity">The intensity (in pixels) of the shaking</param>
        /// <param name="shakeDelay">The delay between each shake</param>
        /// <param name="startAngle">The initial angle of the shaking</param>
        /// <param name="angleDelta">The angle difference between each shake</param>
        /// <param name="shakeFriction">The percentage decrease of the intensity of each shake</param>
        public ScreenShaker(int shakeCount, float shakeIndensity, float shakeDelay, float? startAngle = null, float? angleDelta = null, float shakeFriction = 0.85f)
        {
            UpdateIn120 = true;
            float curAngle = startAngle ?? GetRandom(0, 359f);
            shakeVector = new Vector2[shakeCount + 1];
            for (int i = 0; i < shakeCount; i++)
            {
                shakeVector[i] = GetVector2(shakeIndensity, curAngle += angleDelta ?? GetRandom(120, 240f));
                shakeIndensity *= shakeFriction;
            }
            shakeVector[shakeCount] = Vector2.Zero;
            this.shakeDelay = shakeDelay;
        }

        private float appearTime = 0;
        private readonly float shakeDelay;
        int _index = 0;
        public override void Update()
        {
            if (appearTime >= shakeDelay)
            {
                appearTime -= shakeDelay;
                if (++_index >= shakeVector.Length)
                {
                    Dispose();
                    return;
                }
            }
            float movePercent = (0.4f / shakeDelay + 0.6f) * 0.7f;
            ScreenShakeDelta = ScreenShakeDelta * (1 - movePercent) + shakeVector[_index] * movePercent;
            appearTime += 0.5f;
        }
        public override void Dispose()
        {
            ScreenShakeDelta = Vector2.Zero;
            base.Dispose();
        }
    }
}
