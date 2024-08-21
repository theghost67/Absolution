using Cysharp.Threading.Tasks;
using Game.Sleeves;
using Game.Territories;
using MyBox;
using System;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий карту без характеристик во время сражения.
    /// </summary>
    public class BattleFloatCard : TableFloatCard, IBattleCard
    {
        public new BattleFloatCardDrawer Drawer => ((TableObject)this).Drawer as BattleFloatCardDrawer;
        public override string TableName => base.TableName.Colored(Side.GetSideColor());

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

        public BattleFloatCard(FloatCard data, BattleSide side) : base(data, side.Territory.Transform)
        {
            _side = side;
            TryOnInstantiatedAction(GetType(), typeof(TableFloatCard));
        }
        protected BattleFloatCard(BattleFloatCard src, BattleFloatCardCloneArgs args) : base(src, args)
        {
            _side = args.srcCardSideClone;
            TryOnInstantiatedAction(GetType(), typeof(TableFloatCard));
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

        public async UniTask TryAttachToSideSleeve(BattleSide side, ITableEntrySource source)
        {
            if (this is not ITableSleeveCard sCard)
                throw new InvalidCastException($"Card should implement {nameof(ITableSleeveCard)} interface to have the ability to be attached to sleeves.");
            if (_side.Sleeve.Contains(sCard))
                return;

            _side.Sleeve.Remove(sCard);
            _side = side;
            _side.Sleeve.Add(sCard);
            await UniTask.Delay((int)(ITableSleeveCard.PULL_DURATION * 1000));
        }
        public UniTask TryUse()
        {
            return TryUse(new TableFloatCardUseArgs(this, _side.Territory));
        }

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new BattleFloatCardDrawer(this, parent) { SortingOrderDefault = 10 };
        }
    }
}
