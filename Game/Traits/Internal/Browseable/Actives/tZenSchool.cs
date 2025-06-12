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
            return "<color>При активации на любой союзной карте</color>\n" +
                   $"Перенаправляет всю силу цели в её здоровье: 1 ед. силы = {_healthF.Format(args.stacks)} здоровья. Так же восстанавливает своё здоровье на то же значение. Перезарядка: {CD} х.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _healthF.Value(result.Entity.GetStacks()) * 2);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null 
                && e.target.Card != null && e.target.Card.Strength.ValueRaw < 1;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard card = (BattleFieldCard)e.target.Card;
            int strength = card.Strength;

            await card.Strength.AdjustValue(-strength, trait);
            if (!card.IsKilled)
                await card.Health.AdjustValue(strength * _healthF.ValueInt(e.traitStacks), trait);
            if (!trait.Owner.IsKilled)
                await trait.Owner.Health.AdjustValue(strength, trait);
            trait.SetCooldown(CD);
        }
    }
}
