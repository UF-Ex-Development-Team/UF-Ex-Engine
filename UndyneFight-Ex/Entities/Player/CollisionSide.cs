namespace UndyneFight_Ex.Entities
{
	public partial class Player
	{
		public partial class Heart
		{
			public partial class Shield
			{
				internal class CollisionSide : Entity
				{
					private const float FarTime = 90;
					private Shield father;
					private readonly bool[] blockedArrow = [false, false, false, false];
					private readonly float[] timeDelayed = [FarTime, FarTime, FarTime, FarTime];
					private readonly float[] tapTime = [FarTime, FarTime, FarTime, FarTime];
					private readonly float[] holdTime = [FarTime, FarTime, FarTime, FarTime];

					public CollisionSide() => UpdateIn120 = true;
					public override void Start()
					{
						father = FatherObject as Shield;
						base.Start();
					}
					public override void Update()
					{
						for (int i = 0; i < 4; i++)
						{
							if (!blockedArrow[i])
								timeDelayed[i] += 0.5f;
							tapTime[i] += 0.5f;
							holdTime[i] += 1f;
							if (GameStates.IsKeyPressed120f(father.UpdateKeys[i]))
							{
								blockedArrow[i] = false;
								timeDelayed[i] = 0;
								tapTime[i] = 0;
							}
							if (blockedArrow[i] && father.Way != i)
							{
								blockedArrow[i] = false;
								timeDelayed[i] = FarTime;
							}
							if (GameStates.IsKeyDown(father.UpdateKeys[i]))
								holdTime[i] = 0;
							if (GameStates.IsKeyPressed120f(father.UpdateKeys[i]) || (father.AttachingGB && father.attachedGB.Way == i))
								timeDelayed[i] = 0;
						}
					}
					public override void Draw() { }
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					internal void ArrowBlock(int direction)
					{
						blockedArrow[direction] = father.Way == direction;
						timeDelayed[direction] = blockedArrow[direction] ? 0 : FarTime;
						tapTime[direction] = FarTime;
					}
					[MethodImpl(MethodImplOptions.AggressiveInlining)]

					internal float TimeOf(int way) => timeDelayed[way];
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					internal float TapTimeOf(int way) => tapTime[way];
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					internal float HoldTimeOf(int way) => holdTime[way];
				}
			}
		}
	}
}