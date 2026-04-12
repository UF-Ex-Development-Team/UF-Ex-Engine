using UndyneFight_Ex.Achievements;
using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService;
/// <summary>
/// Data for the user's achievements
/// </summary>
public partial class AchievementManager : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => [];
	/// <summary>
	/// The list of singular achievements
	/// </summary>
	public static Dictionary<string, AchievementObject> AchievementObjects { get; set; } = [];

	/// <inheritdoc/>
	public void Load(SaveInfo info)
	{
		AchievementObjects.Clear();
		foreach (string key in info.Nexts.Keys)
		{
			if (!Achievements.AchievementManager.achievements.TryGetValue(key, out Achievement value))
				continue;
			Insert(value);
			AchievementObjects[key].Load(info.Nexts[key]);
		}
		foreach (KeyValuePair<string, Achievement> achieve in Achievements.AchievementManager.achievements)
		{
			if (!AchievementObjects.ContainsKey(achieve.Key))
				Insert(achieve.Value);
		}
	}
	/// <summary>
	/// Adds a new achievement
	/// </summary>
	/// <param name="achievement">The achievement to add</param>
	public static void Insert(Achievement achievement)
	{
		//Force new achievement to be unachieved due to user IO bug >:(
		achievement.Achieved = false;
		achievement.CurrentProgress = 0;
		AchievementObject obj = new(achievement);
		_ = AchievementObjects.TryAdd(achievement.Title, obj);
	}

	/// <inheritdoc/>
	public SaveInfo Save()
	{
		SaveInfo info = new("Achievements{");
		foreach (Achievement v in Achievements.AchievementManager.achievements.Values)
			info.PushNext(new AchievementObject(v).Save());
		return info;
	}
}