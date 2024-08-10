using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Реализует объект как объект с динамическим описанием.
    /// </summary>
    public interface IDescriptive
    {
        public string Description(IEnumerable<DescChunk> chunks);
    }
}
