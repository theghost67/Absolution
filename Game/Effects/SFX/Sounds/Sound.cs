using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Абстрактный класс, представляющий игровой звук.
    /// </summary>
    public abstract class Sound : Unique
    {
        public readonly string id;
        public readonly AudioClip clip;

        public Sound(string id) : base()
        {
            this.id = id;
            this.clip = Resources.Load<AudioClip>($"SFX/Sounds/{id}");
        }
    }
}
