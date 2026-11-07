using Microsoft.Xna.Framework.Audio;
using static UndyneFight_Ex.AudioUtil;

namespace UndyneFight_Ex;
/// <summary>
/// An audio player for sound effects
/// </summary>
/// <param name="effect">The sound effect to play</param>
internal sealed class EffectAudioPlayer(SoundEffect effect) : IAudioSource
{
	/// <summary>
	/// The duration of the sound effect
	/// </summary>
	private readonly TimeSpan duration = effect.Duration;
	/// <summary>
	/// The sound effect of the player
	/// </summary>
	private readonly SoundEffect soundEffect = effect;
	/// <summary>
	/// The sound effect instance of the player
	/// </summary>
	private SoundEffectInstance effectInstance = effect.CreateInstance();
	/// <summary>
	/// The list of existing effect players
	/// </summary>
	internal static readonly List<EffectAudioPlayer> Instances = [];
	/// <summary>
	/// Registers this player to the global effect player pool
	/// </summary>
	/// <param name="player"></param>
	internal static void Register(EffectAudioPlayer player) => Instances.Add(player);
	public TimeSpan GetDuration() => duration;
	public void Start() => effectInstance.Play();
	public void Stop() => effectInstance.Stop();
	public void Resume() => effectInstance.Resume();
	public void Pause() => effectInstance.Pause();
	public bool IsEnd => effectInstance.State == SoundState.Stopped;
	public bool IsPlaying => effectInstance.State == SoundState.Playing;
	public float Volume { get => effectInstance.Volume; set => effectInstance.Volume = value; }
	/// <summary>
	/// Recreates the effect instance for audio device reload
	/// </summary>
	internal void RecreateInstance() => Try("recreate effect instance", () =>
											 {
												 float volume = effectInstance.Volume;
												 Try("stop effect instance", effectInstance.Stop);
												 Try("dispose effect instance", effectInstance.Dispose);
												 effectInstance = soundEffect.CreateInstance();
												 effectInstance.Volume = volume;
											 });
}
