using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Entities.SimplifiedEasing;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.GlobalResources.Font;

namespace UndyneFight_Ex.Entities
{
    internal class ChallengeResult(Challenge challenge) : Entity
    {
        Texture2D[] ChartIllustrations;
        private class SingleResult : Entity
        {
            SongResult result;
            /// <summary>
            /// The illustration of the single chart
            /// </summary>
            readonly Texture2D illustration;
            /// <summary>
            /// 0 -> Normal, 1 -> No Hit, 2 -> AP
            /// </summary>
            public int ChartSpecial = 0;
            public SingleResult(SongResult result, int index, Texture2D illustration, int ExtraAttributes = 0)
            {
                ChartSpecial = ExtraAttributes;
                this.illustration = illustration;
                this.index = index;
                collidingBox = new CollideRect(Vector2.Zero, new(575, 91));
                this.result = result;
                bool direction = index % 2 == 0;
                Centre = new Vector2(direction ? -320 : 960, index * 100 + 113);    
                RunEase((s) => Centre = s, EaseOut(60, new Vector2(direction ? -320 : 960, index * 100 + 113), new Vector2(343 - index * 22.5f, index * 100 + 113), EaseState.Quint));
            }
            readonly int index;
            public override void Start()
            {
                Tuple<Type, Difficulty> tuple = (FatherObject as ChallengeResult).completedChallenge.Routes[index];
                IWaveSet curChart = Activator.CreateInstance(tuple.Item1) as IWaveSet;
                string curDispName = curChart.Attributes.DisplayName;
                songName = curDispName == string.Empty ? curChart.FightName : curDispName ;
                difficulty = tuple.Item2;
                difColor = (int)difficulty switch
                {
                    0 => Color.White,
                    1 => Color.LawnGreen,
                    2 => Color.LightBlue,
                    3 => Color.MediumPurple,
                    4 => Color.Orange,
                    _ => Color.Gray
                };
                remarkColor = (mark = result.CurrentMark) switch
                {
                    SkillMark.Failed => Color.DarkRed,
                    SkillMark.Ordinary => Color.Green,
                    SkillMark.Acceptable => Color.SpringGreen,
                    SkillMark.Respectable => Color.LightSkyBlue,
                    SkillMark.Excellent => Color.MediumPurple,
                    SkillMark.Eminent => Color.OrangeRed,
                    SkillMark.Impeccable => Color.Goldenrod,
                    _ => throw new ArgumentException($"{nameof(mark)} has something wrong", nameof(mark))
                };
            }
            SkillMark mark;
            Difficulty difficulty;
            Color difColor, remarkColor;
            string songName;
            /// <summary>
            /// The size of the screen
            /// </summary>
            static readonly Vector2 ScreenSize = new(640, 480);
            /// <summary>
            /// The color of the background illustration
            /// </summary>
            static readonly Color IllustrationColor = Color.Lerp(Color.Black, Color.White, 0.5f);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DrawOutlinedText(string text, Vector2 pos, float thickness, Color outline_col, Color main_col, float scale = 1)
            {
                for (int i = 0; i < 8; i++)
                    NormalFont.Draw(text, pos + MathUtil.GetVector2(thickness, i * 45), outline_col, scale, Depth);
                NormalFont.Draw(text, pos, main_col, scale, Depth + 0.01f);
            }
            public override void Draw()
            {
                CollideRect RectBox = CollidingBox;
                Vector2[] Vertices =
                [
                    RectBox.TopLeft + new Vector2(10, 0),
                    RectBox.TopRight + new Vector2(10, 0),
                    RectBox.BottomRight - new Vector2(10, 0),
                    RectBox.BottomLeft - new Vector2(10, 0)
                ];
                SpriteBatch.DrawVertex(FightResources.Sprites.pixUnit, Depth - 0.2f,
                    new VertexPositionColorTexture(new Vector3(Vertices[0], Depth), Color.White, Vector2.Zero),
                    new VertexPositionColorTexture(new Vector3(Vertices[1], Depth), difColor, Vector2.UnitX),
                    new VertexPositionColorTexture(new Vector3(Vertices[2], Depth), remarkColor, Vector2.One),
                    new VertexPositionColorTexture(new Vector3(Vertices[3], Depth), difColor, Vector2.UnitY));
                RectBox.Width -= 5;
                RectBox.Height -= 5;
                RectBox.Offset(new Vector2(2.5f));
                Vertices =
                [
                    RectBox.TopLeft + new Vector2(10, 0),
                    RectBox.TopRight + new Vector2(10, 0),
                    RectBox.BottomRight - new Vector2(10, 0),
                    RectBox.BottomLeft - new Vector2(10, 0)
                ];
                SpriteBatch.DrawVertex(illustration, Depth - 0.15f,
                    new VertexPositionColorTexture(new Vector3(Vertices[0], Depth), IllustrationColor, Vertices[0] / ScreenSize),
                    new VertexPositionColorTexture(new Vector3(Vertices[1], Depth), IllustrationColor, Vertices[1] / ScreenSize),
                    new VertexPositionColorTexture(new Vector3(Vertices[2], Depth), IllustrationColor, Vertices[2] / ScreenSize),
                    new VertexPositionColorTexture(new Vector3(Vertices[3], Depth), IllustrationColor, Vertices[3] / ScreenSize));

                DrawOutlinedText(songName, CollidingBox.TopLeft + new Vector2(18, 9), 2, Color.Black, Color.White);
                string diffStr = difficulty switch
                {
                    _ when difficulty != Difficulty.ExtremePlus => difficulty.ToString(),
                    _ => "Extreme Plus"
                };
                DrawOutlinedText(diffStr, CollidingBox.TopRight - new Vector2(15 + NormalFont.SFX.MeasureString(difficulty.ToString()).X, -5), 2, Color.Black, difColor);
                DrawOutlinedText(MathUtil.FloatToString(MathUtil.Clamp(result.Accuracy * 100f, 0, 105), 2) + "%",
                    CollidingBox.TopLeft + new Vector2(18, 48), 2, Color.Black, Color.Wheat);
                if (ChartSpecial == 1)
                    DrawOutlinedText("No Hit", CollidingBox.TopLeft + new Vector2(430, 30), 2, Color.Black, Color.Orange, 0.8f);
                else if (ChartSpecial == 2)
                    DrawOutlinedText("All Perfect", CollidingBox.TopLeft + new Vector2(410, 30), 2, Color.Black, Color.Gold, 0.8f);
                DrawOutlinedText(result.Score.ToString(), CollidingBox.TopLeft + new Vector2(158, 48), 2, Color.Black, Color.White);
                DrawOutlinedText(mark.ToString(), CollidingBox.TopLeft + new Vector2(410, 60) - NormalFont.SFX.MeasureString(mark.ToString()) / 2, 2, Color.Black, remarkColor, 1.32f);
            }

            public override void Update() { }
        }
        readonly Challenge completedChallenge = challenge;

        public override void Start() => CreateResultUI(completedChallenge);

        float totalAccuracy = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateResultUI(Challenge challenge)
        {
            ChartIllustrations = new Texture2D[challenge.ResultBuffer.Count];
            List<SongResult>.Enumerator enumerator = challenge.ResultBuffer.GetEnumerator();
            curShowAccuracy = 0;
            totalAccuracy = 0;
            totalX = -120;
            appearTime = 0;
            for (int i = 0; i < 3; i++)
            {
                float delay = i * 12;
                int t = i;
                InstanceCreate(new InstantEvent(delay, () =>
                {
                    _ = enumerator.MoveNext();
                    int FCorAP = 0;
                    if (enumerator.Current.AC) FCorAP = 1;
                    if (enumerator.Current.AP) FCorAP = 2;
                    AddChild(new SingleResult(enumerator.Current, t, ChartIllustrations[t++], FCorAP));
                    totalAccuracy += MathUtil.Clamp(enumerator.Current.Accuracy, 0, 105);
                }));
                IWaveSet cur = Activator.CreateInstance(challenge.Routes[i].Item1) as IWaveSet;
                if (File.Exists(Path.Combine($"{AppContext.BaseDirectory}Content\\Musics\\{cur.Music}\\paint.xnb".Split('\\'))))
                {
                    string curRoot = Scene.Loader.RootDirectory;
                    Scene.Loader.RootDirectory = "";
                    ChartIllustrations[i] = GlobalResources.LoadContent<Texture2D>($"Content\\Musics\\{cur.Music}\\paint");
                    Scene.Loader.RootDirectory = curRoot;
                }
            }
            alpha = 0;
        }
        string result;
        Color resultColor;
        float curShowAccuracy;
        float totalX = -120;
        public override void Draw()
        {
            float acc = totalAccuracy * 100;
            result = acc switch
            {
                _ when acc >= 300 => "Impeccable",
                _ when acc >= 297 => "Eminent",
                _ when acc >= 294 => "Excellent",
                _ when acc >= 291 => "Respectable+",
                _ when acc >= 288 => "Respectable",
                _ when acc >= 279 => "Acceptable+",
                _ when acc >= 270 => "Acceptable",
                _ => "Unaccepted"
            };
            resultColor = acc switch
            {
                _ when acc >= 300 => Color.Goldenrod,
                _ when acc >= 297 => Color.OrangeRed,
                _ when acc >= 294 => Color.MediumPurple,
                _ when acc >= 288 => Color.LightSkyBlue,
                _ when acc >= 270 => Color.SpringGreen,
                _ => Color.DarkRed
            };
            FightResources.Font.NormalFont.CentreDraw("Challenge Result", new(320, 31), Color.White);
            FightResources.Font.NormalFont.CentreDraw("Total:" + MathUtil.FloatToString(curShowAccuracy * 100, 1) + "%",
                new Vector2(totalX, 400), Color.Wheat);
            if (calcFinished)
            {
                if (totalAccuracy >= 300)
                    for (float i = 0; i < 5; i += 0.5f)
                        for (int k = 0; k < 16; k++)
                            FightResources.Font.NormalFont.CentreDraw(result, new Vector2(475, 398) + MathUtil.GetVector2(i * 1.5f, k * 22.5f), Color.Lerp(Color.Black, resultColor, alpha - i * 0.2f), 1.5f, 0, 0.5f - i * 0.1f);
                FightResources.Font.NormalFont.CentreDraw(result, new Vector2(475, 398), Color.Lerp(Color.Black, resultColor, alpha), 1.5f, 0, 0.5f);
                if (appearTime % 60 < 30)
                    FightResources.Font.NormalFont.CentreDraw("Press Z to return", new Vector2(320, 449),
                        Color.Lime);
            }
        }
        bool calcFinished = false;
        float alpha = 0;
        int appearTime = 0;
        public override void Update()
        {
            if (IsKeyPressed(InputIdentity.Confirm))
                ResetScene(new GameMenuScene());
            curShowAccuracy = MathHelper.Lerp(curShowAccuracy, totalAccuracy + 0.00002f, 0.07f);
            calcFinished = MathF.Abs(curShowAccuracy - totalAccuracy) < 0.00003f;
            if (calcFinished)
            {
                totalX = MathHelper.Lerp(totalX, 140, 0.12f);
                appearTime++;
                if (alpha < 1)
                    alpha += 0.05f;
            }
            else
                totalX = MathHelper.Lerp(totalX, 320, 0.12f);
        }
    }
}