using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex.GameInterface
{
    /// <summary>
    /// The classic UF-Ex menu
    /// </summary>
    public static class ClassicalGUI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFightSelector() => GameStates.InstanceCreate(new Entities.FightSelector(GameMode.None));
        /// <summary>
        /// Settings for the game menu
        /// </summary>
        public static class MainMenuSettings
        {
            /// <summary>
            /// Whether there is an option to see recordings
            /// </summary>
            public static bool RecordEnabled { set; internal get; } = false;
            /// <summary>
            /// Whether there is an option to see achievements
            /// </summary>
            public static bool AchievementsEnabled { set; internal get; } = true;
        }
    }
}
