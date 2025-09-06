using static UndyneFight_Ex.FightResources.Font;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// The name display entity
	/// </summary>
	public class NameShower : Entity
	{
		public NameShower() { Centre = new Vector2(20, 457); instance = this; }
		/// <summary>
		/// The LV of the chart
		/// </summary>
		public static string level = string.Empty;
		/// <summary>
		/// The <see cref="NameShower"/> instance
		/// </summary>
		public static NameShower instance;
		/// <summary>
		/// The alpha of the name text
		/// </summary>
		public static float nameAlpha = 1;
		/// <summary>
		/// The text to override the name with, set to <see cref="string.Empty"/> if to not override
		/// </summary>
		public static string OverrideName = string.Empty;
		/// <summary>
		/// The name of the player
		/// </summary>
		internal static string name;
		private int DisplayNameTime = 0, DisplayNameBufferTime = 0;
		private void StringDotpadding(string FullName, ref string Name)
		{
			int MinIndex = MathHelper.Clamp(DisplayNameTime > 90 ? (DisplayNameTime - 90) / 30 : 0, 0, FullName.Length - 8);
			if (MinIndex == FullName.Length - 8)
				DisplayNameBufferTime++;
			if (DisplayNameBufferTime >= FullName.Length + 18)
			{
				DisplayNameTime = 0;
				DisplayNameBufferTime = 0;
			}
			//Dot padding
			if (FullName.Length > 9 && MinIndex > 0)
				for (int j = 0; j < int.Min(3, MinIndex); j++)
					Name += ".";
			Name += FullName.Length > 9 ? FullName[MinIndex..(8 + MinIndex)] : FullName;
			if (Name != FullName)
				for (int j = 0; j < 3 - int.Min(3, MinIndex); j++)
					Name += ".";
		}
		public override void Draw()
		{
			DisplayNameTime++;
			Vector2 namePos = new(Centre.X, Centre.Y - FightFont.SFX.MeasureString("HP").Y / 2f + 4);
			string showing = OverrideName == string.Empty ? (PlayerManager.CurrentUser is null ? "guest" : PlayerManager.currentPlayer) : OverrideName, displayName = string.Empty;
			StringDotpadding(showing, ref displayName);
			Vector2 lvPos = new(FightFont.SFX.MeasureString(displayName).X + 22 + Centre.X + (GameRule.nameColor == "Colorful" ? 20 : 0), Centre.Y - FightFont.SFX.MeasureString("HP").Y / 2f + 4);

			switch (GameRule.nameColor)
			{
				case "White":
					FightFont.Draw(displayName, namePos, Color.White * nameAlpha);
					break;
				case "Blue":
					FightFont.Draw(displayName, namePos, Color.LightBlue * nameAlpha);
					break;
				case "Orange":
					FightFont.Draw(displayName, namePos, Color.Orange * nameAlpha);
					break;
				default:
					FightFont.Draw(displayName, namePos, new Color(DrawingLab.HsvToRgb(GameMain.gameTime, 255, 255, 255)), 1, Depth + 0.01f);
					for (int i = 0; i < 3; i++)
					{
						Color col = new(DrawingLab.HsvToRgb(GameMain.gameTime / 1.3f + i * 100 + 16, 255, 255, 255));
						FightFont.Draw(displayName, namePos + MathUtil.GetVector2(MathF.Sin(MathUtil.GetRadian(GameMain.gameTime * 2.4f)) * 15, GameMain.gameTime / 1.5f + i * 120), new Color(col, 64), 1.0f, Depth);
					}
					break;
			}

			string trueLV = (level != string.Empty) ? level : difficulty.ToString();
			FightFont.Draw("lv " + trueLV, lvPos, GameMain.CurrentDrawingSettings.UIColor * nameAlpha);
		}
		public override void Update() { }
	}
}