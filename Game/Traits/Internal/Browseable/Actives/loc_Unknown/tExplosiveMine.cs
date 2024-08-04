using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tExplosiveMine : ActiveTrait
    {
        const string ID = "explosive_mine";
        const string TRAIT_ID = "explosive";
        const string CARD_ID = "mine";

        public tExplosiveMine() : base(ID)
        {
            name = "Фугасная мина";
            desc = "Пора создать себе нового друга!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tExplosiveMine(tExplosiveMine other) : base(other) { }
        public override object Clone() => new tExplosiveMine(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            int stacks = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на пустом союзном поле",
                    $"Создаёт карту <i>{cardName}</i> с {stacks} зарядами навыка <i>{traitName}</i>. Тратит все заряды."),
            });
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.16f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            FieldCard card = CardBrowser.NewField(CARD_ID);

            card.traits.AdjustStacks(TRAIT_ID, trait.GetStacks());
            await trait.Territory.PlaceFieldCard(card, target, trait);
            await trait.SetStacks(0, trait.Side);
        }
    }
}
