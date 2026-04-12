using Microsoft.Xna.Framework.Audio;
using NVorbis;
using System.Diagnostics;

namespace UndyneFight_Ex;

internal class DynamicSong
{
	public string Name;

	private float duration;
	private float bytesOverMilliseconds;

	private byte[] byteArray;
	private int count;
	private int loopLengthBytes;
	private int loopEndBytes;

	private long loopStartSamples = 0;
	private long loopLengthSamples = 0;
	private long loopEndSamples = 0;

	private int channels;
	private int sampleRate;

	private const int bufferDuration = 100;

	// Private

	public DynamicSong(string path)
	{
		ReadOgg(path);
		Name = path.Split("/").Last().Split(".")[0];
	}

	public DynamicSongInstance CreateInstance()
	{
		DynamicSoundEffectInstance dynamicSound = new(sampleRate, (AudioChannels)channels);

		count = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(bufferDuration)) + 4);
		loopLengthBytes = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)loopLengthSamples / sampleRate)));
		loopEndBytes = dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)loopEndSamples / sampleRate)); // doesn't need alignment

		return new(dynamicSound, byteArray, count, loopLengthBytes, loopEndBytes, bytesOverMilliseconds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int AlignTo8Bytes(int unalignedBytes)
	{
		int result = unalignedBytes + 4;
		return result - result % 8;
	}

	private async void ReadOgg(string path)
	{
		using VorbisReader vorbis = new(path);
		channels = vorbis.Channels;
		sampleRate = vorbis.SampleRate;
		duration = (float)vorbis.TotalTime.TotalSeconds;

		float[] buffer = new float[channels * sampleRate / 5];

		List<byte> byteList = [];
		Task task = new(()=>
		{
			while (vorbis.ReadSamples(buffer, 0, buffer.Length) > 0)
			{
				foreach (float item in buffer)
				{
					short temp = (short)(32767f * item);
					if (temp > 32767)
					{
						byteList.Add(0xFF);
						byteList.Add(0x7F);
					}
					else if (temp < -32768)
					{
						byteList.Add(0x80);
						byteList.Add(0x00);
					}
					byteList.Add((byte)temp);
					byteList.Add((byte)(temp >> 8));
				}
			}
		});
		task.RunSynchronously();
		await task;
		byteArray = [.. byteList];
		bytesOverMilliseconds = byteArray.Length / (float)vorbis.TotalTime.TotalMilliseconds;

		_ = long.TryParse(
			vorbis.Tags.All.FirstOrDefault(c => c.Key.Contains("LOOPSTART")).Key?.Split("LOOPSTART=")[1],
			out loopStartSamples
		);

		_ = long.TryParse(
			vorbis.Tags.All.FirstOrDefault(c => c.Key.Contains("LOOPLENGTH")).Key?.Split("LOOPLENGTH=")[1],
			out loopLengthSamples
		);

		_ = long.TryParse(
			vorbis.Tags.All.FirstOrDefault(c => c.Key.Contains("LOOPEND")).Key?.Split("LOOPEND=")[1],
			out loopEndSamples
		);

		if (loopStartSamples != 0)
		{
			if (loopEndSamples == 0)
				loopEndSamples = (long)duration * sampleRate / 1000;
			if (loopLengthSamples == 0)
				loopLengthSamples = loopEndSamples - loopStartSamples;
		}
	}
}