using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfAttack : ActiveTrait
    {
        const string ID = "order_of_attack";
        const string TRAIT_ID_TO_GIVE = "order_of_attack_wait";
        const string TRAIT_ID_TO_REMOVE = "order_of_defence";
        static readonly TraitStatFormula _healthDecF = new(true, 0.50f, 0.50f);
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tOrderOfAttack() : base(ID)
        {
            name = "Приказ об атаке";
            desc = "За родину мать!";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tOrderOfAttack(tOrderOfAttack other) : base(other) { }
        public override object Clone() => new tOrderOfAttack(this);

        public override string DescRich(ITableTrait trait)
        {
            int stacks = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории",
                    $"Все карты рядом с владельцем в начале след. хода инициируют свои действия <u>{stacks}</u> раз. " +
                    $"Уменьшает здоровье у стороны-владельца на {_healthDecF.Format(trait, stacks)}. Тратит все заряды всех видов приказов у владельца."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(800, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.30f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = (BattleFieldCard)e.target.Card;
            BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, _range).WithCard().Select(f => f.Card).ToArray();

            int stacks = trait.GetStacks();
            foreach (BattleFieldCard card in cards)
                await card.Traits.AdjustStacks(TRAIT_ID_TO_GIVE, stacks, trait);

            float health = -_healthDecF.Value(trait, stacks);
            await owner.Side.Health.AdjustValueScale(health, trait);

            await owner.Traits.SetStacks(ID, 0, trait.Side);
            await owner.Traits.SetStacks(TRAIT_ID_TO_REMOVE, 0, trait.Side);
        }
    }
}
