namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка данных пассивных трейтов (см. <see cref="PassiveTraitList"/>).
    /// </summary>
    public class PassiveTraitListElement : TraitListElement
    {
        public new PassiveTraitList List { get; }
        public new PassiveTrait Trait { get; }

        public PassiveTraitListElement(PassiveTraitList list, PassiveTrait trait) : base(list, trait)
        {
            List = list;
            Trait = trait;
        }
        protected PassiveTraitListElement(PassiveTraitListElement src, TraitListElementCloneArgs args) : base(src, args)
        {
            List = (PassiveTraitList)base.List;
            Trait = (PassiveTrait)base.Trait;
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TraitListElementCloneArgs cArgs)
                 return new PassiveTraitListElement(this, cArgs);
            else return null;
        }
    }
}
