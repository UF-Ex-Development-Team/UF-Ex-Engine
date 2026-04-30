using UndyneFight_Ex.Fight;
using static System.MathF;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities;

internal partial class StateShower
{
	internal partial class ResultShower
	{
		private class RatingResult : Entity
		{
			private static readonly float[] thresholds = [-10, 60, 70, 80, 85, 90, 95, float.PositiveInfinity];
			public RatingResult(float old, float cur)
			{
				int l = 0, r = thresholds.Length - 1;
				while (l < r)
				{
					int mid = (l + r + 1) >> 1;
					if (cur < thresholds[mid])
						r = mid - 1;
					else
						l = mid;
				}
				ProgressMade = old < thresholds[l];
				AddChild(ratingShowing = new());
				oldRating = old;
				curRating = cur;
			}
			private readonly float oldRating, curRating;
			private readonly RatingShowing ratingShowing;

			private class LineDrawer(RatingShowing ratingShowing) : Entity
			{
				private readonly RatingShowing rshow = ratingShowing;
				private int appearTime = 0;

				public override void Draw()
				{
					float alpha = Min(1.0f, appearTime / 60f);
					Color color = Color.White * alpha;

					float height = 30;

					DrawingLab.DrawLine(
						new(0, rshow.CollidingBox.TopLeft.Y - 5), new Vector2(0, 20) + rshow.CollidingBox.TopLeft,
						3.5f, color, 0.5f);
					DrawingLab.DrawLine(
						new(0, rshow.CollidingBox.TopLeft.Y - 5 + height), new Vector2(0, 20 + height) + rshow.CollidingBox.TopLeft,
						3.5f, color, 0.5f);
					DrawingLab.DrawLine(
						new(640, rshow.CollidingBox.TopRight.Y - 5), new Vector2(0, 20) + rshow.CollidingBox.TopRight,
						3.5f, color, 0.5f);
					DrawingLab.DrawLine(
						new(640, rshow.CollidingBox.TopRight.Y - 5 + height), new Vector2(0, 20 + height) + rshow.CollidingBox.TopRight,
						3.5f, color, 0.5f);
				}

				public override void Update() => appearTime++;
			}
			public void IntoCentre()
			{
				DelayEventProcessor.AddTimeRangedEvent(0, () =>
				{
					CollideRect v = ratingShowing.CollidingBox;
					v.SetCentre(Vector2.Lerp(ratingShowing.CollidingBox.GetCentre(), new(320, 180), 0.2f));
					ratingShowing.SetArea(v);
					ratingShowing.CoinColor = Color.Lerp(ratingShowing.CoinColor, Color.Transparent, 0.2f);
					ratingShowing.MedalAlpha = float.Lerp(ratingShowing.MedalAlpha, 0, 0.2f);
				}, 100, false);
				string text = Localization.GetText(curRating == 100 ? "RatingBox.FullCongrat" : "RatingBox.Congrat");
				DelayEventProcessor.AddInstantEvent(120, () => InstanceCreate(new TextPrinter(text, new Vector2(320 - GlobalResources.Font.FightFont.SFX.MeasureString(text).X / 2, 280))
				{
					Depth = 1
				}));
				DelayEventProcessor.AddInstantEvent(210, () =>
				{
					string[] Texts = curRating switch
					{
						<85 => [
							Localization.GetText("RatingBox.FlavourText[0][0]"),
							Localization.GetText("RatingBox.FlavourText[0][1]")
						],
						<95 => [
							Localization.GetText("RatingBox.FlavourText[1][0]"),
							Localization.GetText("RatingBox.FlavourText[1][1]")
						],
						<100 => [
							Localization.GetText("RatingBox.FlavourText[2][0]"),
							Localization.GetText("RatingBox.FlavourText[2][1]")
						],
						_ => [
							Localization.GetText("RatingBox.FlavourText[3][0]"),
							Localization.GetText("RatingBox.FlavourText[3][1]")
						]
					};
					for (int i = 0; i < Texts.Length; i++)
						InstanceCreate(new TextPrinter($"$${Texts[i]}", new Vector2(320 - GlobalResources.Font.FightFont.SFX.MeasureString(Texts[i]).X * 0.62f / 2, 335 + i * 35), new TextSizeAttribute(0.62f), new TextSpeedAttribute(15))
						{
							Depth = 1,
						});
				});
				ChangeState(RatingShowState.Encourage);
			}
			public bool ProgressMade { get; private init; }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void AddCoin(int coins)
			{
				ratingShowing.CoinString = "+" + coins;
				DelayEventProcessor.AddInstantEvent(120, () =>
				{
					ratingShowing.CoinColor = Color.Gold;
					ratingShowing.CoinString = MathUtil.FloatToString(PlayerManager.CurrentUser.ShopData.CashManager.Coins);
				});
				float alpha = 1;
				DelayEventProcessor.AddTimeRangedEvent(60, () =>
				{
					alpha -= 1 / (59f * 2f);
					ratingShowing.CoinColor = Color.Gold * alpha;
				}, 59, true);
			}
			public override void Update()
			{
				if (curState == 0)
				{
					if (curRating - oldRating > 0.01f)
						ChangeState(RatingShowState.AddingRating);
					else
						ChangeState(RatingShowState.KeepRating);
				}
				switch (curState)
				{
					case RatingShowState.AddingRating:
						ratingShowing.SkillColor = Color.Lerp(Color.Transparent, Color.Lime, Min(1, appearTime / 30f));
						ratingShowing.SkillString = "+" + MathUtil.FloatToString(curRating - oldRating, 2);
						if (appearTime == 90)
							ChangeState(RatingShowState.ShowRating);
						break;
					case RatingShowState.KeepRating:
						ratingShowing.SkillColor = Color.Lerp(Color.Transparent, Color.Silver, Min(1, appearTime / 30f));
						ratingShowing.SkillString = Localization.GetText("ResultScreen.NoProgress");
						if (appearTime == 90)
							ChangeState(RatingShowState.ShowRating);
						break;
					case RatingShowState.ShowRating:
						if (appearTime <= 30)
							ratingShowing.SkillColor = Color.Lerp(ratingShowing.SkillColor, Color.Transparent, appearTime / 30f);
						else if (appearTime >= 40)
						{
							ratingShowing.SkillColor = Color.Lerp(Color.Transparent, Color.White, Min(1, (appearTime - 40) / 30f));
							ratingShowing.SkillString = MathUtil.FloatToString(curRating, 2);
						}
						break;
					case RatingShowState.Encourage:
						if (appearTime <= 30)
							ratingShowing.SkillColor = Color.Lerp(ratingShowing.SkillColor, Color.Transparent, appearTime / 30f);
						else if (appearTime >= 40)
						{
							ratingShowing.SkillColor = Color.Lerp(Color.Transparent, Color.Gold, Min(1, (appearTime - 40) / 30f));
							ratingShowing.SkillString = $"{MathUtil.FloatToString(oldRating, 2)}->{MathUtil.FloatToString(curRating, 2)}";
						}
						break;
				}
				appearTime++;
			}
			public override void Draw() { }
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void ChangeState(RatingShowState state)
			{
				curState = state;
				appearTime = 0;
			}
			private RatingShowState curState;
			private int appearTime;
			private enum RatingShowState
			{
				AddingRating = 1,
				KeepRating = 2,
				ShowRating = 3,
				Encourage = 4
			}
		}
	}
}