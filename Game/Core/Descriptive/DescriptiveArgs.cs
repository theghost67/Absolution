using System;

namespace Game
{
    /// <summary>
    /// Абстрактный класс, представляющий аргументы для динамического описания объекта (см. <see cref="IDescriptive"/>).
    /// </summary>
    public abstract class DescriptiveArgs : IEquatable<DescriptiveArgs>
    {
        public readonly ITableObject table; // can be null
        public readonly IDescriptive data;
        public readonly bool isCard;

        public bool linkFormat;
        public bool linksAreAvailable;

        public DescriptiveArgs(IDescriptive objectData, ITableObject tableObject) 
        {
            data = objectData;
            table = tableObject;
            isCard = objectData is Cards.Card;

            linksAreAvailable = false;
            linkFormat = false;
        }
        public bool Equals(DescriptiveArgs other)
        {
            return data.Equals(other.data);
        }
    }
}
