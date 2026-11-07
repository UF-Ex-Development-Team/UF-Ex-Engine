using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

public partial class Souls
{
	public class CyanMoveState : Player.MoveState
	{
		public CyanMoveState() : base(Color.Cyan, null) => MoveFunction = SoulMove;

		public float PlungeSpeed { get; set; } = 15f;
		public float PlungeDecay { get; set; } = 0.1f;
		private Vector2 _plungeSpeed = Vector2.Zero;

		private const float COOLDOWN = 62.5f;
		private float _curTime = 0.0f;

		public void SoulMove(Player.Heart s)
		{
			CollideRect curPos = s.CollidingBox;

			Vector2 curCentre = curPos.GetCentre();

			float speed = s.Speed;
			if (IsKeyDown(InputIdentity.Cancel))
			{ speed *= 0.5f; }
			Vector2 delta = Vector2.Zero;
			bool flag = false;
			Vector2 plungeBuffer = Vector2.Zero;
			for (int i = 0; i < 4; i++)
			{
				if (IsKeyDown(s.movingKey[i]))
				{
					delta += GetVector2(speed * 0.5f, i * 90);

					if (_curTime > COOLDOWN && IsKeyPressed120f(InputIdentity.Alternate))
					{
						flag = true;
						plungeBuffer += GetVector2(1f, i * 90);
					}
				}
			}
			if (flag)
			{
				_curTime = 0;
				plungeBuffer.Normalize();
				plungeBuffer *= 0.5f * PlungeSpeed;
			}
			_plungeSpeed += plungeBuffer;
			if (_curTime <= COOLDOWN)
				_curTime += 0.5f;
			_plungeSpeed *= 1 - PlungeDecay;
			delta += _plungeSpeed;

			Vector2 nexCentre = curCentre + delta;

			FightBox box = s.controllingBox;
			BoxVertex[] Vertices = box.Vertices;

			// calculate all Vertices' normal vector

			nexCentre = DoBoxRestriction(curCentre, nexCentre, Vertices);

			s.Centre = nexCentre;
		}
	}
	/// <summary>
	/// Cyan soul process logic
	/// </summary>
	public static Player.MoveState CyanSoul => new CyanMoveState();
}