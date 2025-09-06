using static UndyneFight_Ex.Fight.Functions.ScreenDrawing;

namespace UndyneFight_Ex.Entities.Advanced
{
	/// <summary>
	/// The effect used in the OST Undyne remake
	/// </summary>
	public class ZaWarudo : GameObject
	{
		internal static bool exist = false;
		private int appearTime = 0;
		private readonly int totalTime;
		private readonly float scale;
		private readonly float oldScreenSize;
		private readonly Vector2 oldScreenPos;
		private Color oldScreenColor;
		private Color oldBoundColor;
		private Color frozenColor = Color.Black;
		private readonly float frozenDepth = 60;

		/// <summary>
		/// The effect used in the OST Undyne remake
		/// </summary>
		/// <param name="time">The duration of the effect</param>
		/// <param name="scale">The scale of the effect</param>
		/// <param name="frozenColor">The color of the bound</param>
		/// <param name="backColor">The background color</param>
		public ZaWarudo(int time, float scale, Color? frozenColor = null, Color? backColor = null)
		{
			if (exist)
			{
				totalTime = -1;
				Dispose();
				return;
			}
			exist = true;
			oldBoundColor = BoundColor;
			if (frozenColor != null)
			{
				BoundColor = frozenColor ?? BoundColor;
				this.frozenColor = frozenColor ?? Color.Black;
			}
			oldScreenSize = ScreenScale;
			oldScreenColor = BackGroundColor;
			oldScreenPos = ScreenPositionDelta;

			BackGroundColor = backColor ?? Color.DarkBlue * 0.27f;
			MakeFlicker(Color.LightBlue);
			totalTime = time;
			this.scale = scale;
			GameMain.GameSpeed = 0.5f;
		}

		/// <inheritdoc/>
		public override void Update()
		{
			if (totalTime <= 0)
			{
				Dispose();
				return;
			}
			if (frozenColor != Color.Black)
			{
				BoundDistance = (appearTime <= totalTime)
				? BoundDistance * 0.87f + Vector4.One * frozenDepth * 0.13f
				: BoundDistance *= 0.87f;
			}
			appearTime++;
			if (appearTime <= 16)
				ScreenScale += (16 - appearTime) / 720f;
			if (appearTime is >= 8 and <= 36)
			{
				GameMain.GameSpeed = GameMain.gameSpeed * 0.8f + scale * 0.2f;
			}
			if (appearTime >= totalTime)
			{
				ScreenScale = GameMain.CurrentDrawingSettings.screenScale * 0.91f + oldScreenSize * 0.09f;
				ScreenPositionDelta = ScreenPositionDelta * 0.91f + oldScreenPos * 0.09f;
				BackGroundColor = new Color(GameMain.CurrentDrawingSettings.backGroundColor.ToVector4() * 0.9f + oldScreenColor.ToVector4() * 0.1f);
				GameMain.GameSpeed = (1 - scale) * (appearTime - totalTime) / 45f + scale;
			}
			else //Center screen at average position of hearts
			{
				//Get list of hearts
				List<Vector2> Pos = [];
				foreach (Player.Heart item in Player.hearts)
					Pos.Add(item.Centre);
				Vector2 AvgPos = Vector2.Zero;
				Pos.ForEach((pos) => AvgPos += pos);
				AvgPos /= -Pos.Count;
				AvgPos += new Vector2(320, 240);
				ScreenPositionDelta = ScreenPositionDelta * 0.85f + AvgPos * 0.15f;
			}
			if (appearTime >= totalTime + 45)
				Dispose();
		}
		/// <inheritdoc/>
		public override void Dispose()
		{
			if (totalTime <= 0)
				goto A;
			exist = false;
			if (frozenColor != Color.Black)
				BoundDistance = Vector4.One;
			GameMain.gameSpeed = 1.0f;
			ScreenScale = oldScreenSize;
			BackGroundColor = oldScreenColor;
			BoundColor = oldBoundColor;
		A:
			base.Dispose();
		}
	}
}
