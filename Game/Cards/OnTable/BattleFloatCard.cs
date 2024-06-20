using Game.Sleeves;
using Game.Territories;
using MyBox;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий карту без характеристик во время сражения.
    /// </summary>
    public class BattleFloatCard : TableFloatCard, IBattleCard
    {
        public new BattleFloatCardDrawer Drawer => _drawer;
        public override string TableName => base.TableName.Colored(Side.PosColor());

        public BattleTerritory Territory => _side.Territory;
        public BattleSide Side
        {
            get => _side;
            set
            {
                if (this is not ITableSleeveCard sCard)
                    return;

                _side.Sleeve.Remove(sCard);
                _side = value;
                _side.Sleeve.Add(sCard);
            }
        }
        BattleFloatCardDrawer _drawer;
        BattleSide _side;

        public BattleFloatCard(FloatCard data, BattleSide side, bool withDrawer = true) : base(data, null, withDrawer: false)
        {
            _side = side;
            if (withDrawer)
                CreateDrawer(_side.Territory.transform);
        }
        protected BattleFloatCard(BattleFloatCard src, BattleFloatCardCloneArgs args) : base(src, args)
        {
            _side = args.srcCardSideClone;
        }

        public override void Dispose()
        {
            base.Dispose();
            _drawer?.Dispose();
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleFloatCardCloneArgs cArgs)
                 return new BattleFloatCard(this, cArgs);
            else return null;
        }
        public bool TryUse()
        {
            return TryUse(new TableFloatCardUseArgs(this, _side.Territory));
        }

        protected override void DrawerSetter(TableCardDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (BattleFloatCardDrawer)value;
        }
        protected override TableCardDrawer DrawerCreator(Transform parent)
        {
            BattleFloatCardDrawer drawer = new(this, parent);
            drawer.SetSortingOrder(10, asDefault: true);
            return drawer;
        }
    }
}
