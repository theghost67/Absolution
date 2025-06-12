namespace Game.Cards
{
    public class cFinn : FieldCard
    {
        public cFinn() : base("finn", "furious_swing", "ooo")
        {
            name = "Финн";
            desc = "Мальчик, жизнь которого была наполнена самыми безумными испытаниями. Во время странствий по землям Ууу, он смог отточить " +
                   "свои навыки владения мечами до полного совершенства. Обычной зубочистки ему хватит, чтобы разрубить вас пополам. А ещё он любит помогать, " +
                   "поэтому непременно поможет вам отправиться на тот свет, если вы попробуете на него напасть.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cFinn(cFinn other) : base(other) { }
        public override object Clone() => new cFinn(this);
    }
}
