namespace UndyneFight_Ex
{
	public class GameEventArgs(GameObject gameObject, string info) : EventArgs
	{
		public string ActionName { get; set; } = info;
		public GameObject Source { get; set; } = gameObject;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose() => Disposed = true;
		public bool Disposed { get; private set; }
	}
}