using System;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Абстрактный класс, представляющий аргументы для динамического описания объекта (см. <see cref="IDescriptive"/>).
    /// </summary>
    public abstract class DescriptiveArgs : IEquatable<DescriptiveArgs>
    {
        public readonly IDescriptive src;
        public readonly bool isCard;
        public readonly List<object> custom;

        public bool linkFormat;
        public bool linksAreAvailable; // used to show a hint in description
        public int turnAge;

        public DescriptiveArgs(IDescriptive src) 
        {
            this.src = src;
            this.isCard = src is Cards.Card;
            this.custom = new List<object>(src.DescCustomParams());

            linksAreAvailable = false;
            linkFormat = false;
            turnAge = -1;
        }
        public bool Equals(DescriptiveArgs other)
        {
            return src.Equals(other.src);
        }
    }
}
