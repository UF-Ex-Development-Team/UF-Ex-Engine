using UndyneFight_Ex.ChampionShips;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex;

public static class FightSystem
{
	internal static bool CheckLevelExist { get; set; } = true;
	public static void Initialize(List<Type> loadItems)
	{
		if (loadItems == null)
		{
			if (CheckLevelExist)
				throw new Exception("There is no levels in your game!");
			else
				return;
		}
		mainSongs.Clear();
		Dictionary<string, WaveData> mainList = [];
		foreach (Type charts in loadItems)
		{
			mainSongs.Add(charts);
			object obj = Activator.CreateInstance(charts);
			IWaveSet wave = obj is IWaveSet waveSet ? waveSet : ((IChampionShip)obj).GameContent;
			//Clean up
			if (obj is IDisposable disposable)
				disposable.Dispose();
			if (obj is WaveConstructor wc)
				wc.Dispose();
			mainList.Add(wave.FightName, new(wave, obj is IChampionShip champ ? [.. champ.DifficultyPanel.Values] : DefaultWaveDifficulties, false));
		}
		AllWaves = mainList.Concat(AllWaves).ToDictionary(k => k.Key, v => v.Value);
		mainList.Clear();
		mainSongs.ForEach(MainGameSongs.Push);
		mainSongs.ForEach(AllSongs.Push);
	}
	public static Dictionary<string, WaveData> AllWaves { get; internal set; } = [];
	public struct WaveData(IWaveSet wave, Difficulty[] difficulties, bool isChamp)
	{
		public IWaveSet Wave = wave;
		public Difficulty[] Difficulties = difficulties;
		public bool IsChampionShip = isChamp;
	}
	private static readonly Difficulty[] DefaultWaveDifficulties = [Difficulty.Noob, Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Extreme];
	private static readonly List<Type> mainSongs = [];
	/// <summary>
	/// Current song set selected
	/// </summary>
	public static SongSet CurrentSongs { get; private set; }
	/// <summary>
	/// List of all charts
	/// </summary>
	public static SongSet AllSongs { get; private set; } = new SongSet("All");
	/// <summary>
	/// Main charts
	/// </summary>
	public static SongSet MainGameSongs { get; private set; } = new SongSet("MainGameSong");
	/// <summary>
	/// Custom charts
	/// </summary>
	public static SongSet CustomSongs { get; set; } = new SongSet("Custom Charts");
	/// <summary>
	/// Main fights (Essentially unused)
	/// </summary>
	public static FightSet MainGameFights { get; private set; } = new FightSet("MainGameFight");
	/// <summary>
	/// List of championships
	/// </summary>
	public static List<ChampionShip> ChampionShips { get; private set; } = [];
	/// <summary>
	/// Current selected championship
	/// </summary>
	public static ChampionShip CurrentChampionShip { get; internal set; }
	/// <summary>
	/// List of challenges
	/// </summary>
	public static List<Challenge> Challenges { get; internal set; } = [];
	/// <summary>
	/// Challenge dictionary, by title -> challenge
	/// </summary>
	internal static Dictionary<string, Challenge> ChallengeDictionary = [];
	/// <summary>
	/// Other song sets
	/// </summary>
	public static List<SongSet> ExtraSongSets { get; internal set; } = [];
	/// <summary>
	/// Adds a song set
	/// </summary>
	/// <param name="songSet">The song set to add</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void PushSongSet(SongSet songSet)
	{
		foreach (var curSongChart in songSet.Values)
		{
			AllSongs.Push(curSongChart);
			object obj = Activator.CreateInstance(curSongChart);
			IWaveSet wave = obj is IWaveSet waveSet ? waveSet : ((IChampionShip)obj).GameContent;
			//Clean up
			if (obj is IDisposable disposable)
				disposable.Dispose();
			if (obj is WaveConstructor wc)
				wc.Dispose();
			AllWaves.Add(wave.FightName, new(wave, obj is IChampionShip champ ? [.. champ.DifficultyPanel.Values] : DefaultWaveDifficulties, true));
		}
		ExtraSongSets.Add(songSet);
	}
	/// <summary>
	/// Removes a song set
	/// </summary>
	/// <param name="songSet">The song set to remove</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RemoveSongSet(SongSet songSet) => ExtraSongSets.Remove(songSet);
	/// <summary>
	/// Adds a championship
	/// </summary>
	/// <param name="championShip">The championship to add</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void PushChampionShip(ChampionShip championShip)
	{
		Type[] championships = championShip.Fights.Values;
		championShip.WaveList = new IWaveSet[championships.Length];
		for (int i = 0; i < championships.Length; i++)
		{
			AllSongs.Push(championships[i]);
			object obj = Activator.CreateInstance(championships[i]);
			IWaveSet wave = obj is IWaveSet waveSet ? waveSet : ((IChampionShip)obj).GameContent;
			//Clean up
			if (obj is IDisposable disposable)
				disposable.Dispose();
			if (obj is WaveConstructor wc)
				wc.Dispose();
			championShip.WaveList[i] = wave;
			AllWaves.Add(wave.FightName, new(wave, obj is IChampionShip champ ? [.. champ.DifficultyPanel.Values] : DefaultWaveDifficulties, true));
		}
		ChampionShips.Add(championShip);
	}
	/// <summary>
	/// Adds a challenge
	/// </summary>
	/// <param name="challenge">The challenge to add</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void PushChallenge(Challenge challenge)
	{
		Challenges.Add(challenge);
		ChallengeDictionary.Add(challenge.Title, challenge);
	}
	/// <summary>
	/// Adds a fight
	/// </summary>
	/// <param name="classicFight">The fight to add</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void PushExtra(Fight.IExtraOption classicFight) => MainGameFights.Push(classicFight.GetType());
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void SelectSongSet(ChampionShip championShip) => CurrentSongs = championShip.Fights;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void SelectMainSet()
	{
		CurrentChampionShip = null;
		CurrentSongs = MainGameSongs;
	}
	/// <summary>
	/// Gets all the playable charts
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<Type> GetPlayableCharts()
	{
		List<Type> result = [.. MainGameSongs.Values];
		foreach (SongSet s in ExtraSongSets)
			result.AddRange(s.Values);
		foreach (ChampionShip c in ChampionShips)
			if (c.CheckTime() == ChampionShip.ChampionShipStates.End)
				result.AddRange(c.Fights.Values);
		return result;
	}
}