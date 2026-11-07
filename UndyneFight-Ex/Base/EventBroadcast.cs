namespace UndyneFight_Ex;
/// <summary>
/// An event to be broadcasted that can be detected
/// </summary>
/// <param name="gameObject">The object to broadcast from</param>
/// <param name="info">The name of the event</param>
public class GameEventArgs(GameObject gameObject, string info) : EventArgs
{
	/// <summary>
	/// The name of the event
	/// </summary>
	public string ActionName { get; set; } = info;
	/// <summary>
	/// The object that broadcasted the event
	/// </summary>
	public GameObject Source { get; set; } = gameObject;
	/// <summary>
	/// Disposes the event
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose() => Disposed = true;
	internal bool Disposed { get; private set; }
}