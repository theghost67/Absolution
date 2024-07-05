using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий функции для проигрывания звуковых эффектов.
    /// </summary>
    public sealed class SFX : MonoBehaviour
    {
        const int SOUNDS_SOURCE_INDEX = 2; // other indexes are for music
        const float MUSIC_SWITCH_DURATION = 2.0f;
        const float MUSIC_GLOBAL_SCALE = 0.2f;

        public static float MusicPitchScale
        {
            get => _musicVolumeScale;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("value");
                _musicPitchScale = value;
                foreach (MusicSource source in _musicSources)
                    source.UpdatePitch();
            }
        }
        public static float MusicVolumeScale
        {
            get => _musicVolumeScale;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("value");
                _musicVolumeScale = value;
                foreach (MusicSource source in _musicSources)
                    source.UpdateVolume();
            }
        }
        public static float SoundVolumeScale
        {
            get => _soundVolumeScale;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("value");
                _soundVolumeScale = value;
                foreach (AudioSource source in _soundSources)
                    source.volume = value;
            }
        }

        public static event Action<BeatInfo> OnBeat;
        public static bool IsAnyMusicPlaying
        {
            get
            {
                foreach (MusicSource source in _musicSources)
                    if (source.IsPlaying) return true;
                return false;
            }
        }

        static float _musicPitchScale;
        static float _musicVolumeScale;
        static float _soundVolumeScale;
        static bool _musicSourceIsSwitched;

        static List<MusicSource> _musicSources;
        static List<AudioSource> _soundSources;

        class MusicSource
        {
            public bool IsPlaying => _source.isPlaying;
            public float Time => _source.time;
            public float Volume
            {
                get => _ownVolume;
                set
                {
                    _ownVolume = value;
                    UpdateVolume();
                }
            }
            public float Pitch
            {
                get => _ownPitch;
                set
                {
                    _ownPitch = value;
                    UpdatePitch();
                }
            }
            public AudioClip Clip
            { 
                get => _source.clip;
                set => _source.clip = value; 
            }
            public Tween Tween => _volumeTween;

            public bool resetOnStop;
            readonly AudioSource _source;

            float _ownPitch;
            float _ownVolume;
            float _tweenVolume;

            int _playingBeatIndex;
            BeatMap _playingBeatMap;
            Music _playingMusic;
            Tween _volumeTween;

            public MusicSource(AudioSource source)
            {
                _ownVolume = 1.0f;
                _source = source; 
            }

            public void Update()
            {
                if (_playingMusic == null) return;
                Beat beat = _playingBeatMap[_playingBeatIndex];
                if (Time < _playingBeatMap.Delay + beat.time) return;

                OnBeat?.Invoke(new BeatInfo(beat, Volume));
                if (++_playingBeatIndex >= _playingBeatMap.Count)
                     _playingMusic = null;
            }
            public void UpdateVolume()
            {
                UpdateVolumeInternal(TweenVolume());
            }
            public void UpdatePitch()
            {
                _source.pitch = _ownPitch * _musicPitchScale;
            }

            public void Play(Music music)
            {
                if (music == null) return;
                _playingMusic = music;
                _volumeTween.Kill();
                _playingBeatMap = music.beatMap;
                UpdateVolume();
                _source.Play();
            }
            public void Stop()
            {
                _playingMusic = null;
                _volumeTween.Kill();
                _source.Stop();
                if (resetOnStop) Reset();
            }
            public void Reset()
            {
                _source.time = 0;
            }

            public Tween Fade(float to)
            {
                return Fade(_ownVolume, to);
            }
            public Tween Fade(float from, float to)
            {
                if (!IsPlaying) return null;

                _volumeTween.Kill();
                _tweenVolume = from;
                _volumeTween = DOVirtual.Float(_tweenVolume, to, MUSIC_SWITCH_DURATION, v =>
                {
                    _tweenVolume = v;
                    UpdateVolumeInternal(v);
                });
                return _volumeTween;
            }
            public bool IsFading()
            {
                return _volumeTween.IsActive() && _volumeTween.IsPlaying();
            }

            void UpdateVolumeInternal(float tweenVolume)
            {
                _source.volume = _ownVolume * _musicVolumeScale * tweenVolume * MUSIC_GLOBAL_SCALE;
            }
            float TweenVolume()
            {
                if (IsFading())
                     return _tweenVolume;
                else return 1f;
            }
        }
        public enum FadeStyle
        {
            Instant = 0, // stops currently playing music, starts requested one

            // internal use
            _Linear = 1,
            _Cubic = 2,
            _StopAndStart = 4,
            _Simultaneously = 8,
            _Delayed = 16,

            ResetOnStop = 32,

            StopAndStartLinear = _Linear | _StopAndStart, // stops currently playing music, fades requested one via linear function
            StopAndStartCubic  = _Cubic  | _StopAndStart, // stops currently playing music, fades requested one via cubic function

            SimultaneouslyLinear = _Linear | _Simultaneously,  // fades currently playing music from 1 to 0, fades requested one from 0 to 1, all via linear function
            SimultaneouslyCubic  = _Cubic  | _Simultaneously,  // fades currently playing music from 1 to 0, fades requested one from 0 to 1, all via cubic function

            DelayedLinear = _Linear | _Delayed,  // fades currently playing music from 1 to 0, waits for 1 second, fades requested one from 0 to 1, all via linear function
            DelayedCubic  = _Cubic  | _Delayed,   // fades currently playing music from 1 to 0, waits for 1 second, fades requested one from 0 to 1, all via cubic function
        }

        public static async UniTask AwaitMusic()
        {
            while (true)
            {
                bool isAnyFading = false;
                foreach (MusicSource source in _musicSources)
                    isAnyFading |= source.IsFading();
                if (isAnyFading)
                    await UniTask.Yield();
                else break;
            }
        }
        public static void StopMusic(FadeStyle style = FadeStyle.SimultaneouslyCubic)
        {
            PlayMusic(null, style);
        }

        public static void PlayMusic(string musicId, FadeStyle style = FadeStyle.SimultaneouslyCubic)
        {
            Music music = musicId == null ? null : AudioBrowser.GetMusic(musicId);
            AudioClip clip = music.clip;

            MusicSource currentSource = _musicSources[_musicSourceIsSwitched ? 1 : 0];
            MusicSource requestedSource = _musicSources[_musicSourceIsSwitched ? 0 : 1];

            currentSource.resetOnStop = style.HasFlag(FadeStyle.ResetOnStop);
            requestedSource.Stop();
            requestedSource.Clip = clip;
            _musicSourceIsSwitched = !_musicSourceIsSwitched;

            if (style == FadeStyle.Instant)
            {
                currentSource.Stop();
                requestedSource.Play(music);
                return;
            }

            bool isLinear = style.HasFlag(FadeStyle._Linear);
            if (style.HasFlag(FadeStyle._StopAndStart))
            {
                currentSource.Stop();
                requestedSource.Play(music);
                requestedSource.Fade(0, 1).SetEase(isLinear ? Ease.Linear : Ease.InCubic);
            }
            else if (style.HasFlag(FadeStyle._Simultaneously))
            {
                requestedSource.Play(music);
                currentSource.Fade(0).SetEase(isLinear ? Ease.Linear : Ease.OutCubic).OnComplete(currentSource.Stop);
                requestedSource.Fade(1).SetEase(isLinear ? Ease.Linear : Ease.InCubic);
            }
            else if (style.HasFlag(FadeStyle._Delayed))
            {
                currentSource.Fade(0).SetEase(isLinear ? Ease.Linear : Ease.OutCubic).OnComplete(() =>
                {
                    currentSource.Stop();
                    requestedSource.Play(music);
                    requestedSource.Fade(1).SetEase(isLinear ? Ease.Linear : Ease.InCubic).SetDelay(1);
                });
            }
            else throw new NotSupportedException();
        }
        public static void PlaySound(string soundId, bool oneShot = true)
        {
            Sound sound = AudioBrowser.GetSound(soundId);
            AudioClip clip = sound.clip;
            AudioSource source = _soundSources[SOUNDS_SOURCE_INDEX];

            if (oneShot)
                source.PlayOneShot(clip);
            else
            {
                source.Stop();
                source.clip = clip;
                source.Play();
            }
        }

        private SFX() { }
        private void Start()
        {
            _musicPitchScale = 1.0f;
            _musicVolumeScale = 1.0f;
            _soundVolumeScale = 1.0f;

            _musicSources = new List<MusicSource>();
            _soundSources = new List<AudioSource>();

            // 2 sources for music, 1 for sounds
            _musicSources.Add(new MusicSource(gameObject.AddComponent<AudioSource>()));
            _musicSources.Add(new MusicSource(gameObject.AddComponent<AudioSource>()));
            _soundSources.Add(gameObject.AddComponent<AudioSource>());
        }
        private void Update()
        {
            foreach (MusicSource source in _musicSources)
                source.Update();
        }
    }
}
