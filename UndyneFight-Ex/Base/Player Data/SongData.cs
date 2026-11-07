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
		public bool FC { get; private set; }
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
		public void UpdateNew(SongResult result, int prevScore, float prevAcc)
		{
			scoreData.PrevScore = prevScore;
			scoreData.PrevAcc = prevAcc;
			Score = Math.Max(result.Score, Score);
			FC |= result.FC;
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
			FC = info["AC"] == "true";
			AP = info["AP"] == "true";
			Accuracy = MathHelper.Clamp(info[info.keysForIndexes.ContainsKey("Accuracy") ? "Accuracy" : "Acc"].ToFloat(), 0, 1);
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
			FC = result.FC;
			AP = (Mark = result.CurrentMark) == SkillMark.Impeccable;
		}
		/// <summary>
		/// Converts the song state to save info
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SaveInfo ToInfo() => new(GetShorthandName(difficulty) + ":score=" + Score +
				",AC=" + (FC ? "true" : "false") +
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
		foreach ((string name, SaveInfo state) in info.Nexts)
			CurrentSongStates.Add(ToDif(name), new SongState(state));
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
	public void UpdateNew(Difficulty dif, SongResult result, int prevScore, float prevAcc)
	{
		_ = CurrentSongStates.TryAdd(dif, new SongState(dif, result));
		CurrentSongStates[dif].UpdateNew(result, prevScore, prevAcc);
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
	public SongData? Acquire(string name) => songData.TryGetValue(name, out SongData data) ? data : null;
	/// <summary>
	/// Checks whether a chart was played
	/// </summary>
	/// <param name="curFight">The name of the chart</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool SongPlayed(string curFight) => songData.ContainsKey(curFight);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void FinishedSong(string songName, Difficulty difficulty, SongResult result, int prevScore, float prevAcc)
	{
		_ = songData.TryAdd(songName, new SongData(songName));
		songData[songName].UpdateNew(difficulty, result, prevScore, prevAcc);
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		foreach ((string name, SaveInfo state) in info.Nexts)
		{
			songData.Add(name, new SongData(name));
			songData[name].Load(state);
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
		/// Data of a single song
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
	private static (float CompleteDiff, float ComplexDiff, float APDiff) GetDifficulty(IWaveSet waveSet, Difficulty difficulty)
	{
		SongInformation Information = waveSet.Attributes;

		float dif1 = 0, dif2 = 0, dif3 = 0;

		_ = Information?.CompleteDifficulty.TryGetValue(difficulty, out dif1);
		_ = Information?.ComplexDifficulty.TryGetValue(difficulty, out dif2);
		_ = Information?.APDifficulty.TryGetValue(difficulty, out dif3);

		return (dif1, dif2, dif3);
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
		Dictionary<string, FightSystem.WaveData> songType = FightSystem.AllWaves;
		foreach (FightSystem.WaveData waveDat in songType.Values)
		{
			IWaveSet waveSet = waveDat.Wave;
			for (int j = 0; j <= 5; j++)
			{
				(float CompleteDiff, float ComplexDiff, float APDiff) = GetDifficulty(waveSet, (Difficulty)j);
				completeMax = MathF.Max(completeMax, CompleteDiff);
				fcMax = MathF.Max(fcMax, APDiff);
				apMax = MathF.Max(apMax, APDiff);
				_ = all.Add(ComplexDiff);
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
			foreach ((Difficulty dif, SongData.SongState cur) in song.CurrentSongStates)
			{
				(float CompleteDiff, float ComplexDiff, float APDiff) = GetDifficulty(songType[song.SongName].Wave, dif);

				_ = best7.Add(new(song.SongName, dif, cur.Accuracy, ComplexDiff, ReRate(cur.Accuracy), 85 / ideal));
				if (best7.Count >= 8)
					_ = best7.Remove(best7.Min);
				if (cur.Mark != SkillMark.Failed)
					comp1 = SelectLarge(comp1, new(song.SongName, dif, cur.Accuracy, CompleteDiff, 1.0f, 5 / completeMax));
				if (cur.AP)
					ap1 = SelectLarge(ap1, new(song.SongName, dif, cur.Accuracy, APDiff, 1.0f, 5 / apMax));
				if (cur.FC)
					fc1 = SelectLarge(fc1, new(song.SongName, dif, cur.Accuracy, APDiff, 1.0f, 5 / fcMax));
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
		float ap1 = 0,  comp1 = 0, fc1 = 0,
			apMax = 0, fcMax = 0, completeMax = 0;
		List<float> alls = [];
		Dictionary<string, FightSystem.WaveData> songType = FightSystem.AllWaves;
		foreach (FightSystem.WaveData waveData in songType.Values)
		{
			IWaveSet waveSet = waveData.Wave;
			for (int j = 0; j <= 5; j++)
			{
				(float CompleteDiff, float ComplexDiff, float APDiff) = GetDifficulty(waveSet, (Difficulty)j);
				completeMax = MathF.Max(completeMax, CompleteDiff);
				fcMax = MathF.Max(fcMax, APDiff);
				apMax = MathF.Max(apMax, APDiff);
				alls.Add(ComplexDiff);
			}
		}
		foreach (SongData i in _songManager.AllData)
		{
			SongData song = i;
			foreach ((Difficulty curDiff, SongData.SongState curState) in song.CurrentSongStates)
			{
				if (!songType.ContainsKey(song.SongName))
					continue;
				(float CompleteDiff, float ComplexDiff, float APDiff) = GetDifficulty(songType[song.SongName].Wave, curDiff);

				_ = best7.Add(ComplexDiff * ReRate(curState.Accuracy) + MathUtil.GetRandom(-0.00001f, 0.00001f));
				if (curState.Mark != SkillMark.Failed)
					comp1 = MathF.Max(comp1, CompleteDiff);
				if (curState.AP)
					ap1 = MathF.Max(ap1, APDiff);
				if (curState.FC)
					fc1 = MathF.Max(fc1, APDiff);
				//Achievement logic
				if (PlayerManager.CurrentUser == null)
					continue;
				SongPlayData playData = new()
				{
					Result = new(curState.Mark, curState.Score, curState.Accuracy, curState.FC, curState.AP),
					Name = song.SongName,
					GameMode = GameMode.None,
					CompleteThreshold = CompleteDiff,
					ComplexThreshold = ComplexDiff,
					APThreshold = APDiff,
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
		float best7Rating = sum / ideal * 85f;
		float fcRating = fc1 / fcMax * 5f;
		float apRating = ap1 / apMax * 5f;
		float compRating = comp1 / completeMax * 5f;
		return new(best7Rating + fcRating + apRating + compRating, sum + fc1 + ap1 + comp1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static float ReRate(float accuracy)
	{
		if (accuracy > 1)
			return 1;
		float del = 1 - accuracy;
		float lim = MathF.Pow(del * 3, 0.7f) / 2.4f + del * 2.0f;
		return MathF.Max(0, 1 - lim);
	}
}