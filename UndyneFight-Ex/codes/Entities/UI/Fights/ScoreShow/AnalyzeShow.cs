using static System.MathF;
using static UndyneFight_Ex.DrawingLab;
using static UndyneFight_Ex.Entities.Player;
using static UndyneFight_Ex.GlobalResources.Font;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    internal partial class StateShower
    {
        internal partial class ResultShower
        {
            private class AnalyzeShow : Entity
            {
                public float Alpha { private get; set; }
                public bool Enabled { get; internal set; }

                private readonly Analyzer analyzer;

                public AnalyzeShow(Analyzer analyzer)
                {
                    Depth = 0.3f;
                    this.analyzer = analyzer;
                    Analyze();
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private void Analyze()
                {
                    AnalyzerData[] data = [..analyzer.CurrentData];
                    Array.Sort(data);

                    leftTime = data[0].Time;
                    rightTime = data[^1].Time;
                    totalTime = rightTime - leftTime;

                    int maximumSoulCount = 0;

                    List<SoulChangeData> soulChangeData = [];
                    List<ArrowData> arrowData = [];
                    for (int i = 0; i < data.Length; i++)
                    {
                        AnalyzerData datum = data[i];
                        if (datum is SoulChangeData)
                        {
                            soulChangeData.Add(datum as SoulChangeData);
                        }
                        else if (datum is SoulListData)
                        {
                            SoulListData sld = datum as SoulListData;
                            if (!sld.IsInsert)
                                soulChangeData.Add(new SoulChangeData(-1, sld.SoulID, sld.Time));
                            maximumSoulCount = Math.Max(maximumSoulCount, sld.SoulID + 1);
                        }
                        else if (datum is ArrowData)
                        {
                            arrowData.Add(datum as ArrowData);
                        }
                    }
                    colorAlternates = new Dictionary<float, int>[maximumSoulCount];
                    for (int i = 0; i < maximumSoulCount; i++)
                    {
                        colorAlternates[i] = [];
                    }

                    foreach (SoulChangeData datum in soulChangeData)
                    {
                        if (colorAlternates[datum.SoulID].ContainsKey(datum.Time))
                            continue;
                        colorAlternates[datum.SoulID].Add(datum.Time, datum.SoulColor);
                    }

                    int pos;
                    foreach (ArrowData datum in arrowData)
                    {
                        pos = (int)PosLerp(0, SplitCount, leftTime, rightTime + 1, datum.Time);
                        remarkCount[pos, datum.JudgementResult]++;
                    }
                    for (int i = 0; i < SplitCount; i++)
                    {
                        float v = 0;
                        for (int j = 0; j < 6; j++)
                        {
                            v += remarkCount[i, j];
                        }
                        remarkHeightMax = Max(remarkHeightMax, v);
                        remarkTotal[i] = v;
                    }

                    foreach (ArrowData datum in arrowData)
                    {
                        pos = (int)PosLerp(0, SplitCount, leftTime, rightTime + 1, datum.Time);
                        averageDelta[pos] += datum.DeltaTime / remarkTotal[pos];
                        if (datum.DeltaTime > 0.01f)
                            averagePositiveDelta[pos] += datum.DeltaTime / remarkTotal[pos];
                        else if (datum.DeltaTime < 0.01f)
                            averageNegativeDelta[pos] += datum.DeltaTime / remarkTotal[pos];
                    }
                }

                private const int SplitCount = 120;

                private static readonly Color[] remarkColor = [Color.DarkRed, Color.Lime, Color.LightBlue, Color.Gold, Color.Orange, Color.OrangeRed];
                private static readonly int[] remarkOrder = [3, 4, 5, 2, 1, 0];

                private float totalTime, leftTime, rightTime;
                private Dictionary<float, int>[] colorAlternates;
                private readonly float[,] remarkCount = new float[SplitCount, 6];
                private readonly float[] remarkTotal = new float[SplitCount];

                private readonly float[] averageDelta = new float[SplitCount];
                private readonly float[] averagePositiveDelta = new float[SplitCount];
                private readonly float[] averageNegativeDelta = new float[SplitCount];

                private float remarkHeightMax = 0;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static float PosLerp(float lPos, float rPos, float lTime, float rTime, float curTime) =>
                    (rPos - lPos) * ((curTime - lTime) / (rTime - lTime)) + lPos;
                public override void Draw()
                {
                    if (!Enabled)
                        return;

                    float lastX;
                    int y = 194;
                    float graphL = 212, graphR = 614;
                    for (int i = 0; i < colorAlternates.Length; i++)
                    {
                        y += 2;

                        lastX = graphL;
                        Color color = Color.Transparent;
                        foreach (KeyValuePair<float, int> kvp in colorAlternates[i])
                        {
                            float x = PosLerp(graphL, graphR, leftTime, rightTime, kvp.Key);
                            DrawLine(new(lastX, y), new(x - 1, y), 1, Color.Lerp(Color.Transparent, color, Alpha), 0.5f);

                            color = kvp.Value switch
                            {
                                -1 => Color.Transparent,
                                0 => Color.Red,
                                1 => Color.Lime,
                                2 => Color.Blue,
                                3 => Color.Orange,
                                4 => Color.MediumPurple,
                                _ => Color.Gray,
                            };
                            lastX = x;
                        }
                        DrawLine(new(lastX, y), new(graphR - 1, y), 1, Color.Lerp(Color.Transparent, color, Alpha), 0.5f);
                    }

                    lastX = graphL;
                    graphL -= 3;
                    graphR += 3;
                    for (int i = 1; i < SplitCount; i++)
                    {
                        float x = PosLerp(graphL, graphR, 0, SplitCount, i);

                        y = 191;
                        for (int j = 0; j < 6; j++)
                        {
                            int remark = remarkOrder[j];
                            if (remarkCount[i, remark] < 0.5f)
                                continue;
                            int height = (int)(remarkCount[i, remark] * 100 / remarkHeightMax);
                            FormalDraw(FightResources.Sprites.pixUnit,
                            new Rectangle((int)lastX, y - height, (int)x - (int)lastX, height), Color.Lerp(Color.Transparent, remarkColor[remark], Alpha));
                            y -= height;
                        }
                        lastX = x;
                    }
                    graphL = 212;
                    graphR = 614;

                    float centreY = 282;
                    DrawLine(new Vector2(graphL - 4, centreY), new Vector2(graphR, centreY), 2, Color.Lerp(Color.Transparent, Color.Silver, Alpha), 0.99f);

                    lastX = graphL;

                    FightFont.Draw("early", new Vector2(graphL + 2, centreY + 5), Color.Lerp(Color.Transparent, Color.Orange, Alpha), MathF.PI / 2, 0.6f, 0.99f);
                    FightFont.Draw("late", new Vector2(graphL + 2, centreY - 42), Color.Lerp(Color.Transparent, Color.Violet, Alpha), MathF.PI / 2, 0.6f, 0.99f);

                    float lastY = averageDelta[0], lastYP = averagePositiveDelta[0], lastYN = averageNegativeDelta[0];
                    for (int i = 1; i < SplitCount; i++)
                    {
                        float x = PosLerp(graphL, graphR, 0, SplitCount - 1, i);
                        float y1 = averageDelta[i], yp = averagePositiveDelta[i], yn = averageNegativeDelta[i];
                        y1 = Clamp(-5, y1, 5);
                        yp = Clamp(-5, yp, 5);
                        yn = Clamp(-5, yn, 5);

                        DrawLine(new Vector2(lastX, lastY * 10 + centreY), new Vector2(x, y1 * 10 + centreY),
                            1, Color.Lerp(Color.Transparent, Color.White, Alpha), 0.5f);

                        if (Abs(yp) > 0.01f || Abs(lastYP) > 0.01f)
                            DrawLine(new Vector2(lastX, lastYP * 10 + centreY), new Vector2(x, yp * 10 + centreY),
                                1, Color.Lerp(Color.Transparent, Color.Orange, Alpha), 0.55f);

                        if (Abs(yn) > 0.01f || Abs(lastYN) > 0.01f)
                            DrawLine(new Vector2(lastX, lastYN * 10 + centreY), new Vector2(x, yn * 10 + centreY),
                                1, Color.Lerp(Color.Transparent, Color.Violet, Alpha), 0.55f);

                        lastY = y1;
                        lastYP = yp;
                        lastYN = yn;

                        lastX = x;
                    }
                }

                public override void Update() { }
            }
        }
    }
}