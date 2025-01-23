using UndyneFight_Ex.Entities;
using UndyneFight_Ex.IO;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.MiscUtil;

namespace UndyneFight_Ex.UserService
{
    /// <summary>
    /// Song data of a chart
    /// </summary>
    /// <param name="name">The chart to check (Use <see cref="IWaveSet.FightName"/>)</param>
    public class SongData(string name) : ISaveLoad
    {
        /// <summary>
        /// The chart results
        /// </summary>
        public class SongState
        {
            private readonly Difficulty difficulty;
            /// <summary>
            /// The score of the chart
            /// </summary>
            public int Score { get; private set; }
            /// <summary>
            /// The skill mark of the chart
            /// </summary>
            public SkillMark Mark { get; private set; }
            /// <summary>
            /// Whether the chart was completed in Full Combo
            /// </summary>
            public bool AC { get; private set; }
            /// <summary>
            /// Whether the chart was completed in All Perfect
            /// </summary>
            public bool AP { get; private set; }
            /// <summary>
            /// The accuracy of the chart
            /// </summary>
            public float Accuracy { get; set; }
            static bool record { set => StateShower.ResultShower.record = value; }
            internal struct ScoreData
            {
                public int PrevScore;
                public float PrevAcc;
            }
            internal static ScoreData scoreData = new();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UpdateNew(SongResult result)
            {
                scoreData.PrevScore = Score;
                scoreData.PrevAcc = Accuracy;
                record = (Score - scoreData.PrevScore) > 0;
                int newScore = result.Score;
                Score = Math.Max(newScore, Score);
                AC |= result.AC;
                AP |= result.AP;
                Mark = (SkillMark)Math.Min((int)Mark, (int)result.CurrentMark);
                Accuracy = MathF.Max(Accuracy, result.Accuracy);
            }

            public SongState(SaveInfo info)
            {
                difficulty = ToDif(info.Title);
                AC = info["AC"] == "true";
                AP = info["AP"] == "true";
                Accuracy = MathHelper.Clamp(MathUtil.FloatFromString(info[info.keysForIndexs.ContainsKey("Accuracy") ? "Accuracy" : "Acc"]), 0, 1);
                Score = Convert.ToInt32(info["score"]);
                Mark = ToMark(info["mark"]);
            }
            public SongState(Difficulty dif, SongResult result)
            {
                difficulty = dif;
                int newScore = result.CurrentMark == SkillMark.Failed ? result.Score / 2 : result.Score;
                Score = newScore;
                AC = result.AC == true;
                AP = result.CurrentMark == SkillMark.Impeccable;
                Mark = result.CurrentMark;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SaveInfo ToInfo() => new(GetShorthandName(difficulty) + ":score=" + Score +
                    ",AC=" + (AC ? "true" : "false") +
                    ",AP=" + (AP ? "true" : "false") +
                    ",Acc=" + MathUtil.FloatToString(Accuracy, 5) +
                    ",mark=" + GetShorthandName(Mark));
        }
        /// <summary>
        /// The name of the song (<see cref="IWaveSet.FightName"/>)
        /// </summary>
        public string SongName { get; } = name;
        /// <inheritdoc/>
        public List<ISaveLoad> Children => null;
        /// <summary>
        /// The list of <see cref="SongState"/>s for each difficulty
        /// </summary>
        public Dictionary<Difficulty, SongState> CurrentSongStates { get; } = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SkillMark ToMark(string s) => s switch
        {
            "Impeccable" or "Im" => SkillMark.Impeccable,
            "Eminent" or "Em" => SkillMark.Eminent,
            "Excellent" or "Ex" => SkillMark.Excellent,
            "Respectable" or "Re" => SkillMark.Respectable,
            "Acceptable" or "Acc"=> SkillMark.Acceptable,
            "Ordinary" or "Ord" => SkillMark.Ordinary,
            "Failed" or "F" => SkillMark.Failed,
            _ => throw new NotImplementedException()
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Difficulty ToDif(string s) => s switch
        {
            "Noob" or "Nb" => Difficulty.Noob,
            "Easy" or "Ez" => Difficulty.Easy,
            "Normal" or "Nr" => Difficulty.Normal,
            "Hard" or "Hd" => Difficulty.Hard,
            "Extreme" or "Ex" => Difficulty.Extreme,
            "ExtremePlus" or "Ex+" => Difficulty.ExtremePlus,
            _ => throw new NotImplementedException()
        };
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Load(SaveInfo info)
        {
            foreach (KeyValuePair<string, SaveInfo> v in info.Nexts)
                CurrentSongStates.Add(ToDif(v.Key), new SongState(v.Value));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SaveInfo GetInformation(Difficulty difficulty) => CurrentSongStates[difficulty].ToInfo();
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SaveInfo Save()
        {
            SaveInfo info = new(SongName + "{");
            for (int i = 0; i < CurrentSongStates.Keys.Count; i++)
                info.PushNext(GetInformation(CurrentSongStates.Keys.ElementAt(i)));
            return info;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateNew(Difficulty dif, SongResult result)
        {
            if (!CurrentSongStates.ContainsKey(dif))
                CurrentSongStates.Add(dif, new SongState(dif, result));
            CurrentSongStates[dif].UpdateNew(result);
        }
    }
    public class SongManager : ISaveLoad
    {
        /// <inheritdoc/>
        public List<ISaveLoad> Children => null;

        public IEnumerable<SongData> AllDatas => songData.Values;
        readonly Dictionary<string, SongData> songData = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SongData Require(string name) => songData[name];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool SongPlayed(string curFight) => songData.ContainsKey(curFight);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FinishedSong(string songName, Difficulty difficulty, SongResult result)
        {
            if (!songData.ContainsKey(songName))
                songData.Add(songName, new SongData(songName));
            songData[songName].UpdateNew(difficulty, result);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Load(SaveInfo info)
        {
            foreach (KeyValuePair<string, SaveInfo> v in info.Nexts)
            {
                songData.Add(v.Key, new SongData(v.Key));
                songData[v.Key].Load(v.Value);
            }
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SaveInfo Save()
        {
            SaveInfo info = new("NormalFights{");
            for (int i = 0; i < songData.Count; i++)
                info.PushNext(songData.ElementAt(i).Value.Save());
            return info;
        }
    }
    public class RatingCalculator(SongManager songManager)
    {
        public class RatingList
        {
            public struct SingleSong(string name, Difficulty difficulty, float accuracy, float threshold, float transferAccuracy, float scoreScale) : IComparable
            {
                public Difficulty difficulty = difficulty;
                public float accuracy = accuracy;
                public float threshold = threshold;
                public float transferAccuracy = transferAccuracy;
                public float scoreResult = threshold * transferAccuracy * scoreScale;
                public string name = name;

                public readonly int CompareTo(object obj)
                {
                    if (obj is not SingleSong)
                        return 0;
                    SingleSong song = (SingleSong)obj;
                    int v = scoreResult.CompareTo(song.scoreResult);
                    return v != 0 ? v : name.CompareTo(song.name);
                }
            }
            public SortedSet<SingleSong> StrictDonors { get; private set; } = [];
            SingleSong completeDonor, fcDonor, apDonor;
            public SingleSong CompleteDonor => completeDonor;
            public SingleSong FCDonor => fcDonor;
            public SingleSong APDonor => apDonor;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Submit(IEnumerable<SingleSong> strictDonors, SingleSong completeDonor, SingleSong fcDonor, SingleSong apDonor)
            {
                this.completeDonor = completeDonor;
                this.fcDonor = fcDonor;
                this.apDonor = apDonor;
                foreach (SingleSong song in strictDonors)
                    _ = StrictDonors.Add(song);
            }
        }

        readonly SongManager _songManager = songManager;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Tuple<float, float, float> GetDifficulty(IWaveSet waveSet, Difficulty difficulty)
        {
            SongInformation Information = waveSet.Attributes;

            float dif1 = 0, dif2 = 0, dif3 = 0;

            if (Information != null)
            {
                if (Information.CompleteDifficulty.TryGetValue(difficulty, out float value))
                    dif1 = value;
                if (Information.ComplexDifficulty.TryGetValue(difficulty, out value))
                    dif2 = value;
                if (Information.APDifficulty.TryGetValue(difficulty, out value))
                    dif3 = value;
            }

            return new(dif1, dif2, dif3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RatingList GenerateList()
        {
            RatingList.SingleSong ap1 = new("NULL", Difficulty.Noob, 0, 0, 0, 0);
            RatingList.SingleSong comp1 = new("NULL", Difficulty.Noob, 0, 0, 0, 0);
            RatingList.SingleSong fc1 = new("NULL", Difficulty.Noob, 0, 0, 0, 0);

            float apMax = 0, fcMax = 0, completeMax = 0;
            SortedSet<float> alls = [];
            Dictionary<string, IWaveSet> songType = [];
            foreach (Type charts in FightSystem.AllSongs.Values)
            {
                object chartObj = Activator.CreateInstance(charts);
                IWaveSet waveSet = chartObj is IWaveSet ? chartObj as IWaveSet : (chartObj as IChampionShip).GameContent;
                songType.TryAdd(waveSet.FightName, waveSet);
                for (int j = 0; j <= 5; j++)
                {
                    Tuple<float, float, float> chartDiffs = GetDifficulty(waveSet, (Difficulty)j);
                    completeMax = MathF.Max(completeMax, chartDiffs.Item1);
                    fcMax = MathF.Max(fcMax, chartDiffs.Item3);
                    apMax = MathF.Max(apMax, chartDiffs.Item3);
                    _ = alls.Add(chartDiffs.Item2);
                }
            }
            for (int i = 0; alls.Count < 7; i++)
                _ = alls.Add(0 - i * 0.0001f);
            float ideal = 0.001f;
            for (int i = 0; i < 7; i++)
            {
                float g = MathF.Max(0, alls.Max);
                _ = alls.Remove(g);
                ideal += g;
            }

            SortedSet<RatingList.SingleSong> best7 = [];
            static RatingList.SingleSong SelectLarge(RatingList.SingleSong x, RatingList.SingleSong y) => x.scoreResult > y.scoreResult ? x : y;
            foreach (SongData i in _songManager.AllDatas)
            {
                SongData song = i;
                if (!songType.ContainsKey(song.SongName))
                    continue;
                foreach (KeyValuePair<Difficulty, SongData.SongState> j in song.CurrentSongStates)
                {
                    SongData.SongState cur = j.Value;
                    Tuple<float, float, float> dif = GetDifficulty(songType[song.SongName], j.Key);

                    _ = best7.Add(new(song.SongName, j.Key, cur.Accuracy, dif.Item2, ReRate(cur.Accuracy), 85 / ideal));
                    if (best7.Count >= 8)
                        _ = best7.Remove(best7.Min);
                    if (cur.Mark != SkillMark.Failed)
                        comp1 = SelectLarge(comp1, new(song.SongName, j.Key, cur.Accuracy, dif.Item1, 1.0f, 5 / completeMax));
                    if (cur.AP)
                        ap1 = SelectLarge(ap1, new(song.SongName, j.Key, cur.Accuracy, dif.Item3, 1.0f, 5 / apMax));
                    if (cur.AC)
                        fc1 = SelectLarge(fc1, new(song.SongName, j.Key, cur.Accuracy, dif.Item3, 1.0f, 5 / fcMax));
                }
            }
            RatingList result = new();
            while (best7.Count >= 8)
                _ = best7.Remove(best7.Min);
            result.Submit(best7, comp1, fc1, ap1);
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 CalculateRating()
        {
            SortedSet<float> best7 = [];
            float ap1 = 0;
            float comp1 = 0;
            float fc1 = 0;

            float apMax = 0, fcMax = 0, completeMax = 0;
            List<float> alls = [];
            Dictionary<string, IWaveSet> songType = [];
            foreach (Type i in FightSystem.AllSongs.Values)
            {
                object o = Activator.CreateInstance(i);
                IWaveSet waveSet = o is IWaveSet ? o as IWaveSet : (o as IChampionShip).GameContent;
                songType.TryAdd(waveSet.FightName, waveSet);
                for (int j = 0; j <= 5; j += 1)
                {
                    Tuple<float, float, float> v = GetDifficulty(waveSet, (Difficulty)j);
                    completeMax = MathF.Max(completeMax, v.Item1);
                    fcMax = MathF.Max(fcMax, v.Item3);
                    apMax = MathF.Max(apMax, v.Item3);
                    alls.Add(v.Item2);
                }
            }
            foreach (SongData i in _songManager.AllDatas)
            {
                SongData song = i;
                foreach (KeyValuePair<Difficulty, SongData.SongState> j in song.CurrentSongStates)
                {
                    SongData.SongState cur = j.Value;
                    if (!songType.ContainsKey(song.SongName))
                        continue;
                    Tuple<float, float, float> dif = GetDifficulty(songType[song.SongName], j.Key);

                    _ = best7.Add(dif.Item2 * ReRate(cur.Accuracy) + MathUtil.GetRandom(-0.00001f, 0.00001f));
                    if (cur.Mark != SkillMark.Failed)
                        comp1 = MathF.Max(comp1, dif.Item1);
                    if (cur.AP)
                        ap1 = MathF.Max(ap1, dif.Item3);
                    if (cur.AC)
                        fc1 = MathF.Max(fc1, dif.Item3);
                    //Achievement logic
                    if (PlayerManager.CurrentUser != null)
                    {
                        SongResult res = new(j.Value.Mark, j.Value.Score, j.Value.Accuracy, j.Value.AC, j.Value.AP);

                        SongInformation att = songType[song.SongName].Attributes;
                        SongPlayData playData = (att?.ComplexDifficulty.ContainsKey(j.Key) ?? false)
                        ? new SongPlayData()
                        {
                            Result = res,
                            Name = song.SongName,
                            GameMode = GameMode.None,
                            CompleteThreshold = att.CompleteDifficulty[j.Key],
                            ComplexThreshold = att.ComplexDifficulty[j.Key],
                            APThreshold = att.APDifficulty[j.Key],
                            Difficulty = j.Key
                        }
                        : new SongPlayData()
                        {
                            Result = res,
                            Name = song.SongName,
                            GameMode = GameMode.None,
                            CompleteThreshold = 0,
                            ComplexThreshold = 0,
                            APThreshold = 0,
                            Difficulty = j.Key
                        };
                        Achievements.AchievementManager.CheckSongAchievements(playData);
                    }
                }
            }
            for (int i = 0; best7.Count < 7; i++)
                _ = best7.Add(0 - i * 0.00001f);
            for (int i = 0; alls.Count < 7; i++)
                alls.Add(0 - i * 0.00001f);
            float sum = 0.001f, ideal = 0.001f;
            for (int i = 0; i < 7; i++)
            {
                float f = MathF.Max(0, best7.Max), g = MathF.Max(0, alls.Max());
                _ = best7.Remove(f);
                _ = alls.Remove(g);
                ideal += g;
                sum += f;
            }
            float rating0 = sum / ideal * 85f;
            float rating1 = fc1 / fcMax * 5f;
            float rating2 = ap1 / apMax * 5f;
            float rating3 = comp1 / completeMax * 5f;
            return new(rating0 + rating1 + rating2 + rating3, sum + fc1 + ap1 + comp1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ReRate(float accuracy)
        {
            if (accuracy > 1)
                return 1;
            float del = 1 - accuracy;
            float lim = MathF.Pow(del * 3, 0.7f) / 2.4f + del * 2.0f;
            return MathF.Max(0, 1 - lim);
        }
    }
}