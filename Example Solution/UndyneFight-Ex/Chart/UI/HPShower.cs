using UndyneFight_Ex.SongSystem;
using static System.MathF;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources.Sprites;
using static UndyneFight_Ex.GameMain;
using static UndyneFight_Ex.GlobalResources.Font;

namespace UndyneFight_Ex.Entities;

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
	/// The color of existing HP of the HP bar
	/// </summary>
	public Color HPExistColor { get => hpExistColor; set => hpExistColor = hpExistCurrent = value; }
	/// <summary>
	/// The color of Max HP of the HP bar
	/// </summary>
	public Color HPLoseColor { get => hpLoseColor; set => hpLoseColor = hpLoseCurrent = value; }
	/// <summary>
	/// The color of the KR bar
	/// </summary>
	public Color HPKRColor { get => hpKRColor; set => hpKRColor = hpKRCurrent = value; }
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
	/// <summary>
	/// The rectangle area of the HP bar
	/// </summary>
	public CollideRect CurrentArea => fullarea;
	public override void Draw()
	{
		float AlphaScale = CurrentDrawingSettings.UIColor.A / 255f;
		Vector2 hpPos = Vertical ? new Vector2(CollidingBox.GetCentre().X, FullRect.Down + 45) : new Vector2(CollidingBox.X - 30, CollidingBox.GetCentre().Y);
		GeneralDraw(Image, hpPos, CurrentDrawingSettings.UIColor, new Vector2(1.1f));
		if (HeartAttribute.KR && PlayerInstance.hpControl.KRHPExist)
			GeneralDraw(pixUnit, KRRect.GetCentre(), hpKRCurrent * AlphaScale, KRRect.Size, depth: 0.06f);
		GeneralDraw(pixUnit, collidingBox.GetCentre(), hpExistCurrent * AlphaScale, collidingBox.Size, depth: 0.05f);
		Depth = 0.0f;
		GeneralDraw(pixUnit, FullRect.GetCentre(), hpLoseCurrent * AlphaScale, FullRect.Size, depth: 0);

		string hpString;
		HeartAttribute.HP = float.Clamp(HeartAttribute.HP, 0, HeartAttribute.MaxHP);
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
				string hptext = string.Format("{0:N2}", HeartAttribute.HP * 20 / HeartAttribute.MaxHP).Replace(',', '.');
				if (hptext.Length == 1)
					hptext = "0" + hptext;
				hpString = hptext + " / 20.00";
			}
			if (Heart?.Shields?.Circle.Consumption > 1)
			{
				hpString += $"/ {MathUtil.FloatToString(Heart.Shields.Circle.Consumption * 8 - 8, 2)}";
			}
		}
		if (!Vertical)
		{
			if (HeartAttribute.KR)
				GeneralDraw(krText, new Vector2(FullRect.Right + 20, hpPos.Y), CurrentDrawingSettings.UIColor, new Vector2(1.1f));
			FightFont.Draw(hpString, new Vector2(FullRect.Right + (HeartAttribute.KR ? 45 : 20), collidingBox.Y + 4), Buffed ? Color.Gold * (CurrentDrawingSettings.UIColor.A / 255f) : CurrentDrawingSettings.UIColor);
		}
		else
		{
			if (HeartAttribute.KR)
				GeneralDraw(krText, new Vector2(hpPos.X, FullRect.Down + 20), CurrentDrawingSettings.UIColor, new Vector2(1.1f));
			if (((CurrentScene as FightScene).Mode & GameMode.Practice) != 0)
			{
				FightFont.CentreDraw(hpString, new Vector2(FullRect.GetCentre().X + 4, FullRect.GetCentre().Y + FullRect.Height / 2f + (HeartAttribute.KR ? 49 : 24)), CurrentDrawingSettings.UIColor, 1, 0, 0);
			}
			else
			{
				Vector2 pos = new(FullRect.GetCentre().X, FullRect.Down + 18);
				FightFont.CentreDraw(MathUtil.FloatToString(RoundHP), pos, CurrentDrawingSettings.UIColor, 1, 0, 0);
				pos = new(FullRect.GetCentre().X, FullRect.Up - 18);
				FightFont.CentreDraw(MathUtil.FloatToString(HeartAttribute.MaxHP), pos, CurrentDrawingSettings.UIColor, 1, 0, 0);
			}
		}
	}

	public override void Update()
	{
		//Calculate position
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

		float scale = 0, AlphaScale = CurrentDrawingSettings.UIColor.A / 255f;
		if (Buffed)
			scale = 1 - MathHelper.Clamp(1.25f - PlayerInstance.hpControl.LostSpeed * 0.5f, 0.1f, 1.0f);
		hpExistCurrent = Color.Lerp(hpExistColor, Color.Firebrick, scale) * AlphaScale;
		hpLoseCurrent = Color.Lerp(hpLoseColor, Color.Firebrick, scale) * AlphaScale;
		hpKRCurrent = Color.Lerp(hpKRColor, Color.Firebrick, scale) * AlphaScale;
	}
}