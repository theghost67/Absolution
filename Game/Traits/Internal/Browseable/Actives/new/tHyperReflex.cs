using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHyperReflex : ActiveTrait
    {
        const string ID = "hyper_reflex";
        const string REFLEX_KEY = "reflex";
        const string TURN_KEY = "turn";

        enum ReflexModeState
        {
            Removed,
            Enabled,
            Disabled,
            Cooldown,
        }

        public tHyperReflex() : base(ID)
        {
            name = "Сверхрефлекс";
            desc = "Я быстрый, как ветер!.. Сегодня штиль, идиот.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tHyperReflex(tHyperReflex other) : base(other) { }
        public override object Clone() => new tHyperReflex(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            ReflexModeState reflexState = GetReflexModeState(args.table);
            string str = "<color>При активации на территории или в рукаве</color>\nПереключает режим рефлекса. При включённом режиме рефлекса, " +
                         "один раз за ход позволяет уклониться от атаки путём перемещения на соседнее поле (левое, правое).";
            if (reflexState != ReflexModeState.Removed)
            {
                string reflexModeStr = reflexState != ReflexModeState.Disabled ? "<color=green>ВКЛ</color>" : "<color=red>ВЫКЛ</color>";
                str += $" Режим рефлекса: {reflexModeStr}.";
            }
            return str;
        }

        public override bool IsUsableInSleeve()
        {
            return true;
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && GetReflexModeState(e.trait) != ReflexModeState.Cooldown;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            ReflexModeState reflexMode = GetReflexModeState(trait);

            if (reflexMode == ReflexModeState.Disabled)
            {
                await e.AnimActivationShort($"{name}: ВКЛ");
                owner.OnInitiationPreReceived.Add(trait.GuidStr, OnInitiationPreReceived);
                SetReflexModeState(trait, ReflexModeState.Enabled);
            }
            else
            {
                await e.AnimActivationShort($"{name}: ВЫКЛ");
                owner.OnInitiationPreReceived.Remove(trait.GuidStr);
                SetReflexModeState(trait, ReflexModeState.Disabled);
            }
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;
            SetReflexModeState(e.trait, ReflexModeState.Disabled);
            e.trait.Storage[TURN_KEY] = -1;
        }

        async UniTask OnInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard attacker = (BattleFieldCard)sender;
            BattleTerritory terr = attacker.Territory;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.ReceiverField != trait.Field) return;

            int lastActivationTurn = (int)trait.Storage[TURN_KEY];
            if (trait.TurnAge == lastActivationTurn) return;

            trait.Storage[TURN_KEY] = trait.TurnAge;
            BattleField[] freeFields = terr.Fields(trait.Field.pos, TerritoryRange.ownerDouble).WithoutCard().ToArray();
            if (freeFields.Length == 0) return;

            await trait.AnimActivation();
            await trait.Owner.TryAttachToField(freeFields.First(), trait);
        }

        ReflexModeState GetReflexModeState(ITableTrait trait)
        {
            bool hasKey = trait.Storage.TryGetValue(REFLEX_KEY, out object isInReflexMode);
            if (!hasKey)
                return ReflexModeState.Removed;
            else if (isInReflexMode == null)
                return ReflexModeState.Disabled;
            else if ((bool)isInReflexMode)
                return ReflexModeState.Enabled;
            else return ReflexModeState.Cooldown;
        }
        void SetReflexModeState(ITableTrait trait, ReflexModeState value)
        {
            switch (value)
            {
                case ReflexModeState.Removed: trait.Storage.Remove(REFLEX_KEY); break;
                case ReflexModeState.Enabled: trait.Storage[REFLEX_KEY] = true; break;
                case ReflexModeState.Disabled: trait.Storage[REFLEX_KEY] = null; break;
                case ReflexModeState.Cooldown: trait.Storage[REFLEX_KEY] = false; break;
            }
        }
    }
}
