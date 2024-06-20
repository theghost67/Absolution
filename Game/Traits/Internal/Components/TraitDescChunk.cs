namespace Game.Traits
{
    /// <summary>
    /// Структура, представляющая фрагмент описания трейта, состоящий из заголовка и содержания.
    /// </summary>
    public readonly struct TraitDescChunk
    {
        public readonly string header;
        public readonly string contents;

        public TraitDescChunk(string header, string contents)
        {
            this.header = header;
            this.contents = contents;
        }
        public override string ToString()
        {
            return $"{header}\n{contents}";
        }
    }
}
