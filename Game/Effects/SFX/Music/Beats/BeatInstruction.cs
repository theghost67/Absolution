namespace Game.Effects
{
    /// <summary>
    /// Класс, представляющий одну из инструкций <see cref="BeatMap"/>. Инструкция может содержать несколько битов сразу.
    /// </summary>
    public abstract class BeatInstruction
    {
        public BeatInstruction() { }
        public abstract void AppendTo(BeatMap map);
    }
}
