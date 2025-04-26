namespace UndyneFight_Ex.Entities
{
    public partial class Player : Entity
    {
        internal enum AnalyzerDataType
        {
            SoulColor = 0,
            Arrow = 1,
            Barrage = 2,
            SoulList = 3,
        }
        internal abstract class AnalyzerData(float time) : IComparable
        {
            public abstract AnalyzerDataType DataType { get; }
            public float Time { get; private init; } = time;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(object obj) => Time.CompareTo((obj as AnalyzerData).Time);
        }
        internal class SoulChangeData(int soulColor, int soulID, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.SoulColor;
            public int SoulColor { get; init; } = soulColor;
            public int SoulID { get; init; } = soulID;
        }
        internal class SoulListData(int soulID, bool isInsert, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.SoulColor;
            public bool IsInsert { get; init; } = isInsert;
            public int SoulID { get; init; } = soulID;
        }
        internal class ArrowData(float deltaTime, int judgementResult, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.Arrow;
            public float DeltaTime { get; init; } = deltaTime;
            public int JudgementResult { get; init; } = judgementResult;
        }
        internal class Barrage(int judgementType, float time) : AnalyzerData(time)
        {
            public override AnalyzerDataType DataType => AnalyzerDataType.Barrage;
            public int JudgementType { get; init; } = judgementType;
        }
        internal Analyzer GameAnalyzer { get; init; } = new();
        internal class Analyzer : GameObject
        {
            public List<AnalyzerData> CurrentData { get; init; } = [];
            public override void Update() { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PushData(AnalyzerData analyzerData) => CurrentData.Add(analyzerData);
        }
    }
}