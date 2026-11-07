using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.FightResources.Sounds;
using static UndyneFight_Ex.FightResources.Sprites;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Achievements;

/// <summary>
/// v0.3.0+ Achievement UI
/// </summary>
public class AchievementUI : Entity
{
	private readonly (Achievement Achievement, string Name, bool Achieved)[] Achievements = [];
	private int Selection = 0;
	private readonly Vector2[] TargetPosition, ActualPosition;
	private readonly Vector2[] TargetBoxPosition, ActualBoxPosition;
	private int KeyHolding = 0;
	private readonly int State = 0;
	/// <summary>
	/// v0.3.0+ Achievement UI
	/// </summary>
	public AchievementUI()
	{
		UpdateIn120 = true;
		int AchCount = AchievementManager.achievements.Count;
		Achievements = new (Achievement Achievement, string Name, bool Achieved)[AchCount];
		ActualPosition = new Vector2[AchCount];
		TargetPosition = new Vector2[AchCount];
		TargetBoxPosition = new Vector2[AchCount];
		ActualBoxPosition = new Vector2[AchCount];
		int i = 0;
		foreach (Achievement achievement in AchievementManager.achievements.Values)
		{
			Achievements[i] = new(achievement, achievement.Title, achievement.Achieved);
			TargetPosition[i] = ActualPosition[i] = new(30, 240 + i * 80);
			TargetBoxPosition[i] = ActualBoxPosition[i] = new(650, 240 + i * 80);
			++i;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdatePositions()
	{
		for (int i = 0; i < TargetPosition.Length; ++i)
		{
			TargetPosition[i] = new(30, 240 + (i - Selection) * 80);
			TargetBoxPosition[i].Y = 240 + (i - Selection) * 80;
		}
	}
	/// <inheritdoc/>
	public override void Update()
	{
		//Exit
		if (State == 0 && IsKeyPressed120f(InputIdentity.Cancel))
		{
			Fight.Functions.PlaySound(select);
			ResetScene(new GameMenuScene());
		}
		//Check valid user
		if (PlayerManager.CurrentUser == null)
			return;
		//Lerp
		int i = 0;
		foreach (Vector2 TarPos in TargetPosition)
		{
			ActualPosition[i] = Vector2.Lerp(ActualPosition[i], TarPos, 0.06f);
			ActualBoxPosition[i] = Vector2.Lerp(ActualBoxPosition[i], TargetBoxPosition[i], 0.06f);
			++i;
		}
		//Selection
		if (InputPressed("down") || InputPressed("up"))
			KeyHolding++;
		if (!InputPressed("down") && !InputPressed("up"))
			KeyHolding = 0;
		//Hold button
		if (InputPressed("down") || (KeyHolding > 60 && KeyHolding % 15 == 0 && InputPressed("down")))
		{
			Selection = MathUtil.Posmod(++Selection, Achievements.Length);
			Fight.Functions.PlaySound(changeSelection);
			UpdatePositions();
		}
		else if (InputPressed("up") || (KeyHolding > 60 && KeyHolding % 15 == 0 && InputPressed("up")))
		{
			Selection = MathUtil.Posmod(--Selection, Achievements.Length);
			Fight.Functions.PlaySound(changeSelection);
			UpdatePositions();
		}
	}
	private readonly Color[] colors = [Color.Coral, Color.LightGoldenrodYellow, Color.Lime, Color.Azure];
	private static int colorTime = 0, colorIndex = 0;
	/// <inheritdoc/>
	public override void Draw()
	{
		//Cover background
		colorTime++;
		DrawingLab.DrawLine(new(0, 240), new Vector2(640, 240), 480, Color.Black, 0);
		Color bgCol = Color.Lerp(Color.Black, Color.Lerp(colors[colorIndex], colors[(colorIndex + 1) % colors.Length], colorTime / 120f), 0.5f);
		DrawingLab.DrawLine(new(0, 240), new Vector2(640, 240), 480, bgCol * 0.7f, 0.01f);
		if (colorTime == 120)
		{
			colorIndex++;
			colorIndex %= colors.Length;
			colorTime = 0;
		}
		//Background lines
		for (int k = -1; k < 15; k++)
		{
			float interval = 640/15f;
			DrawingLab.DrawLine(new Vector2(colorTime * interval / 120f + k * interval, 0), new Vector2(colorTime * interval / 120f + k * interval, 480), 2, bgCol, 0.09f);
			DrawingLab.DrawLine(new Vector2(0, colorTime * interval / 120f + k * interval), new Vector2(640, colorTime * interval / 120f + k * interval), 2, bgCol, 0.09f);
		}
		if (PlayerManager.CurrentUser == null)
		{
			FightResources.Font.NormalFont.CentreDraw("Login to view your achievements", new Vector2(320, 240), Color.Yellow, 1, 0.1f);
			return;
		}
		GlobalResources.Font.NormalFont.Draw("Achievements", new Vector2(450, 20), Color.Yellow, 1, 0.1f);
		int i = 0, completedCount = 0;
		foreach (Achievement achievement in AchievementManager.achievements.Values)
		{
			Color AchBGCol = i == Selection ? Color.Yellow : Color.White;
			//Background
			SpriteBatch.DrawVertex(pixUnit, 0.1f,
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(0, -30), 0), AchBGCol, Vector2.Zero),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(240, -30), 0), AchBGCol, Vector2.UnitX),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(270, 0), 0), AchBGCol, Vector2.One),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(240, 20), 0), AchBGCol, Vector2.One),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(0, 20), 0), AchBGCol, Vector2.UnitY)
				);
			//Black background
			SpriteBatch.DrawVertex(pixUnit, 0.2f,
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, -28), 0), Color.Black, Vector2.Zero),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(239, -28), 0), Color.Black, Vector2.UnitX),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(267, 0), 0), Color.Black, Vector2.One),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, 0), 0), Color.Black, Vector2.UnitY)
				);
			//Progress bar
			float progressPercent = int.Clamp(achievement.CurrentProgress / achievement.FullProgress, 0, 1);
			SpriteBatch.DrawVertex(pixUnit, 0.3f,
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, 18), 0), Color.Lime, Vector2.Zero),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2 + 237 * progressPercent, 18), 0), Color.Lime, Vector2.UnitX),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2 + 265 * progressPercent, 0), 0), Color.Lime, Vector2.One),
				new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, 0), 0), Color.Lime, Vector2.UnitY)
				);
			FightResources.Font.NormalFont.Draw(achievement.Title, ActualPosition[i] + new Vector2(10, -17), Color.White, 0.75f, 0.3f);
			if (progressPercent == 1)
			{
				for (int k = 0; k < 8; k++)
					FightResources.Font.NormalFont.CentreDraw("Completed", ActualPosition[i] + new Vector2(120, 9) + MathUtil.GetVector2(1, k * 45), Color.ForestGreen, 0.6f, 0.4f);
				FightResources.Font.NormalFont.CentreDraw("Completed", ActualPosition[i] + new Vector2(120, 9), Color.Wheat, 0.6f, 0.5f);
				completedCount++;
			}
			else
			{
				string progStr = (progressPercent * 100).ToString("F2") + "%";
				for (int k = 0; k < 8; k++)
					FightResources.Font.NormalFont.CentreDraw(progStr, ActualPosition[i] + new Vector2(120, 9) + MathUtil.GetVector2(1, k * 45), Color.ForestGreen, 0.6f, 0.4f);
				FightResources.Font.NormalFont.CentreDraw(progStr, ActualPosition[i] + new Vector2(120, 9), Color.Wheat, 0.6f, 0.5f);
			}
			//Info box
			DrawingLab.DrawLine(ActualBoxPosition[i], ActualBoxPosition[i] + new Vector2(300, 0), 200, Color.White, 0.6f);
			DrawingLab.DrawLine(ActualBoxPosition[i] + new Vector2(4, 0), ActualBoxPosition[i] + new Vector2(296, 0), 192, Color.Black, 0.7f);
			FightResources.Font.NormalFont.CentreDraw("Requirements:", ActualBoxPosition[i] + new Vector2(150, -75), Color.White, 1, 0.8f);
			FightResources.Font.NormalFont.LimitDraw(achievement.AchievementIntroduction, ActualBoxPosition[i] + new Vector2(15, -42), Color.White, new Vector2(280, 150), 25, 1, 0.8f);
			DrawingLab.DrawLine(ActualBoxPosition[i] + new Vector2(20, -55), ActualBoxPosition[i] + new Vector2(280, -55), 2, Color.Silver, 0.8f);
			++i;
		}
		string compText = $"Completed: {completedCount}/{AchievementManager.achievements.Count}";
		GlobalResources.Font.NormalFont.Draw(compText, new Vector2(640 - FightResources.Font.NormalFont.SFX.MeasureString(compText).X * 0.7f, 45), Color.Yellow, 0.7f, 0.1f);
		//Right side box
		GeneralDraw(pixUnit, new(480, 260), Color.White, new(290, 360), depth: 0.5f);
		GeneralDraw(pixUnit, new(480, 260), Color.Black, new(280, 350), depth: 0.501f);
		Achievement curSelAch = AchievementManager.achievements.ElementAt(Selection).Value;
		FightResources.Font.NormalFont.CentreDraw(curSelAch.Title, new(480, 110), Color.White, float.Min(1, 200f / FightResources.Font.NormalFont.SFX.MeasureString(curSelAch.Title).X), 0.51f);
		DrawingLab.DrawLine(new(380, 130), new(580, 130), 2, Color.Gray, 0.51f);
		FightResources.Font.NormalFont.LimitDraw(curSelAch.AchievementIntroduction, new(360, 150), Color.White, 270, 25, 0.8f, 0.52f);
	}
	private static bool InputPressed(string input) => input switch
	{
		"confirm" => IsKeyPressed120f(InputIdentity.Confirm) || MouseSystem.IsLeftClick(),
		"cancel" => IsKeyPressed120f(InputIdentity.Cancel) || MouseSystem.IsRightClick(),
		"up" => IsKeyPressed120f(InputIdentity.MainUp) || IsKeyPressed120f(InputIdentity.SecondUp) || MouseSystem.MouseWheelDelta > 0,
		"down" => IsKeyPressed120f(InputIdentity.MainDown) || IsKeyPressed120f(InputIdentity.SecondDown) || MouseSystem.MouseWheelDelta < 0,
		"left" => IsKeyPressed120f(InputIdentity.MainLeft) || IsKeyPressed120f(InputIdentity.SecondLeft),
		"right" => IsKeyPressed120f(InputIdentity.MainRight) || IsKeyPressed120f(InputIdentity.SecondRight),
		_ => false
	};
}