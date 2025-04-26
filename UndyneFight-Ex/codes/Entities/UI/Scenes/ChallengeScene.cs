using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using UndyneFight_Ex.Fight;
using UndyneFight_Ex.SongSystem;
using UndyneFight_Ex.UserService;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.GlobalResources;

namespace UndyneFight_Ex.Entities
{
    internal class ChallengeWinScene : Scene
    {
        public ChallengeWinScene(Challenge challenge) : base(new ChallengeResult(challenge))
        {
            IsInChallenge = false;
            if (PlayerManager.CurrentUser != null)
            {
                PlayerManager.CurrentUser.ChallengeData.FinishChallenge(challenge);
                PlayerManager.Save();
            }
        }
    }

    /// <summary>
    /// Challenge selection
    /// </summary>
    public class ChallengeScene(Scene previous_scene, Action exit_event) : Scene
    {
        /// <summary>
        /// The list of challenges
        /// </summary>
        private readonly List<Challenge> ChallengeList = [];
        /// <summary>
        /// The position of the challenge card
        /// </summary>
        private readonly List<Vector2> CardPosition = [], TargetCardPosition = [];
        /// <summary>
        /// The alpha of the card
        /// </summary>
        private readonly List<float> CardAlpha = [];
        private readonly HashSet<Texture2D> CardIcons = [];
        private readonly List<List<(string Name, Difficulty Difficulty, float Complex)>> ChallengeCharts = [];
        /// <summary>
        /// The size of a card
        /// </summary>
        private readonly List<Vector2> CardSize = [], TargetCardSize = [];
        private readonly List<List<Texture2D>> Illustrations = [];
        private int SelectedChallenge = 0, Time = 0;
        private float minY = -50;
        private SelectState state = SelectState.None;
        private readonly float[] TopIndices = [0, 1.75f, 3.25f, 4];
        private readonly float[] BottomIndices = [0, 0.75f, 2.25f, 4];
        private enum SelectState
        {
            None = 0, SelectedChallenge = 1
        }
        public override void Update()
        {
            //Return to previous scene
            if (IsKeyPressed120f(InputIdentity.Cancel))
            {
                if (state == SelectState.SelectedChallenge)
                {
                    TargetCardSize[SelectedChallenge] = new(200, 300);
                    state = SelectState.None;
                }
                else
                {
                    if (IsReEngine)
                        CurrentScene.CurrentDrawingSettings.defaultWidth = 960;
                    ResetRendering();
                    ResetScene(previous_scene);
                    exit_event();
                }
            }
            if (FightSystem.Challenges.Count == 0)
                return;
            //Initialize list
            if (ChallengeList.Count == 0)
            {
                CurrentScene.CurrentDrawingSettings.defaultWidth = 640;
                ResetRendering();
                for (int i = 0; i < FightSystem.Challenges.Count; i++)
                    ChallengeCharts.Add([]);
                FightSystem.Challenges.ForEach((challenge) =>
                {
                    CardSize.Add(new(200, 300));
                    TargetCardSize.Add(new(200, 300));
                    CardPosition.Add(new Vector2(320 + ChallengeList.Count * 360, 240));
                    TargetCardPosition.Add(new Vector2(320 + ChallengeList.Count * 360, 240));
                    CardAlpha.Add(ChallengeList.Count != 1 ? 0 : 1);
                    CardIcons.Add(LoadContent<Texture2D>(challenge.IconPath, Loader));
                    Illustrations.Add([]);
                    foreach (Tuple<Type, Difficulty> v in challenge.Routes)
                    {
                        IWaveSet wave = Activator.CreateInstance(v.Item1) as IWaveSet;
                        string DispName = wave.Attributes.DisplayName;
                        ChallengeCharts[ChallengeList.Count].Add(new(DispName == string.Empty ? wave.FightName : DispName, v.Item2, wave.Attributes.ComplexDifficulty.TryGetValue(v.Item2, out float ComplexVal) ? ComplexVal : 0));
                        if (File.Exists(Path.Combine($"{AppContext.BaseDirectory}Content\\Musics\\{wave.Music}\\paint.xnb".Split('\\'))))
                            Illustrations[^1].Add(LoadContent<Texture2D>($"Content\\Musics\\{wave.Music}\\paint"));
                    }
                    ChallengeList.Add(challenge);
                });
                for (int i = 0; i < TargetCardPosition.Count; i++)
                {
                    TargetCardPosition[i] = new Vector2(320 + (i - SelectedChallenge) * 360, 240);
                }
            }
            for (int i = 0; i < CardPosition.Count; i++)
            {
                CardSize[i] = Vector2.LerpPrecise(CardSize[i], TargetCardSize[i], 0.1f);
                CardPosition[i] = Vector2.LerpPrecise(CardPosition[i], TargetCardPosition[i], 0.1f);
                CardAlpha[i] = MathHelper.LerpPrecise(CardAlpha[i], i == SelectedChallenge ? 1 : 0.5f, 0.1f);
            }
            if (state == SelectState.None)
            {
                if (IsKeyPressed120f(InputIdentity.MainLeft) && SelectedChallenge > 0)
                {
                    FightResources.Sounds.changeSelection.CreateInstance().Play();
                    SelectedChallenge--;
                    for (int i = 0; i < TargetCardPosition.Count; i++)
                    {
                        TargetCardPosition[i] = new Vector2(320 + (i - SelectedChallenge) * 360, 240);
                    }
                }
                else if (IsKeyPressed120f(InputIdentity.MainRight) && SelectedChallenge < ChallengeList.Count - 1)
                {
                    FightResources.Sounds.changeSelection.CreateInstance().Play();
                    SelectedChallenge++;
                    for (int i = 0; i < TargetCardPosition.Count; i++)
                    {
                        TargetCardPosition[i] = new Vector2(320 + (i - SelectedChallenge) * 360, 240);
                    }
                }
            }
            if (IsKeyPressed120f(InputIdentity.Confirm))
            {
                if (state == SelectState.SelectedChallenge)
                {
                    SongFightingScene.SceneParams[] ListParams = new SongFightingScene.SceneParams[ChallengeList[SelectedChallenge].Routes.Length];
                    int i = 0;
                    foreach (Tuple<Type, Difficulty> ChallengeChart in ChallengeList[SelectedChallenge].Routes)
                    {
                        IWaveSet cur = Activator.CreateInstance(ChallengeChart.Item1) as IWaveSet;
                        string path = "Content\\Musics\\" + cur.Music;
                        Texture2D texture = null;
                        if (File.Exists(Path.Combine($"{AppContext.BaseDirectory}{path}\\paint.xnb".Split('\\'))))
                            lock (this)
                            {
                                texture = LoadContent<Texture2D>($"{AppContext.BaseDirectory}{path}\\paint");
                            }
                        if (!File.Exists(Path.Combine($"{AppContext.BaseDirectory}{path}.xnb".Split('\\'))))
                            path += "\\song";
                        Debug.WriteLine(texture.Name);
                        ListParams[i] = new(cur, texture, (int)ChallengeList[SelectedChallenge].Routes[i].Item2, path, JudgementState.Strict, GameMode.RestartDeny);
                        ++i;
                    }
                    IsInChallenge = true;
                    ResetScene(new SongLoadingScene(ChallengeList[SelectedChallenge], ListParams));
                    GameStates.Broadcast(new(null, "MusicFadeOut"));
                }
                else
                {
                    state = SelectState.SelectedChallenge;
                    TargetCardSize[SelectedChallenge] = new(500, 300);
                    Functions.PlaySound(FightResources.Sounds.select);
                }
            }
            base.Update();
        }
        public override void Draw()
        {
            if (FightSystem.Challenges.Count == 0)
                return;
            #region Card drawing
            for (int i = ChallengeList.Count - 1; i >= 0; i--)
            {
                float TextAlpha = (500 - CardSize[i].X) / 300f;
                bool isSelected = i == SelectedChallenge;
                Color TargetColor = isSelected ? Color.White : Color.Lerp(Color.White, Color.Black, CardAlpha[i]);
                Challenge curChallenge = ChallengeList[i];
                Vector2 ThisCardPosition = CardPosition[i];
                GeneralDraw(FightResources.Sprites.pixUnit, ThisCardPosition, TargetColor, CardSize[i]);
                GeneralDraw(FightResources.Sprites.pixUnit, ThisCardPosition, Color.Black, CardSize[i] - new Vector2(10));
                string ChallengeName = ChallengeList[i].Title;
                Font.NormalFont.CentreDraw(ChallengeName, ThisCardPosition + new Vector2(5, -120), isSelected ? Color.White : Color.Lerp(Color.White, Color.Black, CardAlpha[i]), MathF.Min(0.7f, (CardSize[i].X - 30) / Font.NormalFont.SFX.MeasureString(ChallengeName).X), 1);
                Font.NormalFont.CentreDraw(curChallenge.Desc, ThisCardPosition - new Vector2(0, 90), TargetColor, MathF.Min(1, (CardSize[i].X - 20) / Font.NormalFont.SFX.MeasureString(curChallenge.Desc).X), 1);
                FormalDraw(CardIcons.ElementAt(i), ThisCardPosition - new Vector2(45, 70), TargetColor * TextAlpha, 0, Vector2.Zero);
                //Best score
                if (PlayerManager.CurrentUser.ChallengeData.AllData.TryGetValue(curChallenge.Title, out SingleChallenge challenge))
                {
                    float scorePercent = challenge.TripleAccuracy;
                    Vector2 pos = ThisCardPosition + new Vector2(5, 70);
                    FightResources.Font.NormalFont.CentreDraw($"Best Score:\n{scorePercent * 100:F2}%", pos, TargetColor * TextAlpha, 1, Depth + 0.0001f);
                    scorePercent /= 3f;
                    SkillMark mark = scorePercent >= 0.997f
                    ? SkillMark.Impeccable
                    : (scorePercent >= 0.99f)
                        ? SkillMark.Eminent
                        : (scorePercent >= 0.98f)
                            ? SkillMark.Excellent
                            : scorePercent >= 0.96f ? SkillMark.Respectable
                                    : scorePercent >= 0.9f ? SkillMark.Acceptable :
                                    scorePercent >= 0.75f ? SkillMark.Ordinary : SkillMark.Failed;
                    FightResources.Font.NormalFont.CentreDraw(mark.ToString(), pos + new Vector2(0, 50), mark switch
                    {
                        SkillMark.Failed => Color.DarkRed,
                        SkillMark.Ordinary => Color.Green,
                        SkillMark.Acceptable => Color.SpringGreen,
                        SkillMark.Respectable => Color.LightSkyBlue,
                        SkillMark.Excellent => Color.MediumPurple,
                        SkillMark.Eminent => Color.OrangeRed,
                        SkillMark.Impeccable => Color.Goldenrod,
                        _ => throw new ArgumentException($"{nameof(mark)} has something wrong", nameof(mark))
                    } * TextAlpha * CardAlpha[i], 1, Depth + 0.0001f);
                }
                else
                    FightResources.Font.NormalFont.CentreDraw("No Data", ThisCardPosition + new Vector2(5, 90), Color.Red * TextAlpha * CardAlpha[i], 1, Depth + 0.0001f);
                //Challenge Details
                if (TextAlpha < 1)
                {
                    for (int k = 0; k < ChallengeCharts[SelectedChallenge].Count; k++)
                    {
                        Color ChartNameCol = ChallengeCharts[SelectedChallenge][k].Difficulty switch
                        {
                            Difficulty.Noob => Color.White,
                            Difficulty.Easy => Color.LawnGreen,
                            Difficulty.Normal => Color.LightBlue,
                            Difficulty.Hard => Color.MediumPurple,
                            Difficulty.Extreme => Color.Orange,
                            _ => Color.Gray
                        };
                        string ChartName = $"* {ChallengeCharts[SelectedChallenge][k].Name}";
                        Vector2 ChartNamePos = new(CardPosition[SelectedChallenge].X - CardSize[SelectedChallenge].X / 2f + 10, 180 + k * 60);
                        float ChartNameSize = MathF.Min(1, (CardSize[SelectedChallenge].X - 20) / Font.NormalFont.SFX.MeasureString(ChartName).X);
                        string ComplexName = $"Complex Difficulty: {ChallengeCharts[SelectedChallenge][k].Complex}";
                        Vector2 ComplexPos = new(CardPosition[SelectedChallenge].X - CardSize[SelectedChallenge].X / 2f + 10, 210 + k * 60);
                        ChartNameSize = MathF.Min(ChartNameSize, (CardSize[SelectedChallenge].X - 20) / Font.NormalFont.SFX.MeasureString(ComplexName).X);
                        for (int j = 0; j < 8; j++)
                        {
                            Font.NormalFont.Draw(ChartName, ChartNamePos + MathUtil.GetVector2(2, j * 45), Color.Black * (1 - TextAlpha), ChartNameSize, 0.99999f);
                            Font.NormalFont.Draw(ComplexName, ComplexPos + MathUtil.GetVector2(2, j * 45), Color.Black * (1 - TextAlpha), 0.75f * ChartNameSize, 0.99999f);
                        }
                        Font.NormalFont.Draw(ChartName, ChartNamePos, ChartNameCol * (1 - TextAlpha), ChartNameSize, 1);
                        Font.NormalFont.Draw(ComplexName, ComplexPos, Color.White * (1 - TextAlpha), 0.75f * ChartNameSize, 1);
                        string ConfirmText = "Press Confirm to start";
                        Font.FightFont.CentreDraw(ConfirmText, new Vector2(320, 360), Color.LightGray * (1 - TextAlpha), MathF.Min(1, (CardSize[SelectedChallenge].X - 20) / Font.NormalFont.SFX.MeasureString(ConfirmText).X), 1);
                    }
                    //Background images
                    Vector2[] Vertices;
                    float displacement = (CardSize[SelectedChallenge].X - 10) / 4;
                    for (int j = 0; j < 3; j++)
                    {
                        if (Illustrations[SelectedChallenge][j] is not null)
                        {
                            Vertices =
                            [
                                new Vector2(CardPosition[SelectedChallenge].X - CardSize[SelectedChallenge].X / 2f + 5 + displacement * TopIndices[j], 95),
                                new Vector2(CardPosition[SelectedChallenge].X - CardSize[SelectedChallenge].X / 2f + 5 + displacement * TopIndices[j + 1], 95),
                                new Vector2(CardPosition[SelectedChallenge].X - CardSize[SelectedChallenge].X / 2f + 5 + displacement * BottomIndices[j + 1], 385),
                                new Vector2(CardPosition[SelectedChallenge].X - CardSize[SelectedChallenge].X / 2f + 5 + displacement * BottomIndices[j], 385),
                            ];
                            SpriteBatch.DrawVertex(Illustrations[SelectedChallenge][j], 0.85f,
                                new VertexPositionColorTexture(new Vector3(Vertices[0], 0.85f), Color.White * 0.5f * (1 - TextAlpha), new Vector2(TopIndices[j] / 4f, 0)),
                                new VertexPositionColorTexture(new Vector3(Vertices[1], 0.85f), Color.White * 0.5f * (1 - TextAlpha), new Vector2(TopIndices[j + 1] / 4f, 0)),
                                new VertexPositionColorTexture(new Vector3(Vertices[2], 0.85f), Color.White * 0.5f * (1 - TextAlpha), new Vector2(BottomIndices[j + 1] / 4f, 1)),
                                new VertexPositionColorTexture(new Vector3(Vertices[3], 0.85f), Color.White * 0.5f * (1 - TextAlpha), new Vector2(BottomIndices[j] / 4f, 1)));
                        }
                    }
                }
            }
            #endregion
            #region Side text
            Font.NormalFont.Draw("Strict", new Vector2(10, 340), Color.Red);
            Font.NormalFont.Draw("Mode", new Vector2(120, 340), Color.White);
            Font.NormalFont.Draw("No Restarts", new Vector2(10, 370), Color.Red);
            Font.NormalFont.Draw("No Modifiers", new Vector2(10, 400), Color.Red);
            #endregion
            #region Overlay UI
            Time++;
            Color OverlayColor = Color.Lerp(Color.Black, Color.Red, MathF.Abs(MathF.Sin(Time / 82f) * 0.3f));
            Color OverlayLineColor = Color.Lerp(Color.White, Color.Red, MathF.Abs(MathF.Sin(Time / 82f) * 0.3f));
            //Top Effect
            minY = 40 + MathF.Sin(Time / 140f) * 5;
            float Theta = 55 + MathF.Sin(Time / 70f) * 1;
            Vector2 OriginPoint = new Vector2(640, minY) + MathUtil.GetVector2(MathF.Sin(Time / 35f) * 5, Theta + 90);
            OriginPoint += MathUtil.GetVector2(45, Theta) + MathUtil.GetVector2(45, Theta - 120);
            Vector2[] Triangle = [OriginPoint + MathUtil.GetVector2(45, Theta - 120), OriginPoint, OriginPoint - MathUtil.GetVector2(45, Theta)];
            for (int i = 0; i < 20; i++)
            {
                DrawingLab.DrawTriangle(Triangle[0], Triangle[1], Triangle[2], OverlayColor, 0.9999f);
                Triangle[0] = Triangle[2];
                DrawingLab.DrawLine(OriginPoint, (OriginPoint -= MathUtil.GetVector2(45, Theta)) - MathUtil.GetVector2(0.3f, Theta), 3, OverlayLineColor, 1);
                DrawingLab.DrawLine(OriginPoint, (OriginPoint -= MathUtil.GetVector2(45, Theta - 120)) - MathUtil.GetVector2(0.3f, Theta - 120), 3, OverlayLineColor, 1);
                Triangle[1] = OriginPoint;
                Triangle[2] = OriginPoint - MathUtil.GetVector2(45, Theta);
                FormalDraw(FightResources.Sprites.pixUnit, Triangle[2], OverlayColor, new Vector2(200, 45), MathUtil.GetRadian(-90), Vector2.Zero);
            }
            //Bottom effect
            OriginPoint = new Vector2(0, 480 - minY) - MathUtil.GetVector2(MathF.Sin(Time / 35f) * 5, Theta + 90);
            OriginPoint -= MathUtil.GetVector2(45, Theta) + MathUtil.GetVector2(45, Theta - 120);
            Triangle = [OriginPoint - MathUtil.GetVector2(45, Theta - 120), OriginPoint, OriginPoint + MathUtil.GetVector2(45, Theta)];
            for (int i = 0; i < 20; i++)
            {
                DrawingLab.DrawTriangle(Triangle[0], Triangle[1], Triangle[2], OverlayColor, 0.9999f);
                Triangle[0] = Triangle[2];
                DrawingLab.DrawLine(OriginPoint, (OriginPoint += MathUtil.GetVector2(45, Theta)) - MathUtil.GetVector2(0.3f, Theta), 3, OverlayLineColor, 1);
                DrawingLab.DrawLine(OriginPoint, (OriginPoint += MathUtil.GetVector2(45, Theta - 120)) - MathUtil.GetVector2(0.3f, Theta - 120), 3, OverlayLineColor, 1);
                Triangle[1] = OriginPoint;
                Triangle[2] = OriginPoint + MathUtil.GetVector2(45, Theta);
                FormalDraw(FightResources.Sprites.pixUnit, Triangle[2], OverlayColor, new Vector2(200, 45), MathUtil.GetRadian(90), Vector2.Zero);
            }
            #endregion
            base.Draw();
        }
    }
}