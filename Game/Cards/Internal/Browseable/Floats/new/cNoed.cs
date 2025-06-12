using Cysharp.Threading.Tasks;
using Game.Territories;
using Game.Traits;
using System.Linq;

namespace Game.Cards
{
    public class cNoed : FloatCard
    {
        const string ID = "noed";
        const string TRAIT_ID = "exposed";

        public cNoed() : base(ID)
        {
            name = "Никому не скрыться от смерти";
            desc = "Любимый всеми убийцами мира тотем, позволяющий убить любую жертву одним движением пальца. Не можешь победить? Бери НОЕД.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cNoed(cNoed other) : base(other) { }
        public override object Clone() => new cNoed(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"Даёт всем вражеским картам навык <nobr><u>{traitName}</u></nobr>.";
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            BattleFieldCard[] cards = card.Side.Opposite.Fields().WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard c in cards)
                await c.Traits.Passives.AdjustStacks(TRAIT_ID, 1, card);
        }
    }
}
