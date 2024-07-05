using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Абстрактный класс, представляющий игровую мелодию.
    /// </summary>
    public abstract class Music
    {
        public readonly string id;
        public readonly string clipPath;
        public readonly AudioClip clip;
        public readonly BeatMap beatMap;

        public Music(string id, string clipPath)
        {
            this.id = id;
            this.clipPath = clipPath;
            this.clip = Resources.Load<AudioClip>(clipPath);
            this.beatMap = CreateBeatMap();
        }
        protected abstract BeatMap CreateBeatMap();
    }
}
