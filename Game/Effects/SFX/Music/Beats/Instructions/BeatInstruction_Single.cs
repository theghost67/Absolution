namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий одну из инструкций бит-мапа. Добавляет один бит в <see cref="BeatMap"/>.
    /// </summary>
    public class BeatInstruction_Single : BeatInstruction
    {
        public readonly float length;
        public readonly int intensity;
        public readonly BeatFlags flags;

        public BeatInstruction_Single(float length, int intensity, BeatFlags flags = default) : base()
        {
            this.length = length;
            this.intensity = intensity;
            this.flags = flags;
        }
        public override void AppendTo(BeatMap map) => map.Add(new Beat(map, length, intensity, flags));
    }
}
