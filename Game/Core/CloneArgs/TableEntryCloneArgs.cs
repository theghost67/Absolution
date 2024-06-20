namespace Game
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования записи стола.
    /// </summary>
    public class TableEntryCloneArgs : CloneArgs
    {
        public readonly ITableEntrySource sourceClone;
        public TableEntryCloneArgs(ITableEntrySource sourceClone)
        {
            this.sourceClone = sourceClone;
        }
    }
}
