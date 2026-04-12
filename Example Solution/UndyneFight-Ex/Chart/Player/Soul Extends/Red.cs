using static UndyneFight_Ex.GameStates;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities;

public partial class Souls
{
	/// <summary>
	/// Red soul processing logic
	/// </summary>
	public static Player.MoveState RedSoul { get; private set; } = new(Color.Red, SoulMove);
	/// <summary>
	/// Basic soul moving logic
	/// </summary>
	/// <param name="s"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SoulMove(Player.Heart s)
	{
		CollideRect curPos = s.CollidingBox;
		Vector2 curCentre = curPos.GetCentre();
		float speed = s.Speed;
		if (IsKeyDown(InputIdentity.Cancel))
			speed *= 0.5f;
		Vector2 delta = Vector2.Zero;
		for (int i = 0; i < 4; i++)
		{
			if (IsKeyDown(s.movingKey[i]))
				delta += GetVector2(speed * 0.5f, i * 90);
		}
		Vector2 nexCentre = curCentre + delta;
		FightBox box = s.controllingBox;
		BoxVertex[] Vertices = box.Vertices;
		// calculate all Vertices' normal vector
		nexCentre = DoBoxRestriction(curCentre, nexCentre, Vertices);
		s.Centre = nexCentre;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector2 DoBoxRestriction(Vector2 curCentre, Vector2 nexCentre, BoxVertex[] Vertices)
	{
		_ = new Vector2[Vertices.Length];
		for (int i = 0; i < Vertices.Length; i++)
		{
			Vector2 a = Vertices[i].CurrentPosition, b = Vertices[(i + 1) % Vertices.Length].CurrentPosition,
			normal = Rotate(b - a, 90), centre = (a + b) / 2, along = (b - a) / 2,
			del1 = curCentre - centre, del2 = nexCentre - centre;

			float distance = along.Length();
			normal.Normalize();
			along.Normalize();

			//project the vector to the along vector to make sure the heart can be control by the segment
			float dirDelta1 = MathUtil.ScalarProject(along, del1), dirDelta2 = MathUtil.ScalarProject(along, del2);
			if (MathF.Abs(dirDelta1) > distance + 0.2f && MathF.Abs(dirDelta2) > distance + 0.2f)
				continue;

			//project the vector to the normal vector and get the distance of heart and line
			float dis1 = MathUtil.ScalarProject(normal, del1), dis2 = MathUtil.ScalarProject(normal, del2);
			if (dis1 < 0)
			{
				_ = -dis1;
				dis2 = -dis2;
				normal = -normal;
			}

			if (dis2 < 8)
			{
				dis2 = 8;
				// linear combination
				nexCentre = centre + along * dirDelta2 + dis2 * normal;
			}
		}
		return nexCentre;
	}
}