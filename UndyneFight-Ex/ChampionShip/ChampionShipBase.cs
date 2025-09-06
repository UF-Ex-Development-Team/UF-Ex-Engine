using UFData;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex.ChampionShips
{
	/// <summary>
	/// A championship class
	/// </summary>
	/// <param name="fightSet"></param>
	public class ChampionShip(SongSet fightSet)
	{
		/// <summary>
		/// The list of fights of the championship
		/// </summary>
		public SongSet Fights { get; } = fightSet;
		/// <summary>
		/// The title of the championship
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// The subtitle of the championship (Usually used as the duration of the championship)
		/// </summary>
		public string SubTitle { get; set; }
		/// <summary>
		/// The host of the championship (Useless for now)
		/// </summary>
		public string EditorName { get; set; }
		/// <summary>
		/// The description of the championship
		/// </summary>
		public string Introduce { get; set; }
		/// <summary>
		/// The path to the icon of the championship
		/// </summary>
		public string IconPath { get; set; } = "ChampionShips\\TCS";
		/// <summary>
		/// List of available divisions of the championship
		/// </summary>
		public HashSet<string> DivisionExist { get; set; }
		/// <summary>
		/// The state of the championship
		/// </summary>
		public enum ChampionShipStates
		{
			/// <summary>
			/// The championship has not yet begun
			/// </summary>
			NotStart = 1,
			/// <summary>
			/// The championship is occurring
			/// </summary>
			Starting = 2,
			/// <summary>
			/// The championship is not available
			/// </summary>
			NotAvailable = 3,
			/// <summary>
			/// The championship ended
			/// </summary>
			End = 4
		}
		/// <summary>
		/// The function to check for time (Returns <see cref="ChampionShipStates"/>)
		/// </summary>
		public Func<ChampionShipStates> CheckTime { get; set; }
		/// <summary>
		/// The starting time of the championship
		/// </summary>
		public DateTime Start { get; set; }
		/// <summary>
		/// The ending time of the championship
		/// </summary>
		public DateTime End { get; set; }
		/// <summary>
		/// Converts the championship to info
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ChampionshipInfo ToInfo() => new(Title, Start, End, GetDiv());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Dictionary<string, DivisionInformation> GetDiv()
		{
			Type[] types = Fights.Values;
			IChampionShip[] songs = new IChampionShip[types.Length];
			for (int i = 0; i < types.Length; i++)
				songs[i] = Activator.CreateInstance(types[i]) as IChampionShip;
			Dictionary<string, DivisionInformation> result = [];
			for (int i = 0; i < types.Length; i++)
			{
				foreach (KeyValuePair<string, Difficulty> v in songs[i].DifficultyPanel)
				{
					result.TryAdd(v.Key, new(v.Key, [], new()));
					Dictionary<string, Tuple<int, Difficulty>> map = result[v.Key].Info;
					map.Add(songs[i].GameContent.FightName, new(i, v.Value));
				}
			}
			return result;
		}
	}
}