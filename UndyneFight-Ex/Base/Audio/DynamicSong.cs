using Microsoft.Xna.Framework.Audio;
using NVorbis;
using System.Buffers;
using static UndyneFight_Ex.AudioUtil;

namespace UndyneFight_Ex;
/// <summary>
/// A song that is dynamically loaded
/// </summary>
internal sealed class DynamicSong
{
	private float bytesOverMilliseconds;
	private byte[] byteArray = [];
	private int count;
	private int loopLengthBytes;
	private int loopEndBytes;
	private long loopLengthSamples;
	private long loopEndSamples;
	private int channels;
	private int sampleRate;
	private const int bufferDuration = 100;
	/// <summary>
	/// Loads the OGG file with the given path
	/// </summary>
	/// <param name="path">The path of the OGG file</param>
	public DynamicSong(string path) => ReadOgg(path);
	/// <summary>
	/// Creates an instance of the dynamic song
	/// </summary>
	/// <returns>The dynamic song instance</returns>
	public DynamicSongInstance CreateInstance()
	{
		DynamicSoundEffectInstance dynamicSound = new(sampleRate, (AudioChannels)channels);
		count = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(bufferDuration)) + 4);
		loopLengthBytes = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)loopLengthSamples / sampleRate)));
		loopEndBytes = dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)loopEndSamples / sampleRate));
		return new(dynamicSound, byteArray, count, loopLengthBytes, loopEndBytes, bytesOverMilliseconds);
	}
	/// <summary>
	/// Aligns an integer to the enarest 8 bytes
	/// </summary>
	/// <param name="value">The integer to align</param>
	/// <returns>The aligned integer</returns>
	private static int AlignTo8Bytes(int value)
	{
		int result = value + 4;
		return result - result % 8;
	}
	/// <summary>
	/// Reads the OGG file and converts it to a byte array
	/// </summary>
	/// <param name="path">The path of the OGG file</param>
	private async void ReadOgg(string path)
	{
		using VorbisReader vorbis = new(path);
		channels = vorbis.Channels;
		sampleRate = vorbis.SampleRate;
		float duration = (float)vorbis.TotalTime.TotalSeconds;
		float[] buffer = new float[Math.Max(1024, channels * sampleRate / 5)];
		int estimatedBytes = Math.Max(4096, (int)Math.Ceiling(vorbis.TotalTime.TotalSeconds * sampleRate * channels * 2.0));
		byte[] rented = ArrayPool<byte>.Shared.Rent(estimatedBytes);
		int writePos = 0;
		await Task.Run(() =>
		{
			int read;
			while ((read = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
			{
				for (int i = 0; i < read; i++)
				{
					short sample = (short)(32767f * buffer[i]);
					if (writePos + 2 > rented.Length)
					{
						byte[] expanded = ArrayPool<byte>.Shared.Rent(Math.Max(writePos + 2, rented.Length * 2));
						Buffer.BlockCopy(rented, 0, expanded, 0, writePos);
						ArrayPool<byte>.Shared.Return(rented);
						rented = expanded;
					}
					rented[writePos++] = (byte)sample;
					rented[writePos++] = (byte)(sample >> 8);
				}
			}
		});
		byteArray = new byte[writePos];
		Array.Copy(rented, byteArray, writePos);
		ArrayPool<byte>.Shared.Return(rented);
		bytesOverMilliseconds = byteArray.Length / (float)vorbis.TotalTime.TotalMilliseconds;
		_ = long.TryParse(vorbis.Tags.All.FirstOrDefault(c => c.Key.Contains("LOOPLENGTH")).Key?.Split("LOOPLENGTH=")[1], out loopLengthSamples);
		_ = long.TryParse(vorbis.Tags.All.FirstOrDefault(c => c.Key.Contains("LOOPEND")).Key?.Split("LOOPEND=")[1], out loopEndSamples);
		if (loopEndSamples == 0 && loopLengthSamples != 0)
			loopEndSamples = (long)(duration * sampleRate);
		if (loopLengthSamples == 0 && loopEndSamples != 0)
			loopLengthSamples = loopEndSamples;
	}
}
/// <summary>
/// An instance of a dynamic song
/// </summary>
internal sealed class DynamicSongInstance
{
	/// <summary>
	/// The dynamic sound effect instance
	/// </summary>
	private DynamicSoundEffectInstance dynamicSound;
	private readonly byte[] byteArray;
	private int position;
	private readonly int count;
	private readonly int loopLengthBytes;
	private readonly int loopEndBytes;
	private readonly float bytesOverMilliseconds;
	private bool enabled = true;
	/// <summary>
	/// Creates a new instance of a dynamic song
	/// </summary>
	/// <param name="dynamicSound">The dynamic sound effect instance</param>
	/// <param name="byteArray">The byte array containing the audio data</param>
	/// <param name="count">The number of bytes to submit</param>
	/// <param name="loopLengthBytes">The length of the loop in bytes</param>
	/// <param name="loopEndBytes">The end position of the loop in bytes</param>
	/// <param name="bytesOverMilliseconds">The number of bytes per millisecond</param>
	public DynamicSongInstance(DynamicSoundEffectInstance dynamicSound, byte[] byteArray, int count, int loopLengthBytes, int loopEndBytes, float bytesOverMilliseconds)
	{
		this.dynamicSound = dynamicSound;
		this.byteArray = byteArray;
		this.count = count;
		this.loopLengthBytes = loopLengthBytes;
		this.loopEndBytes = loopEndBytes;
		this.bytesOverMilliseconds = bytesOverMilliseconds;
		dynamicSound.BufferNeeded += UpdateBuffer;
	}
	/// <summary>
	/// The volume of the dynamic song instance, clamped between 0 and 1
	/// </summary>
	public float Volume { get => dynamicSound.Volume; set => dynamicSound.Volume = Math.Clamp(value, 0, 1); }
	/// <summary>
	/// The pitch of the dynamic song instance, clamped between -1 and 1
	/// </summary>
	public float Pitch { get => dynamicSound.Pitch; set => dynamicSound.Pitch = Math.Clamp(value, -1, 1); }
	/// <summary>
	/// The state of the dynamic song instance
	/// </summary>
	public SoundState State => dynamicSound is null || dynamicSound.IsDisposed ? SoundState.Stopped : dynamicSound.State;
	/// <summary>
	/// Whether the dynamic song instance should loop
	/// </summary>
	public bool IsLoop { get; set; }
	/// <summary>
	/// The length of the dynamic song instance in seconds
	/// </summary>
	public float Length => byteArray.Length / bytesOverMilliseconds * 0.001f;
	/// <summary>
	/// Plays the dynamic song instance
	/// </summary>
	public void Play() { dynamicSound.Pitch = 0; dynamicSound.Play(); }
	/// <summary>
	/// Pauses the dynamic song instance
	/// </summary>
	public void Pause() => dynamicSound?.Pause();
	/// <summary>
	/// Resumes the dynamic song instance
	/// </summary>
	public void Resume() => dynamicSound?.Resume();
	/// <summary>
	/// Stops the dynamic song instance and disposes of the dynamic sound effect instance
	/// </summary>
	public void Stop()
	{
		if (dynamicSound is null)
			return;

		Try("stop dynamic song", dynamicSound.Stop);
		Try("dispose dynamic song", dynamicSound.Dispose);
		dynamicSound = null;
	}
	/// <summary>
	/// Sets the position of the dynamic song instance in milliseconds
	/// </summary>
	/// <param name="milliseconds">The position in milliseconds</param>
	public void SetPosition(float milliseconds) { position = (int)float.Floor(milliseconds * bytesOverMilliseconds); while (position % 8 != 0) position--; }
	/// <summary>
	/// Gets the position of the dynamic song instance in milliseconds
	/// </summary>
	/// <returns>The current position in milliseconds</returns>
	public float GetPosition() => position / bytesOverMilliseconds;
	/// <summary>
	/// Updates the buffer of the dynamic sound effect instance
	/// </summary>
	/// <param name="sender">The sender of the event</param>
	/// <param name="e">The event arguments</param>
	private void UpdateBuffer(object sender, EventArgs e)
	{
		if (!enabled)
		{
			dynamicSound.Stop();
			return;
		}
		try
		{
			position = int.Min(position, (byteArray?.Length ?? position + 1) - 1);
			dynamicSound.SubmitBuffer(byteArray, position, count / 2);
			dynamicSound.SubmitBuffer(byteArray, position + count / 2, count / 2);
			position += count;
		}
		catch { }
		if (loopEndBytes > 0 && loopLengthBytes > 0 && position + count >= loopEndBytes)
			position -= loopLengthBytes;
		if (position + count > (byteArray?.Length ?? position + 1))
		{
			if (IsLoop)
				position = 0;
			else
				enabled = false;
		}
	}
}
/// <summary>
/// A dynamic song player
/// </summary>
/// <param name="path"></param>
internal sealed class DynamicSongPlayer(string path) : IAudioSource
{
	/// <summary>
	/// The dynamic song that this player plays
	/// </summary>
	private readonly DynamicSong dynamicSong = new(path);
	/// <summary>
	/// All dynamic song instances that this player has created
	/// </summary>
	private readonly List<DynamicSongInstance> allInstances = [];
	/// <summary>
	/// The last position of the dynamic song player in milliseconds (If available)
	/// </summary>
	private float lastPosition;
	/// <summary>
	/// The list of existing dynamic song players
	/// </summary>
	internal static readonly List<DynamicSongPlayer> Instances = [];
	/// <summary>
	/// Registers a dynamic song player to the global pool
	/// </summary>
	/// <param name="player"></param>
	internal static void Register(DynamicSongPlayer player) => Instances.Add(player);
	/// <summary>
	/// Clears all dynamic song instances from all players
	/// </summary>
	internal static void ClearAllInstances()
	{
		foreach (DynamicSongPlayer player in Instances)
		{
			Try("clear dynamic song instances", () =>
			{
				foreach (DynamicSongInstance instance in player.allInstances)
					Try("stop dynamic song instance", instance.Stop);
				player.allInstances.Clear();
			});
		}
	}
	/// <summary>
	/// Remove all stopped instances
	/// </summary>
	private void RemoveStoppedInstances() => allInstances.RemoveAll(instance => instance.State == SoundState.Stopped);
	public bool IsEnd
	{
		get
		{
			RemoveStoppedInstances();
			return allInstances.Count == 0;
		}
	}
	public bool IsPlaying
	{
		get
		{
			RemoveStoppedInstances();
			return allInstances.Count > 0;
		}
	}
	public float Volume
	{
		get => allInstances.Count == 0 ? 0 : allInstances[0].Volume;
		set
		{
			if (allInstances.Count > 0)
				allInstances[0].Volume = value;
		}
	}
	public TimeSpan GetDuration() => TimeSpan.Zero;
	public void Start()
	{
		DynamicSongInstance instance = dynamicSong.CreateInstance();
		allInstances.Add(instance);
		instance.Play();
	}
	public void Stop()
	{
		foreach (DynamicSongInstance instance in allInstances)
			Try("stop dynamic song", instance.Stop);
		allInstances.Clear();
	}
	internal float GetPosition()
	{
		if (allInstances.Count > 0)
			lastPosition = allInstances[^1].GetPosition();
		return lastPosition;
	}
	internal void SetPosition(float position)
	{
		if (allInstances.Count > 0)
			allInstances[0].SetPosition(position / 62.5f * 1000f);
	}
	public void Resume()
	{
		if (allInstances.Count > 0)
			allInstances[0].Resume();
	}
	public void Pause()
	{
		if (allInstances.Count > 0)
			allInstances[0].Pause();
	}
}
