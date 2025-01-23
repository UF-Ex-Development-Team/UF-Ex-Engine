using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources.Sounds;
using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.GlobalResources.Font;

namespace UndyneFight_Ex.Entities
{
    public partial class StateShower
    {
        internal class FailureShower : Entity
        {
            private const int previousDataCounts = 10;
            private static float[] previousTimeSurvive = new float[previousDataCounts];
            private static bool changedSong = false;
            private static int tryCount = 0;
            private static int halvedScore;

            private static bool retryAvailable = false;
            private readonly int curMode = (int)(CurrentScene as SongFightingScene).Mode;
            private static bool recordMark = true;

            public FailureShower(StateShower result)
            {
                if ((curMode & (int)GameMode.NoGreenSoul) != 0 && (CurrentScene as SongFightingScene).GreenSoulUsed)
                    recordMark = false;
                if ((curMode & (int)GameMode.Practice) != 0 && (CurrentScene as SongFightingScene).HPReached0)
                    recordMark = false;
                if ((curMode & (int)GameMode.Autoplay) != 0)
                    recordMark = false;
                UpdateIn120 = true;
                int halvedScore = result.score / 2;
                int timeSurvive = result.surviveTime;

                FailureShower.halvedScore = halvedScore;
                //判定是否比赛超时
                retryAvailable = FightSystem.CurrentChampionShip != null && FightSystem.CurrentSongs != FightSystem.MainGameSongs
                    ? FightSystem.CurrentChampionShip.CheckTime() != ChampionShips.ChampionShip.ChampionShipStates.NotAvailable
                    : (result.mode & GameMode.RestartDeny) == 0;
                //Modifier check
                if (recordMark)
                    PlayerManager.RecordMark(result.wave.FightName, difficulty, SkillMark.Failed, instance.score / 2, false, false, 0);
                if (changedSong)
                {
                    tryCount = 0;
                    changedSong = false;
                    previousTimeSurvive = new float[previousDataCounts];
                }
                else
                    tryCount++;
                for (int i = 0; i < previousDataCounts - 1; i++)
                {
                    previousTimeSurvive[i + 1] = previousTimeSurvive[i];
                }

                previousTimeSurvive[0] = timeSurvive;

                AddChild(retrySelector = new RetrySelector(result));
            }

            public static Selector retrySelector = null;

            private class RetrySelector : Selector
            {
                private float alpha = 0, blurIntensity = 0, detailY = 485;
                private int appearTime = 0;
                private bool recordSaved = false, displayDetail = false;
                private readonly string DiffText;
                private Color DiffCol;

                public RetrySelector(StateShower s) : base(false)
                {
                    NameShower.nameAlpha = 1;
                    SelectChanger += () =>
                    {
                        if (IsKeyPressed120f(InputIdentity.MainUp) || IsKeyPressed120f(InputIdentity.MainDown))
                            currentSelect ^= 1;
                        currentSelect = MathUtil.Posmod(currentSelect, SelectionCount);
                    };
                    SelectChanged += () => changeSelection.CreateInstance().Play();

                    if (retryAvailable)
                        PushSelection(new ReTry(s.wave));

                    PushSelection(new GiveUp(s.mode));
                    DiffText = difficulty switch
                    {
                        0 => "Noob Mode",
                        1 => "Easy Mode",
                        2 => "Normal Mode",
                        3 => "Hard Mode",
                        4 => "Extreme Mode",
                        5 => "Extreme Plus",
                        _ => "?"
                    };
                    DiffCol = difficulty switch
                    {
                        0 => Color.White,
                        1 => Color.LawnGreen,
                        2 => Color.LightBlue,
                        3 => Color.MediumPurple,
                        4 => Color.Orange,
                        _ => Color.Gray
                    };
                }

                public override void Update()
                {
                    displayDetail ^= IsKeyPressed120f(InputIdentity.Alternate);
                    if (alpha < 1)
                        alpha += 0.025f;
                    blurIntensity = MathHelper.Lerp(blurIntensity, 2, 0.06f);
                    detailY = MathHelper.Lerp(detailY, displayDetail ? 340 : 485, 0.12f);

                    base.Update();

                    if (FightSystem.CurrentSongs != FightSystem.MainGameSongs)
                        return;

                    if (IsKeyPressed120f(InputIdentity.Special) && (!recordSaved) && GameInterface.UFEXSettings.RecordEnabled)
                    {
                        recordSaved = true;
                        PlaySound(heal);
                        Recorder.Save();
                    }
                    appearTime++;
                }

                public override void Draw()
                {
                    //Background drawing
                    Depth -= 0.01f;
                    CollideRect Normal = new(0, 0, 640, 480);
                    FormalDraw(GameoverBackground, Normal, Color.White * (blurIntensity / 100f) * 0.2f);
                    Depth += 0.01f;
                    //Draw name
                    if (!IsInChallenge)
                    {
                        string SongDisplayName = lastParam.Waveset.Attributes.DisplayName;
                        string ChartDiff = lastParam.Waveset.Attributes.ComplexDifficulty.ContainsKey((Difficulty)difficulty) ? lastParam.Waveset.Attributes.ComplexDifficulty[(Difficulty)difficulty].ToString() : "?";
                        NormalFont.CentreDraw($"{((SongDisplayName == string.Empty) ? lastParam.Waveset.FightName : SongDisplayName)}", new Vector2(320, 30), Color.White, 1, 0.1f);
                        NormalFont.CentreDraw(DiffText + " " + ChartDiff, new Vector2(320, 60), DiffCol, 1, 0.1f);
                    }
                    //You lose
                    NormalFont.CentreDraw(tryCount == 1 ? "You lose" : "You lose again", new Vector2(320, 105), Color.Lerp(Color.Black, Color.White, alpha), 1, 0.1f);
                    //Time and score
                    NormalFont.CentreDraw("Time survived : " + MathF.Round((previousTimeSurvive[0] - 2)/ 62.5f, 2) + "s", new Vector2(320, 145), Color.Lerp(Color.Black, Color.White, alpha), 0.92f, 0.1f);
                    NormalFont.CentreDraw(recordMark ? "Halved score : " + halvedScore : "Modifiers used\nNo mark recorded", new Vector2(320, recordMark ? 180 : 195), Color.Lerp(Color.Black, Color.White, alpha), 0.92f, 0.1f);
                    //Space hint
                    NormalFont.CentreDraw("Press Spacebar for more details", new Vector2(320, 860 - detailY), Color.Lerp(Color.Black, Color.GreenYellow, alpha), 0.92f, 0.08f);
                    //Detailed
                    DrawingLab.DrawLine(new Vector2(0, detailY), new Vector2(640, detailY), 3, Color.White, 0.1f);
                    DrawingLab.DrawLine(new Vector2(0, detailY + (480 - detailY) / 2), new Vector2(640, detailY + (480 - detailY) / 2), 480 - detailY, Color.Black, 0.09f);
                    if (instance == null)
                        return;
                    NormalFont.Draw($"Max Combo: {instance.maxCombo}", new Vector2(40, detailY + 10), Color.White, 1, 0.1f);
                    NormalFont.Draw($"Modifiers Used: {!recordMark}", new Vector2(310, detailY + 10), Color.White, 1, 0.1f);
                    NormalFont.Draw("Miss", new Vector2(40, detailY + 40), Color.Red, 1, 0.1f);
                    NormalFont.Draw(instance.miss.ToString(), new Vector2(40, detailY + 70), Color.White, 1, 0.1f);
                    NormalFont.Draw("Okay", new Vector2(190, detailY + 40), Color.Green, 1, 0.1f);
                    NormalFont.Draw(instance.okay.ToString(), new Vector2(190, detailY + 70), Color.White, 1, 0.1f);
                    NormalFont.Draw("Nice", new Vector2(330, detailY + 40), Color.LightBlue, 1, 0.1f);
                    NormalFont.Draw(instance.nice.ToString(), new Vector2(330, detailY + 70), Color.White, 1, 0.1f);
                    NormalFont.Draw("Perfect", new Vector2(480, detailY + 40), Color.Gold, 1, 0.1f);
                    NormalFont.Draw(instance.perfect.ToString(), new Vector2(480, detailY + 70), Color.White, 1, 0.1f);
                    NormalFont.Draw($"(E:{instance.perfectE}, L:{instance.perfectL})", new Vector2(480, detailY + 100), Color.Orange, 0.75f, 0.1f);

                    base.Draw();

                    if (FightSystem.CurrentSongs != FightSystem.MainGameSongs || !GameInterface.UFEXSettings.RecordEnabled)
                        return;

                    if (!recordSaved)
                    {
                        if (appearTime % 120 <= 60 || appearTime > 360)
                            NormalFont.CentreDraw("Press C to save record", new Vector2(320, 415), Color.Gold * alpha, 0.84f, 0.1f);
                    }
                    else
                        NormalFont.CentreDraw("Record saved", new Vector2(320, 415), Color.LawnGreen * alpha, 0.84f, 0.1f);
                }
            }

            private class ReTry : TextSelection
            {
                readonly IWaveSet wave;
                public ReTry(IWaveSet wave) : base("Try again", new Vector2(320, 250)) { Size = 1.0f; this.wave = wave; }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public override void SelectionEvent()
                {
                    waveSet = wave;
                    StartSong();
                    base.SelectionEvent();
                }
            }

            private class GiveUp : TextSelection
            {
                private readonly GameMode mode;
                public GiveUp(GameMode againMode) : base("Quit", new Vector2(320, 300)) { mode = againMode; Size = 1.0f; }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public override void SelectionEvent()
                {
                    recordMark = true;
                    tryCount = 0;
                    DisposeInstance();
                    changedSong = true;
                    IsInChallenge = false;
                    ResetScene(new GameMenuScene());
                    GameMain.ResetRendering();

                    base.SelectionEvent();
                }
            }

            public override void Draw() { }

            public override void Update()
            {
                if (retrySelector.Disposed)
                {
                    Dispose();
                    return;
                }
            }
        }
    }
}