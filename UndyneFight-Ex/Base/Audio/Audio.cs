using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using static UndyneFight_Ex.AudioUtil;

namespace UndyneFight_Ex;

/// <summary>
/// An audio playback wrapper
/// </summary>
public class Audio
{
	/// <summary>
	/// All instances of <see cref="Audio"/> that have been created
	/// </summary>
	internal static readonly List<Audio> Instances = [];
	/// <summary>
	/// Whether the MediaPlayer was playing before an audio device change
	/// </summary>
	private static bool mediaPlayerWasPlaying;
	/// <summary>
	/// The position of the MediaPlayer before an audio device change
	/// </summary>
	private static TimeSpan mediaPlayerPosition;
#nullable enable
	/// <summary>
	/// The <see cref="Audio"/> instance that owns the MediaPlayer before an audio device change
	/// </summary>
	private static Audio? mediaPlayerOwner;
#nullable disable
	/// <summary>
	/// The underlying audio source for this <see cref="Audio"/> instance
	/// </summary>
	private readonly IAudioSource source;
	/// <summary>
	/// Whether this <see cref="Audio"/> instance was playing before an audio device change
	/// </summary>
	private bool lastWasPlaying;
	/// <summary>
	/// The last known position of this <see cref="Audio"/> instance before an audio device change
	/// </summary>
	private float lastPosition = -1f;
	/// <summary>
	/// Loads an audio from the given path and loader
	/// </summary>
	/// <param name="path">The path of the audio file</param>
	/// <param name="loader">The loader to use (Default <see cref="Fight.Functions.Loader"/>)</param>
	/// <exception cref="InvalidOperationException">Occurs when the audio file is not supported</exception>
	public Audio(string path, ContentManager loader = null)
	{
		_ = loader ?? Fight.Functions.Loader;
		if (path.EndsWith(".ogg"))
		{
			DynamicSongPlayer player = new(GlobalData.GetTruePath(path));
			source = player;
			DynamicSongPlayer.Register(player);
		}
		else
		{
			object result = GlobalData.LoadContent<object>(path);
			source = result switch
			{
				SoundEffect soundEffect => RegisterEffect(soundEffect),
				Song song => RegisterSong(song),
				_ => throw new InvalidOperationException($"Unsupported audio content: {path}")
			};
		}

		source.Volume = 1f;
		Instances.Add(this);
	}
	/// <summary>
	/// Registers a <see cref="SoundEffect"/> with the <see cref="EffectAudioPlayer"/> system
	/// </summary>
	/// <param name="soundEffect">The <see cref="SoundEffect"/> to register</param>
	/// <returns>The registered <see cref="EffectAudioPlayer"/></returns>
	private static EffectAudioPlayer RegisterEffect(SoundEffect soundEffect)
	{
		EffectAudioPlayer player = new(soundEffect);
		EffectAudioPlayer.Register(player);
		return player;
	}
	/// <summary>
	/// Registers a <see cref="Song"/> with the <see cref="SongAudioPlayer"/> system
	/// </summary>
	/// <param name="song">The <see cref="Song"/> to register</param>
	/// <returns>The registered <see cref="SongAudioPlayer"/></returns>
	private static SongAudioPlayer RegisterSong(Song song)
	{
		SongAudioPlayer player = new(song);
		SongAudioPlayer.Register(player);
		return player;
	}
	/// <summary>
	/// The volume of the audio
	/// </summary>
	public float Volume { get => source.Volume; set => source.Volume = value; }
	/// <summary>
	/// Whether the audio is currently playing
	/// </summary>
	public bool IsPlaying => source.IsPlaying;
	/// <summary>
	/// The position to start playing from
	/// </summary>
	public float PlayPosition { private get; set; }
	/// <summary>
	/// The duration of the song
	/// </summary>
	public TimeSpan SongDuration => source.GetDuration();
	/// <summary>
	/// Whether the audio has ended
	/// </summary>
	public bool IsEnd => source.IsEnd;
	/// <summary>
	/// Plays the audio
	/// </summary>
	public void Play()
	{
		if (MathF.Abs(PlayPosition) > 0.01f)
		{
			switch (source)
			{
				case SongAudioPlayer song:
					song.SetPosition(PlayPosition);
					break;
				case DynamicSongPlayer dynamicSong:
					dynamicSong.SetPosition(PlayPosition);
					break;
				default:
					DebugState.Log("Attempted to set audio position on an unsupported audio source type: " + source.GetType().Name, true);
					break;
			}
		}
		source.Start();
	}
	/// <summary>
	/// Stops the audio
	/// </summary>
	public void Stop() => source.Stop();
	/// <summary>
	/// Attempts to set the position of the audio, if supported
	/// </summary>
	/// <param name="position">The target position in frames</param>
	/// <returns>Whether the position was set successfully</returns>
	public bool TrySetPosition(float position)
	{
		if (source is not DynamicSongPlayer dynamicSong)
			return false;
		dynamicSong.SetPosition(position);
		return true;
	}
	/// <summary>
	/// Attempts to get the position of the audio, if supported
	/// </summary>
	/// <param name="result">Whether the position was get successfully</param>
	/// <returns>The position of the audio, -1 if not available</returns>
	public float TryGetPosition(out bool result)
	{
		result = source is SongAudioPlayer or DynamicSongPlayer;
		return source switch
		{
			SongAudioPlayer => (float)(MediaPlayer.PlayPosition.TotalMilliseconds * 62.5 / 1000),
			DynamicSongPlayer dynamicSong => dynamicSong.GetPosition() / 1000f * 62.5f,
			_ => -1
		};
	}
	/// <summary>
	/// Resumes the audio if it is currently paused
	/// </summary>
	internal void Resume() => source.Resume();
	/// <summary>
	/// Pauses the audio if it is currently playing
	/// </summary>
	internal void Pause() => source.Pause();
	/// <summary>
	/// Prepares all <see cref="Audio"/> instances for an audio device change by snapshotting their current state
	/// </summary>
	public static void PrepareForDeviceChangeAll()
	{
		mediaPlayerWasPlaying = MediaPlayer.State == MediaState.Playing;
		mediaPlayerPosition = MediaPlayer.PlayPosition;
		mediaPlayerOwner = null;
		Debug.WriteLine($"Audio.PrepareForDeviceChangeAll: snapshotting {Instances.Count} Audio instances");

		for (int i = 0; i < Instances.Count; i++)
		{
			Audio audio = Instances[i];
			try
			{
				audio.lastWasPlaying = audio.source is SongAudioPlayer ? mediaPlayerWasPlaying && mediaPlayerOwner == null : audio.IsPlaying;
				if (audio.source is SongAudioPlayer && audio.lastWasPlaying)
					mediaPlayerOwner = audio;
				float position = audio.TryGetPosition(out bool available);
				audio.lastPosition = audio.source is SongAudioPlayer && mediaPlayerWasPlaying ? (float)(mediaPlayerPosition.TotalMilliseconds * 62.5 / 1000) : available ? position : -1f;
				Debug.WriteLine($"  [{i}] type={audio.source.GetType().Name} willResume={audio.lastWasPlaying} pos={audio.lastPosition}");
			}
			catch (Exception ex)
			{
				audio.lastWasPlaying = false;
				audio.lastPosition = -1f;
				Debug.WriteLine($"  [{i}] snapshot failed: {ex.Message}");
			}
		}
	}
	/// <summary>
	/// Recreate audio instances and resume playback where appropriate
	/// </summary>
	public static void HandleAudioDeviceChanged()
	{
		foreach (EffectAudioPlayer player in EffectAudioPlayer.Instances)
			Try("recreate effect instance", player.RecreateInstance);
		DynamicSongPlayer.ClearAllInstances();
		if (SongAudioPlayer.Instances.Count > 0)
			Try("stop MediaPlayer", MediaPlayer.Stop);
		if (mediaPlayerWasPlaying && mediaPlayerOwner != null)
		{
			mediaPlayerOwner.PlayPosition = (float)(mediaPlayerPosition.TotalMilliseconds * 62.5 / 1000);
			Try("resume MediaPlayer", mediaPlayerOwner.Play);
		}
		int resumed = 0;
		DebugState.Log($"Audio.HandleAudioDeviceChanged: attempting resume for {Instances.Count} instances", true);
		for (int i = 0; i < Instances.Count; i++)
		{
			Audio audio = Instances[i];
			try
			{
				DebugState.Log($"  [{i}] willResume={audio.lastWasPlaying} pos={audio.lastPosition} type={audio.source.GetType().Name}", true);
				if (audio.lastWasPlaying && audio.source is not SongAudioPlayer)
				{
					if (audio.lastPosition >= 0)
						audio.PlayPosition = audio.lastPosition;
					audio.Play();
					resumed++;
				}
			}
			catch (Exception ex)
			{
				DebugState.Log($"  [{i}] resume failed: {ex.Message}", true);
			}
			finally
			{
				audio.lastWasPlaying = false;
				audio.lastPosition = -1f;
			}
		}
		DebugState.Log($"Audio.HandleAudioDeviceChanged: resumed count={resumed}", true);
		mediaPlayerWasPlaying = false;
		mediaPlayerOwner = null;
	}
}