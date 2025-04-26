using UndyneFight_Ex.Entities;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex.GameInterface
{
    /// <summary>
    /// The settings for the base UF-Ex engine
    /// </summary>
    public static class UFEXSettings
    {
        /// <summary>
        /// Whether recording is enabled (It is not advised to set this to true)
        /// </summary>
        public static bool RecordEnabled { get; set; }
        [Obsolete("Gamejolt support is currently removed")]
        public static string GamejoltPrivateKey { get; set; }
        [Obsolete("Gamejolt support is currently removed")]
        public static int GamejoltID { get; set; }
        [Obsolete("Why would you ever do this")]
        public static string MainServerURL { get; set; }
        [Obsolete("Why would you ever do this")]
        public static int MainServerPort { get; set; }
        /// <summary>
        /// Actions to invoke when a chart is completed
        /// </summary>
        public static Action<SongPlayData> OnSongComplete { get; set; }
        /// <summary>
        /// Custom update action
        /// </summary>
        public static event Action Update;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DoUpdate() => Update?.Invoke();
        internal static Func<StateShower, Player.Analyzer, GameObject> SongCompleteCreate { get; set; } = (s, t) => new StateShower.ResultShower(s, t);
    }
}
