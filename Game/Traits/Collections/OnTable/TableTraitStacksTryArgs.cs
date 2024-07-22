namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий параметр для функций при попытке изменения стаков у навыка.
    /// </summary>
    public class TableTraitStacksTryArgs
    {
        public readonly string id;
        public readonly int delta;
        public readonly ITableEntrySource source; // can be null

        public TableTraitStacksTryArgs(string id, int delta, ITableEntrySource source)
        {
            this.id = id;
            this.delta = delta;
            this.source = source;
        }
    }
}
