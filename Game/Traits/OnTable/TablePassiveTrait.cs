using Cysharp.Threading.Tasks;
using Game.Cards;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий любой пассивный трейт на столе (см. <see cref="PassiveTrait"/>).
    /// </summary>
    public class TablePassiveTrait : TableTrait
    {
        public override TableFinder Finder => _finder;
        public new PassiveTrait Data => _data;
        public new TablePassiveTraitDrawer Drawer => ((TableObject)this).Drawer as TablePassiveTraitDrawer;

        readonly TablePassiveTraitFinder _finder;
        readonly PassiveTrait _data;

        public TablePassiveTrait(PassiveTrait data, TableFieldCard owner, Transform parent) : base(data, owner, parent)
        {
            _data = data;
            _finder = new TablePassiveTraitFinder(this);
            TryOnInstantiatedAction(GetType(), typeof(TablePassiveTrait));
        }
        protected TablePassiveTrait(TablePassiveTrait src, TablePassiveTraitCloneArgs args) : base(src, args)
        {
            _data = args.srcTraitDataClone;
            _finder = new TablePassiveTraitFinder(this);
            TryOnInstantiatedAction(GetType(), typeof(TablePassiveTrait));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TablePassiveTraitCloneArgs cArgs)
                 return new TablePassiveTrait(this, cArgs);
            else return null;
        }
        public override int GetStacks()
        {
            if (Owner == null) return 0;
            TablePassiveTraitListElement element = Owner.Traits.Passives[_data.id];
            if (element != null)
                 return element.Stacks;
            else return 0;
        }
        public override UniTask AdjustStacks(int delta, ITableEntrySource source)
        {
            if (Owner == null)
                 return UniTask.CompletedTask;
            else return Owner.Traits.Passives.AdjustStacks(_data.id, delta, source);
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TablePassiveTraitDrawer(this, parent);
        }
    }
}