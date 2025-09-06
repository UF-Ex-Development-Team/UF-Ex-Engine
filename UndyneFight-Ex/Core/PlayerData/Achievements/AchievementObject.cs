using UndyneFight_Ex.Achievements;
using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService
{
	public class AchievementObject(Achievement target) : ISaveLoad
	{
		/// <inheritdoc/>
		public List<ISaveLoad> Children => throw new NotImplementedException();

		private Achievement TargetAchievement { get; init; } = target;
		/// <inheritdoc/>
		public void Load(SaveInfo info) => TargetAchievement.LoadProgress(info.IntValue);
		/// <inheritdoc/>
		public SaveInfo Save() => new($"{TargetAchievement.Title}:value={TargetAchievement.CurrentProgress}");
	}
}