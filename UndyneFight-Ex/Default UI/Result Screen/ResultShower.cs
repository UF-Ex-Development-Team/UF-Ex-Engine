using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.GameInterface;
using UndyneFight_Ex.Server;
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
		private readonly SongPlayData playData;
		private (float Score, float Miss, float Okay, float Nice, float Perfect, float PerfectE, float PerfectL, float Combo, float PAccuracy, float SAccuracy) DisplayValues = (0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		private readonly int DeathCount;
		private readonly float DamageTaken;
		public ResultShower(StateShower scoreResult, Player.Analyzer analyzer)
		{
			judgeState = scoreResult.judgeState;
			gamePlayed = scoreResult.wave;
			DamageTaken = scoreResult.damageTaken;
			DeathCount = scoreResult.deathCount;
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
			totalBarrage = missCount + okayCount + niceCount + perfectCount;
			perfectPercent = perfectCount / (1.0f * totalBarrage);
			score = scoreResult.score;
			collidingBox = new CollideRect(96, 140, 448, 200);
			AP = (AC = scoreResult.miss == 0 && totalBarrage > 0) && scoreResult.okay == 0 && scoreResult.nice == 0;
			GenerateMark();
			UpdateIn120 = true;
			chartIllustration = GlobalData.GetWavePaint(gamePlayed);

			#region Score saving
			SongResult result = new(mark, score, scoreResult.judgeState != JudgementState.Lenient ? GetScorePercent() : 0, AC, AP);
			SongInformation att = gamePlayed.Attributes;
			playData = new SongPlayData()
			{
				Result = result,
				Name = songName,
				GameMode = scoreResult.mode,
				CompleteThreshold = (att?.CompleteDifficulty.TryGetValue((Difficulty)difficulty, out float comThres) ?? false) ? comThres : 0,
				ComplexThreshold = (att?.ComplexDifficulty.TryGetValue((Difficulty)difficulty, out float cpxThres) ?? false) ? cpxThres : 0,
				APThreshold = (att?.APDifficulty.TryGetValue((Difficulty)difficulty, out float apThres) ?? false) ? apThres : 0,
				Difficulty = (Difficulty)difficulty
			};
			if ((scoreResult.mode & GameMode.NoGreenSoul) != 0 && CurrentFightingScene.GreenSoulUsed)
				PushModifiers(Localization.GetText("GameModifiers.NoGreenSoul"));
			if ((scoreResult.mode & GameMode.Practice) != 0 && CurrentFightingScene.HPReached0)
				PushModifiers(Localization.GetText("GameModifiers.Practice"));
			if ((scoreResult.mode & GameMode.Autoplay) != 0)
				PushModifiers(Localization.GetText("GameModifiers.Autoplay"));
			if (CurrentFightingScene.ItemUsed)
				PushModifiers(Localization.GetText("GameModifiers.Items"));
			rerate = RatingCalculator.ReRate(CurrentAccuracy = GetScorePercent());
			if (ModifiersUsed)
				return;

			UFEXSettings.OnSongComplete?.Invoke(playData);

			ModesUsed = Localization.GetText("GameModifiers.None");
			if (CurrentUser == null)
				return;
			oldRating = CurrentUser.Skill;
			int oldCoins = CurrentUser.ShopData.CashManager.Coins;
			float constant = AC ? (AP ? playData.APThreshold : playData.ComplexThreshold) : playData.CompleteThreshold;
			float coinMultiplier = AC ? (AP ? 2 : 1.5f) : 1;
			float judgementMultiplier = scoreResult.judgeState switch
			{
				JudgementState.Lenient => 0.9f,
				JudgementState.Balanced => 1,
				_ => 1.2f,
			};
			CurrentUser.ShopData.CashManager.Coins += (int)(float.Abs(constant) * 10 * GetRandom(0.9f, 1.1f) * coinMultiplier * judgementMultiplier);
#if !DEBUG
			if (!DebugState.IsBeta)
#endif
			RecordMark(songName, difficulty, result);
			coinAdded = CurrentUser.ShopData.CashManager.Coins - oldCoins;
			curRating = CurrentUser.Skill;
			//Upload new skill to server
			if (OverallNetManager.IsSuccessfullyConnected)
			{
				OverallNetManager.ConnectedUserData curDat = OverallNetManager.PlayerData;
				OverallNetManager.PlayerData.Skill = curRating;
				OverallNetManager.PlayerData = curDat;
			}
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
			if (OverallNetManager.IsSuccessfullyConnected)
			{
				OverallNetManager.ConnectedUserData curDat = OverallNetManager.PlayerData;
				curDat.ChartEnded = true;
				curDat.IsReady = false;
				OverallNetManager.PlayerData = curDat;
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
		private float GetScorePercent() => (totalBarrage == 0) ? 0 : MathF.Min(1, score * 1.0f / (totalBarrage * 100));

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
			plus =
				(mark == SkillMark.Ordinary && scorePercent >= 0.85f) ||
				(mark == SkillMark.Acceptable && scorePercent >= 0.93f) ||
				(mark == SkillMark.Respectable && scorePercent >= 0.97f) ||
				(mark == SkillMark.Excellent && AC && (buffed || scorePercent >= 0.99f)) ||
				(mark == SkillMark.Eminent && buffed && scorePercent >= 0.995f && AC && okayCount == 0);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PushModifiers(string Name) => ModesUsed += ((ModesUsedAmt++ > 0) ? ", " : "") + Name;

		private readonly float oldRating, curRating;
		private float alpha = 0;
		private readonly int coinAdded = 0, totalBarrage;
		private readonly IWaveSet gamePlayed;
		private float appearTime = 0;
		private readonly bool AC, AP;
		private SkillMark mark;
		private bool plus;
		private readonly JudgementState judgeState;

		private readonly int missCount, okayCount, niceCount, perfectCount, perfectECount, perfectLCount, score, maxCombo;
		private readonly float perfectPercent;

		public string[] difficultyText = new string[3];
		private readonly GameMode lastMode;
		private readonly Color topColor;
		private string ModesUsed = "";
		private int ModesUsedAmt = 0;
		private bool ModifiersUsed => ModesUsedAmt > 0;
		public static bool record;
		#endregion
		#region UI
		private static int NetResultMaxPage => OverallNetManager.ConnectedUserCount / 10;
		private int NetResultPage = 0;
		private void NetSummaryDraw()
		{
			GLFont NormalFont = Localization.GetFont("NormalFont");
			//Index
			//Name | Time | Score | MONELP
			//-----|------|-------|-------
			NormalFont.Draw("Name", new Vector2(210, 97), Color.White * alpha, 0.75f, 0.3f);
			DrawingLab.DrawLine(new Vector2(267, 78), new Vector2(267, 373), 2, Color.White * alpha, 0.3f);
			NormalFont.Draw("Time", new Vector2(274, 97), Color.White * alpha, 0.75f, 0.3f);
			DrawingLab.DrawLine(new Vector2(328, 78), new Vector2(328, 373), 2, Color.White * alpha, 0.3f);
			NormalFont.Draw("Score(Acc.)", new Vector2(334, 97), Color.White * alpha, 0.75f, 0.3f);
			DrawingLab.DrawLine(new Vector2(465, 78), new Vector2(465, 373), 2, Color.White * alpha, 0.3f);
			NormalFont.Draw("M/O/N/E/L/P", new Vector2(470, 97), Color.White * alpha, 0.75f, 0.3f);
			DrawingLab.DrawLine(new Vector2(200, 118), new Vector2(628, 118), 3, Color.White * alpha, 0.3f);
			List<OverallNetManager.ConnectedUserData> UserList = [.. OverallNetManager.ConnectedUsers.Where(user => user.ChartEnded)];
			//Sort by descending order
			UserList.Sort((accA, accB) => accB.CurrentAccuracy.CompareTo(accA.CurrentAccuracy));
			for (int i = 0; i < 10; i++)
			{
				int getIndex = i + NetResultPage * 10;
				if (getIndex >= OverallNetManager.ConnectedUserCount || getIndex >= UserList.Count)
					break;
				OverallNetManager.ConnectedUserData curUser = UserList[getIndex];
				Color drawCol = i switch
				{
					0 when NetResultPage == 0 => Color.Gold,
					1 when NetResultPage == 0 => Color.Silver,
					2 when NetResultPage == 0 => new Color(205, 127, 50),
					_ => Color.White
				};
				int curY = 127 + (int)(i * 25.4f);
				NormalFont.DrawContainedText(curUser.PlayerName, new Rectangle(210, curY, 60, 20), drawCol * alpha, 0.7f, 0.3f);
				NormalFont.DrawContainedText($"{curUser.SurviveTime / 60:F2}", new Rectangle(274, curY, 50, 20), drawCol * alpha, 0.7f, 0.3f);
				NormalFont.DrawContainedText($"{curUser.CurrentScore}({curUser.CurrentAccuracy * 100:F2}%)", new Rectangle(334, curY, 130, 20), drawCol * alpha, 0.7f, 0.3f);
				NormalFont.DrawContainedText($"{curUser.MissCount}/{curUser.OkayCount}/{curUser.NiceCount}/{curUser.PerfectECount}/{curUser.PerfectLCount}/{curUser.PerfectCount}", new Rectangle(470, curY, 140, 20), drawCol * alpha, 0.7f, 0.3f);
				if (i < 9)
					DrawingLab.DrawLine(new Vector2(200, curY + 18), new Vector2(628, curY + 18), 3, Color.White * alpha, 0.3f);
			}
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
			#region Chart illustration
			Texture2D chartPaint = chartIllustration;
			if (chartPaint != null)
			{
				float xscale = 640f / chartPaint.Width, yscale = 480f / chartPaint.Height;
				for (int i = 0; i < 8; i++)
					GeneralDraw(chartPaint, new Vector2(320 * xscale, 240 * yscale) + GetVector2(5, i * 45), Color.Lerp(Color.Transparent, Color.White, 0.05f), new Vector2(xscale, yscale), 0, new Vector2(320, 240));
				GeneralDraw(chartPaint, new(320 * xscale, 240 * yscale), Color.Lerp(Color.Transparent, Color.White, 0.05f), new Vector2(xscale, yscale), 0, new(320, 240));
			}
			#endregion
			(GLFont NormalFont, float fontScale) = Localization.GetFontData("NormalFont");
			Color col = Color.Lerp(Color.Black, Color.White, alpha);
			//Song name
			DrawingLab.DrawRoundedRectangle(new Vector2(320, 60), new Vector2(480, 30), 10, topColor * alpha, 0.1f);
			NormalFont.DrawContainedTextCentered(GlobalData.GetWavesetDisplayName(gamePlayed), new Rectangle(220, 55, 200, 30), Color.Black * alpha, 0.9f, 0.11f);
			//Icon
			if (chartPaint != null)
				GeneralDraw(chartPaint, new Vector2(160, 60), scale: new Vector2(40f / chartPaint.Width, 30f / chartPaint.Height), depth: 0.11f);
			//Difficulty
			string diffText = gamePlayed.Attributes.ComplexDifficulty.TryGetValue((Difficulty)difficulty, out float diff) ? FloatToString(diff, 1) : "?";
			NormalFont.DrawContainedTextCentered($"{MiscUtil.GetShorthandName((Difficulty)difficulty)} {diffText}", new Rectangle(440, 55, 100, 30), Color.Black * alpha, 0.9f, 0.11f);
			#region Left Side
			//Left side
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.1f, [
				new VertexPositionColor(new Vector3(0, 130, 0.1f), Color.Transparent),
				new VertexPositionColor(new Vector3(290, 130, 0.1f), Color.Black * alpha),
				new VertexPositionColor(new Vector3(200, 380, 0.1f), Color.Black * alpha),
				new VertexPositionColor(new Vector3(0, 380, 0.1f), Color.Transparent),
			]);
			//Upper outline
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.1f, [
				new VertexPositionColor(new Vector3(0, 127, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(290, 127, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(290 + 3 * 9 / 25f, 130, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(0, 130, 0.1f), Color.White * alpha),
			]);
			//Lower outline
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.1f, [
				new VertexPositionColor(new Vector3(0, 380, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(200 + 3 * 9 / 25f, 380, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(200, 383, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(0, 383, 0.1f), Color.White * alpha),
			]);
			//Right outline
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.1f, [
				new VertexPositionColor(new Vector3(200, 383, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(290, 127, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(293, 127, 0.1f), Color.White * alpha),
				new VertexPositionColor(new Vector3(203, 383, 0.1f), Color.White * alpha),
			]);
			//Draw skill rating
			string skillMark = $"SkillRating.{mark}";
			if (plus)
				skillMark += "+";
			float height = 107;
			float skillX = 320;
			Color markCol = Color.Black;
			switch (mark)
			{
				case SkillMark.Impeccable:
					Localization.DrawLocalizedText(skillMark, new Vector2(skillX, height + Fight.Functions.Sin(appearTime * 1.6f) * 12), color: markCol = Color.Lerp(Color.Transparent, Color.Goldenrod, alpha), scale: new Vector2(1.5f), rotation: GetRadian(Fight.Functions.Sin(appearTime * 2.5f) * 3), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Eminent:
					Localization.DrawLocalizedText(skillMark, new Vector2(skillX, height + Fight.Functions.Sin(appearTime * 1.6f) * 12), color: markCol = Color.Lerp(Color.Transparent, Color.OrangeRed, alpha), scale: new Vector2(1.5f), rotation: GetRadian(Fight.Functions.Sin(appearTime) * 1.5f), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Excellent:
					Localization.DrawLocalizedText(skillMark, new Vector2(skillX, height + Fight.Functions.Sin(appearTime * 1.6f) * 6), color: markCol = Color.Lerp(Color.Transparent, Color.MediumPurple, alpha), scale: new Vector2(1.5f), rotation: GetRadian(7), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Respectable:
					Localization.DrawLocalizedText(skillMark, new Vector2(skillX, height), color: markCol = Color.Lerp(Color.Transparent, Color.LightSkyBlue, alpha), scale: new Vector2(1.5f), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Acceptable:
					Localization.DrawLocalizedText(skillMark, new Vector2(skillX, height), color: markCol = Color.Lerp(Color.Transparent, Color.SpringGreen, alpha), scale: new Vector2(1.5f), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Ordinary:
					Localization.DrawLocalizedText(skillMark, new Vector2(skillX, height), color: markCol = Color.Lerp(Color.Transparent, Color.Green, alpha), scale: new Vector2(1.5f), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
				case SkillMark.Failed:
					Localization.DrawLocalizedText(skillMark, new Vector2(skillX, height), color: markCol = Color.Lerp(Color.Transparent, Color.DarkRed, alpha), scale: new Vector2(2.5f * 0.75f), depth: 0.9f, align: Localization.DrawAlign.Middle);
					break;
			}
			markCol *= 0.5f;
			DrawingLab.DrawLineColors(new Vector2(40, height), new Vector2(320, height), 40, [markCol, Color.Transparent, Color.Transparent, markCol], 0.89f);
			DrawingLab.DrawLineColors(new Vector2(600, height), new Vector2(320, height), 40, [markCol, Color.Transparent, Color.Transparent, markCol], 0.89f);
			//Score
			Localization.DrawLocalizedText("ResultScreen.YourScore", new Vector2(10, 140), scale: new Vector2(0.4f), color: col, depth: 0.11f);
			string Judge = Localization.GetText($"JudgementState.{judgeState}Short");
			Color JudgeCol = judgeState switch
			{
				JudgementState.Strict => Color.Red,
				JudgementState.Balanced => Color.Yellow,
				_ => Color.Lime
			};
			NormalFont.Draw(Judge, new(10 + NormalFont.SFX.MeasureString(Localization.GetText("ResultScreen.YourScore")).X * 0.4f, 140), Color.Lerp(Color.Transparent, JudgeCol, alpha), fontScale * 0.4f, 0.5f);
			NormalFont.DrawContainedText(DisplayValues.Score.ToString(), new Rectangle(10, 154, 190, 50), col, 1.2f * fontScale, 0.11f);
			NormalFont.Draw($"({DisplayValues.SAccuracy * 100:F1}%)", new Vector2(205, 152), Color.Lerp(Color.Transparent, CurrentAccuracy >= 1 ? Color.Gold : Color.Silver, alpha), 0.6f * fontScale, 0.11f);
			if (CurrentUser != null)
			{
				if (!ModifiersUsed)
				{
					NormalFont.DrawContainedText(SongData.SongState.scoreData.PrevScore.ToString(), new Rectangle(10, 174, 90, 50), Color.Gray * alpha, 0.4f * fontScale, 0.11f);
					int scoreDiff = Score - SongData.SongState.scoreData.PrevScore;
					NormalFont.DrawContainedText(scoreDiff < 0 ? $"{scoreDiff}" : $"+{scoreDiff}", new Rectangle(100, 174, 190, 50), (scoreDiff < 0 ? Color.Red : Color.Yellow) * alpha, 0.4f * fontScale, 0.11f);
					NormalFont.Draw($"({SongData.SongState.scoreData.PrevAcc * 100:F1}%)", new Vector2(205, 165), Color.Lerp(Color.Transparent, Color.Gray, alpha), 0.4f * fontScale, 0.11f);
					float accDiff = CurrentAccuracy - SongData.SongState.scoreData.PrevAcc;
					NormalFont.Draw($"{(accDiff > 0 ? "+" : "")}{accDiff * 100:F1}%", new Vector2(205, 174), Color.Lerp(Color.Transparent, accDiff < 0 ? Color.Red : Color.Yellow, alpha), 0.4f * fontScale, 0.11f);
				}
				else
					Localization.DrawLocalizedText("ResultScreen.ModifiersUsed", new Vector2(10, 174), scale: new Vector2(0.4f), color: Color.Gray * alpha, depth: 0.11f);
			}
			if (ModifiersUsed && (playData.GameMode & GameMode.Autoplay) != 0)
				NormalFont.Draw("Autoplay", new Vector2(205, 165), Color.Lime * alpha, 0.4f * fontScale, 0.11f);
			//Data
			Color[] colors = [Color.Red, Color.Green, Color.LightBlue, Color.Yellow];
			float[] counts = [DisplayValues.Miss, DisplayValues.Okay, DisplayValues.Nice, DisplayValues.Perfect];
			for (int i = 0; i < 5; i++)
			{
				//Line
				int upperY = 190 + i * 37;
				SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.11f, [
					new VertexPositionColor(new Vector3(0, upperY - 5, 0.11f), col),
					new VertexPositionColor(new Vector3(290 - (upperY - 5 - 130) * 9 / 25f, upperY - 5, 0.11f), col),
					new VertexPositionColor(new Vector3(290 - (upperY - 130) * 9 / 25f, upperY, 0.11f), col),
					new VertexPositionColor(new Vector3(0, upperY, 0.11f), col),
				]);
				if (i == 4)
					continue;
				//Color background
				SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.11f, [
					new VertexPositionColor(new Vector3(0, upperY, 0.11f), Color.Transparent),
					new VertexPositionColor(new Vector3(290 - (upperY - 130) * 9 / 25f, upperY, 0.11f), colors[i] * alpha),
					new VertexPositionColor(new Vector3(290 - (upperY - 98) * 9 / 25f, upperY + 32, 0.11f), colors[i] * alpha),
					new VertexPositionColor(new Vector3(0, upperY + 32, 0.11f), Color.Transparent),
				]);
				//Count and shadow
				string countText = float.Round(counts[i]).ToString();
				string dataKey = DefaultValues.ScoreTypes[i].Name;
				NormalFont.DrawContainedText(countText, new Rectangle(122, upperY + 12, 80, 30), Color.Black * alpha, 1, 0.119f);
				NormalFont.DrawContainedText(countText, new Rectangle(120, upperY + 10, 80, 30), col * alpha, 1, 0.12f);
				NormalFont.DrawContainedText(dataKey, new Rectangle(12, upperY + 12, 120, 30), Color.Black * alpha, 1, 0.119f);
				NormalFont.DrawContainedText(dataKey, new Rectangle(10, upperY + 10, 120, 30), colors[i] * alpha, 1, 0.12f);
				//No hit
				if (AC && i == 0)
				{
					string noHitText = Localization.GetText("ResultScreen.NoHit");
					NormalFont.DrawContainedText(noHitText, new Rectangle(147, upperY + 12, 120, 30), Color.Black, 1, 0.119f);
					NormalFont.DrawContainedText(noHitText, new Rectangle(145, upperY + 10, 120, 30), Color.Orange, 1, 0.12f);
				}
			}
			//PAccuracy
			if (AP)
				GeneralDraw(FightResources.Sprites.allPerfectText, new(100, 360), Color.Lerp(Color.Transparent, Color.White, alpha), new Vector2(0.8f), depth: 0.12f);
			else
				NormalFont.DrawContainedTextCentered($"{DisplayValues.PAccuracy * 100:F2}% ({DisplayValues.Perfect}/{DisplayValues.Miss + DisplayValues.Okay + DisplayValues.Nice + DisplayValues.Perfect})", new Rectangle(10, 350, 190, 30), col, 1, 0.12f);
			#endregion
			#region Right Side
			//Right side
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.1f, [
				new VertexPositionColor(new Vector3(330, 130, 0.1f), Color.Black * alpha),
				new VertexPositionColor(new Vector3(640, 130, 0.1f), Color.Transparent),
				new VertexPositionColor(new Vector3(550, 380, 0.1f), Color.Transparent),
				new VertexPositionColor(new Vector3(240, 380, 0.1f), Color.Black * alpha),
			]);
			//Upper outline
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.11f, [
				new VertexPositionColor(new Vector3(640, 127, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(330, 127, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(330 - 3 * 9 / 25f, 130, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(640, 130, 0.11f), Color.White * alpha),
			]);
			//Lower outline
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.11f, [
				new VertexPositionColor(new Vector3(640, 380, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(240 + 3 * 9 / 25f, 380, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(240, 383, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(640, 383, 0.11f), Color.White * alpha),
			]);
			//Right outline
			SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, 0.11f, [
				new VertexPositionColor(new Vector3(240, 383, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(330, 127, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(333, 127, 0.11f), Color.White * alpha),
				new VertexPositionColor(new Vector3(243, 383, 0.11f), Color.White * alpha),
			]);
			//Modifiers
			NormalFont.Draw(Localization.GetText("ResultScreen.ArrowSpeed", FloatToString(Settings.SettingsManager.DataLibrary.ArrowSpeed, 2)), new Vector2(340, 150), col, 0.8f, 0.12f);
			NormalFont.Draw($"Death Count: {DeathCount}", new Vector2(340 - 33 * 9 / 25f, 183), DeathCount > 0 ? Color.Red * alpha : col, 0.8f, 0.12f);
			NormalFont.Draw($"Damage Taken: {DamageTaken}", new Vector2(340 - 66 * 9 / 25f, 216), DamageTaken == 0 ? Color.Gold * alpha : col, 0.8f, 0.12f);
			NormalFont.Draw($"{Localization.GetText("ResultScreen.Early")}: {DisplayValues.PerfectE}", new Vector2(340 - 99 * 9 / 25f, 246), Color.Lerp(Color.Transparent, Color.Orange, alpha), 0.8f, 0.12f);
			NormalFont.Draw($"{Localization.GetText("ResultScreen.Late")}: {DisplayValues.PerfectL}", new Vector2(340 - 132 * 9 / 25f, 279), Color.Lerp(Color.Transparent, Color.Orange, alpha), 0.8f, 0.12f);
			NormalFont.Draw(Localization.GetText("ResultScreen.MaxCombo", DisplayValues.Combo), new Vector2(340 - 165 * 9 / 25f, 312), Color.Lerp(Color.Transparent, Color.Silver, alpha), 0.8f, 0.12f);
			NormalFont.DrawContainedText(Localization.GetText("ResultScreen.Modifiers", ModesUsed), new Rectangle(340 - 198 * 9 / 25, 345, 300, 30), col, 0.8f, 0.12f);
			#endregion
			#region Bottom
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.LeaveRestart", MiscUtil.GetInputKeys(InputIdentity.Confirm)[0], MiscUtil.GetInputKeys(InputIdentity.Reset)[0]), new Vector2(320, 455), col, 0.8f, 0.5f);
			#endregion
			#region Item
			NormalFont.CentreDraw(Localization.GetText("ResultScreen.UnlockItem"), new Vector2(320, 140), Color.White * itemUnlockAlpha[0], itemUnlockScale, GetRadian(itemTextAngle), 1);
			if (curItem != null)
			{
				NormalFont.CentreDraw(curItem.Name, new Vector2(320, 340), Color.White * itemUnlockAlpha[1], 1, 0, 1);
				GeneralDraw(curItem.Image, new Vector2(320, 240), Color.White * itemUnlockAlpha[1], depth: 1);
			}
			#endregion
		}

		private readonly float CurrentAccuracy = 0, rerate = 0;
		public override void Update()
		{
			//Lerp
			DisplayValues.Score = MathF.Ceiling(float.Lerp(DisplayValues.Score, score, 0.04f));
			DisplayValues.SAccuracy = float.Lerp(DisplayValues.SAccuracy, CurrentAccuracy, 0.04f);
			DisplayValues.Miss = MathF.Ceiling(float.Lerp(DisplayValues.Miss, missCount, 0.04f));
			DisplayValues.Okay = MathF.Ceiling(float.Lerp(DisplayValues.Okay, okayCount, 0.04f));
			DisplayValues.Nice = MathF.Ceiling(float.Lerp(DisplayValues.Nice, niceCount, 0.04f));
			DisplayValues.Perfect = MathF.Ceiling(float.Lerp(DisplayValues.Perfect, perfectCount, 0.04f));
			DisplayValues.PAccuracy = float.Lerp(DisplayValues.PAccuracy, perfectPercent, 0.04f);
			DisplayValues.PerfectE = MathF.Ceiling(float.Lerp(DisplayValues.PerfectE, perfectECount, 0.04f));
			DisplayValues.PerfectL = MathF.Ceiling(float.Lerp(DisplayValues.PerfectL, perfectLCount, 0.04f));
			DisplayValues.Combo = MathF.Ceiling(float.Lerp(DisplayValues.Combo, maxCombo, 0.04f));
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
			if (gamePlayed.Attributes != null)
			{
				float[] difficultyValues = new float[3];
				if (gamePlayed.Attributes.CompleteDifficulty.TryGetValue((Difficulty)difficulty, out difficultyValues[0]))
					difficultyText[0] = difficultyValues[0].ToString("F1");
				if (gamePlayed.Attributes.ComplexDifficulty.TryGetValue((Difficulty)difficulty, out difficultyValues[1]))
					difficultyText[1] = difficultyValues[1].ToString("F1");
				if (gamePlayed.Attributes.APDifficulty.TryGetValue((Difficulty)difficulty, out difficultyValues[2]))
					difficultyText[2] = difficultyValues[2].ToString("F1");
			}

			if (!encouraged)
			{
				//Net result
				if (OverallNetManager.IsSuccessfullyConnected)
				{
					if (IsKeyPressed120f(InputIdentity.MainLeft))
						NetResultPage--;
					else if (IsKeyPressed120f(InputIdentity.MainRight))
						NetResultPage++;
					NetResultPage = int.Clamp(NetResultPage, 0, NetResultMaxPage);
				}
			}

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
			else if (IsKeyPressed120f(InputIdentity.Reset) && !OverallNetManager.IsSuccessfullyConnected)
				StartSong();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CreateNextUI()
		{
			Dispose();
			if (OverallNetManager.IsSuccessfullyConnected)
			{
				OverallNetManager.ConnectedUserData curDat = OverallNetManager.PlayerData;
				curDat.IsPlayingAChart = false;
				curDat.IsReady = false;
				OverallNetManager.PlayerData = curDat;
			}
			ResetScene(new GameMenuScene());
			DelayEventProcessor.AddInstantEvent(2, GameMain.ResetRendering);
		}
	}
}