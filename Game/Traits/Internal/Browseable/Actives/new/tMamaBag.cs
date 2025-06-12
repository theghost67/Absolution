using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMamaBag : ActiveTrait
    {
        const string ID = "mama_bag";
        const string CARD_ID = "market";
        const string TRAIT_ID = "shopping";

        public tMamaBag() : base(ID)
        {
            name = "У мамочки новая сумочка";
            desc = "Да-да-да, ставь рынок наконец!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tMamaBag(tMamaBag other) : base(other) { }
        public override object Clone() => new tMamaBag(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"<color>При активации на незанятом союзном поле рядом</color>\nУстанавливает карту <nobr><u>{cardName}</u></nobr> на указанное поле. Тратит все заряды.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            TraitStacksPair[] traits = new TraitStacksPair[]
            { new(TRAIT_ID, 1) };

            return new DescLinkCollection()
            {
                new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats, linkTraits = traits },
                new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = traits[0].stacks }
            };
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            FieldCard card = CardBrowser.NewField(CARD_ID);
            await trait.Territory.PlaceFieldCard(card, target, trait);
            await trait.SetStacks(0, trait.Side);
        }
    }
}
