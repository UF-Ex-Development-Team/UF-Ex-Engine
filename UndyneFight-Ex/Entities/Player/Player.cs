using static UndyneFight_Ex.Fight.Functions;

namespace UndyneFight_Ex.Entities
{
	public partial class Player : Entity
	{
		internal class CollideEffect : Entity
		{
			private readonly float size;
			private readonly Color color;
			private float light = 1.0f;

			public CollideEffect(Color color, float size)
			{
				controlLayer = Surface.Hidden;
				this.size = (size + 16) / 16f;
				this.color = color * (ScreenDrawing.UIColor.A / 255f);
				Image = FightResources.Sprites.soulCollide;
				UpdateIn120 = true;
				Depth = 0.4f;
			}

			public override void Draw() => FormalDraw(Image, Centre, color * light, size, MathUtil.GetRadian((FatherObject as Heart).Rotation), ImageCentre);

			public override void Update()
			{
				Centre = (FatherObject as Heart).Centre;
				light -= 0.05f;
				if (light < 0)
					Dispose();
			}
		}
		/// <summary>
		/// Creates a collision effect
		/// </summary>
		/// <param name="color">The color of the effect</param>
		/// <param name="size">The size of the effect</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateCollideEffect(Color color, float size) => heartInstance.AddChild(new CollideEffect(color, size * 1.5f));
		/// <summary>
		/// The heart instance of the player you are controlling
		/// </summary>
		public static Heart heartInstance;
		/// <summary>
		/// The list of hearts
		/// </summary>
		public static List<Heart> hearts = [];

		public Player()
		{
			hpControl = new();
			hearts = [];

			heartInstance = new();
			AddChild(hpControl);
			AddChild(heartInstance);
		}

		public partial class Heart : Entity
		{
			/// <summary>
			/// The purple filling effect of purple soul transition
			/// </summary>
			internal class PurpleFiller : Entity
			{
				private readonly Heart user;
				public PurpleFiller(int lineCount, Heart player)
				{
					controlLayer = Surface.Hidden;
					user = player;
					this.lineCount = lineCount;
					appearTime = 0;
				}

				private int appearTime = 0;
				private readonly int halfTime = 14;
				private float percent = 0;
				private readonly int lineCount = 0;

				public override void Draw()
				{
					CollideRect v = user.controlingBox.CollidingBox;
					FormalDraw(FightResources.Sprites.pixUnit, new CollideRect(v.X, appearTime < halfTime ? v.Y : v.Down - v.Height * percent, v.Width, v.Height * percent).ToRectangle(), Color.MediumPurple);
				}

				public override void Update()
				{
					if (++appearTime < halfTime)
						percent = percent * 0.85f + 0.15f;
					else if (appearTime == halfTime)
					{
						user.lastChangeTime = 0;

						if (lineCount == -1)
							return;

						int last = user.purpleLineCount;
						user.purpleLineCount = lineCount;
						int delta = (Move.currentLine - last) / 2;
						Move.currentLine += delta;

						user.purpleLineCount = lineCount;
					}
					else
						percent *= 0.86f;
					if (appearTime == halfTime * 3)
						Dispose();
				}
			}

			#region 基本性质
			/// <summary>
			/// The box the heart is tied to
			/// </summary>
			public FightBox controlingBox;

			/// <summary>
			/// Soul type, 0-> Red, 1-> Green, 2-> Blue, 3-> Orange, 4-> Purple, 5-> Gray
			/// </summary>
			public int SoulType { get; private set; }

			private Vector2 lastCentre;
			private int lastChangeTime = 0;
			#endregion

			#region 各项属性
			/// <summary>
			/// The ID of the player
			/// </summary>
			public int ID { get; init; }
			/// <summary>
			/// The last position of the player
			/// </summary>
			public Vector2 LastCentre => lastCentre;
			/// <summary>
			/// Whether to allow soft falling for blue soul
			/// </summary>
			public bool SoftFalling { get; set; }
			/// <summary>
			/// Whether the arrows will follow the rotation of the soul
			/// </summary>
			public bool FixArrow { get; set; } = false;
			/// <summary>
			/// The alpha of the soul
			/// </summary>
			public float Alpha { get; set; } = 1.0f;
			/// <summary>
			/// The speed of the player (Default 2.5f)
			/// </summary>
			public float Speed { set; get; } = 2.5f;

			private int jumpTimeLimit = 2;
			/// <summary>
			/// The maximum amount of times the player can jump (Default 2)
			/// </summary>
			public int JumpTimeLimit
			{
				set => jumpTimeLeft = jumpTimeLimit = value;
				private get => jumpTimeLimit;
			}

			/// <summary>
			/// The jumping speed of blue soul (Default 6)
			/// </summary>
			public float JumpSpeed { private get; set; } = 6f;
			/// <summary>
			/// The gravity of the blue soul (Default 9.8f)
			/// </summary>
			public float Gravity { private get; set; } = 9.8f;

			public int YFacing => MathUtil.Posmod(missionRotation, 360) switch
			{
				_ when MathUtil.Posmod(missionRotation, 360) is >= 45 and < 135 => 2,
				_ when MathUtil.Posmod(missionRotation, 360) is >= 135 and < 225 => 3,
				_ when MathUtil.Posmod(missionRotation, 360) is >= 225 and < 315 => 0,
				_ => 1
			};
			public int XFacing => MathUtil.Posmod(YFacing - 1, 4);
			/// <summary>
			/// Whether the player is moving
			/// </summary>
			public bool IsMoved { get; private set; }
			/// <summary>
			/// Whether the player is not moving
			/// </summary>
			public bool IsStable { get; private set; }
			/// <summary>
			/// The speed of the slow falling of blue soul (Default 2/3f)
			/// </summary>
			public float UmbrellaSpeed { set => umbrellaSpeed = value; }
			private float umbrellaSpeed = 2/3f;

			private int purpleLineCount = 3;
			/// <summary>
			/// The amount of lines in purple soul mode
			/// </summary>
			public int PurpleLineCount
			{
				set => GameStates.InstanceCreate(new PurpleFiller(value, this));
				get => purpleLineCount;
			}

			private bool enabledRedShield = false;
			/// <summary>
			/// Whether the enable the red shield for non-green soul types
			/// </summary>
			public bool EnabledRedShield
			{
				set { enabledRedShield = value; (CurrentScene as SongFightingScene).GreenSoulUsed = true; }
			}
			/// <summary>
			/// Whether the soul is split into several souls
			/// </summary>
			public bool IsSoulSplit
			{
				set
				{
					if (value)
					{
						_ = new Player();
						isSoulSplit = true;
					}
					else
					{
						heartInstance = hearts[0];
						hearts.Clear();
						hearts.Add(heartInstance);
					}
				}
				get => isSoulSplit;
			}
			/// <summary>
			/// Whether the player can use the soft falling
			/// </summary>
			public bool UmbrellaAvailable
			{
				set => umbrellaAvailable = value;
			}
			/// <summary>
			/// The moving keys for the soul (Right, Down, Left, Up)
			/// </summary>
			public InputIdentity[] movingKey = [InputIdentity.MainRight, InputIdentity.MainDown, InputIdentity.MainLeft, InputIdentity.MainUp];

			private bool isOranged = false;
			/// <summary>
			/// Whether the soul is orange (Forced to move constantly)
			/// </summary>
			public bool IsOranged
			{
				set
				{
					if (isOranged = value)
						ResetOrange();
				}
			}
			#endregion

			#region 被属性控制的私字段

			private float gravitySpeed = 0.0f;

			private bool isSoulSplit = false, umbrellaAvailable = false;

			/// <summary>
			/// 是否被重力摔了
			/// </summary>
			private bool isForced = false;
			private float forcedSpeed = 2f, purpleLineLength = 0;
			private int jumpTimeLeft = 2;
			#endregion

			public Heart()
			{
				CurrentMoveState = _red;
				controlLayer = Surface.Hidden;
				heartInstance = this;
				hearts.Add(this);

				ID = hearts.Count - 1;

				controlingBox = new RectangleBox(this);
				Image = FightResources.Sprites.player;
				collidingBox.Size = new(16);
				Centre = new(320);
				Depth = 0.3f;
				UpdateIn120 = true;
			}
			/// <inheritdoc/>
			public override void Start()
			{
				AddChild(Shields = new());
				GameStates.InstanceCreate(controlingBox);

				Player manager = FatherObject as Player;
				manager.GameAnalyzer.PushData(new SoulListData(ID, true, GametimeF));

				manager.GameAnalyzer.PushData(new SoulChangeData(SoulType, ID, GametimeF));
			}
			/// <inheritdoc/>
			public override void Dispose()
			{
				Player manager = FatherObject as Player;
				manager.GameAnalyzer.PushData(new SoulListData(ID, false, GametimeF));
				controlingBox.Dispose();
				base.Dispose();
			}

			/// <summary>
			/// Merge the current soul with the target soul
			/// </summary>
			/// <param name="another">The soul to merge to</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Merge(Heart another)
			{
				mergeTime = 0;
				mergeMission = another;
				(FatherObject as Player).hpControl.GiveProtectTime(32);
			}
			/// <summary>
			/// Merge all souls
			/// </summary>
			/// <param name="target">The heart target to merge to (Default 0)</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void MergeAll(int target = 0)
			{
				for (int i = 1; i < hearts.Count; i++)
					hearts[i].Merge(hearts[target]);
			}

			private int mergeTime;
			private Heart mergeMission;
			/// <summary>
			/// Splits the current soul
			/// </summary>
			/// <returns>The new soul that was split</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Heart Split()
			{
				Heart v = new()
				{
					Speed = Speed,
					jumpTimeLimit = jumpTimeLimit,
					SoulType = SoulType,
					purpleLineCount = purpleLineCount,
					Gravity = Gravity,
					JumpSpeed = JumpSpeed,
					umbrellaAvailable = umbrellaAvailable,
					umbrellaSpeed = umbrellaSpeed,
					Centre = Centre,
					Alpha = Alpha,
					CurrentMoveState = CurrentMoveState
				};
				v.controlingBox.InstanceMove(controlingBox.CollidingBox);
				isSoulSplit = true;

				(FatherObject as Player).AddChild(v);

				return v;
			}
			/// <summary>
			/// Instantly splits the soul
			/// </summary>
			/// <param name="area">The rectangle of the box of the new soul</param>
			/// <returns>The new soul that was split</returns>
			public Heart InstantSplit(CollideRect area)
			{
				Heart v = new()
				{
					Speed = Speed,
					jumpTimeLimit = jumpTimeLimit,
					SoulType = SoulType,
					purpleLineCount = purpleLineCount,
					Gravity = Gravity,
					JumpSpeed = JumpSpeed,
					umbrellaAvailable = umbrellaAvailable,
					umbrellaSpeed = umbrellaSpeed,
					Centre = area.GetCentre(),
					Alpha = Alpha,
					CurrentMoveState = CurrentMoveState
				};
				v.controlingBox.InstanceMove(area);

				(FatherObject as Player).AddChild(v);

				return v;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal void DrawHeart()
			{
				if (!Fight.FightStates.finishSelecting)
					return;

				Depth = 0.3f;
				int protectTime = (FatherObject as Player).hpControl.protectTime;
				Color drawingColor = isOranged ? Color.Orange : CurrentMoveState.StateColor;
				FormalDraw(Image, Centre, drawingColor * (protectTime > 0 ? ((protectTime % 30) > 8 ? 0.6f : 1) : 1) * Alpha, MathUtil.GetRadian(Rotation), ImageCentre);

				if (SoulType == 4)
				{
					int count = PurpleLineCount + 1;
					float delta = controlingBox.CollidingBox.Height / count;
					for (int i = 1; i < count; i++)
					{
						RectangleBox box = controlingBox as RectangleBox;
						DrawingLab.DrawLine(new Vector2(box.Centre.X, i * delta + box.Up),
							0, purpleLineLength, 3, Color.MediumPurple, 0.1f);
					}
				}
			}

			/// <inheritdoc/>
			public override void Update()
			{
				GravityLine.Recover();

				if (mergeMission != null)
					DoMerge();

				lastCentre = Centre;
				lastChangeTime++;

				if (SoulType != 4)
				{
					purpleLineLength *= 0.84f;
					Centre += positionRest * 0.25f;
					positionRest *= 0.75f;
				}
				else
				{
					Centre += positionRest * 0.3f;
					positionRest *= 0.7f;
				}

				Rotation += GetRotateDelta() * 0.3f * (rotateWay ? 1 : -1);

				if (!Fight.FightStates.roundType)
					CurrentMoveState.MoveFunction.Invoke(this);

				if (isOranged)
					if ((Gametime % 3) == 0)
						GameStates.InstanceCreate(new RetentionEffect(this, 15, Color.Orange * 0.5f)
						{ AngleMode = true });

				IsStable = !(IsMoved = (lastCentre - Centre).Length() >= 0.005f);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void ResetOrange()
			{
				if (SoulType == 2)
				{
					jumpTimeLeft = 0;
					gravitySpeed = 0;
				}
				Move.blueLastWay = false;
				Move.last = -1;
			}

			/// <summary>
			/// Changes the soul type
			/// </summary>
			/// <param name="type">Soul type, see <see cref="SoulType"/> for the types</param>
			/// <param name="resetGravSpd"> Whether to reset blue soul gravity or not (Default false)</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ChangeColor(int type, bool resetGravSpd = false)
			{
				isOranged = type == 3;
				lastChangeTime = 0;
				switch (type)
				{
					case 1:
						if (!(CurrentScene as SongFightingScene).GreenSoulUsed)
							(CurrentScene as SongFightingScene).GreenSoulUsed = true;
						break;
					case 2:
						jumpTimeLeft = JumpTimeLimit;
						if (resetGravSpd)
							gravitySpeed = 0;
						break;
					case 3:
						ResetOrange();
						type = 0;
						break;
					case 4:
						GameStates.InstanceCreate(new PurpleFiller(-1, this));
						break;
				}
				CurrentMoveState = type switch
				{
					0 => _red,
					1 => _green,
					2 => _blue,
					3 => _red,
					4 => _purple,
					5 => _gray,
					_ => throw new ArgumentOutOfRangeException(nameof(type)),
				};
				SoulType = type;
				_ = CreateShinyEffect(CurrentMoveState.StateColor);
				Player manager = FatherObject as Player;
				manager.GameAnalyzer.PushData(new SoulChangeData(SoulType, ID, GametimeF));
			}

			#region 缓动

			private Vector2 positionRest;
			private Vector2 lastBoxCentre;
			private float missionRotation;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void DoMerge()
			{
				float v = 0.78f;
				float v2 = 1 - v;
				Centre = Centre * v + mergeMission.Centre * v2;
				for (int i = 0; i < controlingBox.Vertices.Length; i++)
				{
					controlingBox.Vertices[i].CurrentPosition = controlingBox.Vertices[i].CurrentPosition * v + mergeMission.controlingBox.Vertices[i].MissionPosition * v2;
				}

				controlingBox.InstanceMove(new CollideRect(controlingBox.Vertices[0].CurrentPosition, controlingBox.Vertices[2].CurrentPosition - controlingBox.Vertices[0].CurrentPosition));
				mergeTime++;
				if (mergeTime == 25)
					Dispose();
			}

			/// <summary>
			/// 旋转方向，true则代表顺时针 <br/>
			/// Rotation direction, true-> clockwise
			/// </summary>
			private bool rotateWay;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private float GetRotateDelta()
			{
				float trueRot = (Rotation + 90) % 360;
				float trueMission = (missionRotation + 90) % 360;
				return Math.Min((trueMission - trueRot + 360) % 360, (360 - trueMission + trueRot) % 360);
			}
			/// <summary>
			/// Instantly rotates the soul to the given rotation
			/// </summary>
			/// <param name="rot">The target angle</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void InstantSetRotation(float rot) => Rotation = missionRotation = rot;
			/// <summary>
			/// Rotates the soul to the given rotation
			/// </summary>
			/// <param name="rot">The target angle</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RotateTo(float rot)
			{
				float trueRot = (Rotation + 90) % 360;
				missionRotation = rot;
				float trueMission = (missionRotation + 90) % 360;
				rotateWay = (trueMission - trueRot + 360) % 360 < (360 - trueMission + trueRot) % 360;
			}
			/// <summary>
			/// Applies force to the soul and changes the gravity of the soul to that direction
			/// </summary>
			/// <param name="rotation">The direction to set the gravity to (Must be a multiple of 90)</param>
			/// <param name="speed">The magnitude of gravity</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void GiveForce(float rotation, float speed)
			{
				forcedSpeed = speed;
				isForced = true;
				jumpTimeLeft = 0;
				RotateTo(rotation);
				gravitySpeed = speed;
				GravityLine.Reload();
				heartInstance.Centre += MathUtil.GetVector2(heartInstance.gravitySpeed, missionRotation + 90);
			}
			/// <summary>
			/// Applies force to the soul and changes the gravity of the soul to that direction and instantly rotates the soul to that direction
			/// </summary>
			/// <param name="rotation">The direction to set the gravity to (Must be a multiple of 90)</param>
			/// <param name="speed">The magnitude of gravity</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void GiveInstantForce(float rotation, float speed)
			{
				forcedSpeed = speed;
				isForced = true;
				jumpTimeLeft = 0;
				InstantSetRotation(rotation);
				gravitySpeed = speed;
				GravityLine.Reload();
				heartInstance.Centre += MathUtil.GetVector2(heartInstance.gravitySpeed, missionRotation + 90);
			}
			/// <summary>
			/// Lerps the player to the given position
			/// </summary>
			/// <param name="mission">The target position</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Teleport(Vector2 mission) => positionRest = mission - Centre;
			/// <summary>
			/// Instantly teleports the player to the given position
			/// </summary>
			/// <param name="mission">The target position</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void InstantTP(Vector2 mission)
			{
				Centre = mission;
				if (Shields != null)
				{
					Shields.Circle.Centre = mission;
					Shields.RShield.Centre = mission;
					Shields.BShield.Centre = mission;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void ResetMove()
			{
				Move.last = -1;
				Move.currentLine = 0;
				Move.blueLastWay = false;
			}
			/// <summary>
			/// Sets the angle of the soul as the angle of the screen for the specified duration
			/// </summary>
			/// <param name="duration">The duration to set the angle of the soul for</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void FollowScreen(float duration) => AddInstance(new TimeRangedEvent(duration, () => InstantSetRotation(ScreenDrawing.ScreenAngle)) { UpdateIn120 = true });
			#endregion

			/// <inheritdoc/>
			public override void Draw()
			{
				if (SoulType is 2 or 5)
				{
					Depth -= 0.1f;
					//Multi-Jump indicator
					if (jumpTimeLeft > 0 && jumpTimeLimit > 1)
						FormalDraw(Image, Centre + MathUtil.GetVector2(1, Rotation + 90), Color.Lime * (Alpha * ScreenDrawing.UIColor.A / 255f), MathUtil.GetRadian(Rotation), ImageCentre);
					Depth -= 0.1f;
					//Parachute indicator
					if (umbrellaAvailable)
						FormalDraw(Image, Centre - MathUtil.GetVector2(1, Rotation + 90), Color.Red * (Alpha * ScreenDrawing.UIColor.A / 255f), MathUtil.GetRadian(Rotation), ImageCentre);
					Depth += 0.2f;
				}
				DrawHeart();
#if DEBUG
				GlobalResources.Font.NormalFont.CentreDraw(jumpTimeLeft + "/" + jumpTimeLimit, new Vector2(320, 150), Color.Gray);
				DrawingLab.DrawVector(Centre, MathUtil.GetRadian(Rotation + 90));
#endif
			}
			/// <summary>
			/// Creates a collision effect
			/// </summary>
			/// <param name="color">The color of the effect</param>
			/// <param name="size">The size of the effect</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void CreateCollideEffect2(Color color, float size) => AddChild(new CollideEffect(color, size * 1.5f));
		}

		/// <inheritdoc/>
		public override void Update()
		{
			_ = hearts.RemoveAll(s => s.Disposed);
			if (hearts.Count == 0)
				heartInstance = null;
			else if (heartInstance == null || heartInstance.Disposed)
				heartInstance = hearts[0];
		}
		/// <inheritdoc/>
		public override void Draw() { }
		/// <summary>
		/// The HP controller entity
		/// </summary>
		public HPControl hpControl { get; private set; }
	}
}