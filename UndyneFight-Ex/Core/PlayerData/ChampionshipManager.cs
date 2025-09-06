using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService
{
	public class ChampionshipManager : ISaveLoad
	{
		private readonly Dictionary<string, string> championshipData = [];

		public List<ISaveLoad> Children => throw new NotImplementedException();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Load(SaveInfo info)
		{
			foreach (KeyValuePair<string, SaveInfo> v in info.Nexts)
				championshipData.Add(v.Key, v.Value.StringValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SaveInfo Save()
		{
			SaveInfo info = new("ChampionShips{");
			foreach (KeyValuePair<string, string> v in championshipData)
				info.PushNext(new(v.Key + ":" + v.Value));
			return info;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SignUp(string title, string div) => championshipData.Add(title, div);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool InChampionship(string championship) => championshipData.ContainsKey(championship);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ChampionshipDivision(string championship) => string.IsNullOrEmpty(championship) ? null : championshipData.TryGetValue(championship, out string value) ? value : null;
	}
}