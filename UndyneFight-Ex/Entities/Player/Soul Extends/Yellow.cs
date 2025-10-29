namespace UndyneFight_Ex.Entities;
/// <summary>
/// Extra movement modes for souls
/// </summary>
public partial class Souls
{
	/// <summary>
	/// Yellow soul processing logic
	/// </summary>
	public static Player.MoveState YellowSoul { get; private set; } = new(Color.Yellow, (s) =>
	{
		SoulMove(s);
		if (GameStates.IsKeyPressed120f(InputIdentity.Confirm))
			GameStates.InstanceCreate(new SoulBullet(s));
	});
}