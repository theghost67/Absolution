namespace Game
{
    /// <summary>
    /// Реализует уникальность объекта через уникальный идентификатор.
    /// </summary>
    public interface IUnique
    {
        public int Guid { get; }
        public string GuidStr { get; }
    }
}
