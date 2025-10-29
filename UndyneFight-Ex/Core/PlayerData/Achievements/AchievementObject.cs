using UndyneFight_Ex.Achievements;
using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService;
/// <summary>
/// A singular achievement
/// </summary>
/// <param name="target"></param>
public class AchievementObject(Achievement target) : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => throw new NotImplementedException();
	/// <inheritdoc/>
	public void Load(SaveInfo info) => target.LoadProgress(info.IntValue);
	/// <inheritdoc/>
	public SaveInfo Save() => new($"{target.Title}:value={target.CurrentProgress}");
}