using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.FightResources.Sounds;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities;

internal partial class StateShower
{
	internal class FailureShower : Entity
	{
		private const int previousDataCounts = 10;
		private static float[] previousTimeSurvive = new float[previousDataCounts];
		private static bool changedSong = false, retryAvailable = false, recordMark = true;
		private static int tryCount = 0, halvedScore;
		private readonly int curMode = (int)(CurrentScene as SongFightingScene).Mode;

		public FailureShower(StateShower result)
		{
			bool NoGreenSoulOrAutoPlay = (((curMode & (int)GameMode.NoGreenSoul) != 0) || ((curMode & (int)GameMode.Autoplay) != 0)) && CurrentFightingScene.GreenSoulUsed;
			bool Practice = (curMode & (int)GameMode.Practice) != 0 && CurrentFightingScene.HPReached0;
			if (NoGreenSoulOrAutoPlay || Practice || CurrentFightingScene.ItemUsed)
				recordMark = false;
			UpdateIn120 = true;
			int halvedScore = result.score / 2, timeSurvive = result.surviveTime;

			FailureShower.halvedScore = halvedScore;
			//判定是否比赛超时
			retryAvailable = FightSystem.CurrentChampionShip != null && FightSystem.CurrentSongs != FightSystem.MainGameSongs
				? FightSystem.CurrentChampionShip.CheckTime() != ChampionShips.ChampionShip.ChampionShipStates.NotAvailable
				: (result.mode & GameMode.RestartDeny) == 0;
			//Modifier check
			if (recordMark)
				PlayerManager.RecordMark(result.wave.FightName, difficulty, SkillMark.Failed, instance.score / 2, false, false, 0);
			if (changedSong)
			{
				tryCount = 0;
				changedSong = false;
				previousTimeSurvive = new float[previousDataCounts];
			}
			else
				tryCount++;
			for (int i = 0; i < previousDataCounts - 1; i++)
				previousTimeSurvive[i + 1] = previousTimeSurvive[i];

			previousTimeSurvive[0] = timeSurvive;

			AddChild(retrySelector = new RetrySelector(result));
		}

		public static Selector retrySelector = null;

		private class RetrySelector : Selector
		{
			private float alpha = 0, blurIntensity = 0, detailY = 485;
			private bool displayDetail = false;
			private readonly string DiffText;
			private Color DiffCol;

			public RetrySelector(StateShower s) : base(false)
			{
				NameShower.nameAlpha = 1;
				SelectChanger += () =>
				{
					if (IsKeyPressed120f(InputIdentity.MainUp) || IsKeyPressed120f(InputIdentity.MainDown))
						currentSelect ^= 1;
					currentSelect = MathUtil.Posmod(currentSelect, SelectionCount);
				};
				SelectChanged += () => changeSelection.CreateInstance().Play();

				if (retryAvailable)
					PushSelection(new ReTry(s.wave));

				PushSelection(new GiveUp());
				DiffText = Localization.GetText($"Difficulty[{difficulty}]");
				DiffCol = difficulty switch
				{
					0 => Color.White,
					1 => Color.LawnGreen,
					2 => Color.LightBlue,
					3 => Color.MediumPurple,
					4 => Color.Orange,
					_ => Color.Gray
				};
			}
			public override void Update()
			{
				displayDetail ^= IsKeyPressed120f(InputIdentity.Alternate);
				if (alpha < 1)
					alpha += 0.025f;
				blurIntensity = float.Lerp(blurIntensity, 2, 0.06f);
				detailY = float.Lerp(detailY, displayDetail ? 340 : 485, 0.12f);
				base.Update();
			}
			public override void Draw()
			{
				GLFont NormalFont = Localization.GetFont("NormalFont");
				//Background drawing
				Depth -= 0.01f;
				CollideRect Normal = new(0, 0, 640, 480);
				FormalDraw(GameoverBackground, Normal, Color.White * (blurIntensity / 100f) * 0.2f);
				Depth += 0.01f;
				//Draw name
				if (!IsInChallenge && lastParam is not null)
				{
					string ChartDiff = lastParam.Waveset.Attributes.ComplexDifficulty.ContainsKey((Difficulty)difficulty) ? MathUtil.FloatToString(lastParam.Waveset.Attributes.ComplexDifficulty[(Difficulty)difficulty]) : "?";
					NormalFont.CentreDraw($"{GlobalData.GetWavesetDisplayName(lastParam.Waveset)}", new Vector2(320, 30), Color.White, 1, 0.1f);
					NormalFont.CentreDraw($"{GlobalData.ChartDifficultyNames[lastParam.Waveset.FightName][(Difficulty)difficulty]} {ChartDiff}", new Vector2(320, 60), DiffCol, 1, 0.1f);
				}
				Color lerpCol = Color.Lerp(Color.Black, Color.White, alpha);
				//You lose
				Localization.DrawLocalizedText(tryCount == 1 ? "FailScreen.Lose" : "FailScreen.LoseAgain", new Vector2(320, 105), color: lerpCol, depth: 0.1f, align: Localization.DrawAlign.Middle);
				//Time and score
				Localization.DrawLocalizedText("FailScreen.Survived", new Vector2(320, 145), [MathUtil.FloatToString(MathF.Round((previousTimeSurvive[0] - 2) / 62.5f, 2))], color: lerpCol, scale: new(0.92f), depth: 0.1f, align: Localization.DrawAlign.Middle);
				Localization.DrawLocalizedText(recordMark ? "FailScreen.Halved" : "FailScreen.Modifiers", new Vector2(320, recordMark ? 180 : 195), recordMark ? [halvedScore] : [], scale: new(0.92f), color: lerpCol, depth: 0.1f, align: Localization.DrawAlign.Middle);
				//Space hint
				Localization.DrawLocalizedText("FailScreen.Details", new Vector2(320, 860 - detailY), [MiscUtil.GetInputKeys(InputIdentity.Alternate)[0]], scale: new(0.92f), color: Color.Lerp(Color.Black, Color.GreenYellow, alpha), depth: 0.08f, align: Localization.DrawAlign.Middle);
				//Detailed
				DrawingLab.DrawLine(new Vector2(0, detailY), new Vector2(640, detailY), 3, Color.White, 0.1f);
				DrawingLab.DrawLine(new Vector2(0, detailY + (480 - detailY) / 2), new Vector2(640, detailY + (480 - detailY) / 2), 480 - detailY, Color.Black, 0.09f);
				if (instance == null)
					return;
				Localization.DrawLocalizedText("ResultScreen.MaxCombo", new Vector2(40, detailY + 10), [instance.maxCombo], depth: 0.1f);
				Localization.DrawLocalizedText("ChartJudgement.Miss", new Vector2(40, detailY + 40), color: Color.Red, depth: 0.1f);
				NormalFont.Draw(instance.miss.ToString(), new Vector2(40, detailY + 70), Color.White, 1, 0.1f);
				Localization.DrawLocalizedText("ChartJudgement.Okay", new Vector2(190, detailY + 40), color: Color.Green, depth: 0.1f);
				NormalFont.Draw(instance.okay.ToString(), new Vector2(190, detailY + 70), Color.White, 1, 0.1f);
				Localization.DrawLocalizedText("ChartJudgement.Nice", new Vector2(330, detailY + 40), color: Color.LightBlue, depth: 0.1f);
				NormalFont.Draw(instance.nice.ToString(), new Vector2(330, detailY + 70), Color.White, 1, 0.1f);
				Localization.DrawLocalizedText("ChartJudgement.Perfect", new Vector2(480, detailY + 40), color: Color.Gold, depth: 0.1f);
				NormalFont.Draw(instance.perfect.ToString(), new Vector2(480, detailY + 70), Color.White, 1, 0.1f);
				Localization.DrawLocalizedText("ResultScreen.EarlyLateShort", new Vector2(480, detailY + 100), [instance.perfectE, instance.perfectL], color: Color.Orange, scale: new(0.75f), depth: 0.1f);

				base.Draw();
			}
		}
		private class ReTry : TextSelection
		{
			private readonly IWaveSet wave;
			public ReTry(IWaveSet wave) : base(Localization.GetText("FailScreen.TryAgain"), new Vector2(320, 250)) { Size = 1.0f; this.wave = wave; }
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override void SelectionEvent()
			{
				waveSet = wave;
				StartSong();
				base.SelectionEvent();
			}
		}
		private class GiveUp : TextSelection
		{
			public GiveUp() : base(Localization.GetText("FailScreen.Quit"), new Vector2(320, 300)) => Size = 1.0f;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override void SelectionEvent()
			{
				recordMark = true;
				tryCount = 0;
				DisposeInstance();
				changedSong = true;
				GameMain.ResetRendering();
				ResetScene(new GameMenuScene());

				base.SelectionEvent();
				IsInChallenge = false;
			}
		}
		public override void Draw() { }
		public override void Update()
		{
			if (retrySelector.Disposed)
			{
				Dispose();
				return;
			}
		}
	}
}