using static System.MathF;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources.Sprites;
using static UndyneFight_Ex.GameMain;
using static UndyneFight_Ex.GlobalResources.Font;

namespace UndyneFight_Ex.Entities
{
    internal class HPShower : Entity
    {
        private CollideRect KRRect;
        private CollideRect FullRect;
        /// <summary>
        /// Whether the HP bar is vertical
        /// </summary>
        public bool Vertical { set; private get; } = false;
        private static bool Buffed => ((CurrentScene as FightScene).Mode & GameMode.Buffed) != 0;
        /// <summary>
        /// The color of the HP bar of Existing HP
        /// </summary>
        public Color HPExistColor { get => hpExistColor; set => hpExistColor = hpExistCurrent = value; }
        /// <summary>
        /// The color of the HP bar of Max HP
        /// </summary>
        public Color HPLoseColor { get => hpExistColor; set => hpLoseColor = hpExistCurrent = value; }
        /// <summary>
        /// The color of the KR bar
        /// </summary>
        public Color HPKRColor { set => hpKRColor = hpKRCurrent = value; }
        private Color hpExistColor, hpExistCurrent;
        private Color hpLoseColor, hpLoseCurrent;
        private Color hpKRColor, hpKRCurrent;

        public static HPShower instance;

        public HPShower()
        {
            instance = this;
            Image = hpText;
            collidingBox = new(320, 455 - 12, 100, 24);
            KRRect.Height = 24;
            KRRect.Y = 458 - 12;

            HPKRColor = Color.Fuchsia;
            HPLoseColor = Color.Red;
            HPExistColor = Color.Lime;
        }

        private CollideRect fullarea = new(320, 443, 100, 24);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetArea(CollideRect rect) => fullarea = rect;
        public CollideRect CurrentArea => fullarea;
        public override void Draw()
        {
            Vector2 hpPos = Vertical ? new Vector2(CollidingBox.GetCentre().X, FullRect.Down + 45) : new Vector2(CollidingBox.X - 30, CollidingBox.GetCentre().Y);
            FormalDraw(Image, hpPos, CurrentDrawingSettings.UIColor, 1.1f, 0.0f, ImageCentre);
            if (HeartAttribute.KR && PlayerInstance.hpControl.KRHPExist)
            {
                Depth = 0.06f;
                FormalDraw(pixUnit, KRRect.ToRectangle(), hpKRCurrent);
            }
            Depth = 0.05f;
            FormalDraw(pixUnit, collidingBox.ToRectangle(), hpExistCurrent);
            Depth = 0.0f;
            FormalDraw(pixUnit, FullRect.ToRectangle(), hpLoseCurrent);

            string hpString;
            HeartAttribute.HP = MathUtil.Clamp(HeartAttribute.HP, 0, HeartAttribute.MaxHP);
            float RoundHP = Round(HeartAttribute.HP, 2);
            float CeilHP = Ceiling(HeartAttribute.HP);
            if (((CurrentScene as FightScene).Mode & GameMode.Practice) != 0)
                hpString = "inf";
            else
            {
                if (((CurrentScene as FightScene).Mode & GameMode.Buffed) == 0 && HeartAttribute.BuffedLevel == 0)
                    hpString = $"{CeilHP} / {Ceiling(HeartAttribute.MaxHP)}";
                else if (HeartAttribute.BuffedLevel != 0)
                {
                    hpString = $"{MathUtil.FloatToString(RoundHP, 2)} / {Ceiling(HeartAttribute.MaxHP)}";
                }
                else
                {
                    float hp = HeartAttribute.HP, max = HeartAttribute.MaxHP;
                    float scale = 20 / max;
                    string hptext = string.Format("{0:N2}", hp * scale);
                    if (hptext.Length == 1)
                        hptext += "0";
                    hpString = hptext + " / 20.00";
                }
                if (Heart.Shields?.Circle.Consumption > 1)
                {
                    hpString += $"/ {MathUtil.FloatToString(Heart.Shields.Circle.Consumption * 8 - 8, 2)}";
                }
            }
            if (!Vertical)
            {
                if (HeartAttribute.KR)
                    FormalDraw(krText, new Vector2(FullRect.Right + 20, hpPos.Y), CurrentDrawingSettings.UIColor, 1.1f, 0.0f, ImageCentre);
                FightFont.Draw(hpString, new Vector2(FullRect.Right + (HeartAttribute.KR ? 45 : 20), collidingBox.Y + 4), Buffed ? Color.Gold : CurrentDrawingSettings.UIColor);
            }
            else
            {
                if (HeartAttribute.KR)
                    FormalDraw(krText, new Vector2(hpPos.X, FullRect.Down + 20), CurrentDrawingSettings.UIColor, 1.1f, 0.0f, ImageCentre);
                if (((CurrentScene as FightScene).Mode & GameMode.Practice) != 0)
                {
                    FightFont.Draw(hpString, new Vector2(FullRect.Right + 1, collidingBox.Y + (HeartAttribute.KR ? 49 : 24)), CurrentDrawingSettings.UIColor, 1, 0, 0);
                }
                else
                {
                    Vector2 pos = new(FullRect.GetCentre().X, FullRect.Down + 18);
                    FightFont.CentreDraw(RoundHP.ToString(), pos, CurrentDrawingSettings.UIColor, 1, 0, 0);
                    pos = new(FullRect.GetCentre().X, FullRect.Up - 18);
                    FightFont.CentreDraw(HeartAttribute.MaxHP.ToString(), pos, CurrentDrawingSettings.UIColor, 1, 0, 0);
                }
            }
        }

        public override void Update()
        {
            CalculatePosition();

            float scale = 1;
            if (Buffed)
                scale = MathHelper.Clamp(1.25f - PlayerInstance.hpControl.LostSpeed * 0.5f, 0.1f, 1.0f);
            scale = 1 - scale;
            hpExistCurrent = Color.Lerp(hpExistColor, Color.Firebrick, scale);
            hpLoseCurrent = Color.Lerp(hpLoseColor, Color.Firebrick, scale);
            hpKRCurrent = Color.Lerp(hpKRColor, Color.Firebrick, scale);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculatePosition()
        {
            FullRect = fullarea;
            collidingBox = fullarea;

            if (Vertical)
            {
                collidingBox.Height = HeartAttribute.HP * fullarea.Height / HeartAttribute.MaxHP;
                collidingBox.Y += fullarea.Height - collidingBox.Height + 1;
            }
            else
            {
                collidingBox.Width = HeartAttribute.HP * fullarea.Width / HeartAttribute.MaxHP;
            }

            float KRSize = Min(PlayerInstance.hpControl.KRHP, HeartAttribute.HP) * 100.0f / HeartAttribute.MaxHP;
            if (!Vertical)
            {
                KRRect.X = Math.Max(collidingBox.X + 1, collidingBox.Right - KRSize);
                KRRect.Y = collidingBox.Y;
                KRRect.Width = collidingBox.Right - KRRect.X + 1;
                KRRect.Height = collidingBox.Height;
            }
            else
            {
                KRRect.Y = Math.Max(collidingBox.Y + 1, collidingBox.Down - KRSize);
                KRRect.X = collidingBox.X;
                KRRect.Height = collidingBox.Down - KRRect.Y + 1;
                KRRect.Width = collidingBox.Width;
            }
        }
    }
}