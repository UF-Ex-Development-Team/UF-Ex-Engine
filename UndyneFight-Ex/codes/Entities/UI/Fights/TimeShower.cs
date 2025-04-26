using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;
namespace UndyneFight_Ex.Entities
{
    internal class TimeShower : Entity
    {
        public TimeShower() => UpdateIn120 = true;
        public override void Draw()
        {
            Color col = GameMain.CurrentDrawingSettings.UIColor;
            if (del)
            {
                col.R = (byte)Math.Max(col.R - 2, 0);
                col.G = (byte)Math.Max(col.G - 2, 0);
                col.B = (byte)Math.Max(col.B - 2, 0);
            }
            int d = (int)((GametimeF - GametimeDelta) / 62.5f * 60f);
            if (d < 0)
                d = 0;
            int min = d / 3600, sec = d / 60 % 60;
            float ms = MathF.Round(d % 60 * 100/60f);
            FightResources.Font.NormalFont.CentreDraw($"{min}:{(sec < 10 ? "0" : "") + sec}:{(ms < 10 ? "0" : "") + ms}",
                new Vector2(94, 30), col);
            FightResources.Font.FightFont.Draw("(Beta 10)", new Vector2(585, 0), Color.Gray * 0.5f, 0.5f, 1);
#if DEBUG
            if ((CurrentScene as SongFightingScene).waveset is not null and WaveConstructor)
            {
                WaveConstructor waveset = (CurrentScene as SongFightingScene).waveset as WaveConstructor;
                if (!waveset._isMultiBPM)
                    FightResources.Font.NormalFont.CentreDraw($"Beat: {FloatToString(GametimeF / waveset.SingleBeat, 1)}", new Vector2(94, 50), col, 0.7f, 0.3f);
                else
                {
                    while (waveset.BeatTime(curBeat) < GametimeF)
                        curBeat += 0.1f;
                    FightResources.Font.NormalFont.CentreDraw($"Beat: {FloatToString(curBeat, 1)}", new Vector2(94, 50), col, 0.7f, 0.3f);
                }
            }
#endif
        }
        private int appearTime = 0;
        private bool del = false;
        private float curBeat = 0;
        public override void Update() => del = (++appearTime % 62 == 0) ? RandBool() : del;
    }
}