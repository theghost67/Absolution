using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Абстрактный класс, представляющий игровую мелодию.
    /// </summary>
    public abstract class Music : Unique
    {
        public readonly string id;
        public readonly AudioClip clip;
        public readonly BeatMap beatMap;

        public Music(string id) : base()
        {
            this.id = id;
            this.clip = Resources.Load<AudioClip>($"SFX/Music/{id}");
            this.beatMap = CreateBeatMap();
        }
        protected abstract BeatMap CreateBeatMap();
    }
}
