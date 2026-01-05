using UndyneFight_Ex.UserService;

namespace UndyneFight_Ex.Achievements;
/// <summary>
/// The type of data to check for the achievement
/// </summary>
public enum CheckerType
{
	/// <summary>
	/// To check user data
	/// </summary>
	User = 1,
	/// <summary>
	/// To check chart data
	/// </summary>
	Song = 2
}
/// <summary>
/// Interface for achievement progress checkers
/// </summary>
public interface IAchievementCheck
{
	/// <summary>
	/// The type of data to check
	/// </summary>
	CheckerType CheckType { get; }
	/// <summary>
	/// The method to use to evaluate the progress
	/// </summary>
	/// <param name="input">Whatever argument should be used for evaluation</param>
	/// <returns></returns>
	int ProgressCheck(object input);
}
/// <summary>
/// An achievement checker for user data
/// </summary>
/// <param name="checker">The function to use</param>
public class UserDataChecker(Func<User, int> checker) : IAchievementCheck
{
	private readonly Func<User, int> checker = checker;
	/// <inheritdoc/>
	public CheckerType CheckType => CheckerType.User;
	/// <inheritdoc/>
	public int ProgressCheck(object input) => checker((User)input);
}
/// <summary>
/// An achievement checker for chart data
/// </summary>
/// <param name="checker">The function to use</param>
public class SongDataChecker(Func<SongSystem.SongPlayData, int> checker) : IAchievementCheck
{
	private readonly Func<SongSystem.SongPlayData, int> checker = checker;

	/// <inheritdoc/>
	public CheckerType CheckType => CheckerType.Song;
	/// <inheritdoc/>
	public int ProgressCheck(object input) => checker((SongSystem.SongPlayData)input);
}
/// <summary>
/// The achievement class
/// </summary>
/// <param name="title">The title of the achievement</param>
/// <param name="introduction">The description of the achievement</param>
/// <param name="totalProgress">The total progress of the achievement</param>
/// <param name="progressChecker">The class to check the progress with</param>
public class Achievement(string title, string introduction, int totalProgress, IAchievementCheck progressChecker)
{
	/// <summary>
	/// Checks whether the achievement is just achieved
	/// </summary>
	/// <param name="checkObj">The argument to pass into the <see cref="IAchievementCheck.ProgressCheck(object)"/></param>
	/// <returns></returns>
	public bool CheckProgress(object checkObj)
	{
		bool last = CurrentProgress >= FullProgress && !Locked;
		bool res = (Achieved = (CurrentProgress = Math.Max(CurrentProgress, ProgressChecker.ProgressCheck(checkObj))) >= FullProgress && !Locked) && !last;
		if (res)
			OnAchieve?.Invoke(this);
		return res;
	}
	/// <summary>
	/// Updates the progress when loading from a save file
	/// </summary>
	/// <param name="progress">The progress fetched from the save file</param>
	internal void LoadProgress(int progress) => Achieved = (CurrentProgress = progress) >= FullProgress;

	internal static event Action<Achievement> OnAchieve;
	/// <summary>
	/// The description of the achievement
	/// </summary>
	public string AchievementIntroduction { get; set; } = introduction;
	/// <summary>
	/// The class to check the progress with
	/// </summary>
	public IAchievementCheck ProgressChecker { private get; set; } = progressChecker;
	/// <summary>
	/// The type of data to check
	/// </summary>
	public CheckerType CheckType => ProgressChecker.CheckType;
	/// <summary>
	/// The total progress of the achievement
	/// </summary>
	public int FullProgress { get; init; } = totalProgress;
	/// <summary>
	/// The title of the achievement
	/// </summary>
	public string Title { get; set; } = title;
	/// <summary>
	/// The current progress of the achievement
	/// </summary>
	public int CurrentProgress { get; internal set; } = 0;
	/// <summary>
	/// Whether the achievement had been achieved
	/// </summary>
	public bool Achieved { get; set; }
	/// <summary>
	/// Whether it is a hidden achievement or not
	/// </summary>
	public bool Hidden { get; init; } = false;
	/// <summary>
	/// Whether the achievement is forcefully disabled
	/// </summary>
	public bool Locked { get; set; } = false;
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