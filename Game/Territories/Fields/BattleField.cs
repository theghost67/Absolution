using Cysharp.Threading.Tasks;
using Game.Cards;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий игровое поле во время сражения, принадлежащее одной из сторон, с возможностью хранения карты типа <see cref="BattleFieldCard"/>.
    /// </summary>
    public class BattleField : TableField, IBattleObject
    {
        public new BattleTerritory Territory => _side.Territory;
        public new BattleField Opposite => _side.Territory?.FieldOpposite(pos);
        public new BattleFieldCard Card => base.Card as BattleFieldCard;
        public new BattleFieldDrawer Drawer => ((TableObject)this).Drawer as BattleFieldDrawer;
        public BattleSide Side => _side;
        readonly BattleSide _side;

        public BattleField(BattleSide side, int2 position, Transform parent) : base(side.Territory, position, parent)
        {
            _side = side;
            TryOnInstantiatedAction(GetType(), typeof(BattleField));
        }
        public BattleField(BattleField src, BattleFieldCloneArgs args) : base(src, args) 
        {
            _side = args.srcFieldSideClone;
            TryOnInstantiatedAction(GetType(), typeof(BattleField));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleFieldCloneArgs cArgs)
                return new BattleField(this, cArgs);
            else return null;
        }

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new BattleFieldDrawer(this, parent);
        }
        protected override TableFieldCard CardCloner(TableFieldCard src, TableFieldCloneArgs args)
        {
            if (src == null) return null;
            BattleFieldCloneArgs argsCast = (BattleFieldCloneArgs)args;
            BattleFieldCardCloneArgs cardCArgs = new((FieldCard)src.Data.Clone(), this, _side, argsCast.terrCArgs);
            return (BattleFieldCard)src.Clone(cardCArgs);
        }
        protected override UniTask OnHealthPostSetBase(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleField field = (BattleField)stat.Owner;
            return field._side.Health.AdjustValue(e.totalDeltaValue, e.source);
        }

        public BattleWeight CalculateWeight(int[] excludedWeights)
        {
            return BattleWeight.Zero(this);
        }
    }
}
