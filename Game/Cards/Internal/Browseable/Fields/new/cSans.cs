namespace Game.Cards
{
    public class cSans : FieldCard
    {
        public cSans() : base("sans", "bad_time", "special_attack")
        {
            name = "Санс";
            desc = "Этот скелет знает как нужно паКОСТИть! Но, как только он заметит существо с малейшим признаком проявления жестокости к другим, его ждёт урок морали... " +
                   "А если вдруг и это не поможет, что ж, ему следует приготовиться к НЕСКОНЧАЕМЫМ СТРАДАНИЯМ. Но не волнуйтесь, Санс добрый - он всегда даёт возможность уйти. (что бы это не значило...)";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cSans(cSans other) : base(other) { }
        public override object Clone() => new cSans(this);
    }
}
