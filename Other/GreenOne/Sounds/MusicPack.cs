using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GreenOne
{
    /// <summary>
    /// Класс, содержащий набор музыкальных клипов.
    /// </summary>
    [Obsolete("Use SFX class instead (move implementation)")] public sealed class MusicPack
    {
        #region Variables
        const float FADE_DURATION = 1;
        static readonly Dictionary<string, MusicPack> _set = new();

        public float Volume
        {
            get => _volume;
            set => RecalculateVolume(value);
        }
        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;

        readonly AudioSource _source;
        readonly AudioClip[] _clips;

        float _volume;
        bool _isPlaying;
        bool _isPaused;

        Tweener _fadeTween;
        Tween _playTween;
        int _playIndex;
        #endregion

        #region Functions
        MusicPack(string id, string folder, float volume)
        {
            _playIndex = -1;
            _clips = SoundSystem.LoadClips(folder).Shuffle().ToArray();

            _source = SoundSystem.GameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;

            Volume = volume;
            SoundSystem.OnGlobalVolumeSet += () => RecalculateVolume(_volume);

            _set.Add(id, this);
        }

        public static void Initiailize()
        {
            new MusicPack("World", "World music", volume: 1);
            new MusicPack("Location", "Location music", volume: 1);
            new MusicPack("Battle", "Battle music", volume: 1);
        }
        public static MusicPack Get(string id)
        {
            return _set[id];
        }

        public async UniTaskVoid PlayFading()
        {
            if (_isPaused)
                UnpauseInstantly();
            else if (!_isPlaying)
                PlayInstantly();
            else return;

            await FadeInVolume();
        }
        public async UniTaskVoid StopFading()
        {
            if (!_isPlaying) return;
            if (!_isPaused)
                await FadeOutVolume();

            StopInstantly();
        }

        public async UniTaskVoid PauseFading()
        {
            if (_isPaused) return;

            await FadeOutVolume();
            PauseInstantly();
        }
        public async UniTaskVoid UnpauseFading()
        {
            if (!_isPaused) return;

            UnpauseInstantly();
            await FadeInVolume();
        }

        public void PlayInstantly()
        {
            if (_isPlaying || _isPaused) return;
            _isPlaying = true;

            _source.clip = GetSoundClip();
            _source.Play();
            _playTween = DOVirtual.DelayedCall(_source.clip.length + 1, () =>
            {
                _isPlaying = false;
                PlayFading().Forget();
            });
        }
        public void StopInstantly()
        {
            if (!_isPlaying) return;
            _isPlaying = false;

            _source.Stop();
            _playTween.Kill();
        }

        public void PauseInstantly()
        {
            if (_isPaused) return;
            _isPaused = true;

            _playTween.Pause();
            _source.Pause();
        }
        public void UnpauseInstantly()
        {
            if (!_isPaused) return;
            _isPaused = false;

            _playTween.Play();
            _source.UnPause();
        }

        AudioClip GetSoundClip()
        {
            return _clips[++_playIndex % _clips.Length];
        }
        void RecalculateVolume(float value)
        {
            _volume = value * SoundSystem.GlobalVolume;
            RecalculateSourceVolume(value);
        }
        void RecalculateSourceVolume(float value)
        {
            _source.volume = value;
        }

        async UniTask FadeInVolume()
        {
            _fadeTween?.Kill();
            _fadeTween = DOVirtual.Float(0, _volume, FADE_DURATION, RecalculateSourceVolume);
            await _fadeTween.AsyncWaitForCompletion();
        }
        async UniTask FadeOutVolume()
        {
            _fadeTween?.Kill();
            _fadeTween = DOVirtual.Float(_volume, 0, FADE_DURATION, RecalculateSourceVolume);
            await _fadeTween.AsyncWaitForCompletion();
        }
        #endregion
    }
}