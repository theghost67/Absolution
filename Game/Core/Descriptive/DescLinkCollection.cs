using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Класс, представляющий коллекцию из <see cref="DescriptiveArgs"/>которые могут быть использованы <br/>
    /// от лица объекта с id для вызова функции <see cref="IDescriptive.DescDynamic(DescriptiveArgs)"/>.
    /// </summary>
    public class DescLinkCollection : List<DescriptiveArgs>
    {
        public static readonly DescLinkCollection empty = new();
    }
}
