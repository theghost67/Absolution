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
        public new BattleSleeveDrawer Drawer => ((TableObject)this).Drawer as BattleSleeveDrawer;
        public new BattleSleeveCardsCollection Cards => base.Cards as BattleSleeveCardsCollection;
        public BattleSide Side => _side;
        readonly BattleSide _side;

        public BattleSleeve(BattleSide side, Transform parent) : base(side.Deck, forMe: side.isMe, parent) 
        {
            _side = side;
            TryOnInstantiatedAction(GetType(), typeof(BattleSleeve));
        }
        protected BattleSleeve(BattleSleeve src, BattleSleeveCloneArgs args) : base(src, args) 
        {
            _side = args.srcSleeveSideClone;
            TryOnInstantiatedAction(GetType(), typeof(BattleSleeve));
        }

        public new IBattleSleeveCard this[int index] => Cards[index];
        public new IEnumerator<IBattleSleeveCard> GetEnumerator() => Cards.GetEnumerator();

        public override object Clone(CloneArgs args)
        {
            if (args is BattleSleeveCloneArgs cArgs)
                return new BattleSleeve(this, cArgs);
            else return null;
        }
        protected override Drawer DrawerCreator(Transform parent)
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
