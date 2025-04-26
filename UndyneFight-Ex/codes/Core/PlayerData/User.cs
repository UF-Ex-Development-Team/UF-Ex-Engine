using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex.UserService
{
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
        /// Nested save infos
        /// </summary>
        List<ISaveLoad> Children { get; }
    }
    public partial class User : ISaveLoad
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static User CreateNew(string name, string password)
        {
            User user = new();
            SaveInfo info;
            info = new SaveInfo("StartInfo->{");
            Random rand = new();
            long uuid = rand.NextInt64();
            info.Nexts.Add("Password", new SaveInfo("Password:" + MathUtil.StringHash(password)));
            info.Nexts.Add("PlayerName", new SaveInfo("PlayerName:" + name));
            info.Nexts.Add("UUID", new SaveInfo("UUID:" + uuid));
            info.Nexts.Add("Coins", new SaveInfo("Coins:0"));
            info.Nexts.Add("Achievements", new SaveInfo("Achievements{"));
            info.Nexts.Add("ChampionShips", new SaveInfo("ChampionShips{"));
            info.Nexts.Add("NormalFights", new SaveInfo("NormalFights{"));
            info.Nexts.Add("VIP", new SaveInfo("VIP:false"));
            info.Nexts.Add("AC", new SaveInfo("AC{"));
            info.Nexts.Add("AP", new SaveInfo("AP{"));
            info.Nexts.Add("Mark", new SaveInfo("Mark{"));
            info.Nexts.Add("Skill", new SaveInfo("Skill:0"));
            info.Nexts.Add("GameJolt", new SaveInfo("GameJolt{"));
            info.Nexts.Add("Settings", new SaveInfo("Settings{"));
            info.Nexts.Add("Keybinds", new SaveInfo("Keybinds{"));
            info.Nexts.Add("ShopData", new SaveInfo("ShopData{"));
            info.Nexts.Add("ChallengeData", new SaveInfo("ChallengeData{"));
            user.Load(info);
            return user;
        }
        /// <inheritdoc/>
        public List<ISaveLoad> Children => null;

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
        public ChampionshipManager ChampionshipData { get; private set; }
        public ChallengeData ChallengeData { get; private set; }
        /// <summary>
        /// Custom save infos
        /// </summary>
        public SaveInfo Custom { get; private set; }

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
            info.Nexts.TryAdd("Skill", new("value:0"));
            //Get online async (force false)
            info.Nexts.TryAdd("CAsync", new("value:false"));
            Skill = info.GetDirectory("Skill").FloatValue;
            OnlineAsync = info.GetDirectory("CAsync").BoolValue;
            //Get name and password
            PlayerName = info.GetDirectory("PlayerName").StringValue;
            Password = Convert.ToInt64(info.GetDirectory("Password").StringValue);
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
            info.Nexts.TryAdd("Achievements", new SaveInfo("Achievements{"));
            info.Nexts.TryAdd("Settings", new SaveInfo("Settings{"));
            info.Nexts.TryAdd("Customs", new SaveInfo("Customs{"));
            info.Nexts.TryAdd("ChallengeData", new SaveInfo("ChallengeData{"));
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
            //ShopData.Load(info.Nexts["ShopData"]);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InChampionShip(string championship) => ChampionshipData.InChampionship(championship);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ChampionShipDiv(string championship) => ChampionshipData.ChampionshipDivision(championship);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SongPlayed(string curFight) => SongManager.SongPlayed(curFight);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SongData GetSongData(string curFight) => SongManager.Require(curFight);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckPassword(string password)
        {
            if (Password == MathUtil.StringHash(password))
            {
                PasswordMemory = password;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Invokes logout event
        /// </summary>
        public static void Logout()
        {
            GameStates.KeyChecker.InputKeys = new(GameStates.KeyChecker.DefaultKeys);
            StoreData.UserItems = [];
        }
        public bool OnlineAsync { get; set; } = false;
        public string PasswordMemory { get; set; }
    }
}