using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService;

public class ChampionshipManager : ISaveLoad
{
	private readonly Dictionary<string, string> championshipData = [];

	/// <inheritdoc/>
	public List<ISaveLoad> Children => throw new NotImplementedException();
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		foreach (KeyValuePair<string, SaveInfo> v in info.Nexts)
			championshipData.Add(v.Key, v.Value.StringValue);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo info = new("ChampionShips{");
		foreach (KeyValuePair<string, string> v in championshipData)
			info.PushNext(new(v.Key + ":" + v.Value));
		return info;
	}
	/// <summary>
	/// Signs the user to a championship
	/// </summary>
	/// <param name="title">The title of the championship</param>
	/// <param name="div">The division the user takes part in</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SignUp(string title, string div) => championshipData.Add(title, div);
	/// <summary>
	/// Whether the user has participated in a championship
	/// </summary>
	/// <param name="championship">The name of the championship</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InChampionship(string championship) => championshipData.ContainsKey(championship);
	/// <summary>
	/// Gets the division the user participated in in the championship
	/// </summary>
	/// <param name="championship">The name of the championship</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ChampionshipDivision(string championship) => string.IsNullOrEmpty(championship) ? null : championshipData.TryGetValue(championship, out string value) ? value : null;
}