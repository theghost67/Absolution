using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;
using Game.Effects;

namespace Game.Traits
{
    /// <summary>
    /// Представляет активный навык на карте стола с возможностью отслеживания целей.
    /// </summary>
    public class BattleActiveTrait : TableActiveTrait, IBattleTrait
    {
        public new BattleFieldCard Owner => _owner;
        public new BattleTerritory Territory => _owner.Territory;
        public new BattleField Field => _owner.Field;
        public BattleSide Side => _owner.Side;

        public new BattleActiveTraitDrawer Drawer => ((TableObject)this).Drawer as BattleActiveTraitDrawer;

        public BattleArea Area => _area;
        public BattleRange Range => Data.range;
        public BattleWeight Weight => Data.Weight(this);

        readonly BattleFieldCard _owner;
        readonly BattleArea _area;
        readonly string _eventsGuid;

        public BattleActiveTrait(ActiveTrait data, BattleFieldCard owner, Transform parent) : base(data, owner, parent)
        {
            _owner = owner;
            _area = new BattleArea(this, owner);
            _eventsGuid = this.GuidGen(3);

            _area.OnCardSeen.Add(_eventsGuid, OnCardSeen);
            _area.OnCardUnseen.Add(_eventsGuid, OnCardUnseen);
            TryOnInstantiatedAction(GetType(), typeof(BattleActiveTrait));
        }
        protected BattleActiveTrait(BattleActiveTrait src, BattleActiveTraitCloneArgs args) : base(src, args)
        {
            _owner = args.srcTraitOwnerClone;
            BattleAreaCloneArgs areaCArgs = new(this, _owner, args.terrCArgs);
            _area = (BattleArea)src._area.Clone(areaCArgs);
            TryOnInstantiatedAction(GetType(), typeof(BattleActiveTrait));
        }

        public override void Dispose()
        {
            base.Dispose();
            _area.Dispose();
        }
        public override object Clone(CloneArgs args)
        {
            if (args is BattleActiveTraitCloneArgs cArgs)
                return new BattleActiveTrait(this, cArgs);
            else return null;
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new BattleActiveTraitDrawer(this, parent);
        }

        public bool TryUseWithAim(BattleSide by)
        {
            if (Owner == null) return false;
            if (Side != by) return false;
            if (Territory.PhaseSide != by) return false;
            if (TurnDelay > 0) return false;
            if (!IsReadyToUse())
            {
                if (_owner.Field != null)
                    _owner.Drawer?.CreateTextAsSpeech("НЕТ ЦЕЛЕЙ", Color.red);
                return false;
            }
            _area.StartTargetAiming(AimFilter, AimFinished);
            return true;
        }

        UniTask OnCardSeen(object sender, BattleFieldCard card)
        {
            BattleArea area = (BattleArea)sender;
            BattleActiveTrait trait = (BattleActiveTrait)area.observer;
            return trait.Data.OnTargetStateChanged(new BattleTraitTargetStateChangeArgs(trait, card, true));
        }
        UniTask OnCardUnseen(object sender, BattleFieldCard card)
        {
            BattleArea area = (BattleArea)sender;
            BattleActiveTrait trait = (BattleActiveTrait)area.observer;
            return trait.Data.OnTargetStateChanged(new BattleTraitTargetStateChangeArgs(trait, card, false));
        }

        bool IsReadyToUse()
        {
            bool isUsable = _owner.Field == null && Data.IsUsable(new TableActiveTraitUseArgs(this, null));
            if (!isUsable && _owner.Field != null)
            {
                foreach (BattleField possibleTarget in Territory.Fields(Owner.Field.pos, Range.potential))
                {
                    if (!Data.IsUsable(new TableActiveTraitUseArgs(this, possibleTarget))) continue;
                    isUsable = true;
                    break;
                }
            }
            return isUsable;
        }
        bool AimFilter(BattleField aimedField)
        {
            return Data.IsUsable(new TableActiveTraitUseArgs(this, aimedField));
        }
        void AimFinished(BattleField aimedField)
        {
            TableActiveTraitUseArgs args = new(this, aimedField);
            PlayerQueue.Enqueue(new PlayerAction()
            {
                conditionFunc = () => IsUsable(args),
                successFunc = () => TryUse(aimedField),
            });
        }
    }
}
