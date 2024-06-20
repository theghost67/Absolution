using System;
using UnityEngine;

namespace GreenOne
{
    /// <summary>
    /// Класс, представляющий систему воспроизведения звуков.
    /// </summary>
    public sealed class SoundSystem : MonoBehaviour
    {
        public static event Action OnGlobalVolumeSet;
        public static float GlobalVolume
        {
            get => _globalVolume;
            set
            {
                _globalVolume = value;
                OnGlobalVolumeSet?.Invoke();
            }
        }
        public static GameObject GameObject => _gameObject;

        static float _globalVolume;
        static GameObject _gameObject;
        static AudioSource _defaultSource;

        public static AudioClip[] LoadClips(string folder)
        {
            return Resources.LoadAll<AudioClip>($"SFX/{folder}/");
        }
        void Start()
        {
            _gameObject = gameObject;
            _globalVolume = 0.18f;
            _defaultSource = GetComponent<AudioSource>();
        }
    }
}
