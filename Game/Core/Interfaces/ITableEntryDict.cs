using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Реализует список объектов типа <see cref="TableEntry"/> только для чтения.
    /// </summary>
    public interface ITableEntryDict : IReadOnlyDictionary<string, TableEntry>, ICloneableWithArgs { }
}
