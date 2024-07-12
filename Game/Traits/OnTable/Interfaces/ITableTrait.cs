using Cysharp.Threading.Tasks;
using Game.Cards;

namespace Game.Traits
{
    /// <summary>
    /// Реализует объект как навык на столе.
    /// </summary>
    public interface ITableTrait : ITableObject, ITableEntrySource, ICloneableWithArgs
    {
        public TableFieldCard Owner { get; }
        public Trait Data { get; }
        public TableTraitStorage Storage { get; }

        public new TableTraitDrawer Drawer { get; }
        Drawer ITableObject.Drawer => Drawer;

        public int GetStacks();
        public UniTask AdjustStacks(int delta, ITableEntrySource source);
        public UniTask SetStacks(int value, ITableEntrySource source);
    }
}
