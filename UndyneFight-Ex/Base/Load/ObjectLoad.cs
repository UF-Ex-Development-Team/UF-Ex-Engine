namespace UndyneFight_Ex;

internal partial class GameMain : Game
{
	internal static List<Type> FightList = [];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public async void LoadObject()
	{
		Scene.PrepareLoader(Content.ServiceProvider);
		GlobalData.Loader = new(Content.ServiceProvider) { RootDirectory = "Content" };

		Task task = new(()=>
		{
			FightSystem.Initialize(FightList);
			Settings.SettingsManager.Initialize();
			Surface.Initialize();
		});
		task.RunSynchronously();
		await task;
	}
}