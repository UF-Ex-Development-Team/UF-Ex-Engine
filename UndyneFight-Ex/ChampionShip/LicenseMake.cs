using UndyneFight_Ex.IO;

namespace UndyneFight_Ex.ChampionShips
{
	internal class LicenseMaker
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float ReRate(float accuracy)
		{
			float del = 1 - accuracy;
			return accuracy > 1 ? 1 : MathF.Max(0, 1 - MathF.Pow(del * 3, 0.7f) / 2.4f + del * 2);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetScore(ChampionShip championShip)
		{
			StreamWriter writer = new("Content\\result.txt");
			Dictionary<string, SaveInfo> players = GetPlayers();
			SortedList<Tuple<int, float>, string> values = [];

			SongSystem.IChampionShip[] songs;
			List<SongSystem.IChampionShip> tempSongs = [];
			for (int i = 0; i < championShip.Fights.Values.Length; i++)
			{
				SongSystem.IChampionShip v = Activator.CreateInstance(championShip.Fights.Values[i]) as SongSystem.IChampionShip;
				if (!v.GameContent.Attributes.Hidden)
					tempSongs.Add(v);
			}
			songs = [.. tempSongs];

			Dictionary<string, float[]> maxScores = [];
			foreach (SaveInfo s in players.Values)
			{
				if (!s.Nexts["ChampionShips"].Nexts.ContainsKey(championShip.Title))
					continue;
				string div = s.Nexts["ChampionShips"].Nexts[championShip.Title].StringValue;
				if (!maxScores.ContainsKey(div))
					maxScores.Add(div, new float[songs.Length]);
				SaveInfo t = s.Nexts["NormalFights"];
				for (int i = 0; i < songs.Length; i++)
				{
					if (!songs[i].DifficultyPanel.ContainsKey($"div.{div}"))
						continue;
					float v = GetResult(t, songs[i].GameContent.FightName, (int)songs[i].DifficultyPanel[$"div.{div}"]);
					maxScores[div][i] = MathF.Max(maxScores[div][i], v);
				}
			}
			foreach (KeyValuePair<string, float[]> pair in maxScores)
			{
				string str = $"(div.{pair.Key})";
				for (int i = 0; i < songs.Length; i++)
					str += $" {pair.Value[i]}";
				values.Add(new(Convert.ToInt32(pair.Key), -4000), $"full mark: {str}");
			}
			foreach (SaveInfo s in players.Values)
			{
				if (!s.Nexts["ChampionShips"].Nexts.ContainsKey(championShip.Title))
					continue;
				string res = s.Nexts["ChampionShips"].Nexts[championShip.Title].StringValue;
				string div = res;
				res += $"->{s.Nexts["PlayerName"].StringValue}:".PadRight(22);
				SaveInfo t = s.Nexts["NormalFights"];
				float sum = 0;
				float cur;
				char curID = 'A';
				for (int i = 0; i < songs.Length; i++)
				{
					if (!songs[i].DifficultyPanel.ContainsKey($"div.{div}"))
						continue;
					cur = GetResult(t, songs[i].GameContent.FightName, (int)songs[i].DifficultyPanel[$"div.{div}"]);
					cur /= maxScores[div][i];
					float reCur = ReRate(cur);
					res += $" {curID}:{MathUtil.FloatToString(reCur, 3)}({MathUtil.FloatToString(cur, 3)})".PadRight(21);
					curID++;
					sum += reCur;
				}
				res += $" Total:{sum}";
				values.Add(new(Convert.ToInt32(div[0]), -sum), res);
			}
			for (int i = 0; i < values.Values.Count; i++)
				writer.WriteLine(values.Values[i]);
			writer.Flush();
			writer.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetResult(SaveInfo s, string songName, int dif) =>
			s.Nexts.TryGetValue(songName, out SaveInfo value) ? Convert.ToInt32(value.Nexts[((SongSystem.Difficulty)dif).ToString()]["score"]) : 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Dictionary<string, SaveInfo> GetPlayers()
		{
			Dictionary<string, SaveInfo> info = [];
			string[] files = Directory.GetFiles("Datas\\Users");
			for (int i = 0; i < files.Length; i++)
			{
				SaveInfo s = FileIO.ReadFile(files[i].Split('.')[0]);

				if (s != null)
					info.TryAdd(s.Nexts["PlayerName"].StringValue, s);
			}
			return info;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MakeLicense()
		{
			string res = "Licenses";
			string[] files = Directory.GetFiles(res + "\\Input");
			string newpos;
			_ = Directory.CreateDirectory(newpos = res + "\\Result");
			foreach (string v in files)
			{
				string resName = v.Split('\\')[^1].Split('.')[0];

				List<string> texts = [];
				StreamReader sr = new(v);

				while (!sr.EndOfStream)
				{
					string str = sr.ReadLine();
					texts.Add(str + "\n");
				}

				sr.Close();

				try
				{
					string div = texts[0].Split('=')[1];
					div = div.Trim();
					IOEvent.WriteCustomFile($"{newpos}\\{resName}.Tmpf", IOEvent.StringToByte("div:" + div));
				}
				catch
				{
					continue;
				}
			}
		}
	}
}