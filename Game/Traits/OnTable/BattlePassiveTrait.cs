using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Представляет пассивный трейт на карте стола с возможностью отслеживания целей.
    /// </summary>
    public class BattlePassiveTrait : TablePassiveTrait, IBattleTrait
    {
        public new BattlePassiveTraitDrawer Drawer => _drawer;
        public new BattleFieldCard Owner => _owner;
        public BattleTerritory Territory => _owner.Territory;
        public BattleSide Side => _owner.Side;

        public BattleArea Area => _area;
        public BattleRange Range => Data.range;
        public BattleWeight Weight => Data.Weight(this);

        readonly BattleFieldCard _owner;
        readonly BattleArea _area;
        BattlePassiveTraitDrawer _drawer;

        public BattlePassiveTrait(PassiveTrait data, BattleFieldCard owner, Transform parent, bool withDrawer = true) : base(data, owner, parent, withDrawer: false)
        {
            _owner = owner;
            _area = new BattleArea(this, owner);
            _area.OnCardSeen.Add(OnCardSeen);
            _area.OnCardUnseen.Add(OnCardUnseen);

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected BattlePassiveTrait(BattlePassiveTrait src, BattlePassiveTraitCloneArgs args) : base(src, args)
        {
            _owner = args.srcTraitOwnerClone;

            BattleAreaCloneArgs areaCArgs = new(this, _owner, args.terrCArgs);
            _area = (BattleArea)src._area.Clone(areaCArgs);
        }

        public override void Dispose()
        {
            base.Dispose();
            _area.Dispose();
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattlePassiveTraitCloneArgs cArgs)
                return new BattlePassiveTrait(this, cArgs);
            else return null;
        }

        protected override void DrawerSetter(TableTraitDrawer value)
        {
            _drawer = (BattlePassiveTraitDrawer)value;
        }
        protected override TableTraitDrawer DrawerCreator(Transform parent)
        {
            return new BattlePassiveTraitDrawer(this, parent);
        }

        UniTask OnCardSeen(object sender, BattleFieldCard card)
        {
            BattleArea area = (BattleArea)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)area.observer;
            return trait.Data.OnTargetStateChanged(new BattleTraitTargetStateChangeArgs(trait, card, true));
        }
        UniTask OnCardUnseen(object sender, BattleFieldCard card)
        {
            BattleArea area = (BattleArea)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)area.observer;
            return trait.Data.OnTargetStateChanged(new BattleTraitTargetStateChangeArgs(trait, card, false));
        }
    }
}
