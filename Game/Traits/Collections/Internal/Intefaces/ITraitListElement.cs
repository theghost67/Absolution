using System;

namespace Game.Traits
{
    /// <summary>
    /// Реализует объект как один из элементов списка данных навыков (см. <see cref="TraitList"/>).
    /// </summary>
    public interface ITraitListElement : IEquatable<ITraitListElement>, ICloneableWithArgs
    {
        public TraitList List { get; }
        public Trait Trait { get; }
        public int Stacks { get; }
    }
}
