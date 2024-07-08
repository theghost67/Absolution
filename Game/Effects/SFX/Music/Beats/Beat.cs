namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий один "бит" музыки.
    /// </summary>
    public readonly struct Beat
    {
        public readonly int index;       // index in map
        public readonly float pos;       // could be skipped via 'length'
        public readonly float length;    // skips more positions on append (if equals to 1, will wait one beat)
        public readonly float time;      // clip time when this beat plays
        public readonly int intensity;   // use in OnBeat events how you like (note: ignore beats with intensity < 1)
        public readonly BeatFlags flags; // same as with intensity

        // NOTE 1: call constructor only when passing parameter to BeatMap.Add() function
        // NOTE 2: can pass 1/2, 1/4, 1/8 as length

        public Beat(BeatMap map, float length, int intensity, BeatFlags flags = default)
        {
            if (intensity < 0)
                throw new System.ArgumentOutOfRangeException("intensity");
            if (length <= 0)
                throw new System.ArgumentOutOfRangeException("length");

            this.index = map.Count;
            this.pos = map.LastPos;
            this.length = length;
            this.time = pos * map.BpmScale;
            this.intensity = intensity;
            this.flags = flags;
        }
    }
}
