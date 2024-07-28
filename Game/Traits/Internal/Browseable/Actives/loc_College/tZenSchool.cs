using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tZenSchool : ActiveTrait
    {
        const string ID = "zen_school";
        const int STRENGTH_TO_HEALTH_ABS = 2;
        const int COOLDOWN = 2;

        public tZenSchool() : base(ID)
        {
            name = "Школа дзена";
            desc = "Познай путь дзена, брат мой.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tZenSchool(tZenSchool other) : base(other) { }
        public override object Clone() => new tZenSchool(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на любой союзной карте",
                    $"перенаправляет всю силу цели в её здоровье: 1 ед. силы = {STRENGTH_TO_HEALTH_ABS} ед здоровья. Так же восстанавливает своё здоровье на то же значение. Перезарядка: {COOLDOWN} х."),
            });
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleActiveTrait trait)
        {
            return new(0, 0.20f);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null && e.target.Card != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleFieldCard card = (BattleFieldCard)e.target.Card;
            int strength = card.strength;

            e.trait.Storage.turnsDelay += COOLDOWN;
            await card.strength.AdjustValue(-strength, e.trait);
            await card.health.AdjustValue(strength * 2, e.trait);
            await e.trait.Owner.health.AdjustValue(strength, e.trait);
        }
    }
}
