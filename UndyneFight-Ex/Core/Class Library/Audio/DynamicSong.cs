using Microsoft.Xna.Framework.Audio;
using NVorbis;

namespace UndyneFight_Ex
{
	public class DynamicSong
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

		private int chunkId;
		private int fileSize;
		private int riffType;
		private int fmtId;
		private int fmtSize;
		private int fmtCode;

		private int channels;
		private int sampleRate;

		private int fmtAvgBps;
		private int fmtBlockAlign;
		private int bitDepth;

		private int fmtExtraSize;

		private int dataID;
		private int dataSize;

		private const int bufferDuration = 100;

		// Private

		public DynamicSong(string path, float? startPos = null, float? endPos = null)
		{
			ReadOgg(path, startPos, endPos);
			Name = path.Split("/").Last();
			Name = Name.Split(".")[0];
		}

		public DynamicSongInstance CreateInstance()
		{
			DynamicSoundEffectInstance dynamicSound = new(sampleRate, (AudioChannels)channels);

			count = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(bufferDuration)) + 4);
			loopLengthBytes = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)loopLengthSamples / sampleRate)));
			loopEndBytes = dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)loopEndSamples / sampleRate)); // doesn't need alignment

			return new DynamicSongInstance(dynamicSound, byteArray, count, loopLengthBytes, loopEndBytes, bytesOverMilliseconds);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int AlignTo8Bytes(int unalignedBytes)
		{
			int result = unalignedBytes + 4;
			result -= result % 8;
			return result;
		}

		private void ReadOgg(string path, float? startPos = null, float? endPos = null)
		{
			VorbisReader vorbis = new(path);
			channels = vorbis.Channels;
			sampleRate = vorbis.SampleRate;
			startPos ??= 0;
			endPos ??= (float)vorbis.TotalTime.TotalMilliseconds;
			duration = (float)(endPos - startPos);

			float[] buffer = new float[channels * sampleRate / 5];

			List<byte> byteList = [];
			int count;
			while ((count = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
			{
				for (int i = 0; i < count; i++)
				{
					short temp = (short)(32767f * buffer[i]);
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
				{
					loopEndSamples = (long)duration * sampleRate / 1000;
				}

				if (loopLengthSamples == 0)
				{
					loopLengthSamples = loopEndSamples - loopStartSamples;
				}
			}
		}

		private void ReadWav(string path, string absolutePath)
		{
			byte[] allBytes = File.ReadAllBytes(absolutePath);
			int byterate = BitConverter.ToInt32([allBytes[28], allBytes[29], allBytes[30], allBytes[31]], 0);
			duration = (int)Math.Floor((allBytes.Length - 8) / (float)byterate * 1000);

			Stream waveFileStream = TitleContainer.OpenStream(path);
			BinaryReader reader = new(waveFileStream);

			chunkId = reader.ReadInt32();
			fileSize = reader.ReadInt32();
			riffType = reader.ReadInt32();
			fmtId = reader.ReadInt32();
			fmtSize = reader.ReadInt32();
			fmtCode = reader.ReadInt16();

			channels = reader.ReadInt16();
			sampleRate = reader.ReadInt32();

			fmtAvgBps = reader.ReadInt32();
			fmtBlockAlign = reader.ReadInt16();
			bitDepth = reader.ReadInt16();

			if (fmtSize == 18)
			{
				// Read any extra values
				fmtExtraSize = reader.ReadInt16();
				_ = reader.ReadBytes(fmtExtraSize);
			}

			dataID = reader.ReadInt32();
			dataSize = reader.ReadInt32();

			byteArray = reader.ReadBytes(dataSize);
			bytesOverMilliseconds = byteArray.Length / duration;

			// Load metainfo, or specifically, TXXX "LOOP_____" tags

			_ = new char[4];
			int sectionSize;
			long sectionBasePosition;
			_ = new char[4];
			int localSectionSize;

			bool isData;
			char inChar;
			string tagTitle;
			string tagData;

			while (waveFileStream.Position < waveFileStream.Length - 10) // -10s are to prevent overrunning the end of the file when a partial header or filler bytes are present
			{
				char[] sectionHeader = reader.ReadChars(4);
				sectionSize = reader.ReadInt32();
				sectionBasePosition = waveFileStream.Position;

				if (new string(sectionHeader) != "id3 ")
				{
					waveFileStream.Position += sectionSize;
					continue;
				}

				waveFileStream.Position += 10; // skip the header

				while ((waveFileStream.Position < sectionBasePosition + sectionSize - 10) && (waveFileStream.Position < waveFileStream.Length))
				{
					char[] localSectionHeader = reader.ReadChars(4);
					localSectionSize = 0;
					// need to read this as big-endian
					for (int i = 0; i < 4; i++)
					{
						localSectionSize = (localSectionSize << 8) + reader.ReadByte();
					}
					_ = reader.ReadInt16(); // probably also needs endian swap... if we were paying attention to it, which we don't need to

					if (new string(localSectionHeader) != "TXXX")
					{
						waveFileStream.Position += localSectionSize;
						continue;
					}

					isData = false;
					tagTitle = "";
					tagData = "";

					_ = reader.ReadByte(); // text encoding byte, we're gonna just ignore this

					for (int i = 0; i < localSectionSize - 1; i++) // -1 due to aforementioned ignored byte
					{
						inChar = reader.ReadChar();
						if (isData)
						{
							tagData += inChar;
						}
						else if (inChar == '\x00')
						{
							isData = true;
						}
						else
						{
							tagTitle += inChar;
						}
					}

					// Process specific tag types we're looking for. If you want to use this for general tag-reading, you'll need to implement that yourself,
					// keeping in mind this code has also filtered for TXXX records only.

					switch (tagTitle)
					{
						case "LOOPSTART":
							_ = long.TryParse(tagData, out loopStartSamples);
							break;
						case "LOOPLENGTH":
							_ = long.TryParse(tagData, out loopLengthSamples);
							break;
						case "LOOPEND":
							_ = long.TryParse(tagData, out loopEndSamples);
							break;
					}
				}

				if (loopEndSamples == 0)
				{
					loopEndSamples = (long)duration * sampleRate / 1000;
				}

				if (loopLengthSamples == 0)
				{
					loopLengthSamples = loopEndSamples - loopStartSamples;
				}
			}
		}
	}
}