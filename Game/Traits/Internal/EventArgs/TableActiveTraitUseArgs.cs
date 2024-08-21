using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий параметр для функций, вызываемых при использовании активного навыка стола на цели.
    /// </summary>
    public class TableActiveTraitUseArgs
    {
        public readonly TableActiveTrait trait;
        public readonly TableField target;
        public readonly int traitStacks;
        public readonly bool isInBattle;

        public TableActiveTraitUseArgs(TableActiveTrait trait, TableField target)
        {
            this.trait = trait;
            this.target = target;
            traitStacks = trait.GetStacks();
            isInBattle = trait is IBattleTrait;
        }
    }
}
