using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService;

/// <summary>
/// User's statistic
/// </summary>
public class Statistic : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => null;
	/// <summary>
	/// The death count of the user
	/// </summary>
	public int DeathCount { get; private set; } = 0;
	/// <summary>
	/// The play time of the user
	/// </summary>
	public int PlayedTime
	{
		get
		{
			TimeSpan del = DateTime.Now - span;
			playedTime += (float)del.TotalSeconds;
			span = DateTime.Now;
			return (int)playedTime;
		}
	}
	private float playedTime = 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void AddDeath() => DeathCount++;
	private DateTime span = DateTime.Now;
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		DeathCount = info.Nexts["DeathCount"].IntValue;
		playedTime = info.Nexts["PlayedTime"].FloatValue;
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo info = new("Statistic{");
		info.PushNext(new SaveInfo("DeathCount:" + DeathCount));
		info.PushNext(new SaveInfo("PlayedTime:" + MathUtil.FloatToString(playedTime, 4)));
		return info;
	}
}