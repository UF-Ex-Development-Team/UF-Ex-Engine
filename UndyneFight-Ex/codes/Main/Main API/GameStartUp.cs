using Microsoft.Xna.Framework.Content;
using UndyneFight_Ex.Achievements;
using UndyneFight_Ex.ChampionShips;
using UndyneFight_Ex.Entities;
using UndyneFight_Ex.UserService;

namespace UndyneFight_Ex.GameInterface
{
    /// <summary>
    /// The game start up event class
    /// </summary>
    public static class GameStartUp
    {
        [Obsolete("Currently unused")]
        public static bool CheckLevelExist { set => FightSystem.CheckLevelExist = value; }
        /// <summary>
        /// Your intro UI
        /// </summary>
        public static Action MainSceneIntro { get; set; } = () => GameStates.InstanceCreate(new IntroUI());

        private static List<Type> MainGameFights;
        /// <summary>
        /// The list of main songs in the game
        /// </summary>
        /// <param name="fights">The list of charts</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMainSongs(List<Type> fights) => MainGameFights = fights;
        /// <summary>
        /// Add main songs to the game
        /// </summary>
        /// <param name="fights">The list of charts</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddMainSongs(List<Type> fights)
        {
            MainGameFights.AddRange(fights);
            GameMain.fights.AddRange(fights);
            FightSystem.Initialize(fights);
        }
        /// <summary>
        /// Adds an item to the shop (It does not exist)
        /// </summary>
        /// <param name="item"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushStoreItem(StoreItem item) => StoreData.AddToItemList(item);
        /// <summary>
        /// Adds a championship
        /// </summary>
        /// <param name="system"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushChampionShip(ChampionShip system) => FightSystem.PushChampionShip(system);
        /// <summary>
        /// Adds a challenge
        /// </summary>
        /// <param name="challenge"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushChallenge(Challenge challenge) => FightSystem.PushChallenge(challenge);
        /// <summary>
        /// Adds an achievement
        /// </summary>
        /// <param name="achievement"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushAchievement(Achievement achievement) => Achievements.AchievementManager.PushAchievement(achievement);
        /// <summary>
        /// Adds extra fights
        /// </summary>
        /// <param name="classicFight"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushExtra(Fight.IExtraOption classicFight) => FightSystem.PushExtra(classicFight);
        /// <summary>
        /// Starts the game
        /// </summary>
        public static void StartGame()
        {
            GameMain.fights = MainGameFights;
            using GameMain game = new();
            game.Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSongset(SongSet songset) => FightSystem.PushSongSet(songset);
        /// <summary>
        /// The title set up display
        /// </summary>
        public static Type SetUpShower
        {
            set => StartingShower.TitleSetUp = value;
        }
        /// <summary>
        /// The title display
        /// </summary>
        public static Type TitleShower
        {
            set => StartingShower.TitleShower = value;
        }
        /// <summary>
        /// The UI used for achievements
        /// </summary>
        public static GameObject AchievementUI;
        /// <summary>
        /// User defined loading screen settings
        /// </summary>
        public static class LoadingSettings
        {
            /// <summary>
            /// The position of the game logo
            /// </summary>
            public static Vector2 TitleCentrePosition { set; internal get; } = new(320, 220);
            /// <summary>
            /// The file path to the game logo sprite
            /// </summary>
            public static string TitleTextureRoot { set; internal get; }
        }
        /// <summary>
        /// Content initialization action
        /// </summary>
        public static Action<ContentManager> Initialize { get; set; }
    }
}
