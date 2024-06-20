namespace Game
{
    /// <summary>
    /// Реализует возможность клонирования объекта, используя параметр для клонирования.
    /// </summary>
    public interface ICloneableWithArgs
    {
        public object Clone(CloneArgs args);
    }
}
