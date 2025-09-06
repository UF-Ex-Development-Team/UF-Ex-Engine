using System.Text.Json.Serialization;
using static UndyneFight_Ex.MiscUtil;

namespace UndyneFight_Ex.SongSystem
{
	/// <summary>  
	///Impeccable    -> All Perfect <br></br>
	///Eminent       -> No Hit + 99% score<br></br>
	///Excellent     -> No Hit + 98% score<br></br>
	///Respectable   -> 96% score<br></br>
	///Acceptable    -> 92% score<br></br>
	///Ordinary      -> 75% score<br></br>
	/// </summary>
	public enum SkillMark
	{
		[ShorthandName("Im")]
		Impeccable = 1,
		[ShorthandName("Em")]
		Eminent = 2,
		[ShorthandName("Ex")]
		Excellent = 3,
		[ShorthandName("Re")]
		Respectable = 4,
		[ShorthandName("Acc")]
		Acceptable = 5,
		[ShorthandName("Ord")]
		Ordinary = 6,
		[ShorthandName("F")]
		Failed = 7
	}
	public enum JudgementState
	{
		Lenient = 1,
		Balanced = 2,
		Strict = 3
	}
	public class SongPlayData
	{
		public SongResult Result { get; set; } = SongResult.Empty;
		public GameMode GameMode { get; set; } = GameMode.None;
		public string Name { get; set; } = string.Empty;
		public float CompleteThreshold { get; set; } = 0;
		public float ComplexThreshold { get; set; } = 0;
		public float APThreshold { get; set; } = 0;
		public Difficulty Difficulty { get; set; } = Difficulty.NotSelected;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCheat(GameMode mode) => ((int)mode & (int)GameMode.NoGreenSoul) != 0 || ((int)mode & (int)GameMode.Practice) != 0 || ((int)mode & (int)GameMode.Autoplay) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsCheat() => IsCheat(GameMode);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Update(SongPlayData data)
		{
			if (data.Difficulty != Difficulty)
				throw new ArgumentException("Cannot update because the difficulty of the origin is different from the target");
			if (data.IsCheat())
				throw new ArgumentException("Cannot update with a cheated data");
			CompleteThreshold = data.CompleteThreshold;
			ComplexThreshold = data.ComplexThreshold;
			APThreshold = data.APThreshold;
			Result = SongResult.PickBest(Result, data.Result);
		}
	}
	public struct SongResult(SkillMark currentMark = SkillMark.Failed, int score = 0, float acc = 0, bool ac = false, bool ap = false)
	{
		public static readonly SongResult Empty = new();
		[JsonInclude]
		public SkillMark CurrentMark = currentMark;
		[JsonInclude]
		public int Score = score;
		[JsonInclude]
		public bool AC = ac;
		[JsonInclude]
		public bool AP = ap;
		[JsonInclude]
		public float Accuracy = acc;

		public static SongResult PickBest(SongResult result1, SongResult result2)
		{
			SongResult result = new(
				(SkillMark)Math.Min((int)result1.CurrentMark, (int)result2.CurrentMark),
				Math.Max(result1.Score, result2.Score),
				MathF.Max(result1.Accuracy, result2.Accuracy),
				result1.AC || result2.AC,
				result2.AP || result1.AP
				);
			return result;
		}
	}
	public enum Difficulty
	{
		[ShorthandName("Nb")]
		Noob = 0,
		[ShorthandName("Ez")]
		Easy = 1,
		[ShorthandName("Nr")]
		Normal = 2,
		[ShorthandName("Hd")]
		Hard = 3,
		[ShorthandName("Ex")]
		Extreme = 4,
		[ShorthandName("Ex+")]
		ExtremePlus = 5,
		[ShorthandName("NS")]
		NotSelected = 6
	}

	[Flags]
	public enum GameMode
	{
		None = 0,
		NoHit = 1,
		PerfectOnly = 2,
		NoGreenSoul = 4,
		Practice = 8,
		Buffed = 16,
		Autoplay = 32,
		RestartDeny = 64,
	}
}