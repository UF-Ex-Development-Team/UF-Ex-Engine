namespace UndyneFight_Ex.Server
{
	public class AliveData(int timeAliveSeconds = 600)
	{
		private readonly int timeAliveSeconds = timeAliveSeconds;
		private DateTime _lastRefreshTime = DateTime.Now;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Refresh() => _lastRefreshTime = DateTime.Now;
	}

	public class User : AliveData
	{
		public long UUID { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
