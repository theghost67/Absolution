namespace Game
{
    /// <summary>
    /// Структура, представляющая запись изменения какого-либо числового аспекта (переменной) объекта на столе.
    /// </summary>
    public readonly struct TableEntry : ICloneableWithArgs
    {
        public readonly float value;
        public readonly ITableEntrySource source;
        public readonly int turn;

        public TableEntry(float value, ITableEntrySource source, int turn = 0)
        {
            if (value == 0)
                throw new System.ArgumentException("Entry value cannot be zero, as it has no effect.");

            this.value = value;
            this.source = source;
            this.turn = turn;
        }
        private TableEntry(TableEntry src, TableEntryCloneArgs args)
        {
            value = src.value;
            source = args.sourceClone;
            turn = src.turn;
        }

        public object Clone(CloneArgs args)
        {
            if (args is TableEntryCloneArgs cArgs)
                 return new TableEntry(this, cArgs);
            else return null;
        }
    }
}
