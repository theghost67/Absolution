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
        const int HEALTH_THRESHOLD_PER_STACK = 4;

        public tRecruitment() : base(ID)
        {
            name = "Перевербовка";
            desc = "";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tRecruitment(tRecruitment other) : base(other) { }
        public override object Clone() => new tRecruitment(this);

        public override string DescRich(ITableTrait trait)
        {
            int effect = HEALTH_THRESHOLD_PER_STACK * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на вражеской карте рядом",
                    $"Переманивает цель на поле напротив неё, если её здоровье ≤ {effect} ед. и поле напротив свободно. Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 16 * (stacks - 1);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
        {
            return new(0, 0.16f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Opposite.Card == null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);
            BattleActiveTrait trait = (BattleActiveTrait)e.trait;

            await e.target.Card.TryAttachToField(e.target.Opposite, trait);
            await trait.SetStacks(0, trait.Side);
        }
    }
}
