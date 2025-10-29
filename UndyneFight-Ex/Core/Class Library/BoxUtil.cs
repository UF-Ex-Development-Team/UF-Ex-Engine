using UndyneFight_Ex.Entities;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.GameStates;

namespace UndyneFight_Ex;
/// <summary>
/// Utilities for box functions
/// </summary>
public static class BoxUtil
{
	/// <summary>
	/// The current fight box as a <see cref="VertexBox"/>
	/// </summary>
	public static VertexBox VertexBoxInstance => FightBox.instance as VertexBox;
	/// <summary>
	/// Converts the current <see cref="FightBox"/> into a <see cref="VertexBox"/>
	/// </summary>
	/// <param name="heart">The heart to assign to (Default current)</param>

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Vertexify(Player.Heart heart = null)
	{
		Player.Heart curHeart = heart ?? BoxStates.CurrentBox.Detect;
		curHeart.controllingBox.Dispose();
		_ = FightBox.boxes.Remove(curHeart.controllingBox);
		VertexBox box = new(curHeart, BoxStates.CurrentBox as RectangleBox);
		curHeart.controllingBox = box;
		InstanceCreate(box);
	}
	/// <summary>
	/// Converts a <see cref="VertexBox"/> back to a normal <see cref="RectangleBox"/>
	/// </summary>
	/// <param name="area">The area the box takes</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DeVertexify(CollideRect area)
	{
		Player.Heart curHeart = BoxStates.CurrentBox.Detect;
		curHeart.controllingBox.Dispose();
		_ = FightBox.boxes.Remove(curHeart.controllingBox);
		RectangleBox box = new(curHeart, area);
		curHeart.controllingBox = box;
		InstanceCreate(box);
	}
}