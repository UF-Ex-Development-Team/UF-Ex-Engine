using System.Text.Json.Serialization;
using UndyneFight_Ex.Server;
using UndyneFight_Ex.SongSystem;

namespace UFData
{
    public class ChampionshipScoreboard
    {
        public SortedSet<ChampionshipParticipant> Members { get; set; } = [];

        public bool PushScore(User user, DivisionInformation div, SongPlayData data)
        {
            try
            {
                ChampionshipParticipant? p = null;
                foreach (ChampionshipParticipant participant in Members)
                {
                    if (participant.UUID == user.UUID)
                    {
                        p = participant;
                        break;
                    }
                }
                if (p == null)
                    _ = Members.Add(p = new(user.UUID, user.Name, div));
                p.Update(div.Info[data.Name].Item1, data.Result.Accuracy);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
    }
    public class ChampionshipInfo
    {
        public ChampionshipInfo(string name, DateTime start, DateTime end, Dictionary<string, DivisionInformation> divisions)
        {
            Name = name;
            StartTime = start;
            EndTime = end;
            Divisions = divisions;
        }
        public ChampionshipInfo() { }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [JsonIgnore]
        public bool Available => DateTime.UtcNow < EndTime && DateTime.UtcNow > StartTime;

        public Dictionary<string, DivisionInformation> Divisions { get; set; } = [];
        public Dictionary<long, string> Participants { get; set; } = [];
    }
    public record class DivisionInformation(string DivisionName, Dictionary<string, Tuple<int, Difficulty>> Info, ChampionshipScoreboard Scoreboard);
    public class ChampionshipParticipant(long UUID, string name, DivisionInformation curDivision) : IComparable<ChampionshipParticipant>
    {
        public void Update() => _count = false;
        public void Update(int index, float acc)
        {
            AccuracyList[index] = MathF.Max(acc, AccuracyList[index]);
            _count = false;
        }

        public string Division { get; set; } = curDivision.DivisionName;
        public long UUID { get; set; } = UUID;
        public string Name { get; set; } = name;
        [JsonInclude]
        public float[] AccuracyList { get; set; } = new float[curDivision.Info.Count];

        private static float ItemTransfer(float acc)
        {
            if (acc > 1)
                return 1;
            float del = 1 - acc;
            float lim = MathF.Pow(del * 3, 0.7f) / 2.4f + del * 2.0f;
            return MathF.Max(0, 1 - lim);
        }
        private float _total;
        private bool _count;

        [JsonIgnore]
        public float Total
        {
            get
            {
                if (_count)
                    return _total;
                _count = true;
                float s = 0;
                for (int i = 0; i < AccuracyList.Length; i++)
                    s += ItemTransfer(AccuracyList[i]);
                return _total = s;
            }
        }

        public int CompareTo(ChampionshipParticipant? other) => other == null ? 1 : -Total.CompareTo(other.Total);
    }
}