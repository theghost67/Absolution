using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTesting : ActiveTrait
    {
        const string ID = "testing";
        static readonly TraitStatFormula _moxieF = new(false, 0, 0);
        static readonly TraitStatFormula _strengthF = new(false, 9999, 0);
        static readonly TerritoryRange targets = TerritoryRange.oppositeAll;

        public tTesting() : base(ID)
        {
            name = "Тестирование";
            desc = "Итак, начинаем ТЕСТИРОВАНИЕ.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tTesting(tTesting other) : base(other) { }
        public override object Clone() => new tTesting(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При использовании</color>\nТестирует карты все карты напротив владельца на прочность - " +
                   $"если её инициатива ≤ {_moxieF.Format(args.stacks)}, ей будет нанесено {_strengthF.Format(args.stacks)} урона. Тратит все заряды.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.16f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            int moxie = _moxieF.ValueInt(e.traitStacks);
            int strength = _strengthF.ValueInt(e.traitStacks);

            await trait.SetStacks(0, owner.Side);
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, targets).WithCard();
            foreach (BattleFieldCard card in fields.Select(f => f.Card))
            {
                if (card.Moxie <= moxie)
                    await card.Health.AdjustValue(-strength, owner);
            }
        }
    }
}
