using UndyneFight_Ex.GameInterface;
using UndyneFight_Ex.SongSystem;

namespace UndyneFight_Ex.Entities;

/// <summary>
/// The default game menu scene
/// </summary>
public class GameMenuScene : Scene
{
	/// <inheritdoc/>
	public override void Start() => GameStartUp.MainSceneIntro();
}
/// <summary>
/// Gameover scene
/// </summary>
internal class TryAgainScene : Scene
{
	private int appearTime = 0;
	private float alpha = 1;
	private readonly GameObject FailUI;
	public TryAgainScene(StateShower shower) : this() => FailUI = new StateShower.FailureShower(shower);
	public TryAgainScene(Fight.IClassicFight fight, GameMode mode) : this() => FailUI = new Fight.FailureShower(fight, mode);
	private TryAgainScene() => PlayerManager.CurrentUser?.PlayerStatistic.AddDeath();
	public override void Update()
	{
		alpha = float.Lerp(alpha, 0.2f, 0.16f);
		if (++appearTime == 100)
			InstanceCreate(FailUI);
		base.Update();
	}
	public override void Draw()
	{
		base.Draw();
		GeneralDraw(GameStates.GameoverBackground, new Vector2(320, 240), Color.White * alpha, new Vector2(640, 480) / GameStates.GameoverBackground.Bounds.Size.ToVector2());
	}
}
internal class WinScene(StateShower ss, Player.Analyzer analyzer) : Scene(UFEXSettings.SongCompleteCreate(ss, analyzer)) { }