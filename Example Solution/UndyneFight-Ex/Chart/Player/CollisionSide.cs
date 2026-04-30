namespace UndyneFight_Ex.Entities;

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
				private struct CollisionData()
				{
					public bool BlockedArrow = false;
					public float TimeDelayed = FarTime, TapTime = FarTime, HoldTime = FarTime;
				}
				private readonly CollisionData[] collisionData = [new(), new(), new(), new()];

				public CollisionSide() => UpdateIn120 = true;
				public override void Start()
				{
					father = FatherObject as Shield;
					base.Start();
				}
				public override void Update()
				{
					bool shieldAttachingGB = father.AttachingGB;
					for (int i = 0; i < 4; i++)
					{
						CollisionData curColData = collisionData[i];
						InputIdentity curUpdateKey = father.UpdateKeys[i];
						if (!curColData.BlockedArrow)
							curColData.TimeDelayed += 0.5f;
						curColData.TapTime += 0.5f;
						curColData.HoldTime += 1f;
						if (GameStates.IsKeyPressed120f(curUpdateKey))
						{
							curColData.BlockedArrow = false;
							curColData.TimeDelayed = 0;
							curColData.TapTime = 0;
						}
						if (curColData.BlockedArrow && father.Way != i)
						{
							curColData.BlockedArrow = false;
							curColData.TimeDelayed = FarTime;
						}
						if (GameStates.IsKeyDown(curUpdateKey))
							curColData.HoldTime = 0;
						if (GameStates.IsKeyPressed120f(curUpdateKey) || (shieldAttachingGB && father.attachedGB.Way == i))
							curColData.TimeDelayed = 0;
						collisionData[i] = curColData;
					}
				}
				public override void Draw() { }
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				internal void ArrowBlock(int direction)
				{
					CollisionData curColData = collisionData[direction];
					curColData.BlockedArrow = father.Way == direction;
					curColData.TimeDelayed = curColData.BlockedArrow ? 0 : FarTime;
					curColData.TapTime = FarTime;
					collisionData[direction] = curColData;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]

				internal float TimeOf(int way) => collisionData[way].TimeDelayed;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				internal float TapTimeOf(int way) => collisionData[way].TapTime;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				internal float HoldTimeOf(int way) => collisionData[way].HoldTime;
			}
		}
	}
}