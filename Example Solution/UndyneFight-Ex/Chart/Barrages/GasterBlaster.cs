using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.SongSystem;
using static UndyneFight_Ex.Fight.AdvanceFunctions;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.FightResources;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

/// <summary>
/// The base class for a Gaster Blaster, you should not call this
/// </summary>
public abstract class GasterBlaster : Barrage
{
	/// <summary>
	/// The volume of the blaster spawning (Default 0.85f)
	/// </summary>
	public float AppearVolume { get; set; } = 0.85f;
	/// <summary>
	/// The volume of the blaster shooting (Default 0.8f)
	/// </summary>
	public float ShootVolume { get; set; } = 0.8f;
	/// <summary>
	/// Whether will the blaster have no sound played
	/// </summary>
	public static bool IsGBMute { set => spawnSoundPlayed = value; }
	/// <summary>
	/// Whether the blaster shakes the screen when fired
	/// </summary>
	public bool IsShake { get; set; } = false;
	/// <summary>
	/// Whether the color of the blaster is the theme color
	/// </summary>
	public bool ColorIsTheme { get; set; } = false;
	/// <summary>
	/// Overrides the default rotating behavior
	/// </summary>
	public bool OverrideRotation { get; set; } = false;

	private protected float depth_ = 0.6f;
	private protected static int _blasterCount = 0;
	private protected Color drawingColor = Color.White;
	private protected static CollideRect screen = new CollideRect(-250, -250, 890, 730) * ScreenDrawing.ScreenScale;
	internal static bool spawnSoundPlayed = false, shootSoundPlayed = false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float GetDelta() => OverrideRotation ? 0 : Math.Min((missionRotation - Rotation + 360) % 360, (360 - missionRotation + Rotation) % 360);

	private protected float missionRotation, waitingTime, appearTime = 0, recoilSpeed = 0, laserAffectTime = 1, duration;
	private protected Vector2 missionPlace, size, laserPlace, laserSize;
	private protected bool rotateWay, laserIncreasing = true;

	private protected float alpha = 0, beamAlpha = 1f, movingScale = 0.9f;
	/// <inheritdoc/>
	public override void Draw()
	{
		Depth = depth_;
		FormalDraw(Image, Centre, drawingColor * alpha, size, GetRadian(this is NormalGB ? Rotation : missionRotation), ImageCentre);
		//Early exit in cases beam should not be drawn
		if (appearTime < waitingTime || laserSize.Y <= 0 || this is GreenSoulGB)
			return;
		Depth -= 0.001f;
		for (int i = 0; i < 4; i++)
			DrawingLab.DrawLine(laserPlace + GetVector2(14 * i, Rotation), laserPlace + GetVector2(14 * i + 12, Rotation), 14 * i * laserSize.Y * size.Y * 1.2f, drawingColor * beamAlpha, Depth);
		DrawingLab.DrawLine(laserPlace + GetVector2(56, Rotation), laserPlace + GetVector2(1000 + laserSize.X, Rotation), 56 * laserSize.Y * size.Y * 1.2f, drawingColor * beamAlpha, Depth);
	}
	/// <inheritdoc/>
	public override void Update()
	{
		Image ??= Sprites.GBStart[0];
		if (ColorIsTheme)
			drawingColor = ScreenDrawing.ThemeColor;
		appearTime++;
		if (this is NormalGB)
		{
			laserPlace = GetVector2(27 * size.Y, Rotation) + Centre;
			laserSize.X += recoilSpeed;
		}
		if ((int)(appearTime - waitingTime) == -12)
			Image = Sprites.GBStart[1];
		else if ((int)(appearTime - waitingTime) == -9)
			Image = Sprites.GBStart[2];
		else if ((int)(appearTime - waitingTime) == -6)
			Image = Sprites.GBStart[3];
		else if ((int)(appearTime - waitingTime) == -3)
		{
			Image = Sprites.GBStart[4];
			if (IsShake)
				GameStates.InstanceCreate(new Advanced.ScreenShaker(3, 9 * MathF.Min(size.X, size.Y), 3));
		}
		else if ((int)(appearTime - waitingTime) == -1 && !shootSoundPlayed)
		{
			shootSoundPlayed = true;
			PlaySound(Sounds.GBShoot, ShootVolume);
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected void MoveToMission()
	{
		if (alpha <= 1f)
			alpha += 0.06f * (1 / movingScale);
		if (appearTime < waitingTime)
			Centre = Centre * movingScale + missionPlace * (1 - movingScale);
		Rotation += GetDelta() * (0.98f - movingScale) * (rotateWay ? 1 : -1);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected void PushDown()
	{
		Centre -= GetVector2(recoilSpeed += 0.4f, Rotation);
		Image = Sprites.GBShooting[(int)Convert.ToSingle(appearTime % 6 <= 3)];
		beamAlpha = beamAlpha * 0.8f + 0.2f;
		if (!laserIncreasing)
			laserSize.Y = 0.9f + Sin(laserAffectTime++ * 15) * 0.18f;
		else if ((laserSize.Y = laserSize.Y * 0.8f + 0.21f) >= 0.88f)
			laserIncreasing = false;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected void BeamDisappear()
	{
		Centre -= GetVector2(recoilSpeed += 0.45f, Rotation);
		if (recoilSpeed >= 5f)
			beamAlpha *= 0.9f;
		if ((laserSize.Y -= MathF.Sqrt(appearTime - waitingTime - duration) / 36f) <= 0 && (!screen.Contain(Centre)))
			Dispose();
	}
	private class DelayControl : GameObject
	{
		internal enum DelayType
		{
			Pull = 0,
			Stop = 1
		}
		private float delay = 0;
		private readonly DelayType type;
		public DelayControl(float delay, DelayType delayType)
		{
			UpdateIn120 = true;
			type = delayType;
			this.delay = delay;
		}
		public override void Update()
		{
			GasterBlaster control = FatherObject as GasterBlaster;
			float del = type == DelayType.Pull
				? Math.Clamp(delay * 0.1f, 0.5f, 3)
				: Math.Clamp((delay > 10 ? 10 : MathF.Sqrt(delay * 2)) * 0.3f, 0.4f, 1);
			if (delay < del)
				del = delay;
			del /= 2;
			control.waitingTime += del;
			delay -= del;
			if (delay <= 0f)
				Dispose();
		}
	}
	/// <summary>
	/// Delays the blaster by the given frames
	/// </summary>
	/// <param name="delay">The frames to delay</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Delay(float delay) => AddChild(new DelayControl(delay, DelayControl.DelayType.Pull));
	/// <summary>
	/// Stops the blaster for the given frames
	/// </summary>
	/// <param name="delay">The frames to stop</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Stop(float delay) => AddChild(new DelayControl(delay, DelayControl.DelayType.Stop));
	/// <inheritdoc/>
	public abstract override void GetCollide(Player.Heart player);
}
/// <summary>
/// A Green Soul Blaster
/// </summary>
public class GreenSoulGB : GasterBlaster
{
	private Texture2D StuckTexture => (appearTime % 6 > 2) ? Sprites.stuck1 : Sprites.stuck2;

	private readonly Player.Heart missionPlayer;
	/// <summary>
	/// The direction of the blaster
	/// </summary>
	public int Way { get; }

	private Vector2 Position;

	private bool stuck = false;
	private float pushDelta;

	private readonly float timeDelta;
	/// <summary>
	/// Creates a green soul blaster
	/// </summary>
	/// <param name="shootShieldTime">The time for the blaster to fire</param>
	/// <param name="way">The string direction of the blaster</param>
	/// <param name="color">The color type of the blaster</param>
	/// <param name="duration">The duration of the blaster</param>
	public GreenSoulGB(float shootShieldTime, string way, int color, float duration) : this(shootShieldTime, GetWayFromTag(way), color, duration) { }
	/// <summary>
	/// Creates a green soul blaster
	/// </summary>
	/// <param name="shootShieldTime">The time for the blaster to fire</param>
	/// <param name="way">The direction of the blaster</param>
	/// <param name="color">The color type of the blaster</param>
	/// <param name="duration">The duration of the blaster</param>
	public GreenSoulGB(float shootShieldTime, int way, int color, float duration)
	{
		movingScale = shootShieldTime < 30 ? 0.5f + shootShieldTime / 90f : 0.93334f - 3f / shootShieldTime;
		if (Settings.SettingsManager.DataLibrary.Mirror)
			color ^= 1;
		way = Posmod(way, 4);
		timeDelta = Settings.SettingsManager.DataLibrary.ArrowDelay / 16f;
		depth_ = 0.466f + _blasterCount++ / 1000f;
		shootShieldTime += Gametime;
		laserSize.X = 1.0f;
		size = new Vector2(1.0f, 0.7f);
		missionPlayer = Player.heartInstance;
		waitingTime = shootShieldTime - Gametime;
		this.duration = duration;
		Way = way;
		drawingColor = ShieldColorTypes[DrawingColor = color];
		basicRotation = Rotation = (way * 90 + 180) % 360;
		Position = way switch
		{
			0 => new Vector2(270, 0),
			1 => new Vector2(0, 190),
			2 => new Vector2(-270, 0),
			3 => new Vector2(0, -190),
			_ => throw new ArgumentOutOfRangeException(nameof(way), "Way must be between 0 and 3"),
		};
	}
	private readonly float basicRotation;

	internal bool Follow { private get; set; } = false;
	internal bool Ending { get; private set; } = false;
	/// <summary>
	/// The drawing color type of the blaster
	/// </summary>
	public int DrawingColor { get; }
	private int ShieldDirection => missionPlayer.Shields.DirectionOf(DrawingColor);
	private ParticleGather ParticleEffect;
	internal bool Auto => DebugState.ShieldAuto[DrawingColor]; //No safety check here because it should already be done in creation

	private Vector2 _lastPlayerPos;
	private float _lastPlayerRot;
	/// <inheritdoc/>
	public override void Update()
	{
		if (!missionPlayer.FixArrow)
		{
			float resultRotation = basicRotation + missionPlayer.Rotation;
			if (missionRotation != resultRotation)
			{
				Rotation += resultRotation - missionRotation;
				missionRotation = basicRotation + missionPlayer.Rotation;
			}
		}
		else
			missionRotation = basicRotation;
		int dir = ShieldDirection;
		base.Update();
		float adjustedWaitingTime = waitingTime + timeDelta;
		if ((int)(appearTime - adjustedWaitingTime) >= 0)
		{
			if (appearTime <= adjustedWaitingTime + duration)
			{
				if (Auto && dir != Way)
					foreach (Player.Heart p in Player.hearts)
					{
						p.Shields.Rotate(DrawingColor, Way);
						p.Shields.ValidRotated();
					}
				if (appearTime - adjustedWaitingTime >= 0)
				{
					missionPlayer.Shields.MakeShieldParticle(drawingColor, missionPlayer.FixArrow ? missionRotation : missionRotation + missionPlayer.Rotation);
					//check collision
					CalcPush(dir);
					PushDown();
					if ((Follow && ((missionPlayer.Centre - _lastPlayerPos).LengthSquared() > 0.1f)) || missionPlayer.Rotation != _lastPlayerRot)
						ArrangePos();
					GetCollide();
				}
				Rotation = missionRotation * 0.12f + Rotation * 0.88f;
				if (appearTime <= adjustedWaitingTime + 10)
					Centre = Centre * movingScale + missionPlace * (1 - movingScale);
			}
		}
		else
		{
			missionPlace = Rotate(Position, missionPlayer.Rotation) + missionPlayer.Centre;
			if (adjustedWaitingTime - appearTime <= 54)
			{
				if (appearTime < adjustedWaitingTime)
					Centre = Follow ? missionPlace : Centre * movingScale + missionPlace * (1 - movingScale);
				Rotation = missionRotation * 0.12f + Rotation * 0.88f;
				if (alpha < 1)
					alpha += 0.1f;
				if (ParticleEffect is not null)
					ParticleEffect.Centre = Centre;
			}
			if ((int)(adjustedWaitingTime - appearTime) == 55)
			{
				GameStates.InstanceCreate(ParticleEffect = new ParticleGather(missionPlace, 21, 55, drawingColor));
				Centre = GetVector2(120, Rotation) + missionPlace;
				Rotation += Rand(-40, 40);
				missionPlayer.Shields.Consume();
				if (!spawnSoundPlayed)
				{
					PlaySound(Sounds.GBSpawn, AppearVolume);
					spawnSoundPlayed = true;
				}
			}
		}

		if (appearTime >= waitingTime + duration)
		{
			if (!Ending)
			{
				missionPlayer.Shields.ShieldShine(Way, DrawingColor, score);
				missionPlayer.Shields.GetCollideChecker(DrawingColor).ArrowBlock(Way);
			}
			Ending = true;
			BeamDisappear();
		}
		_lastPlayerPos = missionPlayer.Centre;
		_lastPlayerRot = missionPlayer.Rotation;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ArrangePos()
	{
		float rotation = missionRotation;
		Vector2 unitU = GetVector2(1, rotation);
		float distance = Vector2.Dot(unitU, Centre - Heart.Centre);
		Centre = Heart.Centre + unitU * distance;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CalcPush(int dir)
	{
		if (stuck = dir == Way)
			missionPlayer.Shields.Push(this, DrawingColor);
		laserPlace = missionPlayer.Centre + GetVector2(stuck ? -38 + pushDelta : 38, missionRotation);
		pushDelta = missionPlayer.Shields.PushDelta(DrawingColor);
	}
	/// <inheritdoc/>
	public override void Draw()
	{
		base.Draw();
		//Early exit in cases beam should not be drawn
		if (appearTime < waitingTime + timeDelta || laserSize.Y <= 0)
			return;
		Color finCol = drawingColor * beamAlpha;
		Depth -= 0.001f;
		//Override beam drawing
		DrawingLab.DrawLine(laserPlace + GetVector2(2, missionRotation), Centre + GetVector2(Image.Width * size.X / 2 + 10, missionRotation), (laserSize * size).Y * Sprites.GBLaser.Height, finCol, Depth);
		for (int i = 0; i < 3; i++)
			DrawingLab.DrawLine(Centre + GetVector2(Image.Width * size.X * (0.5f - i * 0.1f) + 10, missionRotation), Centre + GetVector2(Image.Width * size.X * (0.4f - i * 0.1f) + 10, missionRotation), (laserSize * size).Y * Sprites.GBLaser.Height * (0.8f - i * 0.2f), finCol, Depth);
		//Stuck drawing
		FormalDraw(StuckTexture, laserPlace + GetVector2(2, missionRotation), finCol, 1.33f * laserSize.Y, GetRadian(missionRotation + 180), new Vector2(0, 35));
	}
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void GetCollide(Player.Heart p = null)
	{
		bool alw = Auto;
		if (!stuck || pushDelta > 22)
		{
			if (alw || alpha < 0.5f)
				return;

			if (appearTime - waitingTime - timeDelta < 4.5f)
			{
				if (appearTime - waitingTime - timeDelta < 2.5f)
					return;
				score = Math.Min(2, score);
				return;
			}
			if (alpha < 0.97f)
			{
				if (alpha > 0.6f)
				{
					score = Math.Min(2, score);
					return;
				}
				score = 1;
			}
			LoseHP(missionPlayer);
			if (!hasHit)
			{
				PushScore(0);
				score = 0;
				hasHit = true;
			}
		}
		else if (pushDelta > 14 && stuck)
			score = Math.Min(2, score);
		else if (pushDelta > 6 && stuck)
			score = 1;
	}
	/// <inheritdoc/>
	public override void Dispose()
	{
		_blasterCount--;
		if (score != 3 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
		{
			if (!hasHit)
				PushScore(0);

			LoseHP(missionPlayer);
			hasHit = true;
		}
		if (!hasHit)
			missionPlayer.Shields.ValidRotated();
		base.Dispose();
	}
}
/// <summary>
/// A normal gaster blaster
/// </summary>
public class NormalGB : GasterBlaster, ICollideAble
{
	/// <summary>
	/// Creates a blaster that automatically aims towards the player
	/// </summary>
	/// <param name="missionPlace">Target position</param>
	/// <param name="spawnPlace">Initial position</param>
	/// <param name="size">Size of the blaster(Width, Height), a small blaster is (1, 0.5f) and a big blaster is (1, 1)</param>
	/// <param name="waitingTime">Time required to pass before firing</param>
	/// <param name="duration">Duration of the blast</param>
	public NormalGB(Vector2 missionPlace, Vector2 spawnPlace, Vector2 size, float waitingTime, float duration) : this(missionPlace, spawnPlace, size,
		(float)(Math.Atan2(Heart.Centre.Y - missionPlace.Y, Heart.Centre.X - missionPlace.X) * 180 / Math.PI), waitingTime, duration)
	{ }
	/// <summary>
	/// Creates a blaster that aims to the given angle
	/// </summary>
	/// <param name="missionPlace">Target position</param>
	/// <param name="spawnPlace">Initial position</param>
	/// <param name="size">Size of the blaster(Width, Height), a small blaster is (1, 0.55f) and a big blaster is (1, 1)</param>
	/// <param name="rotation">The target rotation of the blaster</param>
	/// <param name="waitingTime">Time required to pass before firing</param>
	/// <param name="duration">Duration of the blast</param>
	public NormalGB(Vector2 missionPlace, Vector2 spawnPlace, Vector2 size, float rotation, float waitingTime, float duration)
	{
		movingScale = waitingTime < 30 ? 0.5f + waitingTime / 90f : 0.93334f - 3f / waitingTime;

		if (!spawnSoundPlayed && AppearVolume > 0)
		{
			PlaySound(Sounds.GBSpawn, AppearVolume);
			spawnSoundPlayed = true;
		}
		Centre = spawnPlace;
		missionRotation = rotation;
		Rotation = GetRandom(0, 359);
		this.missionPlace = missionPlace;
		this.size = size;
		laserSize.X = 1.0f;
		Depth = 0.6f;
		this.waitingTime = waitingTime;
		this.duration = duration;

		rotateWay = (missionRotation - Rotation + 360) % 360 < (360 - missionRotation + Rotation) % 360;
	}
	/// <inheritdoc/>
	public override void Update()
	{
		MoveToMission();
		base.Update();
		if ((int)(appearTime - waitingTime) >= 0 && appearTime <= waitingTime + duration)
			PushDown();
		if (appearTime >= waitingTime + duration)
			BeamDisappear();
	}
	/// <summary>
	/// Whether the enable the bug fix for reverse collision
	/// </summary>
	public override void GetCollide(Player.Heart heart)
	{
		//If the Cos(Theta) < 0, then Theta is (90, 180), therefore the heart is behind blaster
		if ((Cos(GetVector2(1, Rotation), Centre - Heart.Centre) > 0) ||
			//Early exit in cases beam should not be drawn
			appearTime > waitingTime + duration + 2 || (appearTime < waitingTime - 2) || alpha <= 0.8f ||
			//No collision in green soul
			heart.SoulType == 1)
			return;

		float dist = Rotation == 0
			? Centre.X - heart.Centre.X //If the blaster is colinear, directly compare the X distance
			: MathUtil.ScalarProject(new(MathF.Tan(GetRadian(Rotation)), -1), heart.Centre - Centre);
		float res = Math.Abs(dist) - (32 * laserSize.Y * size.Y - 2);

		if (res < 0)
		{
			if (!hasHit)
				PushScore(0);
			LoseHP(heart);
			hasHit = true;
		}
		//Early exit if no score should be marked
		if (hasHit || !MarkScore)
			return;
		if (res <= 2)
			OkayCollision();
		else if (res <= 5.4f)
			NiceCollision();
		if (score != 3 && ((CurrentScene as FightScene).Mode & GameMode.PerfectOnly) != 0)
		{
			if (!hasHit)
				PushScore(0);
			LoseHP(heart);
			hasHit = true;
		}
	}
}