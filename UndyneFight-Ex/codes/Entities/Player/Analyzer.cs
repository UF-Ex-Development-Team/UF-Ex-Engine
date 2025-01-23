namespace UndyneFight_Ex.Entities
{
    public partial class Player : Entity
    {
        public enum AnalyzerDataType
        {
            SoulColor = 0,
            Arrow = 1,
            Barrage = 2,
            SoulList = 3,
        }
        public abstract class AnalyzerData(float time) : IComparable
        {
            public abstract AnalyzerDataType DataType { get; }
            public float Time { get; private init; } = time;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(object obj) => Time.CompareTo((obj as AnalyzerData).Time);
        }
        public class SoulChangeData(int soulColor, int soulID, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.SoulColor;
            public int SoulColor { get; init; } = soulColor;
            public int SoulID { get; init; } = soulID;
        }
        public class SoulListData(int soulID, bool isInsert, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.SoulColor;
            public bool IsInsert { get; init; } = isInsert;
            public int SoulID { get; init; } = soulID;
        }
        public class ArrowData(float deltaTime, int judgementResult, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.Arrow;
            public float DeltaTime { get; init; } = deltaTime;
            public int JudgementResult { get; init; } = judgementResult;
        }
        public class Barrage(int judgementType, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.Barrage;
            public int JudgementType { get; init; } = judgementType;
        }
        public Analyzer GameAnalyzer { get; init; } = new();
        public class Analyzer : GameObject
        {
            public List<AnalyzerData> CurrentData { get; init; } = [];
            public override void Update() { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PushData(AnalyzerData analyzerData) => CurrentData.Add(analyzerData);
        }
    }
}