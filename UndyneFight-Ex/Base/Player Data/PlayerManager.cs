using UndyneFight_Ex.IO;
using UndyneFight_Ex.UserService;

namespace UndyneFight_Ex;

public static class PlayerManager
{
	private const string CustomChartNotes = "Importing custom charts are very easy, as long as the source chart is made on the same SDK version and the same .NET version, you can import it!\nRequired files:\n - .dll file of the source charts (Rhythm Recall generates Rhythm Recall.dll)\n - The Content files of the source charts (song.ogg, song.xnb, paint.xnb, those kind of files, you can just copy the entire source Content folder)\n\nTo load a custom chart, just open the game!";
	internal static void Initialize()
	{
		string path = GlobalData.GetTruePath(AppContext.BaseDirectory + "D:\\Microsoft.CodeAnalysis.dll");
		if (File.Exists(path))
		{
			string val = IOEvent.ByteToString(IOEvent.ReadCustomFile(path))[0];
			string[] divs = val.Split(',');
			int[] s = new int[6];
			for (int i = 0; i < 6; i++)
				s[i] = Convert.ToInt32(divs[i]);
		}
		#region Create Folders
		static void CheckCreateDirectory(string path)
		{
			if (!Directory.Exists(path))
				_ = Directory.CreateDirectory(path);
		}
		//User folder
		CheckCreateDirectory(GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Users"));
		//Licenses folder
		CheckCreateDirectory(GlobalData.GetTruePath($"{AppContext.BaseDirectory}Licenses"));
		//Custom Charts folder
		CheckCreateDirectory(GlobalData.GetTruePath($"{GameStates.SavePath}\\Custom Charts"));
		//Settings folder
		CheckCreateDirectory(GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Settings"));
		//Create default settings
		if (!File.Exists(SettingsFilePath + ".Tmpf"))
			SaveSettings();
		SaveInfo loadedInfo = FileIO.ReadFile(SettingsFilePath);
		Settings.Load(SettingsInfo = loadedInfo.Nexts["Settings"]);
		KeyBinds.Load(KeyBindInfo = loadedInfo.Nexts["Keybinds"]);
		//Create custom chart notes file
		FileStream stream = new(GlobalData.GetTruePath($"{GameStates.SavePath}\\Custom Charts\\Note.txt"), FileMode.Create);
		StreamWriter textWriter = new(stream);
		textWriter.Write(CustomChartNotes);
		textWriter.Flush();
		stream.Close();
		#endregion
		//Load user files
		LoadUserData();
	}
	internal static readonly string SettingsFilePath = GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Settings\\Settings");
	internal static UserService.Settings Settings = new();
	internal static SaveInfo SettingsInfo;
	internal static KeybindData KeyBinds = new();
	internal static SaveInfo KeyBindInfo;
	internal static Dictionary<string, SaveInfo> userSaveInfo = [];
	/// <summary>
	/// Logins as the user with the given username
	/// </summary>
	/// <param name="s">The username</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Login(string s)
	{
		if (playerInfo.ContainsKey(s))
		{
			currentPlayer = s;
			_ = userSaveInfo.TryGetValue(currentPlayer, out SaveInfo saveInfo);
			//Load user achievements
			CurrentUser._achievement.Load(saveInfo.Nexts["Achievements"]);
			//Load user challenges
			CurrentUser.ChallengeData.Load(saveInfo.Nexts["ChallengeData"]);
			_ = CurrentUser.CalculateRating();
			Achievements.AchievementManager.CheckUserAchievements();
			GameStates.KeyChecker.InputKeys = new(KeybindData.UserKeys);
			//Store items into user inventory
			ShopItemData.UserItems.Clear();
			CurrentUser.ShopData.Load(saveInfo.Nexts["ShopData"]);
			foreach (StoreItem item in ShopItemData.AllItems.Values)
				if (item.DefaultInShop)
					_ = ShopItemData.UserItems.TryAdd(item.FullName, item);
		}
		else
			GameStates.CheatAffirmed();
		Save();
		//Create backup
		string BackupPath = GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Users\\Backup");
		if (!Directory.Exists(BackupPath))
			_ = Directory.CreateDirectory(BackupPath);
		DateTime curTime = DateTime.Now;
		IOEvent.WriteTmpFile(GlobalData.GetTruePath($"{BackupPath}\\{currentPlayer}_{curTime.Year}_{curTime.Month}_{curTime.Day}_{curTime.Hour}_{curTime.Minute}_{curTime.Second}"), IOEvent.InfoToByte(playerInfo[currentPlayer].Save()));
		//Purge excess backups
		string[] FileList = Directory.GetFiles(BackupPath);
		List<(string Name, long UTC)> Files = [];
		while (FileList.Length > 100)
		{
			foreach (string file in FileList)
				Files.Add(new(file, File.GetCreationTimeUtc(file).ToFileTimeUtc()));
			//Sort by UTC time
			Files.Sort((x, y) => x.UTC.CompareTo(y.UTC));
			File.Delete(Path.Combine(Files[0].Name));
			FileList = Directory.GetFiles(BackupPath);
			Files.Clear();
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string TryLogin(string name, string password) => playerInfo.TryGetValue(name, out User value) ? value.CheckPassword(password) ? "Success!" : "Wrong password!" : "No such user!";
	/// <summary>
	/// Saves the current user's data
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Save()
	{
		if (string.IsNullOrEmpty(currentPlayer))
			return;
		//Save user data
		List<string> res = IOEvent.InfoToString(userSaveInfo[currentPlayer] = playerInfo[currentPlayer].Save());
		string path = GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Users\\{currentPlayer}");
		IOEvent.WriteTmpFile(path, IOEvent.StringToByte(res));
#if DEBUG
		string tmp = string.Empty;
		int tabCount = 0;
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
		path = GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Users\\{currentPlayer} Data.txt");
		if (File.Exists(path))
			File.Delete(path);
		using FileStream stream2 = new(path, FileMode.Create);
		using StreamWriter textWriter = new(stream2);
		textWriter.Write(tmp);
#endif
	}
	public static void SaveSettings()
	{
		SaveInfo info = new("StartInfo->");
		info.PushNext(Settings.Save());
		info.PushNext(KeyBinds.Save());
		IOEvent.WriteTmpFile(SettingsFilePath, IOEvent.InfoToByte(info));
	}
	/// <summary>
	/// Stores the chart completion result for the current user
	/// </summary>
	/// <param name="songName">The name of the chart</param>
	/// <param name="difficulty">The difficulty played</param>
	/// <param name="result">The chart result</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RecordMark(string songName, int difficulty, SongSystem.SongResult result)
	{
		if (string.IsNullOrEmpty(currentPlayer))
			return;

		User user = CurrentUser;
		SongData.SongState prevData = (user.GetSongData(songName)?.CurrentSongStates.TryGetValue((SongSystem.Difficulty)difficulty, out SongData.SongState dat) ?? false) ? dat : null;
		int prevScore = prevData?.Score ?? 0;
		float prevAcc = prevData?.Accuracy ?? 0;
		user.FinishedSong(songName, (SongSystem.Difficulty)difficulty, result, prevScore, prevAcc);

		float oldSkill = user.AbsoluteSkill;
		user.UpdateSkill(user.CalculateRating());
		float add = user.AbsoluteSkill - oldSkill;
		user.ShopData.CashManager.Coins += (int)(add * 80);

		Save();
	}
	/// <summary>
	/// Stores the chart completion result for the current user
	/// </summary>
	/// <param name="songName">The name of the chart</param>
	/// <param name="difficulty">The difficulty played</param>
	/// <param name="mark">The rating of the play</param>
	/// <param name="score">The score of the play</param>
	/// <param name="fc">Whether it was a Full Combo</param>
	/// <param name="ap">Whether it was an All Perfect</param>
	/// <param name="acc">The accuracy of the chart</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RecordMark(string songName, int difficulty, SongSystem.SkillMark mark, int score, bool fc, bool ap, float acc) => RecordMark(songName, difficulty, new SongSystem.SongResult(mark, score, acc, fc, ap));
	/// <summary>
	/// Removes the user with the given username
	/// </summary>
	/// <param name="s">The username to remove</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Remove(string s)
	{
		File.Delete(GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Users\\{s}.Tmpf"));
		_ = playerInfo.Remove(s);
	}
	/// <summary>
	/// Renames the current user
	/// </summary>
	/// <param name="old">The old username</param>
	/// <param name="now">The new username</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Rename(string old, string now)
	{
		User user = CurrentUser;
		CurrentUser.Rename(now);
		Remove(old);
		AddUser(user);
	}
	/// <summary>
	/// Creates a new user
	/// </summary>
	/// <param name="name">The new username</param>
	/// <param name="password">The password of the user</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddNewUser(string name, string password)
	{
		FileIO.CreatePlayerFile(name);
		User user = User.CreateNew(name, password);
		playerInfo.Add(name, user);
		Login(name);
		Save();
	}
	/// <summary>
	/// Creates a new user
	/// </summary>
	/// <param name="info">The user to add</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddUser(User info)
	{
		playerInfo.Add(currentPlayer = info.PlayerName, info);
		Save();
	}
	public static void LoadUserData()
	{
		string[] files = Directory.GetFiles(GlobalData.GetTruePath($"{GameStates.SavePath}\\Datas\\Users"));
		foreach (string s in files)
		{
			//Only load tmpf user files
			if (!s.EndsWith(".Tmpf"))
				continue;
			//Get the username from the file path
			string[] v = s.Split('\\');
			string s2 = v[^1][..^5];
			//Read user data and store it
			SaveInfo i1 = FileIO.ReadFile(s[..^5]);
			User user = new();
			user.Load(i1);
			playerInfo.Add(s2, user);
		}
	}
	/// <summary>
	/// The current user that is logged in
	/// </summary>
	public static User CurrentUser => string.IsNullOrEmpty(currentPlayer) ? null : playerInfo.TryGetValue(currentPlayer, out User value) ? value : null;
	/// <summary>
	/// The name of the current user
	/// </summary>
	public static string currentPlayer { get; set; }
	/// <summary>
	/// Whether is user is logged in
	/// </summary>
	public static bool UserLogin => !string.IsNullOrEmpty(currentPlayer);
	/// <summary>
	/// The rating of the user
	/// </summary>
	public static float PlayerSkill => CurrentUser.Skill;
	/// <summary>
	/// The list of users in the data folder
	/// </summary>
	public static Dictionary<string, User> playerInfo { get; set; } = [];
}
