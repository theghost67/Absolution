using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Представляет активный трейт на карте стола с возможностью отслеживания целей.
    /// </summary>
    public class BattleActiveTrait : TableActiveTrait, IBattleTrait
    {
        public new BattleActiveTraitDrawer Drawer => _drawer;
        public new BattleFieldCard Owner => _owner;
        public BattleTerritory Territory => _owner.Territory;
        public BattleSide Side => _owner.Side;

        public BattleArea Area => _area;
        public BattleRange Range => Data.range;
        public BattleWeight Weight => Data.Weight(this);

        readonly BattleFieldCard _owner;
        readonly BattleArea _area;
        BattleActiveTraitDrawer _drawer;

        public BattleActiveTrait(ActiveTrait data, BattleFieldCard owner, Transform parent, bool withDrawer = true) : base(data, owner, parent, withDrawer: false)
        {
            _owner = owner;
            _area = new BattleArea(this, owner);
            _area.OnCardSeen.Add(OnCardSeen);
            _area.OnCardUnseen.Add(OnCardUnseen);

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected BattleActiveTrait(BattleActiveTrait src, BattleActiveTraitCloneArgs args) : base(src, args)
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
            if (args is BattleActiveTraitCloneArgs cArgs)
                return new BattleActiveTrait(this, cArgs);
            else return null;
        }

        public bool TryUseWithAim(BattleSide by)
        {
            if (Owner == null) return false;
            if (Side != by) return false;
            if (!IsReadyToUse()) return false;

            _area.StartTargetAiming(AimFilter, AimFinished);
            return true;
        }

        protected override void DrawerSetter(TableTraitDrawer value)
        {
            _drawer = (BattleActiveTraitDrawer)value;
        }
        protected override TableTraitDrawer DrawerCreator(Transform parent)
        {
            return new BattleActiveTraitDrawer(this, parent);
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
            TryUse(aimedField);
        }
    }
}
