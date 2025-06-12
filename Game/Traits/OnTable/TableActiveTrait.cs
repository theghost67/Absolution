using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий любой активный навык на столе (см. <see cref="ActiveTrait"/>).
    /// </summary>
    public class TableActiveTrait : TableTrait
    {
        public override TableFinder Finder => _finder;
        public new ActiveTrait Data => _data;
        public new TableActiveTraitDrawer Drawer => ((TableObject)this).Drawer as TableActiveTraitDrawer;

        readonly TableActiveTraitFinder _finder;
        readonly ActiveTrait _data;

        public TableActiveTrait(ActiveTrait data, TableFieldCard owner, Transform parent) : base(data, owner, parent)
        {
            _data = data;
            _finder = new TableActiveTraitFinder(this);
            TryOnInstantiatedAction(GetType(), typeof(TableActiveTrait));
        }
        protected TableActiveTrait(TableActiveTrait src, TableActiveTraitCloneArgs args) : base(src, args)
        {
            _data = args.srcTraitDataClone;
            _finder = new TableActiveTraitFinder(this);
            TryOnInstantiatedAction(GetType(), typeof(TableActiveTrait));
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

        public UniTask TryUse(TableField target)
        {
            return TryUse(new TableActiveTraitUseArgs(this, target));
        }
        public async UniTask TryUse(TableActiveTraitUseArgs e)
        {
            if (!_data.IsUsable(e)) return;
            TableEventManager.Add("table", -Guid);
            await _data.Use(e);
            TableEventManager.Remove("table", -Guid);
        }
        public bool IsUsable(TableActiveTraitUseArgs e)
        {
            return !IsDisposed && e.trait.GetStacks() > 0 && _data.IsUsable(e);
        }

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableActiveTraitDrawer(this, parent);
        }
    }
}