namespace Game.Traits
{
    /// <summary>
    /// Абстрактный класс, представляющий один из элементов списка данных трейтов (см. <see cref="TraitList"/>).
    /// </summary>
    public abstract class TraitListElement : ITraitListElement
    {
        public TraitList List { get; }
        public Trait Trait { get; }
        public int Stacks => _stacks;
        int _stacks;

        public TraitListElement(TraitList list, Trait trait)
        {
            List = list;
            Trait = trait;
        }
        protected TraitListElement(TraitListElement src, TraitListElementCloneArgs args)
        {
            List = args.srcListClone;
            Trait = (Trait)src.Trait.Clone();
            _stacks = src._stacks;
        }

        public abstract object Clone(CloneArgs args);
        public bool Equals(ITraitListElement other)
        {
            return List.Set.Owner.Equals(other.List.Set.Owner) && Trait.Equals(other.Trait);
        }

        // NOTE: use only inside of the TraitList instance
        public void AdjustStacksInternal(int delta)
        {
            _stacks += delta;
        }
    }
}
