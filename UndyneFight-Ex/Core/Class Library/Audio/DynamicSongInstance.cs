using Microsoft.Xna.Framework.Audio;

namespace UndyneFight_Ex
{
	public class DynamicSongInstance
	{
		public float Volume
		{
			get => dynamicSound.Volume;
			set => dynamicSound.Volume = Math.Clamp(value, 0, 1);
		}

		public float Pitch
		{
			get => dynamicSound.Pitch;
			set => dynamicSound.Pitch = Math.Clamp(value, -1, 1);
		}

		// Private

		private DynamicSoundEffectInstance dynamicSound;
		private readonly byte[] byteArray;
		private int position;
		private readonly int count;
		private readonly int loopLengthBytes;
		private readonly int loopEndBytes;
		private readonly float bytesOverMilliseconds;

		// Methods

		// Public

		public DynamicSongInstance(DynamicSoundEffectInstance dynamicSound, byte[] byteArray, int count, int loopLengthBytes, int loopEndBytes, float bytesOverMilliseconds)
		{
			this.dynamicSound = dynamicSound;
			this.byteArray = byteArray;
			this.count = count;
			this.loopLengthBytes = loopLengthBytes;
			this.loopEndBytes = loopEndBytes;
			this.bytesOverMilliseconds = bytesOverMilliseconds;

			this.dynamicSound.BufferNeeded += new EventHandler<EventArgs>(UpdateBuffer);
		}

		public SoundState State => dynamicSound.IsDisposed ? SoundState.Stopped : dynamicSound.State;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Play()
		{
			dynamicSound.Pitch = 0;
			dynamicSound.Play();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Pause() => dynamicSound?.Pause();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Stop()
		{
			dynamicSound?.Stop();
			dynamicSound = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPosition(float milliseconds)
		{
			position = (int)Math.Floor(milliseconds * bytesOverMilliseconds);
			while (position % 8 != 0)
				position -= 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetPosition() => position / bytesOverMilliseconds;

		// Private

		private void UpdateBuffer(object sender, EventArgs e)
		{
			if (!_enabled)
			{
				dynamicSound.Stop();
				dynamicSound.Dispose();
				return;
			}
			dynamicSound.SubmitBuffer(byteArray, position, count / 2);
			dynamicSound.SubmitBuffer(byteArray, position + count / 2, count / 2);
			position += count;

			if ((loopEndBytes > 0) && (loopLengthBytes > 0) && (position + count >= loopEndBytes))
			{
				position -= loopLengthBytes;
			}

			if (position + count > byteArray.Length)
			{
				if (IsLoop)
					position = 0;
				else
					_enabled = false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Resume() => dynamicSound.Resume();

		private bool _enabled = true;
		public bool IsLoop { get; set; } = false;
		/// <summary>
		/// Duration of the song
		/// </summary>
		public float Length => byteArray.Length / bytesOverMilliseconds * 0.001f;
	}
}