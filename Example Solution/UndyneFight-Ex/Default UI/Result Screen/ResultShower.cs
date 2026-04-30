using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using UndyneFight_Ex.GameInterface;
using UndyneFight_Ex.SongSystem;
using UndyneFight_Ex.UserService;
using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.MathUtil;
using static UndyneFight_Ex.PlayerManager;

namespace UndyneFight_Ex.Entities;

internal partial class StateShower
{
	internal partial class ResultShower : Entity
	{
		private readonly RatingResult ratingResult;
		private readonly AnalyzeShow analyzeShow;

		private readonly SongPlayData playData;
		private (float Score, float Miss, float Okay, float Nice, float Perfect, float PerfectE, float PerfectL, float Combo, float PAccuracy, float SAccuracy) DisplayValues = (0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		public ResultShower(StateShower scoreResult, Player.Analyzer analyzer)
		{
			AddChild(analyzeShow = new(analyzer));
			judgeState = scoreResult.judgeState;
			gamePlayed = scoreResult.wave;
			topText = Localization.GetText($"Difficulty[{difficulty}]");
			topColor = difficulty switch
			{
				0 => Color.White,
				1 => Color.LawnGreen,
				2 => Color.LightBlue,
				3 => Color.MediumPurple,
				4 => Color.Orange,
				_ => Color.Gray
			};
			string songName = gamePlayed.FightName;
			lastMode = scoreResult.mode;
			missCount = scoreResult.miss;
			okayCount = scoreResult.okay;
			niceCount = scoreResult.nice;
			perfectCount = scoreResult.perfect;
			perfectECount = scoreResult.perfectE;
			perfectLCount = scoreResult.perfectL;
			maxCombo = scoreResult.maxCombo;
			totalNote = missCount + okayCount + niceCount + perfectCount;
			perfectPercent = perfectCount / (1.0f * totalNote);
			hitPercent = (okayCount + niceCount + perfectCount) / (1.0f * totalNote);
			score = scoreResult.score;
			collidingBox = new CollideRect(96, 140, 448, 200);
			AP = (AC = scoreResult.miss == 0 && totalNote > 0) && scoreResult.okay == 0 && scoreResult.nice == 0;
			GenerateMark();
			UpdateIn120 = true;
			chartIllustration = GlobalData.GetWavePaint(gamePlayed);

			#region Score saving
			SongResult result = new(mark, score, scoreResult.judgeState != JudgementState.Lenient ? GetScorePercent() : 0, AC, AP);
			SongInformation att = gamePlayed.Attributes;
			playData = att != null && att.ComplexDifficulty.ContainsKey((Difficulty)difficulty)
				? new SongPlayData()
				{
					Result = result,
					Name = songName,
					GameMode = scoreResult.mode,
					CompleteThreshold = att.CompleteDifficulty[(Difficulty)difficulty],
					ComplexThreshold = att.ComplexDifficulty[(Difficulty)difficulty],
					APThreshold = att.APDifficulty[(Difficulty)difficulty],
					Difficulty = (Difficulty)difficulty
				}
				: new SongPlayData()
				{
					Result = result,
					Name = songName,
					GameMode = scoreResult.mode,
					CompleteThreshold = 0,
					ComplexThreshold = 0,
					APThreshold = 0,
					Difficulty = (Difficulty)difficulty
				};
			if (float.IsNaN(playData.CompleteThreshold))
				playData.CompleteThreshold = 0;
			if (float.IsNaN(playData.ComplexThreshold))
				playData.ComplexThreshold = 0;
			if (float.IsNaN(playData.APThreshold))
				playData.APThreshold = 0;
			if (((int)scoreResult.mode & (int)GameMode.NoGreenSoul) != 0 && CurrentFightingScene.GreenSoulUsed)
				PushModifiers(Localization.GetText("GameModifiers.NoGreenSoul"));
			if (((int)scoreResult.mode & (int)GameMode.Practice) != 0 && CurrentFightingScene.HPReached0)
				PushModifiers(Localization.GetText("GameModifiers.Practice"));
			if (((int)scoreResult.mode & (int)GameMode.Autoplay) != 0)
				PushModifiers(Localization.GetText("GameModifiers.Autoplay"));
			if (CurrentFightingScene.ItemUsed)
				PushModifiers(Localization.GetText("GameModifiers.Items"));
			rerate = ReRate(CurrentAccuracy = GetScorePercent());
			if (ModifiersUsed)
				return;

			UFEXSettings.OnSongComplete?.Invoke(playData);

			ModesUsed = Localization.GetText("GameModifiers.None");
			if (CurrentUser == null)
				return;
			previousAccuracy = CurrentUser.SongManager.SongPlayed(playData.Name) ? CurrentUser.SongManager.Acquire(playData.Name).CurrentSongStates[(Difficulty)difficulty].Accuracy : 0;
			oldRating = CurrentUser.Skill;
			int oldCoins = CurrentUser.ShopData.CashManager.Coins;
			float constant = AC ? (AP ? playData.APThreshold : playData.ComplexThreshold) : playData.CompleteThreshold;
			float resultMultiplier = AC ? (AP ? 2 : 1.5f) : 1;
			float judgementMultiplier = scoreResult.judgeState switch
			{
				JudgementState.Lenient => 0.9f,
				JudgementState.Balanced => 1,
				_ => 1.2f,
			};
			CurrentUser.ShopData.CashManager.Coins += (int)(float.Abs(constant) * 10 * GetRandom(0.9f, 1.1f) * resultMultiplier * judgementMultiplier);
			RecordMark(songName, difficulty, result);
			Save();
			coinAdded = CurrentUser.ShopData.CashManager.Coins - oldCoins;
			curRating = CurrentUser.Skill;
			ratingResult = new(oldRating, curRating);
			AddChild(ratingResult);
			if (coinAdded > 0)
				ratingResult.AddCoin(coinAdded);
			#endregion
			//Check achievement
			Achievements.AchievementManager.CheckSongAchievements(playData);
			Achievements.AchievementManager.CheckUserAchievements();
			//Check items
			foreach (StoreItem item in ShopItemData.AllItems.Values)
			{
				//Remove disposable items
				if (ShopItemData.UserItems.ContainsValue(item) && item.Disposable)
				{
					_ = ShopItemData.ConsumeItem(item);
				}
				//Check whether there are unlocked items
				if (!item.InShop && (item.ValidateItem(playData) != item.DefaultInShop))
				{
					ShopItemData.AllItems[item.FullName].InShop = true;
					ValidatedItems.Add(item);
				}
			}
			//Store items into user inventory
			foreach (StoreItem item in ShopItemData.AllItems.Values)
				if (item.InShop)
					_ = ShopItemData.UserItems.TryAdd(item.FullName, item);
			//Apply state
			if (ratingResult?.ProgressMade ?? false)
				ExtraState ^= ExtraResultScreens.RatingIncrease;
			if (ValidatedItems.Count > 0)
			{
				ExtraState ^= ExtraResultScreens.ItemUnlocked;
				Save();
			}
		}
		private readonly List<StoreItem> ValidatedItems = [];
		[Flags]
		private enum ExtraResultScreens
		{
			None = 0,
			ItemUnlocked = 1,
			ItemReceiving = 2,
			ItemTextFading = 4,
			RatingIncrease = 8,
		}
		private ExtraResultScreens ExtraState = ExtraResultScreens.None;
		private readonly Texture2D chartIllustration;
		#region Get Result
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Vector3 SingleCalculateRating(Vector3 Dif, float acc)
		{
			static Tuple<float, float, float> GetDifficulty(IWaveSet waveSet, Difficulty difficulty)
			{
				SongInformation Information = waveSet.Attributes;
				if (Information is null)
					return new(0, 0, 0);
				_ = Information.CompleteDifficulty.TryGetValue(difficulty, out float dif1);
				_ = Information.ComplexDifficulty.TryGetValue(difficulty, out float dif2);
				_ = Information.APDifficulty.TryGetValue(difficulty, out float dif3);
				return new(dif1, dif2, dif3);
			}
			float apMax = 0, fcMax = 0, completeMax = 0;
			SortedSet<float> alls = [];
			foreach (IWaveSet waveSet in GlobalData.WaveCache.Values)
			{
				for (int j = 0; j <= 5; j += 1)
				{
					Tuple<float, float, float> v = GetDifficulty(waveSet, (Difficulty)j);
					completeMax = MathF.Max(completeMax, v.Item1);
					fcMax = MathF.Max(fcMax, v.Item3);
					apMax = MathF.Max(apMax, v.Item3);
					_ = alls.Add(v.Item2);
				}
			}

			for (int i = 0; alls.Count < 7; i++)
				_ = alls.Add(0 - i * 0.00001f);
			float sum = 0.001f, ideal = 0.001f;
			for (int i = 0; i < 7; i++)
			{
				float g = MathF.Max(0, alls.Max);
				_ = alls.Remove(g);
				ideal += g;
			}
			sum += Dif.Y * acc;
			float rating0 = sum / ideal * 85f;
			float rating1 = Dif.Z / apMax * 5f;
			float rating2 = Dif.X / completeMax * 5f;
			return new(rating0, rating1, rating2);
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float GetScorePercent() => (totalNote == 0) ? 0 : MathF.Min(1, score * 1.0f / (totalNote * 100));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void GenerateMark()
		{
			bool buffed = (lastMode & GameMode.Buffed) == GameMode.Buffed;
			float scorePercent = GetScorePercent();
			mark = scorePercent switch
			{
				>= 0.997f when AP => SkillMark.Impeccable,
				>= 0.995f when buffed => SkillMark.Eminent,
				>= 0.99f when AC && okayCount == 0 => SkillMark.Eminent,
				>= 0.99f when buffed => SkillMark.Excellent,
				>= 0.98f when AC => SkillMark.Excellent,
				>= 0.96f => SkillMark.Respectable,
				>= 0.9f => SkillMark.Acceptable,
				>= 0.75f => SkillMark.Ordinary,
				_ => SkillMark.Failed
			};
			plus = mark switch
			{
				SkillMark.Ordinary when scorePercent >= 0.85f => true,
				SkillMark.Acceptable when scorePercent >= 0.93f => true,
				SkillMark.Respectable when scorePercent >= 0.97f => true,
				SkillMark.Excellent when (buffed && AC) || (scorePercent >= 0.99f && AC) => true,
				SkillMark.Eminent when buffed && scorePercent >= 0.995f && AC && okayCount == 0 => true,
				_ => false
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PushModifiers(string Name) => ModesUsed += ((ModesUsedAmt++ > 0) ? " + " : "") + Name;

		private readonly float oldRating, curRating;
		private float alpha = 0;
		private readonly int coinAdded = 0, totalNote;
		private readonly IWaveSet gamePlayed;
		private float appearTime = 0;
		private readonly bool AC, AP;
		private SkillMark mark;
		private bool plus;
		private readonly JudgementState judgeState;

		private readonly int missCount, okayCount, niceCount, perfectCount, perfectECount, perfectLCount, score, maxCombo;
		private readonly float perfectPercent, hitPercent;

		private readonly string topText;
		public string[] difficultyText = new string[3];
		private readonly GameMode lastMode;
		private readonly Color topColor;
		private string ModesUsed = "";
		private int ModesUsedAmt = 0;
		private bool ModifiersUsed => ModesUsedAmt > 0;
		public static bool record;
		private Vector3 CurrentChartConstRating = Vector3.Zero;
		#endregion
		#region UI
		private readonly JToken optionTexts = Localization.GetTranslationData().SelectToken("ResultScreen.Options");
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void MarkDraw()
		{
			string text = $"SkillRating.{mark}";

			_ = Localization.GetFont("NormalFont");
			if (plus)
				text += "+";
			float height = 335;
			switch (mark)
			{
				case SkillMark.Impeccable:
					Localization.DrawLocalizedText(text, new Vector2(414, height + Fight.Functions.Sin(appearTime * 1.6f) * 18), color: Color.Lerp(Color.Transparent, Color.Goldenrod, alpha), scale: new Vector2(2), rotation: GetRadian(Fight.Functions.Sin(appearTime * 2.5f) * 7), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Eminent:
					Localization.DrawLocalizedText(text, new Vector2(414, height + Fight.Functions.Sin(appearTime * 1.6f) * 18), color: Color.Lerp(Color.Transparent, Color.OrangeRed, alpha), scale: new Vector2(2), rotation: GetRadian(Fight.Functions.Sin(appearTime) * 4), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Excellent:
					Localization.DrawLocalizedText(text, new Vector2(414, height + Fight.Functions.Sin(appearTime * 1.6f) * 9), color: Color.Lerp(Color.Transparent, Color.MediumPurple, alpha), scale: new Vector2(2), rotation: GetRadian(7), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Respectable:
					Localization.DrawLocalizedText(text, new Vector2(414, height), color: Color.Lerp(Color.Transparent, Color.LightSkyBlue, alpha), scale: new Vector2(2), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Acceptable:
					Localization.DrawLocalizedText(text, new Vector2(414, height), color: Color.Lerp(Color.Transparent, Color.SpringGreen, alpha), scale: new Vector2(2), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Ordinary:
					Localization.DrawLocalizedText(text, new Vector2(414, height), color: Color.Lerp(Color.Transparent, Color.Green, alpha), scale: new Vector2(2), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Failed:
					Localization.DrawLocalizedText(text, new Vector2(414, height), color: Color.Lerp(Color.Transparent, Color.DarkRed, alpha), scale: new Vector2(2.5f), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
			}
		}
		private readonly float previousAccuracy = 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SummaryDraw()
		{
			MarkDraw();
			Color col = Color.Lerp(Color.Transparent, Color.White, alpha);
			(GLFont NormalFont, float fontScale) = Localization.GetFontData("NormalFont");
			Localization.DrawLocalizedText("ResultScreen.YourScore", new Vector2(214, 107), color: col, depth: 0.5f);
			NormalFont.Draw((DisplayValues.Score = MathF.Ceiling(float.Lerp(DisplayValues.Score, score, 0.04f))).ToString(), new Vector2(392, 106), col, 1.2f * fontScale, 0.5f);
			NormalFont.Draw($"({(DisplayValues.SAccuracy = float.Lerp(DisplayValues.SAccuracy, CurrentAccuracy, 0.04f)) * 100:F1}%)", new Vector2(516, 109), Color.Lerp(Color.Transparent, Color.Silver, alpha), 0.93f * fontScale, 0.5f);
			if (CurrentUser is not null)
			{
				if (CurrentAccuracy > previousAccuracy)
				{
					Localization.DrawLocalizedText("ResultScreen.NewRecord", new Vector2(392, 87), color: Color.Lerp(Color.Transparent, Color.Gold, alpha) * (0.6f + Fight.Functions.Sin(appearTime * 4) / 2f), scale: new(fontScale / 2), depth: 0.5f);
					NormalFont.Draw($"+{Score - SongData.SongState.scoreData.PrevScore}", new Vector2(392, 127), Color.Lerp(Color.Transparent, Color.Gold, alpha) * (0.8f + Fight.Functions.Sin(appearTime * 4) * 0.2f), fontScale / 2, 0.5f);
					NormalFont.Draw($"(+{FloatToString((CurrentAccuracy - SongData.SongState.scoreData.PrevAcc) * 100, 1)}%)", new Vector2(516, 127), Color.Lerp(Color.Transparent, Color.Gold, alpha) * (0.8f + Fight.Functions.Sin(appearTime * 4) * 0.2f), fontScale / 2, 0.5f);
				}
				else
				{
					Localization.DrawLocalizedText("ResultScreen.BestScore", new Vector2(295, 130), [SongData.SongState.scoreData.PrevScore], color: Color.Lerp(Color.Transparent, Color.Gray, alpha), scale: new(fontScale / 2), depth: 0.5f);
					NormalFont.Draw($"({FloatToString(SongData.SongState.scoreData.PrevAcc * 100, 1)}%)", new Vector2(516, 130), Color.Lerp(Color.Transparent, Color.Gray, alpha), fontScale / 2, 0.5f);
				}
			}
			DrawingLab.DrawLine(new Vector2(212, 145), new Vector2(616, 145), 2, Color.Lerp(Color.Transparent, Color.Silver, alpha), 0.5f);

			if (CurrentAccuracy > 0)
			{
				string Judge = Localization.GetText($"JudgementState.{judgeState}Short");
				Color JudgeCol = judgeState switch
				{
					JudgementState.Strict => Color.Red,
					JudgementState.Balanced => Color.Yellow,
					_ => Color.Lime
				};
				NormalFont.Draw(Judge, new(560, 213), Color.Lerp(Color.Transparent, JudgeCol, alpha), fontScale, 0.5f);
			}

			if (AP)
			{
				float tmpDepth = Depth;
				Depth = 0.3f;
				FormalDraw(FightResources.Sprites.allPerfectText, new(300, 170), Color.Lerp(Color.Transparent, Color.White, alpha), 0, Vector2.Zero);
				Depth = tmpDepth;
			}
			else
			{
				if (CurrentAccuracy > 0)
				{
					if (AC)
						NormalFont.Draw(Localization.GetText("ResultScreen.NoHit"), new Vector2(214, 170), Color.Lerp(Color.Transparent, Color.Orange, alpha), 1, 0.3f);
					else
					{
						NormalFont.Draw(Localization.GetText("ChartJudgement.Miss"), new Vector2(214, 170), Color.Lerp(Color.Transparent, Color.Red, alpha), 1, 0.3f);
						NormalFont.Draw((DisplayValues.Miss = MathF.Ceiling(float.Lerp(DisplayValues.Miss, missCount, 0.04f))).ToString(), new Vector2(214 + 79, 170), Color.Lerp(Color.Transparent, Color.LightGray, alpha), 1, 0.3f);
					}

					NormalFont.Draw(Localization.GetText("ChartJudgement.Okay"), new Vector2(346, 170), Color.Lerp(Color.Transparent, Color.Green, alpha), 1, 0.3f);
					NormalFont.Draw((DisplayValues.Okay = MathF.Ceiling(float.Lerp(DisplayValues.Okay, okayCount, 0.04f))).ToString(), new Vector2(346 + 79, 170), Color.Lerp(Color.Transparent, Color.LightGray, alpha), 1, 0.3f);
					NormalFont.Draw(Localization.GetText("ChartJudgement.Nice"), new Vector2(478, 170), Color.Lerp(Color.Transparent, Color.LightBlue, alpha), 1, 0.3f);
					NormalFont.Draw((DisplayValues.Nice = MathF.Ceiling(float.Lerp(DisplayValues.Nice, niceCount, 0.04f))).ToString(), new Vector2(478 + 79, 170), Color.Lerp(Color.Transparent, Color.LightGray, alpha), 1, 0.3f);
				}
				else
					NormalFont.CentreDraw(Localization.GetText("ResultScreen.NoBarrage"), new Vector2(400, 220), Color.Lerp(Color.Black, Color.Red, alpha), 1, 0.3f);
			}
			if (CurrentAccuracy <= 0)
				return;
			NormalFont.Draw(Localization.GetText("ChartJudgement.Perfect"), new Vector2(214, 212), Color.Lerp(Color.Transparent, Color.Yellow, alpha), 1, 0.3f);
			NormalFont.Draw($"{DisplayValues.Perfect = MathF.Ceiling(float.Lerp(DisplayValues.Perfect, perfectCount, 0.04f))} = {(DisplayValues.PAccuracy = float.Lerp(DisplayValues.PAccuracy, perfectPercent, 0.04f)) * 100:F2}%", new Vector2(214 + 125, 212), Color.Lerp(Color.Transparent, Color.LightGray, alpha), 1, 0.3f);
			NormalFont.Draw(Localization.GetText("ResultScreen.EarlyLate", DisplayValues.PerfectE = MathF.Ceiling(float.Lerp(DisplayValues.PerfectE, perfectECount, 0.04f)), DisplayValues.PerfectL = MathF.Ceiling(float.Lerp(DisplayValues.PerfectL, perfectLCount, 0.04f))), new(214, 235), Color.Lerp(Color.Transparent, Color.Orange, alpha), 0.7f, 0.3f);
			NormalFont.Draw(Localization.GetText("ResultScreen.MaxCombo", DisplayValues.Combo = MathF.Ceiling(float.Lerp(DisplayValues.Combo, maxCombo, 0.04f))), new Vector2(214, 255), Color.Lerp(Color.Transparent, Color.Silver, alpha), 1, 0.3f);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RatingDraw()
		{
			GLFont NormalFont = Localization.GetFont("NormalFont");
			//Difficulty unrated
			if (string.IsNullOrEmpty(difficultyText[0]))
			{
				Color col = Color.Lerp(Color.Transparent, Color.Gray, alpha);
				NormalFont.Draw(Localization.GetText("ResultScreen.RatingGained"), new Vector2(211, 92), col, 0.95f, 0.3f);
				NormalFont.Draw("->", new Vector2(211, 160), col, 0.9f, 0.3f);
				NormalFont.Draw("*", new Vector2(330, 160), col, 0.9f, 0.3f);
				NormalFont.Draw(Localization.GetText("ResultScreen.Complete[0]"), new Vector2(211, 130), col, 0.95f, 0.3f);
				NormalFont.Draw(Localization.GetText("ResultScreen.Complete[1]"), new Vector2(211, 193), col, 1, 0.3f);
				DrawingLab.DrawLine(new Vector2(210, 237.5f), new Vector2(618, 241.5f), 2, col, 0.3f);
				NormalFont.Draw(Localization.GetText("ResultScreen.Complex[0]"), new Vector2(211, 260), col, 0.95f, 0.3f);
				NormalFont.Draw(Localization.GetText("ResultScreen.Complex[1]"), new Vector2(211, 323), col, 1, 0.3f);
				if (AC)
					NormalFont.Draw(Localization.GetText("ResultScreen.NextPage"), new Vector2(525, 360), col, 0.5f, 0.3f);

				DrawingLab.DrawLine(new Vector2(202, 80), new Vector2(627, 373), 5, Color.Lerp(Color.Transparent, Color.Red, alpha), 0.4f);
				DrawingLab.DrawLine(new Vector2(627, 80), new Vector2(202, 373), 5, Color.Lerp(Color.Transparent, Color.Red, alpha), 0.4f);
				for (int i = 0; i < 8; i++)
					NormalFont.CentreDraw(Localization.GetText("ResultScreen.Unrated"), new Vector2(410, 235) + GetVector2(3, i * 45), Color.Lerp(Color.Transparent, Color.Lerp(Color.Green, Color.Transparent, 0.3f), alpha), 1.5f, 0.48f);
				NormalFont.CentreDraw(Localization.GetText("ResultScreen.Unrated"), new Vector2(410, 235), Color.Lerp(Color.Transparent, Color.Lime, alpha), 1.5f, 0.5f);
				return;
			}
			NormalFont.Draw(Localization.GetText("ResultScreen.RatingGained"), new Vector2(211, 96), Color.Lerp(Color.Transparent, Color.White, alpha), 0.95f, 0.4f);
			if (curRating > oldRating + 0.001f)
				NormalFont.Draw("+" + FloatToString(curRating - oldRating, 3), new Vector2(431, 96), Color.Lerp(Color.Transparent, Color.Lime, alpha), 1, 0.3f);
			else
				NormalFont.Draw(Localization.GetText("ResultScreen.NoProgress"), new Vector2(431, 96), Color.Lerp(Color.Transparent, Color.Silver, alpha), 1, 0.3f);
			DrawingLab.DrawLine(new Vector2(210, 237.5f), new Vector2(618, 237.5f), 2, Color.Lerp(Color.Transparent, Color.White, alpha), 0.4f);
			NormalFont.Draw("->", new Vector2(211, 164), Color.Lerp(Color.Transparent, Color.Silver, alpha), 0.9f, 0.4f);
			NormalFont.Draw("*", new Vector2(330, 164), Color.Lerp(Color.Transparent, Color.White, alpha), 0.9f, 0.4f);
			NormalFont.Draw($"{FloatToString(rerate * 100, 1)}% ({FloatToString(CurrentAccuracy * 100, 1)}%)", new Vector2(354, 164), Color.Lerp(Color.Transparent, Color.White, alpha), 0.9f, 0.4f);
			if (AC)
				NormalFont.Draw(Localization.GetText(RatingSelection == 0 ? "ResultScreen.NextPage" : "ResultScreen.PrevPage"), new Vector2(RatingSelection == 0 ? 525 : 205, 360), Color.Lerp(Color.Transparent, Color.Gray, alpha), 0.5f, 0.4f);
			if ((RatingSelection == 0 && !AP) || AP)
			{
				NormalFont.Draw("->", new Vector2(211, 294), Color.Lerp(Color.Transparent, Color.Silver, alpha), 0.9f, 0.4f);
				NormalFont.Draw("*", new Vector2(330, 294), Color.Lerp(Color.Transparent, Color.White, alpha), 0.9f, 0.4f);
				NormalFont.Draw($"{FloatToString(rerate * 100, 1)}% ({FloatToString(CurrentAccuracy * 100, 1)}%)", new Vector2(354, 294), Color.Lerp(Color.Transparent, Color.White, alpha), 0.9f, 0.4f);
			}
			if (RatingSelection == 0)
			{
				NormalFont.Draw(Localization.GetText("ResultScreen.Complete[0]"), new Vector2(211, 134), Color.Lerp(Color.Transparent, new Color(0, 255, 0), alpha), 0.95f, 0.4f);
				NormalFont.Draw(difficultyText[0], new Vector2(261, 164), Color.Lerp(Color.Transparent, topColor, alpha), 0.9f, 0.4f);
				NormalFont.Draw(Localization.GetText("ResultScreen.Complete[1]"), new Vector2(211, 197), Color.Lerp(Color.Transparent, Color.White, alpha), 1, 0.3f);
				NormalFont.Draw($"{FloatToString(CurrentChartConstRating.Z, 2)}", new Vector2(520, 197), Color.Lerp(Color.Transparent, Color.PowderBlue, alpha), 1.2f, 0.4f);
				NormalFont.Draw(Localization.GetText("ResultScreen.Complex[0]"), new Vector2(211, 264), Color.Lerp(Color.Transparent, Color.White, alpha), 0.95f, 0.4f);
				NormalFont.Draw(difficultyText[1], new Vector2(261, 295), Color.Lerp(Color.Transparent, topColor, alpha), 0.9f, 0.4f);
				NormalFont.Draw(Localization.GetText("ResultScreen.Complex[1]"), new Vector2(211, 328), Color.Lerp(Color.Transparent, Color.White, alpha), 1, 0.3f);
				NormalFont.Draw($"{FloatToString(CurrentChartConstRating.X, 2)}", new Vector2(520, 326), Color.Lerp(Color.Transparent, Color.PowderBlue, alpha), 1.2f, 0.4f);
			}
			else
			{
				if (AC)
				{
					NormalFont.Draw(Localization.GetText("ResultScreen.FullCombo[0]"), new Vector2(211, 134), Color.Lerp(Color.Transparent, Color.MediumPurple, alpha), 0.95f, 0.4f);
					NormalFont.Draw(difficultyText[2], new Vector2(261, 164), Color.Lerp(Color.Transparent, topColor, alpha), 0.9f, 0.4f);
					NormalFont.Draw(Localization.GetText("ResultScreen.FullCombo[1]"), new Vector2(211, 197), Color.Lerp(Color.Transparent, Color.White, alpha), 1, 0.3f);
					NormalFont.Draw($"{FloatToString(CurrentChartConstRating.Y, 2)}", new Vector2(520, 197), Color.Lerp(Color.Transparent, Color.MediumPurple, alpha), 1.2f, 0.4f);
				}
				if (AP)
				{
					NormalFont.Draw(Localization.GetText("ResultScreen.AllPerfect[0]"), new Vector2(211, 264), Color.Lerp(Color.Transparent, Color.Gold, alpha), 0.95f, 0.4f);
					NormalFont.Draw(difficultyText[2], new Vector2(261, 294), Color.Lerp(Color.Transparent, topColor, alpha), 0.9f, 0.4f);
					NormalFont.Draw(Localization.GetText("ResultScreen.AllPerfect[1]"), new Vector2(211, 327), Color.Lerp(Color.Transparent, Color.White, alpha), 1, 0.3f);
					NormalFont.Draw($"{FloatToString(CurrentChartConstRating.Y, 2)}", new Vector2(520, 326), Color.Lerp(Color.Transparent, Color.Gold, alpha), 1.2f, 0.4f);
				}
			}
			//Modifier unrated
			if (!ModifiersUsed)
				return;
			DrawingLab.DrawLine(new Vector2(202, 80), new Vector2(627, 372), 5, Color.Lerp(Color.Transparent, Color.Red, alpha), 0.45f);
			DrawingLab.DrawLine(new Vector2(627, 80), new Vector2(202, 372), 5, Color.Lerp(Color.Transparent, Color.Red, alpha), 0.45f);
			for (int i = 0; i < 8; i++)
				NormalFont.CentreDraw(Localization.GetText("ResultScreen.Unrated"), new Vector2(410, 235) + GetVector2(3, i * 45), Color.Lerp(Color.Transparent, Color.Lerp(Color.Green, Color.Transparent, 0.3f), alpha), 1.5f, 0.6f);
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.Unrated"), new Vector2(410, 235), Color.Lerp(Color.Transparent, Color.Lime, alpha), 1.5f, 0.7f);
		}
		#endregion
		#region Extra
		private bool encouraged = false, itemUnlocked = false;
		private readonly float[] itemUnlockAlpha = [0, 0];
		private float itemUnlockScale = 1, itemTextAngle = 0;
		private StoreItem curItem = null;
		private void MoveToNextState()
		{
			curItem = null;
			if (ExtraState == ExtraResultScreens.None)
				CreateNextUI();
			else if ((ExtraState & ExtraResultScreens.ItemUnlocked) != 0)
			{
				//There are still unlocked items for the player to receive
				if (itemUnlocked = ValidatedItems.Count >= 1)
				{
					//Set receiving to true
					if ((ExtraState & ExtraResultScreens.ItemReceiving) != 0)
						ExtraState ^= ExtraResultScreens.ItemReceiving;
					itemUnlockAlpha[0] = itemUnlockAlpha[1] = 0;
					RunEase((s) => itemUnlockScale = s, LinkEase(Stable(30, 1), EaseOut(20, 1, 1.5f, EaseState.Quad), EaseIn(20, 1.5f, 1, EaseState.Quad)));
					RunEase((s) => itemTextAngle = s, LinkEase(Stable(30, 0), EaseOut(20, 0, -10, EaseState.Quad), EaseIn(20, -10, 0, EaseState.Quad)));
					RunEase((s) => itemUnlockAlpha[0] = s, LinkEase(false, Stable(30, 0), Stable(0, 1)));
					RunEase((s) => itemUnlockAlpha[1] = s, LinkEase(false, Stable(120, 0), EaseOut(30, 0, 1, EaseState.Sine)));
					DelayEventProcessor.AddInstantEvent(30, () => Fight.Functions.PlaySound(FightResources.Sounds.Ding));
					curItem = ValidatedItems.First();
					ValidatedItems.RemoveAt(0);
				}
				else
				{
					ExtraState ^= ExtraResultScreens.ItemUnlocked;
					if (itemUnlockAlpha[0] == 1)
						RunEase((s) => itemUnlockAlpha[0] = itemUnlockAlpha[1] = s, EaseOut(15, 1, 0, EaseState.Quad));
					MoveToNextState();
				}
			}
			//Rating increase must be final and no other states should be present
			else if (ExtraState == ExtraResultScreens.RatingIncrease)
			{
				if (!encouraged)
				{
					Fight.Functions.PlaySound(FightResources.Sounds.spearAppear);
					ratingResult.IntoCentre();
					encouraged = true;
				}
				else
				{
					ExtraState ^= ExtraResultScreens.RatingIncrease;
					MoveToNextState();
				}
			}
		}
		#endregion
		public override void Draw()
		{
			GLFont NormalFont = Localization.GetFont("NormalFont");
			#region Chart illustration
			Texture2D chartPaint = chartIllustration;
			if (chartPaint != null)
			{
				float xscale = 640f / chartPaint.Width, yscale = 480f / chartPaint.Height;
				for (int i = 0; i < 8; i++)
					FormalDraw(chartPaint, new Vector2(320 * xscale, 240 * yscale) + GetVector2(5, i * 45), Color.Lerp(Color.Transparent, Color.White, 0.05f), new Vector2(xscale, yscale), 0, new(320, 240));
				FormalDraw(chartPaint, new(320 * xscale, 240 * yscale), Color.Lerp(Color.Transparent, Color.White, 0.05f), new Vector2(xscale, yscale), 0, new(320, 240));
			}
			#endregion
			Color col = Color.Lerp(Color.Black, Color.White, alpha);
			#region Main Box
			collidingBox = new CollideRect(200, 78, 428, 295);
			DrawingLab.DrawRectangle(CollidingBox, col, 3f, 0.5f);
			DrawingLab.DrawLine(new Vector2(CollidingBox.Left, CollidingBox.GetCentre().Y), new Vector2(CollidingBox.Right, CollidingBox.GetCentre().Y), 295, Color.Black * 0.5f * alpha, 0.2f);

			if (curSelection == 0)
				SummaryDraw();
			else if (curSelection == 2)
				RatingDraw();
			#endregion
			#region Texts
			//Song name
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.Result", GlobalData.GetWavesetDisplayName(gamePlayed)), new Vector2(320, 40), col, 1.1f, 0.5f);

			//Modifiers used:
			float centre = ratingResult == null ? 320 : 400;
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.Modifiers", ModesUsed), new Vector2(centre, 395), col, 0.8f, 0.5f);

			//Arrow speed
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.ArrowSpeed", FloatToString(Settings.SettingsManager.DataLibrary.ArrowSpeed, 2)), new Vector2(centre, 420), col, 0.8f, 0.5f);

			//Player selection
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.LeaveRestart", MiscUtil.GetInputKeys(InputIdentity.Confirm)[0], MiscUtil.GetInputKeys(InputIdentity.Reset)[0]), new Vector2(centre, 455), col, 0.8f, 0.5f);

			//Difficulty box
			DrawingLab.DrawRectangle(new CollideRect(new Vector2(12, 78), new Vector2(177, 70)), col, 3f, 0.5f);
			DrawingLab.DrawLine(new Vector2(12, 113), new Vector2(189, 113), 70, Color.Black * 0.5f * alpha, 0.2f);
			NormalFont.Draw(Localization.GetText("ResultScreen.Diff"), new Vector2(22, 95), col, 0.8f, 0.3f);
			NormalFont.Draw(topText, new Vector2(20, 120), Color.Lerp(Color.Transparent, topColor, alpha), 0.8f, 0.5f);

			//Side text drawing
			for (int i = 0; i < 3; i++)
			{
				Color color = Color.White;
				float displace = 0;
				if (i == curSelection)
				{
					color = Color.Gold;
					displace = appearTime % 62.5f < 31.25f ? 2 : 0;
				}
				NormalFont.Draw(optionTexts[i].ToString(), new Vector2(25, 177 + 69 * i), Color.Lerp(Color.Transparent, color, alpha), 0.8f, 0.3f);
				DrawingLab.DrawLine(new Vector2(19 + displace, 225 + 69 * i), new(177 - displace, 225 + 69 * i), 2f, Color.Lerp(Color.Transparent, color, alpha), 0.3f);
			}
			#endregion
			DrawingLab.DrawRectangle(new CollideRect(new Vector2(12, 158), new Vector2(177, 215)), col, 3, 0.5f);
			DrawingLab.DrawLine(new Vector2(12, 158 + 215 / 2f), new Vector2(189, 158 + 215 / 2f), 215, Color.Black * 0.5f * alpha, 0.2f);
			#region Item
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.UnlockItem"), new Vector2(320, 140), Color.White * itemUnlockAlpha[0], itemUnlockScale, GetRadian(itemTextAngle), 1);
			if (curItem != null)
			{
				NormalFont.CentreDraw(curItem.Name, new Vector2(320, 340), Color.White * itemUnlockAlpha[1], 1, 0, 1);
				float dep = Depth;
				Depth = 1;
				FormalDraw(curItem.Image, new Vector2(320, 240), Color.White * itemUnlockAlpha[1], 0, new Vector2(curItem.Image.Width, curItem.Image.Height) / 2f);
				Depth = dep;
			}
			#endregion
		}

		private readonly float CurrentAccuracy = 0, rerate = 0;
		private int curSelection = 0, RatingSelection = 0;
		public override void Update()
		{
			if ((ExtraState & ExtraResultScreens.ItemTextFading) != 0 && itemUnlockAlpha[0] == 0)
			{
				ExtraState ^= ExtraResultScreens.ItemUnlocked | ExtraResultScreens.ItemTextFading;
				MoveToNextState();
			}
			appearTime += 0.5f;
			if (alpha < 1f && !encouraged && !itemUnlocked)
				alpha += 0.01f;
			if ((encouraged || itemUnlocked) && alpha > 0.1f)
				alpha -= 0.01f;
			analyzeShow.Alpha = alpha;
			if (gamePlayed.Attributes != null)
			{
				float[] difficultyValues = new float[3];
				if (gamePlayed.Attributes.CompleteDifficulty.TryGetValue((Difficulty)difficulty, out difficultyValues[0]))
					difficultyText[0] = difficultyValues[0].ToString("F1");
				if (gamePlayed.Attributes.ComplexDifficulty.TryGetValue((Difficulty)difficulty, out difficultyValues[1]))
					difficultyText[1] = difficultyValues[1].ToString("F1");
				if (gamePlayed.Attributes.APDifficulty.TryGetValue((Difficulty)difficulty, out difficultyValues[2]))
					difficultyText[2] = difficultyValues[2].ToString("F1");
				CurrentChartConstRating = SingleCalculateRating(new Vector3(difficultyValues[0], difficultyValues[1], difficultyValues[2]), rerate);
			}

			int lastSelection = curSelection, lastSelection2 = RatingSelection;
			if (!encouraged)
			{
				if (IsKeyPressed120f(InputIdentity.MainDown))
					curSelection++;
				else if (IsKeyPressed120f(InputIdentity.MainUp))
					curSelection--;
				if (curSelection == 2 && AC && !string.IsNullOrEmpty(difficultyText[0]))
				{
					if (IsKeyPressed120f(InputIdentity.MainLeft))
						RatingSelection = 0;
					else if (IsKeyPressed120f(InputIdentity.MainRight))
						RatingSelection = 1;
				}
			}
			curSelection = Posmod(curSelection, 3);
			if (lastSelection != curSelection || lastSelection2 != RatingSelection)
				Fight.Functions.PlaySound(FightResources.Sounds.changeSelection);

			analyzeShow.Enabled = curSelection == 1;

			if (IsKeyPressed120f(InputIdentity.Confirm))
			{
				//Fading animation for item receiving
				if ((ExtraState & ExtraResultScreens.ItemReceiving) != 0)
				{
					ExtraState ^= ExtraResultScreens.ItemReceiving | ExtraResultScreens.ItemTextFading;
					RunEase((s) => itemUnlockAlpha[0] = itemUnlockAlpha[1] = s, EaseOut(20, 1, 0, EaseState.Quad));
					curItem = null;
				}
				else
					MoveToNextState();
			}
			else if (IsKeyPressed120f(InputIdentity.Reset))
				StartSong();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CreateNextUI()
		{
			Dispose();
			ResetScene(new GameMenuScene());
			DelayEventProcessor.AddInstantEvent(2, GameMain.ResetRendering);
		}
	}
}