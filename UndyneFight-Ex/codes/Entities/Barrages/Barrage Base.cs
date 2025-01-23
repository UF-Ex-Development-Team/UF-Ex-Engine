using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Entities.Player;

namespace UndyneFight_Ex.Entities
{
    /// <summary>
    /// The interface for a collidable instance
    /// </summary>
    public interface ICollideAble
    {
        /// <summary>
        /// The function to check collision with the player
        /// </summary>
        /// <param name="player">The player to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetCollide(Heart player);
    }
    /// <summary>
    /// <para>A parent class for barrage making, contains commonly used variables and functions</para>
    /// This class is not the parent class for <see cref="Arrow"/>
    /// </summary>
    public abstract class Barrage : Entity, ICollideAble, ICustomMotion
    {
        /// <summary>
        /// Whether the barrage counts towards the score
        /// </summary>
        public bool MarkScore { get; set; } = true;
        /// <summary>
        /// The color type of the barrage
        /// </summary>
        public int ColorType { get; set; }
        /// <summary>
        /// <br>The colors for each green soul shield</br>
        /// <br>0-> Blue, 1 -> Red etc</br>
        /// </summary>
        public Color[] ColorTypes { get; set; } = [Color.LightBlue, Color.LightCoral, new(0, 255, 255), new(255, 128, 255)];
        /// <summary>
        /// Whether the barrage will automatically dispose itself when offscreen
        /// </summary>
        public bool AutoDispose { get; set; } = true;
        /// <summary>
        /// Screen bounds
        /// </summary>
        private static readonly CollideRect screen = new(-80, -80, 720, 560);
        private bool _hasBeenInside = false;
        /// <summary>
        /// Whether the barrage will only be displayed inside the box
        /// </summary>
        public bool Hidden { private get; set; } = false;
        /// <summary>
        /// The current <see cref="JudgementState"/> of the chart
        /// </summary>
        public static JudgementState JudgeState => (GameStates.CurrentScene as SongFightingScene).JudgeState;
        /// <inheritdoc/>
        public Func<ICustomMotion, vec2> PositionRoute { get; set; }
        /// <inheritdoc/>
        public Func<ICustomMotion, float> RotationRoute { get; set; }
        /// <inheritdoc/>
        public float[] RotationRouteParam { get; set; }
        /// <inheritdoc/>
        public float[] PositionRouteParam { get; set; }
        /// <inheritdoc/>
        public float AppearTime { get; set; } = 0;
        /// <inheritdoc/>
        public vec2 CentrePosition { get; }
        /// <inheritdoc/>
        public abstract void GetCollide(Heart player);
        /// <inheritdoc/>
        public override void Update()
        {
            AppearTime += 0.5f;
            Centre = PositionRoute?.Invoke(this) ?? Centre;
            Rotation = RotationRoute?.Invoke(this) ?? Rotation;
            controlLayer = Hidden ? Surface.Hidden : Surface.Normal;
            if (AutoDispose)
            {
                bool inside = screen.Contain(Centre);
                if (inside && (!_hasBeenInside))
                    _hasBeenInside = true;
                if (_hasBeenInside && (!inside))
                    Dispose();
            }
        }
        /// <summary>
        /// Creates a shine effect of the barrage
        /// </summary>
        public void CreateShinyEffect() => base.CreateShinyEffect().Depth = Depth + 0.001f;
    }
}