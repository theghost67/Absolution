namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий параметр для функций при попытке изменения стаков у трейта.
    /// </summary>
    public class TableTraitStacksTryArgs
    {
        public readonly string id;
        public readonly int stacks;
        public readonly ITableEntrySource source; // can be null

        public TableTraitStacksTryArgs(string id, int stacks, ITableEntrySource source)
        {
            this.id = id;
            this.stacks = stacks;
            this.source = source;
        }
    }
}
