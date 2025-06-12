using Cysharp.Threading.Tasks;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Cards
{
    public class cCodeInterception : FloatCard
    {
        const string ID = "code_interception";
        const string CARD_ID = "code_loaf";

        public cCodeInterception() : base(ID)
        {
            name = "План \"Перехват\"";
            desc = "По коням! Вперёд, вперёд, вперёд!";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cCodeInterception(cCodeInterception other) : base(other) { }
        public override object Clone() => new cCodeInterception(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"Создаёт карты <nobr><u>{cardName}</u></nobr> напротив каждой вражеской карты (если поле напротив сводобно). " +
                   $"Здоровье карты будет равняться сумме здоровья и силы карты напротив.";
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            int[] stats = new int[] { 0, 0, -1, 0 };
            return new DescLinkCollection()
            { new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = stats } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            

            BattleFloatCard card = (BattleFloatCard)e.card;
            IEnumerable<BattleField> fields = card.Side.Opposite.Fields().WithCard();

            foreach (BattleField field in fields)
            {
                BattleField opposite = field.Opposite;
                if (opposite.Card != null) continue;
                BattleFieldCard fieldCard = field.Card;
                FieldCard newCard = CardBrowser.NewField(CARD_ID);
                newCard.health = fieldCard.Health + fieldCard.Strength;
                newCard.strength = 0;
                await card.Territory.PlaceFieldCard(newCard, opposite, card);
            }
        }
    }
}
