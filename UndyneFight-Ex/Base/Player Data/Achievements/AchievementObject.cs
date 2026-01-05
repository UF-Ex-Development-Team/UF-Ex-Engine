using UndyneFight_Ex.Achievements;
using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService;
/// <summary>
/// A singular achievement
/// </summary>
/// <param name="target"></param>
public class AchievementObject(Achievement target) : ISaveLoad
{
	/// <summary>
	/// The achievement this object represents
	/// </summary>
	public readonly Achievement Achievement = target;
	/// <inheritdoc/>
	public List<ISaveLoad> Children => throw new NotImplementedException();
	/// <inheritdoc/>
	public void Load(SaveInfo info) => Achievement.LoadProgress(info.IntValue);
	/// <inheritdoc/>
	public SaveInfo Save() => new($"{Achievement.Title}:value={Achievement.CurrentProgress}");
}