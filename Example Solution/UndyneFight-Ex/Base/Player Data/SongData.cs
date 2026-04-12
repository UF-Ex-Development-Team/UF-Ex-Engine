using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.MiscUtil;

namespace UndyneFight_Ex.UserService;

/// <summary>
/// Song data of a chart
/// </summary>
/// <param name="name">The chart to check (Use <see cref="IWaveSet.FightName"/>)</param>
public class SongData(string name) : ISaveLoad
{
	/// <summary>
	/// The chart results
	/// </summary>
	public class SongState
	{
		private readonly Difficulty difficulty;
		/// <summary>
		/// The score of the chart
		/// </summary>
		public int Score { get; private set; }
		/// <summary>
		/// The skill mark of the chart
		/// </summary>
		public SkillMark Mark { get; private set; }
		/// <summary>
		/// Whether the chart was completed in Full Combo
		/// </summary>
		public bool AC { get; private set; }
		/// <summary>
		/// Whether the chart was completed in All Perfect
		/// </summary>
		public bool AP { get; private set; }
		/// <summary>
		/// The accuracy of the chart
		/// </summary>
		public float Accuracy { get; set; }
		internal struct ScoreData
		{
			public int PrevScore;
			public float PrevAcc;
		}
		internal static ScoreData scoreData = new();
		/// <summary>
		/// Updates the user's song data
		/// </summary>
		/// <param name="result">The current result</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateNew(SongResult result)
		{
			scoreData.PrevScore = Score;
			scoreData.PrevAcc = Accuracy;
			Score = Math.Max(result.Score, Score);
			AC |= result.AC;
			AP |= result.AP;
			Mark = (SkillMark)Math.Min((int)Mark, (int)result.CurrentMark);
			Accuracy = MathF.Max(Accuracy, result.Accuracy);
		}
		/// <summary>
		/// Gets a song state based on the save info
		/// </summary>
		/// <param name="info">The info to read</param>
		public SongState(SaveInfo info)
		{
			difficulty = ToDif(info.Title);
			AC = info["AC"] == "true";
			AP = info["AP"] == "true";
			Accuracy = MathHelper.Clamp(MathUtil.FloatFromString(info[info.keysForIndexes.ContainsKey("Accuracy") ? "Accuracy" : "Acc"]), 0, 1);
			Score = Convert.ToInt32(info["score"]);
			Mark = ToMark(info["mark"]);
		}
		/// <summary>
		/// Gets the song state based on the song result
		/// </summary>
		/// <param name="dif">The chart difficulty</param>
		/// <param name="result">The current result</param>
		public SongState(Difficulty dif, SongResult result)
		{
			difficulty = dif;
			Score = result.CurrentMark == SkillMark.Failed ? result.Score / 2 : result.Score;
			AC = result.AC;
			AP = (Mark = result.CurrentMark) == SkillMark.Impeccable;
		}
		/// <summary>
		/// Converts the song state to save info
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SaveInfo ToInfo() => new(GetShorthandName(difficulty) + ":score=" + Score +
				",AC=" + (AC ? "true" : "false") +
				",AP=" + (AP ? "true" : "false") +
				",Acc=" + MathUtil.FloatToString(Accuracy, 5) +
				",mark=" + GetShorthandName(Mark));
	}
	/// <summary>
	/// The name of the song (<see cref="IWaveSet.FightName"/>)
	/// </summary>
	public string SongName { get; } = name;
	/// <inheritdoc/>
	public List<ISaveLoad> Children => null;
	/// <summary>
	/// The list of <see cref="SongState"/>s for each difficulty
	/// </summary>
	public Dictionary<Difficulty, SongState> CurrentSongStates { get; } = [];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static SkillMark ToMark(string s) => s switch
	{
		"Impeccable" or "Im" => SkillMark.Impeccable,
		"Eminent" or "Em" => SkillMark.Eminent,
		"Excellent" or "Ex" => SkillMark.Excellent,
		"Respectable" or "Re" => SkillMark.Respectable,
		"Acceptable" or "Acc" => SkillMark.Acceptable,
		"Ordinary" or "Ord" => SkillMark.Ordinary,
		"Failed" or "F" => SkillMark.Failed,
		_ => throw new NotImplementedException()
	};
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Difficulty ToDif(string s) => s switch
	{
		"Noob" or "Nb" => Difficulty.Noob,
		"Easy" or "Ez" => Difficulty.Easy,
		"Normal" or "Nr" => Difficulty.Normal,
		"Hard" or "Hd" => Difficulty.Hard,
		"Extreme" or "Ex" => Difficulty.Extreme,
		"ExtremePlus" or "Ex+" => Difficulty.ExtremePlus,
		_ => throw new NotImplementedException()
	};
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		foreach (KeyValuePair<string, SaveInfo> v in info.Nexts)
			CurrentSongStates.Add(ToDif(v.Key), new SongState(v.Value));
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private SaveInfo GetInformation(Difficulty difficulty) => CurrentSongStates[difficulty].ToInfo();
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo info = new(SongName + "{");
		foreach (Difficulty diff in CurrentSongStates.Keys)
			if (CurrentSongStates[diff].Score > 0)
				info.PushNext(GetInformation(diff));
		return info;
	}
	/// <summary>
	/// Updates the song state, if it does not exist, create one
	/// </summary>
	/// <param name="dif"></param>
	/// <param name="result"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UpdateNew(Difficulty dif, SongResult result)
	{
		_ = CurrentSongStates.TryAdd(dif, new SongState(dif, result));
		CurrentSongStates[dif].UpdateNew(result);
	}
}
/// <summary>
/// Song data manager
/// </summary>
public class SongManager : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => null;
	/// <summary>
	/// All song data
	/// </summary>
	public IEnumerable<SongData> AllData => songData.Values;
	private readonly Dictionary<string, SongData> songData = [];
	/// <summary>
	/// Gets a specific song data
	/// </summary>
	/// <param name="name">The name of the song</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SongData Acquire(string name) => songData[name];
	/// <summary>
	/// Checks whether a chart was played
	/// </summary>
	/// <param name="curFight">The name of the chart</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool SongPlayed(string curFight) => songData.ContainsKey(curFight);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void FinishedSong(string songName, Difficulty difficulty, SongResult result)
	{
		songData.TryAdd(songName, new SongData(songName));
		songData[songName].UpdateNew(difficulty, result);
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		foreach (KeyValuePair<string, SaveInfo> v in info.Nexts)
		{
			songData.Add(v.Key, new SongData(v.Key));
			songData[v.Key].Load(v.Value);
		}
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo info = new("NormalFights{");
		foreach (SongData data in songData.Values)
			info.PushNext(data.Save());
		return info;
	}
}
/// <summary>
/// A rating calculator to calculate the rating of a user by their rating
/// </summary>
/// <param name="songManager"></param>
public class RatingCalculator(SongManager songManager)
{
	/// <summary>
	/// A list of rating
	/// </summary>
	public class RatingList
	{
		/// <summary>
		/// Data pf a single song
		/// </summary>
		/// <param name="name">The name of the song</param>
		/// <param name="difficulty">The difficulty of the song played</param>
		/// <param name="accuracy">The accuracy of the user</param>
		/// <param name="threshold">The complex difficulty of the song</param>
		/// <param name="transferAccuracy">The adjusted accuracy</param>
		/// <param name="scoreScale">The rating scale</param>
		public struct SingleSong(string name, Difficulty difficulty, float accuracy, float threshold, float transferAccuracy, float scoreScale) : IComparable
		{
			/// <summary>
			/// The difficulty of the song
			/// </summary>
			public Difficulty difficulty = difficulty;
			/// <summary>
			/// The accuracy of the song
			/// </summary>
			public float accuracy = accuracy;
			/// <summary>
			/// The complex difficulty of the song
			/// </summary>
			public float threshold = threshold;
			/// <summary>
			/// The adjusted accuracy
			/// </summary>
			public float transferAccuracy = transferAccuracy;
			/// <summary>
			/// The final rating of the song
			/// </summary>
			public float scoreResult = threshold * transferAccuracy * scoreScale;
			/// <summary>
			/// The name of the song
			/// </summary>
			public string name = name;

			/// <inheritdoc/>
			public readonly int CompareTo(object obj)
			{
				if (obj is SingleSong song)
				{
					int v = scoreResult.CompareTo(song.scoreResult);
					return v != 0 ? v : name.CompareTo(song.name);
				}
				return 0;
			}
		}
		/// <summary>
		/// The 7 charts of highest score accuracy
		/// </summary>
		public SortedSet<SingleSong> StrictDonors { get; private set; } = [];
		private SingleSong completeDonor, fcDonor, apDonor;
		/// <summary>
		/// The chart the user has cleared that has the highest clear constant
		/// </summary>
		public SingleSong CompleteDonor => completeDonor;
		/// <summary>
		/// The chart the user has FCd that has the highest AP constant
		/// </summary>
		public SingleSong FCDonor => fcDonor;
		/// <summary>
		/// The chart the user has APd that has the highest AP constant
		/// </summary>
		public SingleSong APDonor => apDonor;
		/// <summary>
		/// Stores the rating list
		/// </summary>
		/// <param name="strictDonors">The list of highest accuracy charts</param>
		/// <param name="completeDonor">The cleared chart with highest clear constant</param>
		/// <param name="fcDonor">The FCd chart with highest AP constant</param>
		/// <param name="apDonor">The APd chart with highest AP constant</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Submit(IEnumerable<SingleSong> strictDonors, SingleSong completeDonor, SingleSong fcDonor, SingleSong apDonor)
		{
			this.completeDonor = completeDonor;
			this.fcDonor = fcDonor;
			this.apDonor = apDonor;
			foreach (SingleSong song in strictDonors)
				_ = StrictDonors.Add(song);
		}
	}

	private readonly SongManager _songManager = songManager;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Tuple<float, float, float> GetDifficulty(IWaveSet waveSet, Difficulty difficulty)
	{
		SongInformation Information = waveSet.Attributes;

		float dif1 = 0, dif2 = 0, dif3 = 0;

		_ = Information?.CompleteDifficulty.TryGetValue(difficulty, out dif1);
		_ = Information?.ComplexDifficulty.TryGetValue(difficulty, out dif2);
		_ = Information?.APDifficulty.TryGetValue(difficulty, out dif3);

		return new(dif1, dif2, dif3);
	}

	/// <summary>
	/// Generates a rating list
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RatingList GenerateList()
	{
		RatingList.SingleSong ap1 = new("NULL", Difficulty.Noob, 0, 0, 0, 0);
		RatingList.SingleSong comp1 = new("NULL", Difficulty.Noob, 0, 0, 0, 0);
		RatingList.SingleSong fc1 = new("NULL", Difficulty.Noob, 0, 0, 0, 0);

		float apMax = 0, fcMax = 0, completeMax = 0;
		SortedSet<float> all = [];
		Dictionary<string, IWaveSet> songType = GlobalData.WaveCache;
		foreach (IWaveSet waveSet in songType.Values)
		{
			for (int j = 0; j <= 5; j++)
			{
				Tuple<float, float, float> chartDiffs = GetDifficulty(waveSet, (Difficulty)j);
				completeMax = MathF.Max(completeMax, chartDiffs.Item1);
				fcMax = MathF.Max(fcMax, chartDiffs.Item3);
				apMax = MathF.Max(apMax, chartDiffs.Item3);
				_ = all.Add(chartDiffs.Item2);
			}
		}
		for (int i = 0; all.Count < 7; i++)
			_ = all.Add(0 - i * 0.0001f);
		float ideal = 0.001f;
		for (int i = 0; i < 7; i++)
		{
			float g = MathF.Max(0, all.Max);
			_ = all.Remove(g);
			ideal += g;
		}

		SortedSet<RatingList.SingleSong> best7 = [];
		static RatingList.SingleSong SelectLarge(RatingList.SingleSong x, RatingList.SingleSong y) => x.scoreResult > y.scoreResult ? x : y;
		foreach (SongData i in _songManager.AllData)
		{
			SongData song = i;
			if (!songType.ContainsKey(song.SongName))
				continue;
			foreach (KeyValuePair<Difficulty, SongData.SongState> j in song.CurrentSongStates)
			{
				SongData.SongState cur = j.Value;
				Tuple<float, float, float> dif = GetDifficulty(songType[song.SongName], j.Key);

				_ = best7.Add(new(song.SongName, j.Key, cur.Accuracy, dif.Item2, ReRate(cur.Accuracy), 85 / ideal));
				if (best7.Count >= 8)
					_ = best7.Remove(best7.Min);
				if (cur.Mark != SkillMark.Failed)
					comp1 = SelectLarge(comp1, new(song.SongName, j.Key, cur.Accuracy, dif.Item1, 1.0f, 5 / completeMax));
				if (cur.AP)
					ap1 = SelectLarge(ap1, new(song.SongName, j.Key, cur.Accuracy, dif.Item3, 1.0f, 5 / apMax));
				if (cur.AC)
					fc1 = SelectLarge(fc1, new(song.SongName, j.Key, cur.Accuracy, dif.Item3, 1.0f, 5 / fcMax));
			}
		}
		RatingList result = new();
		while (best7.Count >= 8)
			_ = best7.Remove(best7.Min);
		result.Submit(best7, comp1, fc1, ap1);
		return result;
	}
	/// <summary>
	/// Calculates the rating
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2 CalculateRating()
	{
		SortedSet<float> best7 = [];
		float ap1 = 0;
		float comp1 = 0;
		float fc1 = 0;

		float apMax = 0, fcMax = 0, completeMax = 0;
		List<float> alls = [];
		Dictionary<string, IWaveSet> songType = GlobalData.WaveCache;
		foreach (IWaveSet waveSet in songType.Values)
		{
			for (int j = 0; j <= 5; j += 1)
			{
				Tuple<float, float, float> v = GetDifficulty(waveSet, (Difficulty)j);
				completeMax = MathF.Max(completeMax, v.Item1);
				fcMax = MathF.Max(fcMax, v.Item3);
				apMax = MathF.Max(apMax, v.Item3);
				alls.Add(v.Item2);
			}
		}
		foreach (SongData i in _songManager.AllData)
		{
			SongData song = i;
			foreach (KeyValuePair<Difficulty, SongData.SongState> j in song.CurrentSongStates)
			{
				Difficulty curDiff = j.Key;
				SongData.SongState curState = j.Value;
				if (!songType.ContainsKey(song.SongName))
					continue;
				Tuple<float, float, float> dif = GetDifficulty(songType[song.SongName], curDiff);

				_ = best7.Add(dif.Item2 * ReRate(curState.Accuracy) + MathUtil.GetRandom(-0.00001f, 0.00001f));
				if (curState.Mark != SkillMark.Failed)
					comp1 = MathF.Max(comp1, dif.Item1);
				if (curState.AP)
					ap1 = MathF.Max(ap1, dif.Item3);
				if (curState.AC)
					fc1 = MathF.Max(fc1, dif.Item3);
				//Achievement logic
				if (PlayerManager.CurrentUser == null)
					continue;

				SongResult res = new(curState.Mark, curState.Score, curState.Accuracy, curState.AC, curState.AP);
				SongInformation att = songType[song.SongName].Attributes;
				SongPlayData playData = (att?.ComplexDifficulty.ContainsKey(curDiff) ?? false)
				? new SongPlayData()
				{
					Result = res,
					Name = song.SongName,
					GameMode = GameMode.None,
					CompleteThreshold = att.CompleteDifficulty[curDiff],
					ComplexThreshold = att.ComplexDifficulty[curDiff],
					APThreshold = att.APDifficulty[curDiff],
					Difficulty = curDiff
				}
				: new SongPlayData()
				{
					Result = res,
					Name = song.SongName,
					GameMode = GameMode.None,
					CompleteThreshold = 0,
					ComplexThreshold = 0,
					APThreshold = 0,
					Difficulty = curDiff
				};
				Achievements.AchievementManager.CheckSongAchievements(playData);
			}
		}
		for (int i = 0; best7.Count < 7; i++)
			_ = best7.Add(0 - i * 0.00001f);
		for (int i = 0; alls.Count < 7; i++)
			alls.Add(0 - i * 0.00001f);
		float sum = 0.001f, ideal = 0.001f;
		for (int i = 0; i < 7; i++)
		{
			float f = MathF.Max(0, best7.Max), g = MathF.Max(0, alls.Max());
			_ = best7.Remove(f);
			_ = alls.Remove(g);
			ideal += g;
			sum += f;
		}
		float rating0 = sum / ideal * 85f;
		float rating1 = fc1 / fcMax * 5f;
		float rating2 = ap1 / apMax * 5f;
		float rating3 = comp1 / completeMax * 5f;
		return new(rating0 + rating1 + rating2 + rating3, sum + fc1 + ap1 + comp1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float ReRate(float accuracy)
	{
		if (accuracy > 1)
			return 1;
		float del = 1 - accuracy;
		float lim = MathF.Pow(del * 3, 0.7f) / 2.4f + del * 2.0f;
		return MathF.Max(0, 1 - lim);
	}
}