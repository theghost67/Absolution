using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Абстрактный класс, представляющий игровой звук.
    /// </summary>
    public abstract class Sound
    {
        public readonly string id;
        public readonly string clipPath;
        public readonly AudioClip clip;

        public Sound(string id, string clipPath)
        {
            this.id = id;
            this.clipPath = clipPath;
            this.clip = Resources.Load<AudioClip>(clipPath);
        }
    }
}
