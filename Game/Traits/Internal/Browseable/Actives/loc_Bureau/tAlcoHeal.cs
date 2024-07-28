using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAlcoHeal : ActiveTrait
    {
        const string ID = "alco_heal";
        const int HEALTH_ABS_INCREASE_PER_STACK = 2;
        const int MOXIE_ABS_DECREASE_STATIC = 2;
        const int COOLDOWN = 1;

        public tAlcoHeal() : base(ID)
        {
            name = "Алкогольная подпитка";
            desc = "";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tAlcoHeal(tAlcoHeal other) : base(other) { }
        public override object Clone() => new tAlcoHeal(this);

        public override string DescRich(ITableTrait trait)
        {
            float health = HEALTH_ABS_INCREASE_PER_STACK * trait.GetStacks();
            const int MOXIE = MOXIE_ABS_DECREASE_STATIC;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на карте рядом",
                    $"Восстанавливает <u>{health}</u> ед. здоровья цели, уменьшает её инициативу на {MOXIE} ед. Перезарядка: {COOLDOWN} х."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 8 * (stacks - 1);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
        {
            return new(HEALTH_ABS_INCREASE_PER_STACK * 2 * trait.GetStacks());
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard card = (BattleFieldCard)e.target.Card;

            float health = HEALTH_ABS_INCREASE_PER_STACK * trait.GetStacks();
            const int MOXIE = -MOXIE_ABS_DECREASE_STATIC;

            trait.Storage.turnsDelay += COOLDOWN;
            await card.health.AdjustValue(health * trait.GetStacks(), trait);
            await card.moxie.AdjustValue(MOXIE, trait);
        }
    }
}
