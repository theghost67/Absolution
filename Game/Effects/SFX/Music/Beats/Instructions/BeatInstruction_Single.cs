namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий одну из инструкций бит-мапа. Добавляет один бит в <see cref="BeatMap"/>.
    /// </summary>
    public class BeatInstruction_Single : BeatInstruction
    {
        public readonly Beat beat;
        public BeatInstruction_Single(Beat beat) : base() { this.beat = beat; }
        public override void AppendTo(BeatMap map) => map.Add(beat);
    }
}
