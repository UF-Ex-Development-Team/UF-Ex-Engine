namespace UndyneFight_Ex.Entities;
/// <summary>
/// Extra movement modes for souls
/// </summary>
public partial class Souls
{
	/// <summary>
	/// Yellow soul processing logic
	/// </summary>
	public static Player.MoveState YellowSoul { get; private set; } = new(col.Yellow, (s) =>
	{
		SoulMove(s);
		if (Buffer > 0)
			Buffer--;
		if (GameStates.IsKeyPressed120f(InputIdentity.Confirm) && Buffer == 0)
		{
			Buffer = 15;
			GameStates.InstanceCreate(new SoulBullet(s));
		}
	});
	private static int Buffer { get; set; } = 15;
}