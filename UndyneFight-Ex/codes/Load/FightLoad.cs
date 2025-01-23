using UndyneFight_Ex.ChampionShips;

namespace UndyneFight_Ex
{
    public static class FightSystem
    {
        internal static bool CheckLevelExist { get; set; } = true;
        public static void Initialize(List<Type> loadItems)
        {
            if (loadItems == null)
            {
                if (CheckLevelExist)
                    throw new Exception("There is no levels in your game!");
                else
                    return;
            }
            mainSongs.Clear();
            for (int i = 0; i < loadItems.Count; i++)
                mainSongs.Add(loadItems.ElementAt(i));
            mainSongs.ForEach(MainGameSongs.Push);
            mainSongs.ForEach(AllSongs.Push);

            /* string[] files = Directory.GetFiles("Content\\Fights");
             foreach(string s in files)
             {
                 PushFight(s);
             }*/
        }
        /*
        private static void PushFight(string codeFile)
        {
            CompilerParameters cplist = new CompilerParameters();
            cplist.GenerateExecutable = false;
            cplist.GenerateInMemory = true;
            cplist.ReferencedAssemblies.Add("System.dll");
            cplist.ReferencedAssemblies.Add("System.XML.dll");
            cplist.ReferencedAssemblies.Add("System.Data.dll");

            CodeDomProvider provider1 = CodeDomProvider.CreateProvider("CSharp");

            CompilerResults cr = provider1.CompileAssemblyFromFile(cplist, codeFile);

            if (cr.Errors.HasErrors)
            {
                foreach (CompilerError err in cr.Errors)
                {
                    MessageBox.Show(err.ErrorText);
                }
            }
            else
            {
                // 通过反射，执行代码
                Assembly objAssembly = cr.CompiledAssembly;
                object obj = objAssembly.CreateInstance("CodeTest.Test");
                MethodInfo objMI = obj.GetType().GetMethod("ShowMessage");
                objMI.Invoke(obj, new object[] { "This is CodeTest!" });
            }
        }*/

        private static readonly List<Type> mainSongs = [];

        public static SongSet CurrentSongs { get; private set; }
        public static SongSet AllSongs { get; private set; } = new SongSet("All");
        public static SongSet MainGameSongs { get; private set; } = new SongSet("MainGameSong");
        public static SongSet CustomSongs { get; set; } = new SongSet("Custom Charts");
        public static FightSet MainGameFights { get; private set; } = new FightSet("MainGameFight");

        public static List<ChampionShip> ChampionShips { get; private set; } = [];
        public static ChampionShip CurrentChampionShip { get; internal set; }

        public static List<Challenge> Challenges { get; internal set; } = [];
        public static Dictionary<string, Challenge> ChallengeDictionary { get; internal set; } = [];

        public static List<SongSet> ExtraSongSets { get; internal set; } = [];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSongSet(SongSet songSet) => ExtraSongSets.Add(songSet);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSongSet(SongSet songSet) => ExtraSongSets.Remove(songSet);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushChampionShip(ChampionShip championShip)
        {
            ChampionShips.Add(championShip);
            for (int i = 0; i < championShip.Fights.Values.Length; i++)
                AllSongs.Push(championShip.Fights.Values.ElementAt(i));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushChallenge(Challenge challenge)
        {
            Challenges.Add(challenge);
            ChallengeDictionary.Add(challenge.Title, challenge);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushExtra(Fight.IExtraOption classicFight) => MainGameFights.Push(classicFight.GetType());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SelectSongSet(ChampionShip championShip) => CurrentSongs = championShip.Fights;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SelectMainSet()
        {
            CurrentChampionShip = null;
            CurrentSongs = MainGameSongs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Type> GetAllAvailables()
        {
            List<Type> result = [.. from v in MainGameSongs.Values select v];
            foreach (SongSet s in ExtraSongSets)
                result.AddRange(from v in s.Values select v);
            foreach (ChampionShip c in ChampionShips)
                if (c.CheckTime.Invoke() == ChampionShip.ChampionShipStates.End)
                    result.AddRange(from v in c.Fights.Values select v);
            return result;
        }
    }
}