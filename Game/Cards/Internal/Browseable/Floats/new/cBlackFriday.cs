using Cysharp.Threading.Tasks;
using System.Linq;

namespace Game.Cards
{
    public class cBlackFriday : FloatCard
    {
        const string ID = "black_friday";

        public cBlackFriday() : base(ID)
        {
            name = "Чёрная пятница";
            desc = "Некто заметил чёрную карту, валявшуюся на полу, на ней было выгравировано название - Чёрная пятница. Попробовал расплатиться ею, пользователь осознал, " +
                   "что эта карта позволяет покупать товары с огромными скидками, даже если таковых нет. Неизвестный скупал машины, магазины, танки, компании. " +
                   "Его активность быстро обнаружили представители Фонда, забрав карточку-аномалию и его самого с собой.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBlackFriday(cBlackFriday other) : base(other) { }
        public override object Clone() => new cBlackFriday(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return $"Даёт стороне-владельцу золото, равное количеству установленных карт на его стороне территории.";
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            int cardsCount = card.Side.Fields().WithCard().ToArray().Length;
            await card.Side.Gold.AdjustValue(cardsCount, card);
        }
    }
}
