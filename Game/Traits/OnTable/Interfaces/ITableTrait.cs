using Cysharp.Threading.Tasks;
using Game.Cards;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Реализует объект как трейт на столе.
    /// </summary>
    public interface ITableTrait : ITableEntrySource, ITableDrawable, ICloneableWithArgs, IUnique, IDisposable
    {
        public TableFieldCard Owner { get; }
        public Trait Data { get; }
        public TableTraitStorage Storage { get; }

        public new TableTraitDrawer Drawer { get; }
        Drawer ITableDrawable.Drawer => Drawer;

        public int GetStacks();
        public UniTask AdjustStacks(int delta, ITableEntrySource source);
        public UniTask SetStacks(int value, ITableEntrySource source);
    }
}
