﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Представляет пассивный навык на карте стола с возможностью отслеживания целей.
    /// </summary>
    public class BattlePassiveTrait : TablePassiveTrait, IBattleTrait, IBattleFighter
    {
        public new BattleFieldCard Owner => _owner;
        public new BattleTerritory Territory => _owner.Territory;
        public new BattleField Field => _owner.Field;
        public BattleSide Side => _owner.Side;

        public new BattlePassiveTraitDrawer Drawer => ((TableObject)this).Drawer as BattlePassiveTraitDrawer;

        public BattleArea Area => _area;
        public BattleRange Range => Data.range;

        readonly BattleFieldCard _owner;
        readonly BattleArea _area;
        readonly string _eventsGuid;

        public BattlePassiveTrait(PassiveTrait data, BattleFieldCard owner, Transform parent) : base(data, owner, parent)
        {
            _owner = owner;
            _area = new BattleArea(this, owner);
            _eventsGuid = this.GuidGen(3);

            _area.OnCardSeen.Add(_eventsGuid, OnCardSeen);
            _area.OnCardUnseen.Add(_eventsGuid, OnCardUnseen);
            TryOnInstantiatedAction(GetType(), typeof(BattlePassiveTrait));
        }
        protected BattlePassiveTrait(BattlePassiveTrait src, BattlePassiveTraitCloneArgs args) : base(src, args)
        {
            _owner = args.srcTraitOwnerClone;
            BattleAreaCloneArgs areaCArgs = new(this, _owner, args.terrCArgs);
            _area = (BattleArea)src._area.Clone(areaCArgs);
            TryOnInstantiatedAction(GetType(), typeof(BattlePassiveTrait));
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

        public BattleWeight CalculateWeight(int[] excludedWeights)
        {
            return Data.Weight(this);
        }

        protected override Drawer DrawerCreator(Transform parent)
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
