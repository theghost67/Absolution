using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tRecruitment : ActiveTrait
    {
        const string ID = "recruitment";
        static readonly TraitStatFormula _healthF = new(false, 0, 4);

        public tRecruitment() : base(ID)
        {
            name = "Перевербовка";
            desc = "Ты пойдёшь со мной.";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tRecruitment(tRecruitment other) : base(other) { }
        public override object Clone() => new tRecruitment(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на вражеской карте рядом",
                    $"Переманивает цель на поле напротив неё, если её здоровье ≤ {_healthF.Format(trait)}. и поле напротив свободно. Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(16, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.16f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && 
                e.isInBattle && 
                e.target.Card != null &&
                e.target.Card.Health <= _healthF.Value(e.trait) && 
                e.target.Opposite.Card == null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);
            IBattleTrait trait = (IBattleTrait)e.trait;

            await trait.SetStacks(0, trait.Side);
            await e.target.Card.TryAttachToField(e.target.Opposite, trait);
        }
    }
}
