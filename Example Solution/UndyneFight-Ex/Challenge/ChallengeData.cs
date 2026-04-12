using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.UserService;
/// <summary>
/// Singular challenge data
/// </summary>
/// <param name="challenge"></param>
public class SingleChallenge(Challenge challenge) : ISaveLoad
{
	/// <inheritdoc/>
	public List<ISaveLoad> Children => null;

	private readonly Challenge challenge = challenge;
	/// <summary>
	/// The total accuracy
	/// </summary>
	public float TripleAccuracy { get; private set; }
	/// <summary>
	/// Updates the total accuracy
	/// </summary>
	/// <param name="tripleAccuracy">The new total accuracy of the challenge</param>

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Update(float tripleAccuracy) => TripleAccuracy = MathF.Max(tripleAccuracy, TripleAccuracy);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info) => TripleAccuracy = info.FloatValue;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save() => new(challenge.Title + ":value=" + MathUtil.FloatToString(TripleAccuracy, 4));
}
/// <summary>
/// Cumulative challenge data
/// </summary>
public class ChallengeData() : ISaveLoad
{
	/// <summary>
	/// Data of all challenges
	/// </summary>
	public Dictionary<string, SingleChallenge> AllData { get; init; } = [];
	/// <inheritdoc/>
	public List<ISaveLoad> Children => null;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void FinishChallenge(Challenge challenge)
	{
		float sum = 0;
		for (int i = 0; i < challenge.ResultBuffer.Count; i++)
			sum += challenge.ResultBuffer.ElementAt(i).Accuracy;
		SingleChallenge challengeData;
		if (AllData.TryGetValue(challenge.Title, out SingleChallenge value))
			challengeData = value;
		else
			AllData.Add(challenge.Title, challengeData = new(challenge));
		challengeData.Update(sum);
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		foreach (SaveInfo next in info.Nexts.Values)
		{
			SingleChallenge challenge;
			AllData.Add(next.Title, challenge = new SingleChallenge(FightSystem.ChallengeDictionary[next.Title]));
			challenge.Load(next);
		}
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo result = new("ChallengeData{");
		for (int i = 0; i < AllData.Values.Count; i++)
			result.PushNext(AllData.Values.ElementAt(i).Save());
		return result;
	}
}