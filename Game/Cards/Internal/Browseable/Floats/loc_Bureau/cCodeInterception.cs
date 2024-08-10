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
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 4);
            frequency = 1.00f;
        }
        protected cCodeInterception(cCodeInterception other) : base(other) { }
        public override object Clone() => new cCodeInterception(this);

        public override string DescRich(ITableCard card)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return DescRichBase(card, $"Создаёт карты <i>{cardName}</i> напротив каждой вражеской карты (если поле напротив сводобно). " +
                                      $"Здоровье карты будет равняться сумме здоровья и силы карты напротив.");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            await base.OnUse(e);

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
