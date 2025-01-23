using UndyneFight_Ex.UserService;

namespace UndyneFight_Ex.Achievements
{
    public enum CheckerType
    {
        User = 1,
        Song = 2
    }
    public interface IAchievementCheck
    {
        public CheckerType CheckType { get; }
        public int ProgressCheck(object input);
    }
    public class UserDataChecker(Func<User, int> checker) : IAchievementCheck
    {
        private readonly Func<User, int> checker = checker;

        public CheckerType CheckType => CheckerType.User;

        public int ProgressCheck(object input) => checker(input as User);
    }
    public class SongDataChecker(Func<SongSystem.SongPlayData, int> checker) : IAchievementCheck
    {
        private readonly Func<SongSystem.SongPlayData, int> checker = checker;

        public CheckerType CheckType => CheckerType.Song;
        public int ProgressCheck(object input) => checker(input as SongSystem.SongPlayData);
    }
    public class Achievement(string title, string introduction, int totalProgress, IAchievementCheck progressChecker) : IComparable<Achievement>
    {
        public bool CheckProgress(object checkObj)
        {
            bool last = CurrentProgress >= FullProgress && !Locked;
            CurrentProgress = Math.Max(CurrentProgress, ProgressChecker.ProgressCheck(checkObj));
            bool cur = CurrentProgress >= FullProgress && !Locked;
            Achieved = cur;
            bool res = cur && !last;
            if (res)
                OnAchieve?.Invoke(this);
            return res;
        }
        public void LoadProgress(int progress)
        {
            CurrentProgress = progress;
            Achieved = CurrentProgress >= FullProgress;
        }

        internal static event Action<Achievement> OnAchieve;
        /// <summary>
        /// The requirements/description of the achievement
        /// </summary>
        public string AchievementIntroduction { get; set; } = introduction;
        public IAchievementCheck ProgressChecker { private get; set; } = progressChecker;
        /// <summary>
        /// The data to check, either the user or song
        /// </summary>
        public CheckerType CheckType => ProgressChecker.CheckType;
        /// <summary>
        /// The total progress of the achievement
        /// </summary>
        public int FullProgress { get; init; } = totalProgress;
        /// <summary>
        /// Thje title of the achievement
        /// </summary>
        public string Title { get; set; } = title;
        /// <summary>
        /// The current progress of the achievement
        /// </summary>
        public int CurrentProgress { get; private set; } = 0;
        /// <summary>
        /// Whether the achievement had been achieved
        /// </summary>
        public bool Achieved { get; set; }
        public bool OnlineAchieved { get; set; } = false;
        /// <summary>
        /// Whether it is a hidden achievement or not
        /// </summary>
        public bool Hidden { get; init; } = false;
        /// <summary>
        /// Whether the achievement is forcefully disabled
        /// </summary>
        public bool Locked { get; set; } = false;
        /// <summary>
        /// The ID of the achievement
        /// </summary>
        public int ID { private get; set; } = 0;

        public int CompareTo(Achievement other) => other.ID == ID ? other.Title.CompareTo(Title) : other.ID.CompareTo(ID);
    }
    internal static class AchievementManager
    {
        public static Dictionary<string, Achievement> achievements = [];

        public static void CheckUserAchievements()
        {
            if (PlayerManager.CurrentUser == null)
                return;
            foreach (Achievement s in achievements.Values)
                if (!s.Achieved && s.CheckType == CheckerType.User && s.CheckProgress(PlayerManager.CurrentUser))
                    ShowAchieved(s);
        }
        public static void CheckSongAchievements(SongSystem.SongPlayData data)
        {
            if (PlayerManager.CurrentUser == null)
                return;
            foreach (Achievement s in achievements.Values)
                if (!s.Achieved && s.CheckType == CheckerType.Song && s.CheckProgress(data))
                    ShowAchieved(s);
        }
        public static void ShowAchieved(Achievement achievement) => GameStates.InstanceCreate(new AchievementResult(achievement));
        /// <summary>
        /// Adds an achievement
        /// </summary>
        /// <param name="achievement">The achievement to add</param>
        public static void PushAchievement(Achievement achievement) => achievements.Add(achievement.Title, achievement);
    }
}