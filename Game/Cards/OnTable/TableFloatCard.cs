using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Territories;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий любую карту без характеристик на столе (см. <see cref="FloatCard"/>).
    /// </summary>
    public class TableFloatCard : TableCard
    {
        public new FloatCard Data => _data;
        public new TableFloatCardDrawer Drawer => ((TableObject)this).Drawer as TableFloatCardDrawer;
        public override TableFinder Finder => _finder;

        readonly FloatCard _data;
        readonly TableFinder _finder;

        public TableFloatCard(FloatCard data, Transform parent) : base(data, parent)
        {
            _data = data;
            _finder = new TableFloatCardFinder(this);
            TryOnInstantiatedAction(GetType(), typeof(TableFloatCard));
        }
        protected TableFloatCard(TableFloatCard src, TableFloatCardCloneArgs args) : base(src, args)
        {
            _data = args.srcCardDataClone;
            _finder = new TableFloatCardFinder(this);
            TryOnInstantiatedAction(GetType(), typeof(TableFloatCard));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TableFloatCardCloneArgs cArgs)
                return new TableFloatCard(this, cArgs);
            else return null;
        }

        public UniTask TryUse(TableTerritory territory)
        {
            return TryUse(new TableFloatCardUseArgs(this, territory));
        }
        public async UniTask TryUse(TableFloatCardUseArgs e)
        {
            if (!IsUsable(e)) return;
            TableEventManager.Add("table", -Guid);
            if (Drawer != null)
                await AnimUse().AsyncWaitForCompletion();
            await OnUsed(e);
            TableEventManager.Remove("table", -Guid);
        }
        public bool IsUsable(TableFloatCardUseArgs e)
        {
            return !IsDisposed && _data.IsUsable(e);
        }

        protected virtual UniTask OnUsed(TableFloatCardUseArgs e)
        {
            return _data.OnUse(e);
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableFloatCardDrawer(this, parent) { SortingOrderDefault = 10 };
        }

        // TODO: improve
        Tween AnimUse()
        {
            TableFloatCardDrawer drawer = Drawer;
            drawer.ColliderEnabled = false;
            drawer.SetSortingAsTop();
            return drawer.AnimExplosion();
        }
    }
}
