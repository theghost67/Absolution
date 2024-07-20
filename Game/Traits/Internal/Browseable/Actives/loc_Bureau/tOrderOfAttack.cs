using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfAttack : ActiveTrait
    {
        const string ID = "order_of_attack";
        const string TRAIT_ID = "order_of_attack_wait";
        const float SIDE_HEALTH_REL_DECREASE = 1.00f;
        static readonly TerritoryRange targets = TerritoryRange.ownerDouble;

        public tOrderOfAttack() : base(ID)
        {
            name = "Приказ об атаке";
            desc = "";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tOrderOfAttack(tOrderOfAttack other) : base(other) { }
        public override object Clone() => new tOrderOfAttack(this);

        public override string DescRich(ITableTrait trait)
        {
            float health = SIDE_HEALTH_REL_DECREASE * 100;
            int effect = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории",
                    $"Все карты рядом с владельцем в начале след. хода инициируют свои действия {effect} раз. " +
                    $"Уменьшает здоровье у стороны-владельца на {health}%. Тратит все заряды всех видов приказов у владельца."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 800 * Mathf.Pow(stacks - 1, 2);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
        {
            return new(0, 0.2f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            BattleFieldCard owner = (BattleFieldCard)e.target.Card;
            BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, targets).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            int stacks = trait.GetStacks();
            foreach (BattleFieldCard card in cards)
                await card.Traits.Passives.AdjustStacks(TRAIT_ID, stacks, trait);

            float health = SIDE_HEALTH_REL_DECREASE * 100;
            await owner.Side.health.AdjustValueScale(-health, trait);

            await owner.Traits.Passives.SetStacks(ID, 0, trait.Side);
            await owner.Traits.Passives.SetStacks(TRAIT_ID, 0, trait.Side);
        }
    }
}
