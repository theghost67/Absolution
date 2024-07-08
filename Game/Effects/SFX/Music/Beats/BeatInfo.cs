namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий информацию о только что проигранном "бите" музыки (см. <see cref="Beat"/>).<br/>
    /// Используется как параметр для <see cref="SFX.OnBeat"/>.
    /// </summary>
    public sealed class BeatInfo
    {
        public readonly Beat beat;
        public readonly BeatMap beatMap;
        public readonly float volume;
        public readonly bool isFirst;
        public readonly bool isLast;

        public BeatInfo(Beat beat, BeatMap beatMap, float volume, bool isFirst, bool isLast)
        {
            this.beat = beat;
            this.beatMap = beatMap;
            this.volume = volume;
            this.isFirst = isFirst;
            this.isLast = isLast;
        }
    }
}
