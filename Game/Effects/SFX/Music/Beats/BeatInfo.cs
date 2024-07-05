namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий информацию о только что проигранном "бите" музыки (см. <see cref="Beat"/>).<br/>
    /// Используется как параметр для <see cref="SFX.OnBeat"/>.
    /// </summary>
    public sealed class BeatInfo
    {
        public readonly Beat beat;
        public readonly float volume;
        public BeatInfo(Beat beat, float volume)
        {
            this.beat = beat;
            this.volume = volume;
        }
    }
}
