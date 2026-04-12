namespace UndyneFight_Ex;

internal partial class GameMain : Game
{
	internal static List<Type> fights = [];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public async void LoadObject()
	{
		Scene.PrepareLoader(Content.ServiceProvider);
		GlobalData.Loader = new(Content.ServiceProvider) { RootDirectory = "Content" };

		Task task = new(()=>
		{
			FightSystem.Initialize(fights);
			Settings.SettingsManager.Initialize();
			Surface.Initialize();
		});
		task.RunSynchronously();
		await task;
	}
}