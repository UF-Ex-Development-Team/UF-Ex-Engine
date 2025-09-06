using UndyneFight_Ex.IO;
using UndyneFight_Ex.UserService;

namespace UndyneFight_Ex
{
	public static class PlayerManager
	{
		private const string CustomChartNotes = "Importing custom charts are very easy, as long as the source chart is made on the same SDK version and the same .NET version, you can import it!\nRequired files:\n - .dll file of the source charts (Rhythm Recall generates Rhythm Recall.dll)\n - The Content files of the source charts (song.ogg, song.xnb, paint.xnb, those kind of files, you can just copy the entire source Content folder)\n\nTo load a custom chart, just open the game!";
		public static void Initialize()
		{
			string path = AppContext.BaseDirectory + "D:\\Microsoft.CodeAnalysis.dll";
			if (File.Exists(Path.Combine(path.Split('\\'))))
			{
				string val = IOEvent.ByteToString(IOEvent.ReadCustomFile(Path.Combine(path.Split('\\'))))[0];
				string[] divs = val.Split(',');
				int[] s = new int[6];
				for (int i = 0; i < 6; i++)
					s[i] = Convert.ToInt32(divs[i]);
			}

			//Directory.CreateDirectory("Mods\\Scripts");
			//Directory.CreateDirectory("Mods\\Fights");
			#region Create Folders
			//User folder
			path = Path.Combine($"{AppContext.BaseDirectory}Datas\\Users".Split('\\'));
			if (!Directory.Exists(path))
				_ = Directory.CreateDirectory(path);
			//Directory.CreateDirectory("Datas\\Records");
			//Licence folder
			path = Path.Combine($"{AppContext.BaseDirectory}Licences".Split('\\'));
			if (!Directory.Exists(path))
				_ = Directory.CreateDirectory(path);
			//Custom Charts folder
			path = Path.Combine($"{AppContext.BaseDirectory}Custom Charts".Split('\\'));
			if (!Directory.Exists(path))
				_ = Directory.CreateDirectory(path);
			path = Path.Combine($"{AppContext.BaseDirectory}Custom Charts\\Note.txt".Split('\\'));
			if (File.Exists(path))
				File.Delete(path);
			FileStream stream = new(path, FileMode.OpenOrCreate);
			StreamWriter textWriter = new(stream);
			textWriter.Write(CustomChartNotes);
			textWriter.Flush();
			stream.Close();
			#endregion
			path = Path.Combine($"{AppContext.BaseDirectory}Datas\\Users".Split('\\'));
			string[] files = Directory.GetFiles(path);
			foreach (string s in files)
			{
				if (!s.EndsWith(".Tmpf"))
					continue;
				string[] v = s.Split('\\');
				string s2 = v[^1], s3 = s2[..^5];
				SaveInfo i1 = FileIO.ReadFile(s[..^5]);
				User user = new();
				user.Load(i1);
				playerInfos.Add(s3, user);
			}
		}
		internal static Dictionary<string, SaveInfo> userSaveInfo = [];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Login(string s)
		{
			if (playerInfos.ContainsKey(s))
			{
				currentPlayer = s;
				CurrentUser.ApplySettings();
				userSaveInfo.TryGetValue(currentPlayer, out SaveInfo saveInfo);
				CurrentUser._achievement.Load(saveInfo.Nexts["Achievements"]);
				CurrentUser.CalculateRating();
				Achievements.AchievementManager.CheckUserAchievements();
				CurrentUser.KeyBinds.Load(saveInfo.Nexts["Keybinds"]);
				GameStates.KeyChecker.InputKeys = new(KeybindData.UserKeys);
				StoreData.UserItems.Clear();
				CurrentUser.ShopData.Load(saveInfo.Nexts["ShopData"]);
				//Store items into user inventory
				foreach (KeyValuePair<string, StoreItem> item in StoreData.AllItems)
					if (item.Value.DefaultInShop)
						StoreData.UserItems.TryAdd(item.Value.FullName, item.Value);
			}
			else
				GameStates.CheatAffirmed();
			Save();
			//Create backup
			if (!Directory.Exists(Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\Backup")))
				Directory.CreateDirectory(Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\Backup"));
			IOEvent.WriteTmpFile(Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\Backup\\{currentPlayer}_{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}".Split('\\')), IOEvent.InfoToByte(playerInfos[currentPlayer].Save()));
			//Purge excess backups
			if (Directory.Exists(Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\Backup")))
			{
				string[] FileList = Directory.GetFiles(Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\Backup"));
				List<Tuple<string, long>> Files = [];
				while (FileList.Length > 100)
				{
					foreach (string file in FileList)
						Files.Add(new(file, File.GetCreationTimeUtc(file).ToFileTimeUtc()));
					//Sort by UTC time
					Files.Sort((x, y) => x.Item2.CompareTo(y.Item2));
					File.Delete(Path.Combine(Files[0].Item1));
					FileList = Directory.GetFiles(Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\Backup"));
					Files.Clear();
				}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string TryLogin(string name, string password) => playerInfos.TryGetValue(name, out User value) ? value.CheckPassword(password) ? "Success!" : "Wrong password!" : "No such user!";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Save()
		{
			if (string.IsNullOrEmpty(currentPlayer))
				return;
			List<string> res = IOEvent.InfoToString(playerInfos[currentPlayer].Save());
			string path = Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\{currentPlayer}".Split('\\'));
			IOEvent.WriteTmpFile(path, IOEvent.StringToByte(res));
#if DEBUG
			string tmp = string.Empty;
			int tabCount = 0;
			lock (res)
			{
				foreach (string item in res)
				{
					tmp += item + "\n";
					if (item.EndsWith('{'))
					{
						tmp = tmp[..^2] + "\n";
						for (int i = 0; i < tabCount; i++)
							tmp += "\t";
						tmp += "{\n";
						tabCount++;
					}
					else if (item.EndsWith('}'))
					{
						tmp = tmp[..^3];
						tmp += "}\n";
						tabCount--;
					}
					for (int i = 0; i < tabCount; i++)
						tmp += "\t";
				}
				path = Path.Combine($"{AppContext.BaseDirectory}Datas\\Users\\{currentPlayer} Data.txt".Split('\\'));
				if (File.Exists(path))
					File.Delete(path);
				FileStream stream2 = new(path, FileMode.OpenOrCreate);
				StreamWriter textWriter = new(stream2);
				textWriter.Write(tmp);
				textWriter.Flush();
				stream2.Close();
			}
#endif
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RecordMark(string songName, int difficulty, SongSystem.SongResult result)
		{
			if (string.IsNullOrEmpty(currentPlayer))
				return;

			User user = CurrentUser;
			user.FinishedSong(songName, (SongSystem.Difficulty)difficulty, result);

			float oldSkill = user.AbsoluteSkill;
			user.UpdateSkill(user.CalculateRating());
			float add = user.AbsoluteSkill - oldSkill;
			user.ShopData.CashManager.Coins += (int)(add * 80);

			Save();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RecordMark(string songName, int difficulty, SongSystem.SkillMark mark, int score, bool fc, bool ap, float acc)
		{
			if (string.IsNullOrEmpty(currentPlayer))
				return;

			RecordMark(songName, difficulty, new SongSystem.SongResult(mark, score, acc, fc, ap));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove(string s)
		{
			File.Delete("Datas\\Users\\" + s + ".Tmpf");
			_ = playerInfos.Remove(s);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Rename(string old, string now)
		{
			User user = CurrentUser;
			CurrentUser.Rename(now);
			Remove(old);
			AddUser(user);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddNewUser(string name, string password)
		{
			FileIO.CreatePlayerFile(name);
			User user = User.CreateNew(name, password);
			playerInfos.Add(name, user);
			Login(name);
			Save();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddUser(User info)
		{
			playerInfos.Add(currentPlayer = info.PlayerName, info);
			Save();
		}
		/// <summary>
		/// The current user that is logged in
		/// </summary>
		public static User CurrentUser => string.IsNullOrEmpty(currentPlayer) ? null : playerInfos.TryGetValue(currentPlayer, out User value) ? value : null;
		/// <summary>
		/// The name of the current user
		/// </summary>
		public static string currentPlayer;
		/// <summary>
		/// Whether is user is logged in
		/// </summary>
		public static bool UserLogin => !string.IsNullOrEmpty(currentPlayer);

		public static bool IsPlayerVIP => CurrentUser.VIP;
		/// <summary>
		/// The rating of the user
		/// </summary>
		public static float PlayerSkill => CurrentUser.Skill;
		public static Dictionary<string, User> playerInfos = [];
	}
}
