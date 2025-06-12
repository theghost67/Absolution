using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tItsGuard : PassiveTrait
    {
        const string ID = "its_guard";
        const string KEY = "turn";
        static readonly TraitStatFormula _strengthF = new(true, 0.50f, 0.00f);

        public tItsGuard() : base(ID)
        {
            name = "It's a guard!";
            desc = "ОУ ЩИТ, ИТС ДЖАГЕР!";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeDouble);
        }
        protected tItsGuard(tItsGuard other) : base(other) { }
        public override object Clone() => new tItsGuard(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При появлении вражеской карты рядом с владельцем</color>\n" +
                   $"Если поле напротив цели свободно, перемещает владельца на это поле и атакует цель с силой в {_strengthF.Format(args.stacks, true)} от силы владельца. " +
                   $"Не сработает, если цель расположена напротив владельца. Эффект может быть активирован только один раз за ход.";
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            BattleFieldCard owner = trait.Owner;
            if (!e.canSeeTarget || e.target.IsKilled || owner.IsKilled || owner.Field == null) return;

            if (trait.Storage.TryGetValue(KEY, out object turn) && (int)turn == trait.TurnAge)
                return;

            BattleField targetOppositeField = e.target.Field.Opposite;
            if (targetOppositeField.Card != null) return;

            trait.Storage[KEY] = trait.TurnAge;
            await trait.AnimDetectionOnSeen(e.target);
            await owner.TryAttachToField(targetOppositeField, trait);
            if (owner.Field != targetOppositeField) return;

            int strength = (int)Mathf.Ceil(trait.Owner.Strength * _strengthF.Value(e.traitStacks));
            BattleInitiationSendArgs initiation = new(trait.Owner, strength, true, false, e.target.Field);
            await trait.Territory.Initiations.EnqueueAndAwait(initiation);
        }        
    }
}
