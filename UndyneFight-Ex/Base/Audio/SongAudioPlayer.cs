using Microsoft.Xna.Framework.Media;
using static UndyneFight_Ex.AudioUtil;

namespace UndyneFight_Ex;
/// <summary>
/// An audio palyer for songs
/// </summary>
/// <param name="song">The song to play</param>
internal sealed class SongAudioPlayer(Song song) : IAudioSource
{
	/// <summary>
	/// The song that this player plays
	/// </summary>
	private readonly Song song = song;
	/// <summary>
	/// The current position of the song
	/// </summary>
	private TimeSpan position;
	/// <summary>
	/// The list of existing song players
	/// </summary>
	internal static readonly List<SongAudioPlayer> Instances = [];
	/// <summary>
	/// Registers this player to the global song player pool
	/// </summary>
	/// <param name="player"></param>
	internal static void Register(SongAudioPlayer player) => Instances.Add(player);
	public void Start()
	{
		float volume = Settings.SettingsManager.DataLibrary.masterVolume / 100f;
		MediaPlayer.Volume = volume * volume;
		MediaPlayer.Play(song, position);
	}
	/// <summary>
	/// Sets the position of the song in seconds
	/// </summary>
	/// <param name="value">The target position in seconds</param>
	internal void SetPosition(float value)
	{
		value /= 62.5f;
		int seconds = (int)value;
		int milliseconds = (int)((value - seconds) * 1000);
		position = new TimeSpan(0, 0, 0, seconds, milliseconds);
	}
	public TimeSpan GetDuration() => song.Duration;
	public void Resume() => MediaPlayer.Resume();
	public void Pause() => MediaPlayer.Pause();
	public void Stop() => MediaPlayer.Stop();
	public bool IsEnd => MediaPlayer.State == MediaState.Stopped;
	public bool IsPlaying => MediaPlayer.State == MediaState.Playing;
	public float Volume { get => MediaPlayer.Volume; set => MediaPlayer.Volume = value * Settings.SettingsManager.DataLibrary.masterVolume / 100f; }
}
