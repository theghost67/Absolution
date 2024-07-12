using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Palette;
using MyBox;
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
        const float MUSIC_SWITCH_DURATION = 4.0f;
        const float MUSIC_GLOBAL_SCALE = 0.25f / 2f; // TODO: remove '/ 2f' when volume scrollbar will be implemented

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

        public static FadeStyle musicFadeStyle;
        public static FadeEase musicFadeEase;

        static float _musicPitchScale;
        static float _musicVolumeScale;
        static float _soundVolumeScale;
        static bool _musicSourceIsSwitched;
        static string _musicMixLastId;
        static MusicMixCollection _musicMixes;

        static List<MusicSource> _musicSources;
        static List<AudioSource> _soundSources;

        class MusicMixCollection : HashSet<MusicMixInfo>
        {
            public MusicMixCollection() : base() { }
            public MusicMixInfo this[string mixId]
            {
                get
                {
                    if (TryGetValue(mixId, out MusicMixInfo info))
                        return info;
                    MusicMixInfo mixInfo = new(mixId);
                    Add(mixInfo);
                    return mixInfo;
                }
            }
            public bool Add(string mixId)
            {
                return Add(new MusicMixInfo(mixId));
            }
            public bool Remove(string mixId)
            {
                return Remove(this[mixId]);
            }
            public bool TryGetValue(string mixId, out MusicMixInfo info)
            {
                foreach (MusicMixInfo mixInfo in this)
                {
                    if (mixInfo.musicMixId == mixId)
                    {
                        info = mixInfo;
                        return true;
                    }
                }
                info = null;
                return false;
            }
        }
        class MusicMixInfo : IEquatable<MusicMixInfo>
        {
            public readonly string musicMixId;
            public string lastMusicId;
            public float lastMusicTime;
            public MusicMixInfo(string musicMixId)
            {
                this.musicMixId = musicMixId;
            }
            public bool Equals(MusicMixInfo other)
            {
                return musicMixId.Equals(other.musicMixId);
            }
        }
        class MusicSource
        {
            public bool IsPlaying => _source.isPlaying;
            public float Length => _source.clip != null ? _source.clip.length : 0;
            public float Time 
            { 
                get => _source.time;
                set => _source.time = value;
            }
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

            readonly AudioSource _source;
            float _ownPitch;
            float _ownVolume;
            float _tweenVolume;
            bool _updateBeats;
            bool _playing;
            bool _stopping;

            int _playingBeatIndex;
            BeatMap _playingBeatMap;
            Music _playingMusic;
            Tween _volumeTween;
            string _musicMixId;

            public MusicSource(AudioSource source)
            {
                _ownVolume = 1.0f;
                _source = source; 
            }

            public void Play(Music music, string musicMixId)
            {
                if (music == null) return;
                _musicMixId = musicMixId;

                if (_musicMixId != null)
                    _musicMixes[_musicMixId].lastMusicId = music.id;

                _playing = true;
                _stopping = false;
                _updateBeats = true;
                _playingMusic = music;
                _volumeTween.Kill();
                _playingBeatMap = music.beatMap;
                _source.clip = music.clip;
                _source.Play();
            }
            public void Stop()
            {
                if (_musicMixId != null && IsPlaying)
                    _musicMixes[_musicMixId].lastMusicTime = Time;

                _playing = false;
                _stopping = false;
                _updateBeats = false;
                _volumeTween.Kill();
                _source.Stop();
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
                if (to == 0) _stopping = true;
                if (from > 1 || from < 0 || to > 1 || to < 0)
                    throw new ArgumentOutOfRangeException();

                _volumeTween.Kill();
                _tweenVolume = from;
                _volumeTween = DOVirtual.Float(_tweenVolume, to, MUSIC_SWITCH_DURATION, v =>
                {
                    _tweenVolume = v;
                    UpdateVolumeInternal(v);
                });
                return _volumeTween.OnComplete(() => { if (to == 0) Stop(); });
            }
            public bool IsFading()
            {
                return _volumeTween.IsActive() && _volumeTween.IsPlaying();
            }

            public void UpdatePitch()
            {
                _source.pitch = _ownPitch * _musicPitchScale;
            }
            public void UpdateVolume()
            {
                UpdateVolumeInternal(TweenVolume());
            }

            public void OnUpdate()
            {
                if (!_playing) return;
                if (!IsPlaying)
                {
                    _playing = false;
                    OnComplete();
                }

                if (!_updateBeats || _playingBeatMap.Count == 0) return;
                Beat beat = _playingBeatMap[_playingBeatIndex];
                if (Time < _playingBeatMap.Delay + beat.time) return;

                bool isFirstBeat = _playingBeatIndex == 0;
                bool isLastBeat = ++_playingBeatIndex >= _playingBeatMap.Count;

                OnBeat?.Invoke(new BeatInfo(beat, _playingBeatMap, Volume, isFirstBeat, isLastBeat));
                if (isLastBeat) _updateBeats = false;
            }
            private void OnComplete()
            {
                if (_stopping) return;
                Stop();
                if (_musicMixId == null) return;
                _musicMixLastId = null;
                _musicMixes.Remove(_musicMixId);

                Music[] mix = AudioBrowser.GetMusicMix(_musicMixId);
                int indexOfThis = mix.IndexOfItem(_playingMusic);
                if (indexOfThis != -1 && indexOfThis + 1 < mix.Length)
                     PlayMusicInternal(mix[indexOfThis + 1].id, _musicMixId);
                else PlayMusicInternal(mix.First().id, _musicMixId);
            }

            private void UpdateVolumeInternal(float tweenVolume)
            {
                _source.volume = _ownVolume * _musicVolumeScale * tweenVolume * MUSIC_GLOBAL_SCALE;
            }
            private float TweenVolume()
            {
                if (IsFading())
                     return _tweenVolume;
                else return 1f;
            }
        }

        public enum FadeStyle
        {
            Instant,
            InstantStopSmoothStart,
            SmoothStopInstantStart,
            Simultaneously,
        }
        public enum FadeEase
        {
            Linear,
            Sine,
            Quad,
            Cubic,
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
        public static void StopMusic()
        {
            PlayMusic(null);
        }
        public static void PlayMusic(string musicId)
        {
            PlayMusicInternal(musicId, null);
        }
        public static void PlayMusicMix(string musicMixId)
        {
            Music[] mix = musicMixId == null ? null : AudioBrowser.GetMusicMix(musicMixId);
            if (mix == null) return;
            if (musicMixId == _musicMixLastId) return;
            if (_musicMixes.TryGetValue(musicMixId, out MusicMixInfo mixInfo))
            {
                PlayMusicInternal(mixInfo.lastMusicId, musicMixId);
                GetActiveMusicSource().Time = mixInfo.lastMusicTime;
            }
            else PlayMusicInternal(mix.First().id, musicMixId);
        }
        public static void PlaySound(string soundId)
        {
            Sound sound = AudioBrowser.GetSound(soundId);
            AudioClip clip = sound.clip;
            AudioSource source = _soundSources[SOUNDS_SOURCE_INDEX];
            source.PlayOneShot(clip);
        }

        private SFX() { }
        private static MusicSource GetActiveMusicSource()
        {
            if (!IsAnyMusicPlaying) return null;
            if (_musicSourceIsSwitched)
                 return _musicSources[1];
            else return _musicSources[0];
        }
        private static void PlayMusicInternal(string musicId, string musicMixId)
        {
            Music music = musicId == null ? null : AudioBrowser.GetMusic(musicId);
            MusicSource currentSource = _musicSources[_musicSourceIsSwitched ? 1 : 0];
            MusicSource requestedSource = _musicSources[_musicSourceIsSwitched ? 0 : 1];

            requestedSource.Stop();
            _musicSourceIsSwitched = !_musicSourceIsSwitched;
            _musicMixLastId = musicMixId;

            if (musicFadeStyle == FadeStyle.Instant)
            {
                currentSource.Stop();
                requestedSource.Play(music, musicMixId);
                return;
            }

            Ease inEase = musicFadeEase switch
            {
                FadeEase.Linear => Ease.Linear,
                FadeEase.Sine => Ease.InSine,
                FadeEase.Quad => Ease.InQuad,
                FadeEase.Cubic => Ease.InCubic,
                _ => throw new NotSupportedException(),
            };
            Ease outEase = musicFadeEase switch
            {
                FadeEase.Linear => Ease.Linear,
                FadeEase.Sine => Ease.OutSine,
                FadeEase.Quad => Ease.OutQuad,
                FadeEase.Cubic => Ease.OutCubic,
                _ => throw new NotSupportedException(),
            };

            switch (musicFadeStyle)
            {
                case FadeStyle.InstantStopSmoothStart:
                    currentSource.Stop();
                    requestedSource.Play(music, musicMixId);
                    requestedSource.Fade(0, 1).SetEase(inEase);
                    break;

                case FadeStyle.SmoothStopInstantStart:
                    currentSource.Fade(0).SetEase(outEase);
                    requestedSource.Play(music, musicMixId);
                    break;

                case FadeStyle.Simultaneously:
                    currentSource.Fade(0).SetEase(outEase);
                    requestedSource.Play(music, musicMixId);
                    requestedSource.Fade(0, 1).SetEase(inEase);
                    break;

                default: throw new NotSupportedException();
            }
        }

        private void Awake()
        {
            _musicPitchScale = 1.0f;
            _musicVolumeScale = 1.0f;
            _soundVolumeScale = 1.0f;

            musicFadeStyle = FadeStyle.Simultaneously;
            musicFadeEase = FadeEase.Quad;

            _musicMixes = new MusicMixCollection();
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
                source.OnUpdate();
        }
    }
}
