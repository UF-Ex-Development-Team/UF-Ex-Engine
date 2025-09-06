namespace UndyneFight_Ex.Achievements
{
	internal class AchievementResult : Entity
	{
		public AchievementResult(Achievement achievement)
		{
			// actions
			SetID(currentList.Count + 1);
			currentY = targetY;

			collidingBox = new(vec2.Zero, new(156, 76));

			Depth = 0.90f;
			CrossScene = true;

			currentList.Enqueue(this);
			this.achievement = achievement;
			achDesc = achievement.AchievementIntroduction.Length > 17 ? achievement.AchievementIntroduction[..14] + "..." : achievement.AchievementIntroduction;
		}

		private static readonly Queue<AchievementResult> currentList = new();
		private int appearTime = 0;
		private float bufferX = 340;

		private readonly Achievement achievement;
		private readonly string achDesc;

		private int id = 0;

		private float currentY = 0, targetY = 0;

		private void SetID(int index) => targetY = 522 - 10 - (id = index) * 85;

		private const int totalTime = 300;
		private static readonly GLFont font = GlobalResources.Font.NormalFont;

		public override void Draw()
		{
			DrawingLab.DrawRectangle(collidingBox, col.White, 2f, Depth + 0.01f);
			FormalDraw(FightResources.Sprites.pixUnit, collidingBox.ToRectangle(), col.Black);
			font.LimitDraw("Achievement Unlocked!", collidingBox.TopLeft + new vec2(10, 10), col.White, collidingBox.Size - new vec2(20, 5), 10, 0.4f, Depth + 0.02f);
			font.LimitDraw(achievement.Title, collidingBox.TopLeft + new vec2(10, 25), col.White, new vec2(collidingBox.Width, 20), 10, 0.5f, Depth + 0.02f);
			DrawingLab.DrawLine(collidingBox.TopLeft + new vec2(10, 43), collidingBox.TopRight + new vec2(-10, 43), 1, col.Silver, Depth + 0.02f);
			font.LimitDraw(achDesc, collidingBox.TopLeft + new vec2(10, 50), col.White, new vec2(collidingBox.Width, 20), 10, 0.5f, Depth + 0.02f);
		}

		public override void Update()
		{
			if (++appearTime <= totalTime)
				bufferX = MathF.Round(bufferX * 0.8f, 1);
			else
				bufferX += 10;

			currentY = float.Lerp(currentY, targetY, 0.2f);
			Centre = new(540 - 6 + bufferX, currentY);
			if (Centre.Y < 0)
				appearTime--;

			if (appearTime == totalTime + 20)
			{
				foreach (AchievementResult v in currentList)
					v.SetID(Math.Max(v.id - 1, 1));
			}

			if (appearTime > totalTime + 100)
				Dispose();
		}

		public override void Dispose()
		{
			if (currentList.Peek() != this)
				throw new SystemException("Impossible Case!");
			_ = currentList.Dequeue();
			base.Dispose();
		}
	}
}