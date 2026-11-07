using Microsoft.Xna.Framework.Audio;
using OpenTK.Audio.OpenAL;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using static SDL3.SDL;

namespace UndyneFight_Ex;

public static class AudioUtil
{
	private static ALDevice _audioDevice;
	private static ALContext _audioContext;

	public static string? CurrentOpenALDeviceName { get; private set; }
	internal static void Try(string operation, Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			DebugState.Log($"Audio.{operation} failed: {ex.Message}");
		}
	}

	internal static T? Try<T>(string operation, Func<T> action)
	{
		try
		{
			return action();
		}
		catch (Exception ex)
		{
			DebugState.Log($"Audio.{operation} failed: {ex.Message}");
			return default;
		}
	}
#if NET10_0_OR_GREATER
	public static int CurrentDevice
	{
		get; set
		{
			List<(uint Id, string Name)> devices = GetPlaybackDevices();
			if (devices.Count == 0)
				return;

			field = MathUtil.Posmod(value, devices.Count);
			string deviceName = devices[field].Name;
			ChangePlaybackDevice(deviceName);
			DebugState.Log($"Changing audio device to {deviceName}.");
		}
	} = 0;
#else
	public static int CurrentDevice
	{
		get => currentDevice; set
		{
			List<(uint Id, string Name)> devices = GetPlaybackDevices();
			if (devices.Count == 0)
				return;

			currentDevice = MathUtil.Posmod(value, devices.Count);
			string deviceName = devices[currentDevice].Name;
			ChangePlaybackDevice(deviceName);
			DebugState.Log($"Changing audio device to {deviceName}.");
		}
	}
	private static int currentDevice = 0;
#endif
	public static List<(uint Id, string Name)> CurrentPlaybackDevices { get; private set; } = [];

	public static List<(uint Id, string Name)> GetPlaybackDevices()
	{
		List<(uint Id, string Name)> devices = Try("enumerate playback devices", EnumeratePlaybackDevices) ?? [];
		return CurrentPlaybackDevices = devices;
	}

	private static unsafe List<(uint Id, string Name)> EnumeratePlaybackDevices()
	{
		List<(uint Id, string Name)> devices = [];
		if (!SDL_InitSubSystem(SDL_InitFlags.SDL_INIT_AUDIO))
			return devices;

		try
		{
			uint* deviceIds = (uint*)SDL_GetAudioPlaybackDevices(out int count);
			if (deviceIds == null)
				return devices;

			for (int i = 0; i < count; i++)
			{
				uint id = deviceIds[i];
				string name = SDL_GetAudioDeviceName(id);
				if (!string.IsNullOrWhiteSpace(name))
					devices.Add((id, name));
			}

			SDL_free((nint)deviceIds);
			devices.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
			return devices;
		}
		finally
		{
			SDL_QuitSubSystem(SDL_InitFlags.SDL_INIT_AUDIO);
		}
	}
	public static void ChangePlaybackDevice(string deviceName)
	{
		if (string.IsNullOrWhiteSpace(deviceName))
			return;

		CurrentOpenALDeviceName = deviceName;
		RecreateAudioDevice();
	}
	public static void RecreateAudioDevice()
	{
		Try("prepare for device change", Audio.PrepareForDeviceChangeAll);
		Try("recreate OpenAL device", RecreateOpenALDevice);
		Try("restore audio instances", Audio.HandleAudioDeviceChanged);
	}
	private static void RecreateOpenALDevice()
	{
		if (_audioContext != ALContext.Null)
		{
			ALC.MakeContextCurrent(ALContext.Null);
			ALC.DestroyContext(_audioContext);
			_audioContext = ALContext.Null;
		}
		if (_audioDevice != ALDevice.Null)
		{
			ALC.CloseDevice(_audioDevice);
			_audioDevice = ALDevice.Null;
		}

		_audioDevice = ALC.OpenDevice(CurrentOpenALDeviceName);
		if (_audioDevice == ALDevice.Null && !string.IsNullOrWhiteSpace(CurrentOpenALDeviceName))
			_audioDevice = ALC.OpenDevice(null);
		if (_audioDevice == ALDevice.Null)
			throw new InvalidOperationException("OpenAL playback device could not be opened.");

		_audioContext = ALC.CreateContext(_audioDevice, [0]);
		if (_audioContext == ALContext.Null || !ALC.MakeContextCurrent(_audioContext))
			throw new InvalidOperationException("OpenAL playback context could not be created.");
		SoundEffect.Initialize();
	}
	[RegularExpression(@"\(([^)]*)\)")]
	public static string CurrentAudioDeviceName
	{
		get
		{
			int tryCount = 0;
			while (CurrentPlaybackDevices.Count == 0)
			{
				GetPlaybackDevices();
				if (tryCount++ > 5)
					return string.Empty;
			}
			return CurrentPlaybackDevices[CurrentDevice].Name;
		}
	}
	internal interface IAudioSource
	{
		/// <summary>
		/// Starts playing the audio
		/// </summary>
		void Start();
		/// <summary>
		/// Stops the audio
		/// </summary>
		void Stop();
		/// <summary>
		/// Gets the duration of the audio
		/// </summary>
		/// <returns>The duration of the audio</returns>
		TimeSpan GetDuration();
		/// <summary>
		/// Whether the audio is stopped
		/// </summary>
		bool IsEnd { get; }
		/// <summary>
		/// Whether the audio is playing
		/// </summary>
		bool IsPlaying { get; }
		/// <summary>
		/// The volume of the audio, scaled by the master volume
		/// </summary>
		float Volume { get; set; }
		/// <summary>
		/// Resumes the audio if it is paused
		/// </summary>
		void Resume();
		/// <summary>
		/// Pauses the audio if it is playing
		/// </summary>
		void Pause();
	}
	/// <summary>
	/// Play a sound effect, you should play multiple sounds for volume that is larger than 1
	/// </summary>
	/// <param name="effect">The sfx to play</param>
	/// <param name="soundVolume">The volume of the sound, range: [0, 1]</param>
	/// <param name="pitch">The pitch of the sound, range: [-1, 1]</param>
	public static void PlaySound(SoundEffect effect, float soundVolume = 1, float pitch = 0)
	{
		if (effect is null)
		{
			Debug.WriteLine("Sound is not loaded");
			return;
		}
		if (soundVolume > 1)
		{
			int count = (int)soundVolume;
			float rem = soundVolume - count;
			for (int i = 0; i < count; i++)
				PlaySound(effect, 1f);
			if (rem > 0)
				PlaySound(effect, rem);
			return;
		}
		float trueVal = soundVolume * soundVolume;
		if (effect != FightResources.Sounds.ArrowStuck)
			trueVal *= Settings.SettingsManager.DataLibrary.SFXVolume / 100f;
		SoundEffectInstance v = effect.CreateInstance();
		v.Volume = trueVal;
		v.Pitch = pitch;
		v.Play();
	}
	/// <summary>
	/// Play multiple sound effects
	/// </summary>
	/// <param name="effect"></param>
	public static void PlaySound(params SoundEffect[] effect) => Array.ForEach(effect, (s) => PlaySound(s));
}