using System.Diagnostics;
namespace UndyneFight_Ex.Entities;

internal class CheatDetector : Entity
{
	private class FrameDetector : Entity
	{
		public FrameDetector()
		{
			cur = DateTime.Now;
			UpdateIn120 = true;
			CrossScene = true;
		}
		private DateTime cur;
		private int count = 0;
		private float frameAverage = 0;
		private readonly List<int> frames = [];
		private int timeSustain0 = 0, timeSustain1 = 0, timeSustain2 = 0;
		public override void Draw()
		{
#if DEBUG
			GLFont font = GlobalResources.Font.FightFont;
			Color color = Color.White;
			if (timeSustain2 > 1)
				color = Color.DarkRed;
			else if (timeSustain1 > 1)
				color = Color.Red;
			else if (timeSustain0 > 1)
				color = Color.Orange;
			else if (timeSustain0 >= 1)
				color = Color.Yellow;

			font.Draw(frameAverage.ToString("F1"), new(0, 0), color * Fight.Functions.ScreenDrawing.UIColor.A, 0.6f, 0.5f);
#endif
		}

		public override void Update()
		{
			count++;
			DateTime time = DateTime.Now;
			if (time.Second != cur.Second)
			{
				cur = time;
				frames.Add(count);
				while (frames.Count > 5)
					frames.RemoveAt(0);
				frameAverage = 0;
				frames.ForEach(s => frameAverage += s / 5);

				timeSustain0 = count < 120 * GameMain.GameSpeed ? timeSustain0 + 1 : 0;
				timeSustain1 = count < 115 * GameMain.GameSpeed ? timeSustain1 + 1 : 0;
				timeSustain2 = count < 110 * GameMain.GameSpeed ? timeSustain2 + 1 : 0;
#if !DEBUG
				if (timeSustain0 > 10 || timeSustain1 > 6 || timeSustain2 > 3)
				{
                        (CurrentScene as FightScene).PlayDeath();
				}
#endif
				count = 0;
			}
		}
	}
	private class ProcessDetector : GameObject
	{
		public override void Update()
		{
			if ((DateTime.Now.Second % 15) != 0)
				return;
			foreach (Process item in Process.GetProcesses())
			{
				string name = item.ProcessName;
				if (name.Contains("Cheat Engine") || name.Contains("cheatengine"))
				{
					item.Kill();
					break;
				}
			}
		}
	}
	public override void Start()
	{
		AddChild(new FrameDetector());
		AddChild(new ProcessDetector());
		base.Start();
	}
	public override void Draw() { }

	public override void Update() { }
}