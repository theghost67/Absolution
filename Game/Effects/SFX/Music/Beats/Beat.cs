namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий один "бит" музыки.
    /// </summary>
    public readonly struct Beat
    {
        public readonly float pos;     // could be skipped via 'length'
        public readonly int length;    // skips more positions on append (if equals to 1, will wait one beat)
        public readonly float bpm;     // = speed in map
        public readonly float time;    // clip time when this beat plays
        public readonly int intensity; // beat power (can be used in OnBeat events)

        public Beat(float pos, int length, float bpm, int intensity)
        {
            if (intensity < 0)
                throw new System.ArgumentOutOfRangeException("intensity");
            if (bpm <= 0)
                throw new System.ArgumentOutOfRangeException("bpm");
            if (pos <= 0)
                throw new System.ArgumentOutOfRangeException("position");
            if (length <= 0)
                throw new System.ArgumentOutOfRangeException("length");

            this.pos = pos;
            this.length = length;
            this.bpm = bpm;
            this.time = pos * (60 / bpm);
            this.intensity = intensity;
        }
    }
}
