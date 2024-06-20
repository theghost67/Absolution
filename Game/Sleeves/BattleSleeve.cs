using Game.Territories;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий возможность подбирания стороной во время сражения карт рукава типа <see cref="IBattleSleeveCard"/> (из привязанной колоды карт).
    /// </summary>
    public class BattleSleeve : TableSleeve, IEnumerable<IBattleSleeveCard>
    {
        public new BattleSleeveDrawer Drawer => _drawer;
        public new BattleSleeveCardsCollection Cards => _cards;
        public BattleSide Side => _side;

        BattleSide _side;
        BattleSleeveCardsCollection _cards;
        BattleSleeveDrawer _drawer;

        public BattleSleeve(BattleSide side, Transform parent, bool withDrawer = true) : base(side.Deck, isForMe: side.isMe, parent, withDrawer: false) 
        {
            _side = side;
            _cards = (BattleSleeveCardsCollection)base.Cards;

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected BattleSleeve(BattleSleeve src, BattleSleeveCloneArgs args) : base(src, args) 
        {
            _side = args.srcSleeveSideClone;
            _cards = (BattleSleeveCardsCollection)base.Cards;

            args.TryOnClonedAction(src.GetType(), typeof(BattleSleeve));
        }

        public new IBattleSleeveCard this[int index] => _cards[index];
        public new IEnumerator<IBattleSleeveCard> GetEnumerator() => _cards.GetEnumerator();

        public override void Dispose()
        {
            base.Dispose();
            _cards.Clear();
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleSleeveCloneArgs cArgs)
                return new BattleSleeve(this, cArgs);
            else return null;
        }

        protected override void DrawerSetter(TableSleeveDrawer value)
        {
            base.DrawerSetter(value);
            _drawer = (BattleSleeveDrawer)value;
        }
        protected override TableSleeveDrawer DrawerCreator(Transform parent)
        {
            return new BattleSleeveDrawer(this, parent);
        }
        protected override ITableSleeveCardsCollection CollectionCreator()
        {
            return new BattleSleeveCardsCollection();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
