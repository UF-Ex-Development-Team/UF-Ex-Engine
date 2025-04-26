using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace UndyneFight_Ex
{
    public class Audio
    {
        private interface IAudioSource
        {
            /// <summary>
            /// Plays the audio
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
            /// Whether the audio has ended
            /// </summary>
            bool IsEnd { get; }
            /// <summary>
            /// Whether the audio is playing
            /// </summary>
            bool OnPlay { get; }
            /// <summary>
            /// The volume of the audio
            /// </summary>
            float Volume { set; get; }
            /// <summary>
            /// Resume playing the audio
            /// </summary>
            void Resume();
            /// <summary>
            /// Pauses the playing audio
            /// </summary>
            void Pause();
        }
        private class EffectPlayer(SoundEffect effect) : IAudioSource
        {
            private readonly TimeSpan duration = effect.Duration;
            private readonly SoundEffectInstance effect = effect.CreateInstance();

            public TimeSpan GetDuration() => duration;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start() => effect.Play();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Stop() => effect.Stop();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Resume() => effect.Resume();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Pause() => effect.Pause();

            public bool IsEnd => effect.State == SoundState.Stopped;
            public bool OnPlay => effect.State == SoundState.Playing;

            public float Volume { set => effect.Volume = value; get => effect.Volume; }
        }
        private class DynamicSongPlayer(string path, float? startPos = null, float? endPos = null) : IAudioSource
        {
            private readonly DynamicSong _dynamicSong = new(path, startPos, endPos);
            private readonly List<DynamicSongInstance> allInstances = [];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Update() => allInstances.RemoveAll(s => s.State == SoundState.Stopped);

            public bool IsEnd { get { Update(); return allInstances.Count == 0; } }

            public bool OnPlay { get { Update(); return allInstances.Count > 0; } }

            public float Volume { set => allInstances[0].Volume = value; get => allInstances[0].Volume; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TimeSpan GetDuration() => new(0, 0, 0, 0, 0);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start()
            {
                DynamicSongInstance currentInstance;
                allInstances.Add(currentInstance = _dynamicSong.CreateInstance());
                currentInstance.Play();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Stop()
            {
                allInstances.ForEach(s => s.Stop());
                allInstances.Clear();
            }

            private float lastPosition = 0.0f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal float GetPosition()
            {
                if (allInstances.Count > 0)
                    lastPosition = allInstances[^1].GetPosition();
                return lastPosition;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void SetPosition(float position)
            {
                if (allInstances.Count == 0)
                    return;
                allInstances[0].SetPosition(position / 62.5f * 1000f);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Resume() => allInstances[0].Pause();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Pause() => allInstances[0].Resume();
        }
        private class SongPlayer(Song song) : IAudioSource
        {
            private readonly Song song = song;
            public void Start()
            {
                float x = Settings.SettingsManager.DataLibrary.masterVolume / 100f;
                MediaPlayer.Volume = x * x;
                if (position != TimeSpan.Zero)
                    MediaPlayer.Play(song, position);
                else
                    MediaPlayer.Play(song);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Stop() => MediaPlayer.Stop();
            private TimeSpan position = TimeSpan.Zero;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetPosition(float position)
            {
                position /= 62.5f;
                int sec = (int)position;
                int mil = (int)((position - sec) * 1000);
                this.position = new TimeSpan(0, 0, 0, sec, mil);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TimeSpan GetDuration() => song.Duration;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Resume() => MediaPlayer.Resume();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Pause() => MediaPlayer.Pause();

            public bool IsEnd => MediaPlayer.State == MediaState.Stopped;
            public bool OnPlay => MediaPlayer.State == MediaState.Playing;

            public float Volume { set => MediaPlayer.Volume = value * Settings.SettingsManager.DataLibrary.masterVolume / 100f; get => MediaPlayer.Volume; }
        }
        private readonly IAudioSource source;
        public float Volume { set => source.Volume = value; get => source.Volume; }
        /// <summary>
        /// Loads an audio to memory
        /// </summary>
        /// <param name="path">The path to the audo file directory</param>
        /// <param name="loader">The loader to use</param>
        /// <param name="startPos">The inital position to load (in milliseconds) (WIP)</param>
        /// <param name="endPos">The ending position to load (in milliseconds) (WIP)</param>
        public Audio(string path, ContentManager loader = null, float? startPos = null, float? endPos = null)
        {
            loader ??= Fight.Functions.Loader;
            if (path.EndsWith(".ogg"))
            {
                string finPath = string.IsNullOrEmpty(loader.RootDirectory) ? path : (path.StartsWith("Content") ? path : loader.RootDirectory + "\\" + path);
                source = new DynamicSongPlayer(Path.Combine(finPath.Split('\\')), startPos, endPos);
                return;
            }
            //Ensure no "Content" overlap
            object result = GlobalResources.LoadContent<object>(loader.RootDirectory == "Content" && path[..7] == "Content" ? path[8..] : path);
            if (result is SoundEffect)
                source = new EffectPlayer(result as SoundEffect);
            else if (result is Song)
                source = new SongPlayer(result as Song);
            source.Volume = 1f;
        }
        public Audio(SoundEffect effect) => source = new EffectPlayer(effect) { Volume = 1f };
        public Audio(Song song) => source = new SongPlayer(song) { Volume = 1f };
        /// <summary>
        /// Whether the audio had just started to play
        /// </summary>
        public bool OnPlay => source.OnPlay;
        /// <summary>
        /// The position to start the audio in
        /// </summary>
        public float PlayPosition { private get; set; }
        /// <summary>
        /// The duration of the audio
        /// </summary>
        public TimeSpan SongDuration => source.GetDuration();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Play()
        {
            if (MathF.Abs(PlayPosition) > 0.01f)
            {
                if (source is SongPlayer)
                    (source as SongPlayer).SetPosition(PlayPosition);
            }
            source.Start();
            if (MathF.Abs(PlayPosition) > 0.01f)
            {
                if (source is DynamicSongPlayer)
                    (source as DynamicSongPlayer).SetPosition(PlayPosition);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop() => source.Stop();
        /// <summary>
        /// Whether the audio had ended
        /// </summary>
        public bool IsEnd => source.IsEnd;
        /// <summary>
        /// Sets the position of the audio to the specified position<br/>
        /// Only works for <see cref="DynamicSongPlayer"/>
        /// </summary>
        /// <param name="position">The position to set to</param>
        /// <returns>Whether the audio was successfully set to the specified position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetPosition(float position)
        {
            if (source is DynamicSongPlayer)
            {
                (source as DynamicSongPlayer).SetPosition(position);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Gets the position of the audio<br/>
        /// Only works for <see cref="SongPlayer"/> and <see cref="DynamicSongPlayer"/>
        /// </summary>
        /// <param name="result">Whether the audio position was successfully get</param>
        /// <returns>The current position of the audio</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float TryGetPosition(out bool result)
        {
            if (source is SongPlayer)
            {
                result = true;
                return (float)(MediaPlayer.PlayPosition.TotalMilliseconds * 62.5 / 1000);
            }
            else if (source is DynamicSongPlayer)
            {
                result = true;
                return (source as DynamicSongPlayer).GetPosition() / 1000f * 62.5f;
            }
            else
                result = false;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Resume() => source.Resume();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Pause() => source.Pause();
    }
}