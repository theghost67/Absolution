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
        static readonly TraitStatFormula _healthF = new(false, 2, 0);
        const int CD = 1;

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

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return "<color>При использовании на любой союзной карте</color>\n" +
                   $"Перенаправляет всю силу цели в её здоровье: 1 ед. силы = {_healthF.Format(args.stacks)} здоровья. Так же восстанавливает своё здоровье на то же значение. Перезарядка: {CD} х.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.40f);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null && e.target.Card != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            ITableTrait trait = e.trait;
            BattleFieldCard card = (BattleFieldCard)e.target.Card;
            int strength = card.Strength;

            trait.SetCooldown(CD);
            await card.Strength.AdjustValue(-strength, trait);
            await card.Health.AdjustValue(strength * _healthF.ValueInt(e.traitStacks), trait);
            await trait.Owner.Health.AdjustValue(strength, trait);
        }
    }
}
