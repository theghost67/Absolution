namespace Game
{
    /// <summary>
    /// Реализует объект как находимый на территории стола через <see cref="TableFinder"/>.
    /// </summary>
    public interface ITableFindable
    {
        public TableFinder Finder { get; }
    }
}
