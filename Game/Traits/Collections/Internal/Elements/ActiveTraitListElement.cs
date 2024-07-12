namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из элементов списка данных активных навыков (см. <see cref="ActiveTraitList"/>).
    /// </summary>
    public class ActiveTraitListElement : TraitListElement
    {
        public new ActiveTraitList List { get; }
        public new ActiveTrait Trait { get; }

        public ActiveTraitListElement(ActiveTraitList list, ActiveTrait trait) : base(list, trait)
        {
            List = list;
            Trait = trait;
        }
        protected ActiveTraitListElement(ActiveTraitListElement src, TraitListElementCloneArgs args) : base(src, args)
        {
            List = (ActiveTraitList)base.List;
            Trait = (ActiveTrait)base.Trait;
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TraitListElementCloneArgs cArgs)
                return new ActiveTraitListElement(this, cArgs);
            else return null;
        }
    }
}
