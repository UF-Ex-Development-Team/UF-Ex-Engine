using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources.Sounds;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.Settings.SettingsManager.DataLibrary;

namespace UndyneFight_Ex.Entities;

public partial class Arrow
{
	/// <summary>
	/// The possible judgement types of the arrow
	/// </summary>
	public enum JudgementType
	{
		/// <summary>
		/// Normal arrow
		/// </summary>
		Default = 0,
		/// <summary>
		/// The hideous tap arrow
		/// </summary>
		Tap = 1,
		/// <summary>
		/// The useless hold arrow
		/// </summary>
		Hold = 2
	}

	private static JudgementState JudgeState => GameStates.CurrentScene is SongFightingScene songFightScene
				? songFightScene.JudgeState
				: JudgementState.Lenient;
	/// <summary>
	/// The judgement type of the arrow
	/// </summary>
	public JudgementType JudgeType { get; set; } = JudgementType.Default;
	/// <summary>
	/// The time when the arrow should be blocked
	/// </summary>
	public float BlockTime { get; private set; }

	private float strongPerfectNegative, strongPerfectPositive;
	private float weakPerfectNegative, weakPerfectPositive;
	private float niceNegative, nicePositive;

	private bool isSoundPlayed = false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PlayHitSound(float scale)
	{
		if (VolumeFactor <= 0.01f || isSoundPlayed)
			return;
		isSoundPlayed = true;

		PlaySound(SpearBlockSound switch
		{
			0 => Ding,
			1 => ArrowStuck,
			_ => throw new Exception()
		}, SpearBlockingVolume / 100f * scale * VolumeFactor);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Init()
	{
		switch (JudgeState)
		{
			case JudgementState.Strict:
				strongPerfectNegative = -2.0f;
				strongPerfectPositive = 2.0f;
				weakPerfectNegative = -3.3f;
				weakPerfectPositive = 3.3f;
				niceNegative = -6.5f;
				nicePositive = 6.5f;
				break;
			case JudgementState.Balanced:
				strongPerfectNegative = -3.3f;
				strongPerfectPositive = 3.3f;
				weakPerfectNegative = -5f;
				weakPerfectPositive = 5.5f;
				niceNegative = -7.8f;
				nicePositive = 9f;
				break;
			case JudgementState.Lenient:
				strongPerfectNegative = -4f;
				strongPerfectPositive = 4.5f;
				weakPerfectNegative = -5f;
				weakPerfectPositive = 7.5f;
				niceNegative = -8.5f;
				nicePositive = 10f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SmartSound() => PlayHitSound(JudgeType switch
	{
		JudgementType.Default => 1,
		JudgementType.Hold => 0.5f,
		JudgementType.Tap => 2,
		_ => 0
	});
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CheckCollide()
	{
		if (!Mission.Shields.Exist(ArrowColor))
			return;
		int curShieldWay = Mission.Shields.DirectionOf(ArrowColor);
		bool attachedGB = Mission.Shields.AttachedGB(ArrowColor);

		if (TimeDelta < settingDelay + 0.25f && settingDelay > 1.5f && !isSoundPlayed)
			SmartSound();

		//AUTOPLAY
		bool auto = false;
		foreach (Player.Heart p in Player.hearts)
		{
			if (p.Shields == null)
				continue;
			for (int i = 0; i < 4; i++)
				if (DebugState.ShieldAuto[i] && ArrowColor == i)
				{
					auto = true;
					if (BlockTime - GametimeF <= 1 && curShieldWay != way)
					{
						p.Shields.Rotate(i, way);
						p.Shields.ValidRotated();
					}
				}
		}

		float trueTime = rotatingType != 2
			? Mission.Shields.GetCollideChecker(ArrowColor).TimeOf(Way)
			: Mission.Shields.GetCollideChecker(ArrowColor).TimeOf(Way);
		bool sameDir = Mission.Shields.InSameDir(ArrowColor, way);
		if (JudgeType == JudgementType.Tap)
		{
			sameDir = false;
			trueTime = Mission.Shields.GetCollideChecker(ArrowColor).TapTimeOf(Way);
		}
		else if (JudgeType == JudgementType.Hold)
		{
			sameDir = true;
			trueTime = Mission.Shields.GetCollideChecker(ArrowColor).HoldTimeOf(Way);
		}
		if (auto)
			trueTime = 0;

		if (JudgeType == JudgementType.Tap)
		{
			if (auto && TimeDelta >= 0.5f)
				goto A;
			float time;
			if (trueTime == 0)
				time = TimeDelta;
			else
				goto A;
			float timeMax = 6.5f;
			if (JudgeState == JudgementState.Lenient)
				timeMax += 4.5f;
			else if (JudgeState == JudgementState.Balanced)
				timeMax += 2.75f;
			if (time > timeMax)
				goto A;
			int score = GetScore(time * 1.125f);
			HitScore(score, time);
			PlayHitSound(2);

			Dispose();
		}
		else if (JudgeType == JudgementType.Hold)
		{
			if (TimeDelta >= 0.5f)
				goto A;
			if (trueTime > 5f)
				goto A;
			int score = GetScore(trueTime);
			HitScore(score, TimeDelta);
			PlayHitSound(0.5f);
			Dispose();
		}
		else if (TimeDelta < 0.5f || (attachedGB && TimeDelta < 12f && !auto))
		{
			if (attachedGB)
			{
				if (sameDir)
				{
					if (TimeDelta >= 0.5f)
						goto A;
				}
				else
				{
					if (trueTime != 0)
						goto A;
					trueTime = TimeDelta;
				}
			}
			float timeMax = 6;
			if (JudgeState == JudgementState.Lenient)
				timeMax += 3;
			else if (JudgeState == JudgementState.Balanced)
				timeMax += 1.5f;
			if (GoldenMarkIntensity >= 1)
				timeMax += 2;
			if (JudgeType == JudgementType.Tap)
				timeMax += 2f;

			int score;
			if (trueTime > timeMax && way != curShieldWay)
				goto A;
			if (TimeDelta < -0.5f && trueTime < timeMax)
				trueTime = TimeDelta;

			float time = trueTime;
			if (sameDir)
				time = 0;
			score = GetScore(time);
			float del = BlockTime - GametimeF;

			float div = JudgeState switch
			{
				JudgementState.Lenient => 1,
				JudgementState.Balanced => 1.2f,
				JudgementState.Strict => 1.5f,
				_ => throw new ArgumentException($"{JudgeState} is not in proper form", nameof(JudgeState)),
			};
			if (score <= 1 && time > 9f / div && del >= weakPerfectNegative + 0.6f)
				goto A;
			if (score <= 1 && time > 15f / div && del >= niceNegative + 0.6f)
				goto A;

			HitScore(score, time);
			PlayHitSound(1f);
			Dispose();
			return;
		}

	A:
		if (distance / distanceFactor <= -34 - (hasGreenFlag ? 7 : 0))
		{
			Dispose();
			HitScore(0, -100);
			if (((CurrentScene as FightScene).Mode & GameMode.NoGreenSoul) == 0)
			{
				LoseHP(Mission);
				GiveKR(1.2f);
			}
			else
				playerHurt.CreateInstance().Play();
			if (currentScene is SongFightingScene songFightScene)
				songFightScene.Accuracy.PushDelta(0, 0, ArrowColor, way, Mission.Shields);
			return;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetScore(float time) => time switch
	{
		float x when x >= strongPerfectNegative && x <= strongPerfectPositive => 3,
		float x when x >= weakPerfectNegative && x <= weakPerfectPositive => x > strongPerfectPositive ? 4 : 5,
		float x when x >= niceNegative && x <= nicePositive => 2,
		_ => 1
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void HitScore(int score, float time)
	{
		if (!NoScore)
			Fight.AdvanceFunctions.PushScore(score);

		Mission.Shields.GetCollideChecker(ArrowColor).ArrowBlock(Way);
		bool sameDir = false;
		if (GameStates.CurrentScene is SongFightingScene songFightScene)
		{
			if (JudgeType != JudgementType.Tap)
				sameDir = Mission.Shields.InSameDir(ArrowColor, way);

			if (!sameDir)
				songFightScene.PlayerInstance.GameAnalyzer.PushData(new Player.ArrowData(time, score, GametimeF));

			float abs = MathF.Abs(time);
			if (abs <= 2.0f && JudgeState == JudgementState.Strict && !sameDir && !NoScore)
				Fight.AdvanceFunctions.PushBonus(5 - abs * 2.5f);

			if (score == 3 && time > 0)
				time /= 1.9f;
			if (sameDir && !hasGreenFlag)
				time = 0;

			songFightScene.Accuracy.PushDelta(time, score, ArrowColor, way, Mission.Shields);

			bool precise = preciseWarning;
			bool generateTip = precise ? (score != 3) : (score <= 2);
			if (JudgeType == JudgementType.Hold || ForceDisableTimeTips)
				generateTip = false;
			if (generateTip)
			{
				Color tipscolor = Color.CornflowerBlue;
				float xVec = Heart.Centre.Y + 30;
				if (ArrowColor == 0)
				{
					tipscolor = Color.CornflowerBlue;
					xVec = Heart.Centre.X - 30;
				}
				else if (ArrowColor == 1)
				{
					tipscolor = Color.Red;
					xVec = Heart.Centre.X + 30;
				}
				else if (ArrowColor == 2)
				{
					tipscolor = Color.Lime;
					xVec = Heart.Centre.X + 30;
				}
				else if (ArrowColor == 3)
				{
					tipscolor = Color.MediumPurple;
					xVec = Heart.Centre.X - 30;
				}
				if (score >= 4)
					tipscolor = Color.Lerp(tipscolor, Color.Lime * 0.7f, 0.45f);
				if (time > -1)
					CreateEntity(new TimeTips(new(xVec, Heart.Centre.Y - 40), tipscolor, "early", new(0, 1)));
				else
					CreateEntity(new TimeTips(new(xVec, Heart.Centre.Y + 40), tipscolor, "late", new(0, -1)));
			}
		}
		if (score < 3 && score != 0 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
		{
			Fight.AdvanceFunctions.PushScore(0);
			LoseHP(Mission);
		}
		if (!sameDir || GoldenMarkIntensity >= 1 || JudgeState == JudgementState.Lenient)
			Mission.Shields.ValidRotated();
		if (hasGreenFlag)
			Mission.Shields.ValidRotated();
		if (score == 3)
			Mission.Shields.Consume(0.25f);
	}

	/// <inheritdoc/>
	public override void Dispose()
	{
		InstanceCreate(new BreakArrow(Speed, Rotation + additiveRotation + Mission.Shields.GetShield(ArrowColor).deltaRotation * 6, ArrowColor, rotatingType, Centre, Scale * DrawingScale));
		_ = AllArrows?.Remove(this);

		base.Dispose();

		if (!HasTag())
			return;
		foreach (string str in Tags)
			if (AllTaggedArrows.TryGetValue(str, out List<Arrow> value))
				_ = value.Remove(this);
	}
}