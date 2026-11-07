using UndyneFight_Ex.Fight;
using UndyneFight_Ex.Server;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities;

internal partial class StateShower : Entity
{
	private static float FontScale { get; set; } = 0.75f;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void PushBonus(int bonus) => score.Value = Score + bonus;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal SongResult GenerateResult() => new(GenerateCurrentMark(), score, CurrentScorePercent(), FC, AP);
	private int prevSentSec = DateTime.Now.Second;

	#region ????

	/// <summary>
	/// ??????
	/// </summary>
	/// <param name="type">?????0:miss 1:okay 2:nice 3:perfect</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void PushType(DefaultValues.ScoreType type)
	{
		if (CurrentFightingScene.PlayerInstance.hpControl == null)
			return;
		CurrentFightingScene.PlayerInstance.hpControl.GetMark(type);
		switch (type)
		{
			case DefaultValues.ScoreType.Miss:
				MissAction?.Invoke();
				break;
			case DefaultValues.ScoreType.Okay:
				OkayAction?.Invoke();
				break;
			case DefaultValues.ScoreType.Nice:
				NiceAction?.Invoke();
				break;
			case DefaultValues.ScoreType.Perfect:
				PerfectPAction?.Invoke();
				goto default;
			case DefaultValues.ScoreType.PerfectE:
				PerfectEAction?.Invoke();
				goto default;
			case DefaultValues.ScoreType.PerfectL:
				PerfectLAction?.Invoke();
				goto default;
			default:
				PerfectAction?.Invoke();
				break;
		}
		if (type == 0)
		{
			if (PlayerInstance.hpControl.KR && (CurrentFightingScene.Mode & GameMode.NoGreenSoul) != 0 && Heart.SoulType != 1)
				PlayerInstance.hpControl.GiveKR(1);
			combo = 0;
		}
		else
		{
			combo++;
			maxCombo = Math.Max(maxCombo, combo);
		}

		(string DispTxt, Color DispCol) = DefaultValues.ScoreTypes[(int)type].Data;
		switch (type)
		{
			case DefaultValues.ScoreType.Miss:
				miss++;
				break;
			case DefaultValues.ScoreType.Okay:
				okay++;
				break;
			case DefaultValues.ScoreType.Nice:
				nice++;
				break;
			case DefaultValues.ScoreType.Perfect:
				perfect++;
				break;
			case DefaultValues.ScoreType.PerfectE:
				perfectE++;
				goto case DefaultValues.ScoreType.Perfect;
			case DefaultValues.ScoreType.PerfectL:
				perfectL++;
				goto case DefaultValues.ScoreType.Perfect;
		}
		ScoreText v = new(DispTxt, DispCol, combo);
		current?.GetOut();
		current = v;
		totalCount++;

		score.Value = score + (int)(type switch
		{
			DefaultValues.ScoreType.Miss or DefaultValues.ScoreType.Okay => 0,
			DefaultValues.ScoreType.Nice => 40,
			DefaultValues.ScoreType.Perfect => judgeState switch
			{
				JudgementState.Strict => 100,
				JudgementState.Balanced => 98,
				_ => 96,
			},
			_ => 80,
		} * CurrentFightingScene.ScoreMultiplier);
		//Update score to server
		if (OverallNetManager.PlayerData != null && DateTime.Now.Second != prevSentSec)
		{
			OverallNetManager.ConnectedUserData curDat = OverallNetManager.PlayerData;
			curDat.CurrentScore = score;
			curDat.CurrentBarrageCount = totalCount;
			curDat.CurrentAccuracy = instance.CurrentScorePercent();
			OverallNetManager.PlayerData = curDat;
			prevSentSec = DateTime.Now.Second;
		}
	}

	private int miss, okay, nice, perfect, perfectL, perfectE, maxCombo, combo, totalCount = 0, surviveTime = 0;
	private readonly JudgementState judgeState;
	internal int deathCount;
	internal float damageTaken;

	private static int difficulty;

	private Protected<int> score;

	internal static int Score => instance.score;

	internal ScoreText current;

	internal class ScoreText : Entity
	{

		private float scale = FontScale;
		private float alpha, outingSpeed = 0.4f;
		private int appearTime = 0;
		private readonly int combo;
		private readonly string text;
		private Color color;
		private bool isOuting = false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetOut()
		{
			if (alpha < 0.98f)
				outingSpeed = alpha / 3f;
			alpha = alpha * 0.5f + 0.5f;
			if (instance != null)
				controlLayer = instance.controlLayer;
			InstanceCreate(this);
			isOuting = true;
		}

		public ScoreText(string text, Color cl, int combo)
		{
			this.combo = combo;
			color = cl * (CurrentScene.CurrentDrawingSettings.UIColor.A / 255f);
			Centre = new Vector2(540, 80);
			this.text = text;
		}

		public override void Draw()
		{
			if (combo != 0)
				FightResources.Font.NormalFont.CentreDraw("x" + combo, Centre + new Vector2(30, 32) * scale, color * alpha, Math.Min(10, appearTime) / 10f * scale, 0.45f);

			FightResources.Font.NormalFont.CentreDraw(text, Centre, color * alpha, Math.Min(10, appearTime) / 10f * scale * 1.25f, 0.45f);
		}

		public override void Update()
		{
			if (++appearTime == 60)
				GetOut();

			if (isOuting)
			{
				collidingBox.Y -= 3f * outingSpeed;
				outingSpeed += 0.06f;
				alpha -= 0.06f;
				if (scale > 0)
					scale -= 0.02f;
			}
			else if (alpha <= 1f)
			{
				collidingBox.Y -= 1.6f * (1f - alpha);
				alpha = alpha * 0.8f + 1.1f * 0.2f;
			}
			if (alpha <= 0)
				Dispose();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float CurrentScorePercent() => MathF.Min(1, score / (totalCount * 100f));
	internal static bool FC => instance.miss == 0;
	internal static bool AP => FC && (instance.nice + instance.okay) == 0;
	internal SkillMark GenerateCurrentMark()
	{
		bool buffed = (mode & GameMode.Buffed) != 0;
		float scorePercent = MathF.Min(1, score / (totalCount * 100f));
		bool FC = miss == 0;
		bool AP = FC && (okay + nice) == 0;
		return scorePercent switch
		{
			>= 0.997f when AP => SkillMark.Impeccable,
			>= 0.995f when buffed => SkillMark.Eminent,
			>= 0.99f when FC && okay == 0 => SkillMark.Eminent,
			>= 0.99f when buffed => SkillMark.Excellent,
			>= 0.98f when FC => SkillMark.Excellent,
			>= 0.96f => SkillMark.Respectable,
			>= 0.9f => SkillMark.Acceptable,
			>= 0.75f => SkillMark.Ordinary,
			_ => SkillMark.Failed,
		};
	}
	internal static StateShower instance;

	private readonly IWaveSet wave;
	private readonly GameMode mode;
	private readonly float songDuration;

	internal Action MissAction { get; set; }
	internal Action OkayAction { get; set; }
	internal Action NiceAction { get; set; }
	internal Action PerfectAction { get; set; }
	internal Action PerfectPAction { get; set; }
	internal Action PerfectEAction { get; set; }
	internal Action PerfectLAction { get; set; }
	internal Action EndAction { get; set; }
	internal StateShower(IWaveSet waveSet, int difficulty, JudgementState judgeState, GameMode mode, float duration)
	{
		songDuration = duration;
		this.mode = mode;
		this.judgeState = judgeState;
		StateShower.difficulty = difficulty;
		instance = this;
		wave = waveSet;
		//Netplay data
		if (OverallNetManager.IsSuccessfullyConnected)
		{
			AdvanceFunctions.Interactive.AddMissEvent(() => OverallNetManager.PlayerData.MissCount++);
			AdvanceFunctions.Interactive.AddOkayEvent(() => OverallNetManager.PlayerData.OkayCount++);
			AdvanceFunctions.Interactive.AddNiceEvent(() => OverallNetManager.PlayerData.NiceCount++);
			AdvanceFunctions.Interactive.AddPurePerfectEvent(() => OverallNetManager.PlayerData.PerfectCount++);
			AdvanceFunctions.Interactive.AddEarlyPerfectEvent(() => OverallNetManager.PlayerData.PerfectECount++);
			AdvanceFunctions.Interactive.AddLatePerfectEvent(() => OverallNetManager.PlayerData.PerfectLCount++);
		}
		//Advanced information
		if ((bool)UserService.Settings.GetSettingsValue("AdvancedIndicators"))
		{
			MissAction += () => HPShower.instance.IsFC = HPShower.instance.IsAP = false;
			OkayAction += () => HPShower.instance.IsAP = false;
			NiceAction += () => HPShower.instance.IsAP = false;
		}
	}

	public override void Draw()
	{
		Color UICol = GameMain.CurrentDrawingSettings.UIColor;
		GLFont F = FightResources.Font.NormalFont;
		bool isSpectating = OverallNetManager.IsSuccessfullyConnected && !OverallNetManager.PlayerData.IsAlive;
		OverallNetManager.ConnectedUserData specUser = isSpectating ? OverallNetManager.ConnectedUsers[(CurrentScene as SongFightingScene).SpectatingUser] : null;
		int dispScore = isSpectating ? specUser.CurrentScore : score.Value;
		F.CentreDraw(dispScore.ToString(), new Vector2(640 - 72, 20), UICol, 1, Depth);
		int totCount = isSpectating ? specUser.CurrentBarrageCount : totalCount;
		if (totCount != 0)
		{
			if ((CurrentFightingScene.Mode & GameMode.Autoplay) != 0)
				F.CentreDraw("Auto", new Vector2(640 - 72, 40), UICol, FontScale * 0.8f, Depth);
			else
			{
				//Detailed
				string ScoreDisplay = $"{MathF.Round((isSpectating ? specUser.PerfectCount : perfect) * 100f / totCount, 1)}%";
				if (Settings.SettingsManager.DataLibrary.DisplayScorePercent)
					ScoreDisplay += $"({MathF.Round((isSpectating ? specUser.CurrentScore : score) * 1f / totCount, 1)}%)";
				F.CentreDraw(ScoreDisplay, new Vector2(640 - 72, 40), UICol, FontScale * 0.8f, Depth);
			}
		}
		current?.Draw();
	}

	public override void Update()
	{
		surviveTime++;
		current?.Update();
		if (score.Hacked)
			CheatAffirmed();
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void DisposeInstance() => instance = null;
	#endregion
}