using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.UserService;
/// <summary>
/// Interface for savable data
/// </summary>
public interface ISaveLoad
{
	/// <summary>
	/// Saves the <see cref="SaveInfo"/> data
	/// </summary>
	/// <returns>The data saved in the format of <see cref="SaveInfo"/></returns>
	SaveInfo Save();
	/// <summary>
	/// Loads the <see cref="SaveInfo"/> data
	/// </summary>
	/// <param name="info">The <see cref="SaveInfo"/> to save</param>
	void Load(SaveInfo info);
	/// <summary>
	/// Nested save info
	/// </summary>
	List<ISaveLoad> Children { get; }
}
/// <summary>
/// A user account
/// </summary>
public partial class User : ISaveLoad
{
	/// <summary>
	/// The default info of new users
	/// </summary>
	private static readonly Dictionary<string, string> DefaultUserSaveInfo = new()
	{
		["Coins"] = "Coins:0",
		["Achievements"] = "Achievements{",
		["ChampionShips"] = "ChampionShips{",
		["NormalFights"] = "NormalFights{",
		["VIP"] = "VIP:false",
		["AC"] = "AC{",
		["AP"] = "AP{",
		["Mark"] = "Mark{",
		["Skill"] = "Skill:0",
		["GameJolt"] = "GameJolt{",
		["Settings"] = "Settings{",
		["Keybinds"] = "Keybinds{",
		["ShopData"] = "ShopData{",
		["ChallengeData"] = "ChallengeData{",
		["SaveDataVersion"] = "SaveDataVersion:1"
	};
	/// <summary>
	/// Creates a new user
	/// </summary>
	/// <param name="name">The name of the user</param>
	/// <param name="password">The password of the user</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static User CreateNew(string name, string password)
	{
		User user = new();
		SaveInfo info = new("StartInfo->{");
		Random rand = new();
		long uuid = rand.NextInt64();
		info.Nexts.Add("Password", new SaveInfo("Password:" + MathUtil.StringHash(password)));
		info.Nexts.Add("PlayerName", new SaveInfo("PlayerName:" + name));
		info.Nexts.Add("UUID", new SaveInfo("UUID:" + uuid));
		foreach (KeyValuePair<string, string> defSaveInfos in DefaultUserSaveInfo)
			info.Nexts.Add(defSaveInfos.Key, new SaveInfo(defSaveInfos.Value));
		user.Load(info);
		return user;
	}
	/// <inheritdoc/>
	public List<ISaveLoad> Children => null;
	/// <summary>
	/// The song manager for the user
	/// </summary>
	public SongManager SongManager { get; } = new();

	private long _uuid;
	/// <summary>
	/// Is the player a VIP
	/// </summary>
	public bool VIP { get; private set; }
	/// <summary>
	/// The password of the account
	/// </summary>
	public long Password { get; private set; }
	/// <summary>
	/// The name of the player
	/// </summary>
	public string PlayerName { get; private set; }
	/// <summary>
	/// The rating of the player
	/// </summary>
	public float Skill { get; internal set; }
	/// <summary>
	/// The absolute rating of the player
	/// </summary>
	public float AbsoluteSkill { get; internal set; }
	/// <summary>
	/// The statistics of the player
	/// </summary>
	public Statistic PlayerStatistic { get; private set; }
	/// <summary>
	/// The game settings of the player
	/// </summary>
	public Settings Settings { get; private set; }
	/// <summary>
	/// The key binds of the player
	/// </summary>
	internal KeybindData KeyBinds { get; private set; }
	/// <summary>
	/// The player's shop data
	/// </summary>
	public ShopData ShopData { get; private set; }
	/// <summary>
	/// The user's championship data
	/// </summary>
	public ChampionshipManager ChampionshipData { get; private set; }
	/// <summary>
	/// The user's challenge data
	/// </summary>
	public ChallengeData ChallengeData { get; private set; }
	/// <summary>
	/// Custom save info
	/// </summary>
	public SaveInfo Custom { get; private set; }
	/// <summary>
	/// The version of the save data (Useful for when you messed up the save data)
	/// </summary>
	private int SaveDataVersion { get; set; }
	/// <summary>
	/// The latest save data version
	/// </summary>
	private const int LatestSaveDataVersion = 1;
	/// <summary>
	/// The list of actions to invoke for data compatibility
	/// </summary>
	private readonly Dictionary<int, Action<SaveInfo>> SaveDataConvert = new()
	{
		//Clear keybind data cache due to faulty save in beta
		[0] = (info) => info.Nexts.Remove("Keybinds")
	};
	internal AchievementManager _achievement { get; set; }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void FinishedSong(string songName, Difficulty difficulty, SongResult result) => SongManager.FinishedSong(songName, difficulty, result);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void IntoVIP() => VIP = true;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ResetPassword(long password) => Password = password;
	/// <inheritdoc/>

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Load(SaveInfo info)
	{
		//Check for VIP
		VIP = info.GetDirectory("VIP").BoolValue;
		//Get rating
		_ = info.Nexts.TryAdd("Skill", new("value:0"));
		//Get online async (force false)
		_ = info.Nexts.TryAdd("CAsync", new("value:false"));
		Skill = info.GetDirectory("Skill").FloatValue;
		OnlineAsync = info.GetDirectory("CAsync").BoolValue;
		//Get name and password
		PlayerName = info.GetDirectory("PlayerName").StringValue;
		Password = Convert.ToInt64(info.GetDirectory("Password").StringValue);
		info.Nexts.TryAdd("SaveDataVersion", new SaveInfo("SaveDataVersion:0"));
		SaveDataVersion = info.GetDirectory("SaveDataVersion").IntValue;
		//Get Unique User ID, if not then generate one
		if (info.Nexts.ContainsKey("UUID"))
			_uuid = Convert.ToInt64(info.GetDirectory("UUID").StringValue);
		else
		{
			Random rand = new();
			long uuid = rand.NextInt64();
			_uuid = uuid;
		}
		//Get extra information
		_ = info.Nexts.TryAdd("Achievements", new SaveInfo("Achievements{"));
		_ = info.Nexts.TryAdd("Settings", new SaveInfo("Settings{"));
		_ = info.Nexts.TryAdd("Customs", new SaveInfo("Customs{"));
		_ = info.Nexts.TryAdd("ChallengeData", new SaveInfo("ChallengeData{"));
		//Compatibility for old save data versions
		while (SaveDataVersion < LatestSaveDataVersion)
			SaveDataConvert[SaveDataVersion++](info);
		info.SetNext("SaveDataVersion", "SaveDataVersion:" + SaveDataVersion);
		//Get championship data
		ChampionshipData = new();
		ChampionshipData.Load(info.Nexts["ChampionShips"]);
		//Get settings
		Settings = new();
		Settings.Load(info.Nexts["Settings"]);
		//Get keybinds
		KeyBinds = new();
		if (!info.Nexts.ContainsKey("Keybinds"))
			info.PushNext(new KeybindData().Save());
		KeyBinds.Load(info.Nexts["Keybinds"]);
		//Get custom fights
		SaveInfo fightInfo = info.Nexts["NormalFights"];
		//Get player stats
		if (!info.Nexts.ContainsKey("Statistic"))
			info.PushNext(new Statistic().Save());
		SaveInfo statisticInfo = info.Nexts["Statistic"];
		PlayerStatistic = new();
		PlayerStatistic.Load(statisticInfo);
		//Get challenge data
		ChallengeData = new();
		ChallengeData.Load(info.Nexts["ChallengeData"]);
		//Load custom fights
		if (fightInfo.Nexts != null)
			SongManager.Load(fightInfo);
		//Get achievements
		_achievement = new();
		_achievement.Load(info.Nexts["Achievements"]);
		//Get custom data
		Custom = info.Nexts["Customs"];

		UpdateSkill(CalculateRating());
		//Load shop
		bool updated = false;
		if (!info.Nexts.TryGetValue("ShopData", out _))
		{
			SaveInfo value = new("ShopData{");
			info.Nexts.Add("ShopData", value);
			updated = true;
		}
		ShopData = new();
		if (updated)
		{
			ShopData.CashManager.Coins = (int)(AbsoluteSkill * 80);
		}
		PlayerManager.userSaveInfo.Add(PlayerName, info);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Vector2 CalculateRating() => new RatingCalculator(SongManager).CalculateRating();
	/// <summary>
	/// Generates a rating list
	/// </summary>
	/// <returns>The rating list</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RatingCalculator.RatingList GenerateList() => new RatingCalculator(SongManager).GenerateList();

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SaveInfo Save()
	{
		SaveInfo info = new("StartInfo->{");

		info.PushNext(new SaveInfo("VIP:" + (VIP ? "true" : "false")));
		info.PushNext(new SaveInfo("PlayerName:" + PlayerName));
		info.PushNext(new SaveInfo("UUID:" + _uuid));
		info.PushNext(new SaveInfo("Password:" + Password));
		info.PushNext(new SaveInfo("CAsync:" + (OnlineAsync ? "true" : "false")));
		info.PushNext(new SaveInfo("Skill:" + MathUtil.FloatToString(Skill, 3)));
		info.PushNext(new SaveInfo("SaveDataVersion:" + SaveDataVersion));
		info.PushNext(Custom);
		info.PushNext(ChampionshipData.Save());
		info.PushNext(Settings.Save());
		info.PushNext(PlayerStatistic.Save());
		info.PushNext(SongManager.Save());
		info.PushNext(_achievement.Save());
		info.PushNext(ShopData.Save());
		info.PushNext(ChallengeData.Save());
		info.PushNext(KeyBinds.Save());
		return info;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ApplySettings() => Settings.Apply();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void SignUpChampionShip(string title, string div) => ChampionshipData.SignUp(title, div);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Rename(string name) => PlayerName = name;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void UpdateSkill(Vector2 skill)
	{
		Skill = skill.X;
		AbsoluteSkill = skill.Y;
	}
	/// <summary>
	/// Whether the user had participated at the championship
	/// </summary>
	/// <param name="championship">The name of the championship</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InChampionShip(string championship) => ChampionshipData.InChampionship(championship);
	/// <summary>
	/// The division the user participated in in the championship
	/// </summary>
	/// <param name="championship">The name of the championship</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ChampionShipDiv(string championship) => ChampionshipData.ChampionshipDivision(championship);
	/// <summary>
	/// Whether the user had played this song before
	/// </summary>
	/// <param name="curFight">The name of the song</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool SongPlayed(string curFight) => SongManager.SongPlayed(curFight);
	/// <summary>
	/// The song data the user has
	/// </summary>
	/// <param name="curFight">The name of the song</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SongData GetSongData(string curFight) => SongManager.Acquire(curFight);
	/// <summary>
	/// Whether the password matches the user's password
	/// </summary>
	/// <param name="password">The password the player entered</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckPassword(string password)
	{
		if (Password != MathUtil.StringHash(password))
			return false;
		PasswordMemory = password;
		return true;
	}
	/// <summary>
	/// Invokes logout event
	/// </summary>
	public static void Logout()
	{
		KeyChecker.InputKeys = new(KeyChecker.DefaultKeys);
		for (int i = 0; i < KeyChecker.InputKeys.Count; i++)
			KeyChecker.SetIdentityKey(KeyChecker.InputKeys.Keys.ElementAt(i), KeyChecker.DefaultKeys.Values.ElementAt(i));
		ShopItemData.UserItems = [];
	}
	/// <summary>
	/// Whether the data is synced to the server (Currently unused)
	/// </summary>
	public bool OnlineAsync { get; set; } = false;
	/// <summary>
	/// The password stored in memory after login (Currently unused)
	/// </summary>
	public string PasswordMemory { get; set; }
}