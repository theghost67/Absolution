using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLunchBreak : ActiveTrait
    {
        const string ID = "lunch_break";
        const int HEALTH_ABS_INCREASE = 1;
        const int COOLDOWN = 1;

        public tLunchBreak() : base(ID)
        {
            name = "Перекус";
            desc = "Неплохо было бы подкрепиться.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tLunchBreak(tLunchBreak other) : base(other) { }
        public override object Clone() => new tLunchBreak(this);

        public override string DescRich(ITableTrait trait)
        {
            int effect = HEALTH_ABS_INCREASE * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на карте рядом",
                    $"Лечит цель на <u>{effect}</u> ед. Перезарядка: {COOLDOWN} х."),
                new($"При использовании на территории на поле рядом",
                    $"Лечит сторону, которая владеет этим полем, на <u>{effect}</u> ед. Перезарядка: {COOLDOWN} х."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 6 * (stacks - 1);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
        {
            return BattleWeight.none;
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;

            trait.Storage.turnsDelay += COOLDOWN;
            if (target.Card != null)
                 await target.Card.health.AdjustValue(HEALTH_ABS_INCREASE * trait.GetStacks(), trait);
            else await target.health.AdjustValue(HEALTH_ABS_INCREASE * trait.GetStacks(), trait);
        }
    }
}
