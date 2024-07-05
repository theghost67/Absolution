using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tLunchBreak : ActiveTrait
    {
        const string ID = "lunch_break";
        const int HEALTH_INCREASE = 1;

        public tLunchBreak() : base(ID)
        {
            name = "Перекус";
            desc = "Неплохо было бы подкрепиться.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tLunchBreak(tLunchBreak other) : base(other) { }
        public override object Clone() => new tLunchBreak(this);

        public override string DescRich(ITableTrait trait)
        {
            int effect = HEALTH_INCREASE * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на карте рядом, кроме себя (Т)",
                    $"лечит цель на <u>{effect}</u> ед. Перезарядка: 1 ход."),
                new($"При использовании на поле рядом, кроме себя (Т)",
                    $"лечит сторону, которая владеет этим полем, на <u>{effect}</u> ед. Перезарядка: 1 ход."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 2 * stacks;
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

            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            BattleField target = (BattleField)e.target;

            trait.Storage.turnsDelay++;
            if (target.Card != null)
                 await target.Card.health.AdjustValue(HEALTH_INCREASE * trait.GetStacks(), trait);
            else await target.health.AdjustValue(HEALTH_INCREASE * trait.GetStacks(), trait);
        }
    }
}
