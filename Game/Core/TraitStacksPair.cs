namespace Game
{
    /// <summary>
    /// Структура, представляющая пару значений &lt;<see langword="int"/>, <see langword="string"/>&gt; (id навыка, заряды навыка).
    /// </summary>
    public readonly struct TraitStacksPair
    {
        public readonly string id;
        public readonly int stacks;
        public TraitStacksPair(string id, int stacks)
        {
            this.id = id;
            this.stacks = stacks;
        }
    }
}
