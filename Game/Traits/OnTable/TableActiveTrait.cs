using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий любой активный трейт на столе (см. <see cref="ActiveTrait"/>).
    /// </summary>
    public class TableActiveTrait : TableTrait
    {
        public override TableFinder Finder => _finder;
        public new ActiveTrait Data => _data;
        public new TableActiveTraitDrawer Drawer => _drawer;

        readonly TableActiveTraitFinder _finder;
        readonly ActiveTrait _data;
        TableActiveTraitDrawer _drawer;

        public TableActiveTrait(ActiveTrait data, TableFieldCard owner, Transform parent, bool withDrawer = true) : base(data, owner, parent, withDrawer: false)
        {
            _data = data;
            _finder = new TableActiveTraitFinder(this);
            if (withDrawer)
                CreateDrawer(parent);
        }
        protected TableActiveTrait(TableActiveTrait src, TableActiveTraitCloneArgs args) : base(src, args)
        {
            _data = args.srcTraitDataClone;
            _finder = new TableActiveTraitFinder(this);
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TableActiveTraitCloneArgs cArgs)
                return new TableActiveTrait(this, cArgs);
            else return null;
        }
        public override int GetStacks()
        {
            if (Owner == null) return 0;
            TableActiveTraitListElement element = Owner.Traits.Actives[_data.id];
            if (element != null)
                return element.Stacks;
            else return 0;
        }
        public override UniTask AdjustStacks(int delta, ITableEntrySource source)
        {
            if (Owner == null)
                return UniTask.CompletedTask;
            else return Owner.Traits.Actives.AdjustStacks(_data.id, delta, source);
        }

        public bool TryUse(TableField target)
        {
            TableActiveTraitUseArgs e = new(this, target);
            if (!_data.IsUsable(e)) return false;

            TableEventManager.Add();
            _data.OnUse(e).ContinueWith(TableEventManager.Remove);
            return true;
        }

        protected override void DrawerSetter(TableTraitDrawer value)
        {
            _drawer = (TableActiveTraitDrawer)value;
        }
        protected override TableTraitDrawer DrawerCreator(Transform parent)
        {
            return new TableActiveTraitDrawer(this, parent);
        }
    }
}