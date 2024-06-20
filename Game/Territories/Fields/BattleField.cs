using Cysharp.Threading.Tasks;
using Game.Cards;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий игровое поле во время сражения, принадлежащее одной из сторон, с возможностью хранения карты типа <see cref="BattleFieldCard"/>.
    /// </summary>
    public class BattleField : TableField, IBattleEntity
    {
        public new BattleFieldCard Card => _card;
        public new BattleFieldDrawer Drawer => _drawer;
        public new BattleTerritory Territory => _side.Territory;

        public BattleSide Side => _side;
        public string TableName => $"Поле {this.PosToStringRich()}";

        readonly BattleSide _side;
        BattleFieldCard _card;
        BattleFieldDrawer _drawer;

        public BattleField(BattleSide side, int2 position, Transform parent, bool withDrawer = true) : base(side.Territory, position, null, withDrawer: false)
        {
            _side = side;
            health.OnPostSet.Add(OnHealthPostSet);

            if (withDrawer)
                CreateDrawer(parent);
        }
        public BattleField(BattleField src, BattleFieldCloneArgs args) : base(src, args) 
        {
            _side = args.srcFieldSideClone;
            args.TryOnClonedAction(src.GetType(), typeof(BattleField));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleFieldCloneArgs cArgs)
                return new BattleField(this, cArgs);
            else return null;
        }

        protected override void DrawerSetter(TableFieldDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (BattleFieldDrawer)value;
        }
        protected override TableFieldDrawer DrawerCreator(Transform parent)
        {
            return new BattleFieldDrawer(this, parent);
        }

        protected override void CardPropSetter(TableFieldCard card)
        {
            base.CardPropSetter(card);
        }
        protected override void CardBaseSetter(TableFieldCard value)
        {
            base.CardBaseSetter(value);
            _card = (BattleFieldCard)value;
        }
        protected override TableFieldCard CardCloner(TableFieldCard src, TableFieldCloneArgs args)
        {
            if (src == null) return null;
            BattleFieldCloneArgs argsCast = (BattleFieldCloneArgs)args;
            BattleFieldCardCloneArgs cardCArgs = new((FieldCard)src.Data.Clone(), this, _side, argsCast.terrCArgs);
            return (BattleFieldCard)src.Clone(cardCArgs);
        }

        UniTask OnHealthPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleField field = (BattleField)stat.Owner;
            return field._side.health.AdjustValueAbs(e.totalDeltaValue);
        }

        // TODO: add GetObservingEntities?
    }
}
