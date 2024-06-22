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
        public new TablePassiveTraitDrawer Drawer => _drawer;

        readonly TablePassiveTraitFinder _finder;
        readonly PassiveTrait _data;
        TablePassiveTraitDrawer _drawer;

        public TablePassiveTrait(PassiveTrait data, TableFieldCard owner, Transform parent, bool withDrawer = true) : base(data, owner, parent, withDrawer: false)
        {
            _data = data;
            _finder = new TablePassiveTraitFinder(this);
            if (withDrawer)
                CreateDrawer(parent);
        }
        protected TablePassiveTrait(TablePassiveTrait src, TablePassiveTraitCloneArgs args) : base(src, args)
        {
            _data = args.srcTraitDataClone;
            _finder = new TablePassiveTraitFinder(this);
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

        protected override void DrawerSetter(TableTraitDrawer value)
        {
            _drawer = (TablePassiveTraitDrawer)value;
        }
        protected override TableTraitDrawer DrawerCreator(Transform parent)
        {
            return new TablePassiveTraitDrawer(this, parent);
        }
    }
}