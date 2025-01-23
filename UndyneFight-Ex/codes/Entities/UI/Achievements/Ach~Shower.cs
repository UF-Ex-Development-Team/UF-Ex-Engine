using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.FightResources.Sounds;
using static UndyneFight_Ex.FightResources.Sprites;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex.Achievements
{
    /// <summary>
    /// v0.3.0+ Achievement UI
    /// </summary>
    public class AchievementUI : Entity
    {
        private readonly Tuple<Achievement, string, bool>[] Achievements = [];
        private int Selection = 0;
        private readonly Vector2[] TargetPosition, ActualPosition;
        private readonly Vector2[] TargetBoxPosition, ActualBoxPosition;
        private int KeyHolding = 0, State = 0;
        public AchievementUI()
        {
            CurrentScene.CurrentDrawingSettings.defaultWidth = 640;
            ResetRendering();
            UpdateIn120 = true;
            int AchCount = AchievementManager.achievements.Count;
            Achievements = new Tuple<Achievement, string, bool>[AchCount];
            ActualPosition = new Vector2[AchCount];
            TargetPosition = new Vector2[AchCount];
            TargetBoxPosition = new Vector2[AchCount];
            ActualBoxPosition = new Vector2[AchCount];
            int i = 0;
            foreach (var achievement in AchievementManager.achievements)
            {
                Achievements[i] = new(achievement.Value, achievement.Value.Title, achievement.Value.Achieved);
                TargetPosition[i] = ActualPosition[i] = new(320 - MathF.Abs(i * 60), 240 + i * 80);
                TargetBoxPosition[i] = ActualBoxPosition[i] = new(650, 240 + i * 80);
                ++i;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePositions()
        {
            for (int i = 0; i < TargetPosition.Length; ++i)
            {
                TargetPosition[i] = new(320 - MathF.Abs((i - Selection) * 60), 240 + (i - Selection) * 80);
                TargetBoxPosition[i].Y = 240 + (i - Selection) * 80;
            }
        }
        public override void Update()
        {
            //Exit
            if (State == 0 && IsKeyPressed120f(InputIdentity.Cancel))
            {
                Fight.Functions.PlaySound(select);
                if (IsReEngine)
                    CurrentScene.CurrentDrawingSettings.defaultWidth = 960;
                ResetRendering();
                ResetScene(new GameMenuScene());
            }
            //Check valid user
            if (PlayerManager.CurrentUser == null)
                return;
            //Lerp
            int i = 0;
            foreach (Vector2 TarPos in TargetPosition)
            {
                ActualPosition[i] = Vector2.Lerp(ActualPosition[i], TarPos, 0.06f);
                ActualBoxPosition[i] = Vector2.Lerp(ActualBoxPosition[i], TargetBoxPosition[i], 0.06f);
                ++i;
            }
            if (State == 0)
            {
                //Selection
                if (IsKeyDown(InputIdentity.MainDown) || IsKeyDown(InputIdentity.MainUp))
                    KeyHolding++;
                if (!IsKeyDown(InputIdentity.MainDown) && !IsKeyDown(InputIdentity.MainUp))
                    KeyHolding = 0;
                //Hold button
                if (IsKeyPressed120f(InputIdentity.MainDown) || (KeyHolding > 60 && KeyHolding % 15 == 0 && IsKeyDown(InputIdentity.MainDown)))
                {
                    Selection = MathUtil.Posmod(++Selection, Achievements.Length);
                    Fight.Functions.PlaySound(changeSelection);
                    UpdatePositions();
                }
                else if (IsKeyPressed120f(InputIdentity.MainUp) || (KeyHolding > 60 && KeyHolding % 15 == 0 && IsKeyDown(InputIdentity.MainUp)))
                {
                    Selection = MathUtil.Posmod(--Selection, Achievements.Length);
                    Fight.Functions.PlaySound(changeSelection);
                    UpdatePositions();
                }
                //Enter view
                if (IsKeyPressed120f(InputIdentity.Confirm))
                {
                    Fight.Functions.PlaySound(select);
                    State = 1;
                    for (i = 0; i < Achievements.Length; i++)
                        TargetPosition[i].X -= i != Selection ? 600 : 300;
                    TargetBoxPosition[Selection] = new(320, 240);
                }
            }
            else if (State == 1)
            {
                if (IsKeyPressed120f(InputIdentity.Cancel))
                {
                    Fight.Functions.PlaySound(select);
                    State = 0;
                    for (i = 0; i < Achievements.Length; i++)
                        TargetPosition[i].X += i != Selection ? 600 : 300;
                    TargetBoxPosition[Selection] = new(650, 240);
                }
            }
        }
        private const bool Roadmap = false;
        private readonly Color[] colors = [Color.Coral, Color.LightGoldenrodYellow, Color.Lime, Color.Blue];
        private static int colorTime = 0, colorIndex = 0;
        public override void Draw()
        {
            if (!Roadmap)
            {
                //Cover background
                colorTime++;
                DrawingLab.DrawLine(new(0, 240), new Vector2(640, 240), 480, Color.Black, 0);
                Color bgCol = Color.Lerp(colors[colorIndex], colors[(colorIndex + 1) % colors.Length], colorTime / 120f);
                DrawingLab.DrawLine(new(0, 240), new Vector2(640, 240), 480, bgCol * 0.7f, 0.01f);
                if (colorTime == 120)
                {
                    colorIndex++;
                    colorIndex %= colors.Length;
                    colorTime = 0;
                }
                //Background lines
                for (int k = -1; k < 15; k++)
                {
                    float interval = 640/15f;
                    DrawingLab.DrawLine(new Vector2(colorTime * interval / 120f + k * interval, 0), new Vector2(colorTime * interval / 120f + k * interval, 480), 2, bgCol, 0.09f);
                    DrawingLab.DrawLine(new Vector2(0, colorTime * interval / 120f + k * interval), new Vector2(640, colorTime * interval / 120f + k * interval), 2, bgCol, 0.09f);
                }
                if (PlayerManager.CurrentUser == null)
                {
                    FightResources.Font.NormalFont.CentreDraw("Login to view your achievements", new Vector2(320, 240), Color.Yellow, 1, 0.1f);
                    return;
                }
                GlobalResources.Font.NormalFont.Draw("Achievements", new Vector2(450, 20), Color.Yellow, 1, 0.1f);
                int i = 0;
                foreach (var achievement in AchievementManager.achievements)
                {
                    //White background
                    SpriteBatch.DrawVertex(pixUnit, 0.1f,
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(0, -30), 0), Color.White, Vector2.Zero),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(240, -30), 0), Color.White, Vector2.UnitX),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(270, 0), 0), Color.White, Vector2.One),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(240, 20), 0), Color.White, Vector2.One),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(0, 20), 0), Color.White, Vector2.UnitY)
                        );
                    //Black background
                    SpriteBatch.DrawVertex(pixUnit, 0.2f,
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, -28), 0), Color.Black, Vector2.Zero),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(239, -28), 0), Color.Black, Vector2.UnitX),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(267, 0), 0), Color.Black, Vector2.One),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, 0), 0), Color.Black, Vector2.UnitY)
                        );
                    //Progress bar
                    float progressPercent = MathUtil.Clamp(0, achievement.Value.CurrentProgress / achievement.Value.FullProgress, 1);
                    SpriteBatch.DrawVertex(pixUnit, 0.3f,
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, 18), 0), Color.Lime, Vector2.Zero),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2 + 237 * progressPercent, 18), 0), Color.Lime, Vector2.UnitX),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2 + 265 * progressPercent, 0), 0), Color.Lime, Vector2.One),
                        new VertexPositionColorTexture(new Vector3(ActualPosition[i] + new Vector2(2, 0), 0), Color.Lime, Vector2.UnitY)
                        );
                    FightResources.Font.NormalFont.Draw(achievement.Value.Title, ActualPosition[i] + new Vector2(10, -17), Color.White, 0.75f, 0.3f);
                    if (progressPercent == 1)
                    {
                        for (int k = 0; k < 8; k++)
                            FightResources.Font.NormalFont.CentreDraw("Completed", ActualPosition[i] + new Vector2(120, 9) + MathUtil.GetVector2(1, k * 45), Color.YellowGreen, 0.6f, 0.4f);
                        FightResources.Font.NormalFont.CentreDraw("Completed", ActualPosition[i] + new Vector2(120, 9), Color.Yellow, 0.6f, 0.5f);
                    }
                    //Info box
                    DrawingLab.DrawLine(ActualBoxPosition[i], ActualBoxPosition[i] + new Vector2(300, 0), 200, Color.White, 0.6f);
                    DrawingLab.DrawLine(ActualBoxPosition[i] + new Vector2(4, 0), ActualBoxPosition[i] + new Vector2(296, 0), 192, Color.Black, 0.7f);
                    FightResources.Font.NormalFont.CentreDraw("Requirements:", ActualBoxPosition[i] + new Vector2(150, -75), Color.White, 1, 0.8f);
                    FightResources.Font.NormalFont.LimitDraw(achievement.Value.AchievementIntroduction, ActualBoxPosition[i] + new Vector2(15, -42), Color.White, new Vector2(280, 150), 25, 1, 0.8f);
                    DrawingLab.DrawLine(ActualBoxPosition[i] + new Vector2(20, -55), ActualBoxPosition[i] + new Vector2(280, -55), 2, Color.Silver, 0.8f);
                    ++i;
                }
            }
            else
            {
                DrawingLab.DrawLineColors(new Vector2(320, 240), 180, 640, 480, [Color.MonoGameOrange, Color.Bisque, Color.Aquamarine, Color.HotPink], 0.1f);

                for (int i = 0; i < 8; i++)
                    FightResources.Font.FightFont.CentreDraw("Rhythm Recall 0.3.0 Update Roadmap", new Vector2(320, 20) + MathUtil.GetVector2(2, i * 45), Color.Gray, 1, 0.12f);
                FightResources.Font.FightFont.CentreDraw("Rhythm Recall 0.3.0 Update Roadmap", new Vector2(320, 20), Color.Yellow, 1, 0.2f);
                var percentage = 90;
                FightResources.Font.NormalFont.CentreDraw("2024 Dec:\nImplement suggestions and fix all known bugs", new Vector2(250, 120), Color.White, 0.7f, 0.2f);
                DrawingLab.DrawCircleSections(new Vector2(580, 120), 20, 256, 40, Color.Red, 0.12f, 0, 360);
                DrawingLab.DrawCircleSections(new Vector2(580, 120), 20, 256, 40, Color.Green, 0.2f, -90f, 360 * percentage/100f - 90);
                FightResources.Font.SansFont.CentreDraw($"{percentage}%", new Vector2(585, 120), Color.Yellow, 1, 0.21f);
                percentage = 100;
                FightResources.Font.NormalFont.CentreDraw("2025 Feb:\nImplement custom keybinds and chart loader", new Vector2(250, 220), Color.White, 0.7f, 0.2f);
                DrawingLab.DrawCircleSections(new Vector2(580, 220), 20, 256, 40, Color.Red, 0.12f, 0, 360);
                DrawingLab.DrawCircleSections(new Vector2(580, 220), 20, 256, 40, Color.Green, 0.2f, -90f, 360 * percentage / 100f - 90);
                FightResources.Font.SansFont.CentreDraw($"{percentage}%", new Vector2(585, 220), Color.Yellow, 1, 0.21f);
                percentage = 10;
                FightResources.Font.NormalFont.CentreDraw("2025 Apr:\nNew charts and engine documentation", new Vector2(250, 320), Color.White, 0.7f, 0.2f);
                DrawingLab.DrawCircleSections(new Vector2(580, 320), 20, 256, 40, Color.Red, 0.12f, 0, 360);
                DrawingLab.DrawCircleSections(new Vector2(580, 320), 20, 256, 40, Color.Green, 0.2f, -90f, 360 * percentage / 100f - 90);
                FightResources.Font.SansFont.CentreDraw($"{percentage}%", new Vector2(585, 320), Color.Yellow, 1, 0.21f);
                percentage = 60;
                FightResources.Font.NormalFont.CentreDraw("2025 May:\nPublic Release of 0.3.0", new Vector2(250, 420), Color.White, 0.7f, 0.2f);
                DrawingLab.DrawCircleSections(new Vector2(580, 420), 20, 256, 40, Color.Red, 0.12f, 0, 360);
                DrawingLab.DrawCircleSections(new Vector2(580, 420), 20, 256, 40, Color.Green, 0.2f, -90f, 360 * percentage / 100f - 90);
                FightResources.Font.SansFont.CentreDraw($"{percentage}%", new Vector2(585, 420), Color.Yellow, 1, 0.21f);
            }
        }
    }
}