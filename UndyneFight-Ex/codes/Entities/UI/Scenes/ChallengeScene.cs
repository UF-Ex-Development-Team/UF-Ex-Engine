using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
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
        readonly List<Challenge> ChallengeList = [];
        /// <summary>
        /// The position of the challenge card
        /// </summary>
        readonly List<Vector2> CardPosition = [], TargetCardPosition = [];
        /// <summary>
        /// The alpha of the card
        /// </summary>
        readonly List<float> CardAlpha = [];
        readonly List<Texture2D> CardIcons = [];
        /// <summary>
        /// [
        ///     [
        ///         Name, Difficulty, Complex
        ///     ]
        /// ]
        /// </summary>
        List<List<Tuple<string, Difficulty, float>>> ChallengeCharts = [];
        /// <summary>
        /// The size of a card
        /// </summary>
        readonly Vector2 CardSize = new(200, 300);
        int SelectedChallenge = 0;
        public override void Update()
        {
            //Return to previous scene
            if (IsKeyPressed120f(InputIdentity.Cancel))
            {
                if (IsReEngine)
                    CurrentScene.CurrentDrawingSettings.defaultWidth = 960;
                ResetRendering();
                ResetScene(previous_scene);
                exit_event();
            }
            if (FightSystem.Challenges.Count == 0)
                return;
            //Initalize list
            if (ChallengeList.Count == 0)
            {
                CurrentScene.CurrentDrawingSettings.defaultWidth = 640;
                ResetRendering();
                for (int i = 0; i < FightSystem.Challenges.Count; i++)
                    ChallengeCharts.Add([]);
                FightSystem.Challenges.ForEach((challenge) =>
                {
                    CardPosition.Add(new Vector2(320 + ChallengeList.Count * 30, 400));
                    TargetCardPosition.Add(new Vector2(320 + ChallengeList.Count * 30, 400));
                    CardAlpha.Add(ChallengeList.Count != 1 ? 0 : 1);
                    CardIcons.Add(LoadContent<Texture2D>(challenge.IconPath, Loader));
                    foreach (Tuple<Type, Difficulty> v in challenge.Routes)
                    {
                        IWaveSet wave = Activator.CreateInstance(v.Item1) as IWaveSet;
                        string DispName = wave.Attributes.DisplayName;
                        ChallengeCharts[ChallengeList.Count].Add(new(DispName == string.Empty ? wave.FightName : DispName, v.Item2, wave.Attributes.ComplexDifficulty.TryGetValue(v.Item2, out float ComplexVal) ? ComplexVal : 0));
                    }
                    ChallengeList.Add(challenge);
                });
                for (int i = 0; i < TargetCardPosition.Count; i++)
                {
                    TargetCardPosition[i] = new Vector2(320 + (i - SelectedChallenge) * 30, (i == SelectedChallenge ? 400 : 570) + MathF.Pow(Math.Abs(i - SelectedChallenge) * 0.6f, 2.5f));
                }
            }
            for (int i = 0; i < CardPosition.Count; i++)
            {
                CardPosition[i] = Vector2.LerpPrecise(CardPosition[i], TargetCardPosition[i], 0.1f);
                CardAlpha[i] = MathHelper.LerpPrecise(CardAlpha[i], i == SelectedChallenge ? 1 : 0.5f, 0.1f);
            }
            if (IsKeyPressed120f(InputIdentity.MainLeft) && SelectedChallenge > 0)
            {
                FightResources.Sounds.changeSelection.CreateInstance().Play();
                SelectedChallenge--;
                for (int i = 0; i < TargetCardPosition.Count; i++)
                {
                    TargetCardPosition[i] = new Vector2(320 + (i - SelectedChallenge) * 30, (i == SelectedChallenge ? 400 : 570) + MathF.Pow(Math.Abs(i - SelectedChallenge) * 0.6f, 2.5f));
                }
            }
            else if (IsKeyPressed120f(InputIdentity.MainRight) && SelectedChallenge < ChallengeList.Count - 1)
            {
                FightResources.Sounds.changeSelection.CreateInstance().Play();
                SelectedChallenge++;
                for (int i = 0; i < TargetCardPosition.Count; i++)
                {
                    TargetCardPosition[i] = new Vector2(320 + (i - SelectedChallenge) * 30, (i == SelectedChallenge ? 400 : 570) + MathF.Pow(Math.Abs(i - SelectedChallenge) * 0.6f, 2.5f));
                }
            }
            if (IsKeyPressed120f(InputIdentity.Confirm))
            {
                SongFightingScene.SceneParams[] ListParams = new SongFightingScene.SceneParams[ChallengeList[SelectedChallenge].Routes.Length];
                int i = 0;
                foreach (var ChallengeChart in ChallengeList[SelectedChallenge].Routes)
                {
                    var cur = Activator.CreateInstance(ChallengeChart.Item1) as IWaveSet;
                    string path = "Content\\Musics\\" + cur.Music;
                    if (!File.Exists(Path.Combine($"{AppContext.BaseDirectory}{path}.xnb".Split('\\'))))
                        path += "\\song";
                    ListParams[i] = new(cur, null, (int)ChallengeList[SelectedChallenge].Routes[i].Item2, path, JudgementState.Strict, GameMode.RestartDeny);
                    ++i;
                }
                IsInChallenge = true;
                ResetScene(new SongLoadingScene(ChallengeList[SelectedChallenge], ListParams));
                GameStates.Broadcast(new(null, "MusicFadeOut"));
            }
            base.Update();
        }
        public override void Draw()
        {
            if (FightSystem.Challenges.Count == 0)
                return;
            float scale;
            //Card drawing
            for (int i = ChallengeList.Count - 1; i >= 0; i--)
            {
                bool isSelected = i == SelectedChallenge;
                Color TargetColor = isSelected ? Color.White : Color.Lerp(Color.White, Color.Black, CardAlpha[i]);
                FormalDraw(FightResources.Sprites.pixUnit, CardPosition[i], TargetColor, CardSize, 0, CardSize / CardSize.Length() / 2);
                FormalDraw(FightResources.Sprites.pixUnit, CardPosition[i] + new Vector2(2.5f, 0), Color.Black, CardSize - new Vector2(10), 0, CardSize / CardSize.Length() / 2);
                string ChallengeName = ChallengeList[i].Title;
                scale = 0.7f;
                if (Font.NormalFont.SFX.MeasureString(ChallengeName).X * scale > CardSize.X - 30)
                    scale = (CardSize.X - 30) / Font.NormalFont.SFX.MeasureString(ChallengeName).X;
                Font.NormalFont.CentreDraw(ChallengeName, CardPosition[i] + new Vector2(50, -90), isSelected ? Color.White : Color.Lerp(Color.White, Color.Black, CardAlpha[i]), scale, Depth + (i <= SelectedChallenge ? 0.1f : 0));
                FormalDraw(CardIcons[i], CardPosition[i] - new Vector2(0, 50), TargetColor, 0, Vector2.Zero);
            }
            //Info text
            Challenge curChallenge = ChallengeList[SelectedChallenge];
            Font.NormalFont.Draw($"* Challenge - {curChallenge.Title}", new Vector2(10, 10), Color.White);
            scale = 1;
            string challengeDesc = $"* {curChallenge.Desc}";
            if (Font.NormalFont.SFX.MeasureString(challengeDesc).X * scale > 610)
                scale = 610 / Font.NormalFont.SFX.MeasureString(challengeDesc).X;
            Font.NormalFont.Draw(challengeDesc, new Vector2(10, 40), Color.White, scale, Depth);
            for (int i = 0; i < ChallengeCharts[SelectedChallenge].Count; i++)
            {
                Color ChartNameCol = ChallengeCharts[SelectedChallenge][i].Item2 switch
                {
                    Difficulty.Noob => Color.White,
                    Difficulty.Easy => Color.LawnGreen,
                    Difficulty.Normal => Color.LightBlue,
                    Difficulty.Hard => Color.MediumPurple,
                    Difficulty.Extreme => Color.Orange,
                    _ => Color.Gray
                };
                Font.NormalFont.Draw($"* {ChallengeCharts[SelectedChallenge][i].Item1}", new Vector2(10, 80 + i * 60), ChartNameCol);
                Font.NormalFont.Draw($"Complex Difficulty: {ChallengeCharts[SelectedChallenge][i].Item3}", new Vector2(10, 110 + i * 60), Color.White, 0.75f, Depth);
            }
            //Side text
            Font.NormalFont.Draw("Strict", new Vector2(10, 340), Color.Red);
            Font.NormalFont.Draw("Mode", new Vector2(120, 340), Color.White);
            Font.NormalFont.Draw("No Restarts", new Vector2(10, 370), Color.Red);
            Font.NormalFont.Draw("No Modifiers", new Vector2(10, 400), Color.Red);
            //Rating drawing (I hate this code)
            if (PlayerManager.CurrentUser != null)
            {
                Color[] SkillColors = [ Color.Lime, Color.LawnGreen, Color.Blue,
                Color.MediumPurple, Color.Red, Color.OrangeRed, Color.Orange, Color.Gold];
                var collidingBox = new CollideRect(new Vector2(2, 270), new(170, 58));
                float BoxMiddle = collidingBox.Height / 2;
                var name = PlayerManager.CurrentUser.PlayerName;
                var skill = PlayerManager.PlayerSkill;
                Color special = Color.Green;
                if (skill >= 20)
                    for (int i = 2; i < 9; i++)
                    {
                        if (skill >= i * 10)
                            special = SkillColors[i - 2];
                    }
                DrawingLab.DrawLine(new(collidingBox.Left, collidingBox.Y + BoxMiddle), new(collidingBox.Right, collidingBox.Y + BoxMiddle), BoxMiddle * 2, Color.Black * 0.5f, 0);
                DrawingLab.DrawRectangle(collidingBox, Color.White, 3, 0.4f);
                DrawingLab.DrawLine(new Vector2(0, 4) + collidingBox.TopLeft, new Vector2(0, 4) + collidingBox.TopRight,
                    1, special, 0.2f);
                DrawingLab.DrawLine(new Vector2(0, 2) + collidingBox.TopLeft, new Vector2(0, 2) + collidingBox.TopRight,
                    1, special, 0.2f);
                Font.NormalFont.Draw(name, new Vector2(5, 10) + collidingBox.TopLeft,
                    Color.White, 0.8f, 0.2f);
                Font.NormalFont.Draw(MathUtil.FloatToString(skill, 2), new Vector2(5, 38) + collidingBox.TopLeft,
                    Color.White, 0.8f, 0.2f);
                Texture2D starMedal = Sprites.starMedal;
                Vector2 ImgCen = new(starMedal.Width / 2, starMedal.Height / 2);
                if (skill >= 60)
                {
                    FormalDraw(skill > 90 ? starMedal : Image, new Vector2(-12, 30) + collidingBox.TopRight, Color.White, 0, ImgCen);
                    if (skill >= 70)
                    {
                        FormalDraw(skill > 92.5f ? starMedal : Image, new Vector2(-37, 30) + collidingBox.TopRight, Color.White, 0, ImgCen);
                        if (skill >= 80)
                            FormalDraw(skill > 95f ? starMedal : Image, new Vector2(-62, 30) + collidingBox.TopRight, Color.White, 0, ImgCen);
                    }
                }
            }
            base.Draw();
        }
    }
}