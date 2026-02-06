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
	internal SongResult GenerateResult() => new(GenerateCurrentMark(), score, CurrentScorePercent(), AC, AP);

	#region 主要行为

	/// <summary>
	/// 增加一个得分
	/// </summary>
	/// <param name="type">得分种类。0:miss 1:okay 2:nice 3:perfect</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void PushType(int type)
	{
		if (CurrentFightingScene.PlayerInstance.hpControl == null)
			return;
		CurrentFightingScene.PlayerInstance.hpControl.GetMark(type);
		switch (type)
		{
			case 0:
				MissAction?.Invoke();
				break;
			case 1:
				OkayAction?.Invoke();
				break;
			case 2:
				NiceAction?.Invoke();
				break;
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
			combo++;

		maxCombo = Math.Max(maxCombo, combo);
		string DispTxt = "";
		Color DispCol = Color.White;
		switch (type)
		{
			case 0:
				DispTxt = "Miss";
				DispCol = Color.Red;
				miss++;
				break;
			case 1:
				DispTxt = "Okay";
				DispCol = Color.Green;
				okay++;
				break;
			case 2:
				DispTxt = "Nice";
				DispCol = Color.LightBlue;
				nice++;
				break;
			case 3:
				DispTxt = "Perfect!";
				DispCol = Color.Gold;
				perfect++;
				break;
			case 4:
				DispTxt = "PerfectE";
				DispCol = Color.Orange;
				perfect++;
				perfectE++;
				break;
			case 5:
				DispTxt = "PerfectL";
				DispCol = Color.Orange;
				perfect++;
				perfectL++;
				break;
		}
		ScoreText v = new(DispTxt, DispCol, combo);
		current?.GetOut();
		current = v;
		totalCount++;

		int perfectScore = judgeState switch
		{
			JudgementState.Strict => 100,
			JudgementState.Balanced => 98,
			_ => 96,
		};
		if (type == 0)
			return;
		score.Value = score + (int)(type switch
		{
			1 => 0,
			2 => 40,
			3 => perfectScore,
			4 => 80,
			5 => 80,
			_ => throw new NotImplementedException()
		} * CurrentFightingScene.ScoreMultiplier);
	}

	private int miss, okay, nice, perfect, perfectL, perfectE, maxCombo, combo, totalCount = 0, surviveTime = 0;
	private readonly JudgementState judgeState;

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
			color = cl;
			color *= CurrentScene.CurrentDrawingSettings.UIColor.A / 255f;
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

			if (!isOuting)
			{
				if (alpha <= 1f)
				{
					collidingBox.Y -= 1.6f * (1f - alpha);
					alpha = alpha * 0.8f + 1.1f * 0.2f;
				}
			}
			else
			{
				collidingBox.Y -= 3f * outingSpeed;
				outingSpeed += 0.06f;
				alpha -= 0.06f;
				if (scale > 0)
					scale -= 0.02f;
			}
			if (alpha <= 0)
				Dispose();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float CurrentScorePercent() => MathF.Min(1, score / (totalCount * 100f));
	internal static bool AC => instance.miss == 0;
	internal static bool AP => (instance.miss + instance.nice + instance.okay) == 0;
	internal SkillMark GenerateCurrentMark()
	{
		SkillMark mark;
		bool buffed = (mode & GameMode.Buffed) == GameMode.Buffed;
		float scorePercent = MathF.Min(1, score * 1.0f / (totalCount * 100));
		bool AC = miss == 0;
		bool AP = (miss + okay + nice) == 0;
		return scorePercent switch
		{
			>= 0.997f when AP => SkillMark.Impeccable,
			>= 0.995f when buffed => SkillMark.Eminent,
			>= 0.99f when AC && okay == 0 => SkillMark.Eminent,
			>= 0.99f when buffed => SkillMark.Excellent,
			>= 0.98f when AC => SkillMark.Excellent,
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
	internal Action EndAction { get; set; }
	//private float FontScale { get; set; } = 0.75f;
	internal StateShower(IWaveSet waveSet, int difficulty, JudgementState judgeState, GameMode mode, float duration)
	{
		songDuration = duration;
		this.mode = mode;
		this.judgeState = judgeState;
		StateShower.difficulty = difficulty;
		instance = this;
		wave = waveSet;
	}

	public override void Draw()
	{
		Color UICol = GameMain.CurrentDrawingSettings.UIColor;
		GLFont F = FightResources.Font.NormalFont;
		F.CentreDraw(score.Value.ToString(), new Vector2(640 - 72, 20), UICol, 1, Depth);
		if (totalCount != 0)
		{
			//Detailed
			string ScoreDisplay = $"{MathF.Round((float)(perfect * 100.0 / totalCount), 1)}%";
			if (Settings.SettingsManager.DataLibrary.DisplayScorePercent)
				ScoreDisplay += $"({MathF.Round(score * 1.0f / totalCount, 1)}%)";
			F.CentreDraw(ScoreDisplay, new Vector2(640 - 72, 40), UICol, FontScale * 0.8f, Depth);
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