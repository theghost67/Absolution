using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий любую карту без характеристик на столе (см. <see cref="FloatCard"/>).
    /// </summary>
    public class TableFloatCard : TableCard
    {
        public new FloatCard Data => _data;
        public new TableFloatCardDrawer Drawer => _drawer;
        public override TableFinder Finder => _finder;

        readonly FloatCard _data;
        readonly TableFinder _finder;
        TableFloatCardDrawer _drawer;
        bool _isUsing;

        public TableFloatCard(FloatCard data, Transform parent, bool withDrawer = true) : base(data, parent, withDrawer: false)
        {
            _data = data;
            _finder = new TableFloatCardFinder(this);

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected TableFloatCard(TableFloatCard src, TableFloatCardCloneArgs args) : base(src, args)
        {
            _data = args.srcCardDataClone;
            _finder = new TableFloatCardFinder(this);
        }

        public override object Clone(CloneArgs args)
        {
            if (args is TableFloatCardCloneArgs cArgs)
                return new TableFloatCard(this, cArgs);
            else return null;
        }

        public bool TryUse(TableFloatCardUseArgs e)
        {
            if (!IsUsable(e)) return false;
            if (_drawer == null)
            {
                TableEventManager.Add();
                OnUsed(e).ContinueWith(TableEventManager.Remove);
                return true;
            }

            TableEventManager.Add();
            AnimUse().OnComplete(() => OnUsed(e).ContinueWith(TableEventManager.Remove));
            return true;
        }
        public bool IsUsable(TableFloatCardUseArgs e)
        {
            return _data.IsUsable(e);
        }

        public async UniTask AwaitUse()
        {
            while (_isUsing)
                await UniTask.Yield();
        }
        protected virtual UniTask OnUsed(TableFloatCardUseArgs e)
        {
            return _data.OnUse(e);
        }

        protected override void DrawerSetter(TableCardDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (TableFloatCardDrawer)value;
        }
        protected override TableCardDrawer DrawerCreator(Transform parent)
        {
            TableFloatCardDrawer drawer = new(this, parent);
            drawer.SetSortingOrder(10, asDefault: true);
            return drawer;
        }

        // TODO: improve
        Tween AnimUse()
        {
            _drawer.SetCollider(false);
            _drawer.SetSortingAsTop();
            return _drawer.AnimExplosion();
        }
    }
}
