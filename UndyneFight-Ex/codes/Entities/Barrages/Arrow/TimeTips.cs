namespace UndyneFight_Ex.Entities
{
    public partial class Arrow
    {
        private class TimeTips : Entity
        {
            private readonly string text;
            private readonly vec2 pos, fadeSpeed;
            private readonly col color;
            public TimeTips(vec2 pos, col color, string text, vec2 fadeSpeed)
            {
                this.fadeSpeed = fadeSpeed;
                this.color = color;
                this.pos = pos;
                this.text = text;
                UpdateIn120 = true;
            }
            private float appearTime = 0, alpha;
            public override void Update()
            {
                appearTime += 0.5f;
                Centre = pos + MathF.Pow(appearTime / 2, 1.7f) * fadeSpeed;
                alpha = appearTime > 20f ? (40 - appearTime) * 0.05f : 1;
                if (alpha <= 0)
                    Dispose();
            }
            public override void Draw() => GlobalResources.Font.NormalFont.CentreDraw(text, pos, color * (alpha * Fight.Functions.ScreenDrawing.UIColor.A / 255f), 0.5f, 0.5f);
        }
    }
}