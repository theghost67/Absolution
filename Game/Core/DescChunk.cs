namespace Game
{
    /// <summary>
    /// Структура, представляющий фрагмент описания для функции <see cref="IDescriptive.Description(DescChunk[])"/>.
    /// </summary>
    public readonly struct DescChunk
    {
        public readonly string id;
        public readonly object value;
        public DescChunk(string id, object value)
        {
            this.id = id;
            this.value = value;
        }
    }
}
