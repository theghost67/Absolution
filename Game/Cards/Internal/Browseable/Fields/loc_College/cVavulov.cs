namespace Game.Cards
{
    public class cVavulov : FieldCard
    {
        public cVavulov() : base("vavulov", "p competitive_obsession", "p scholar")
        {
            name = "Вавулов";
            desc = "Особая паразитарная форма жизни, распространённая в образовательных учреждениях и выживающая " +
                   "только за счёт своего хозяина - ученика поумнее его. При его потере, становится полностью беспомощным.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 1f;
        }
        protected cVavulov(cVavulov other) : base(other) { }
        public override object Clone() => new cVavulov(this);
    }
}
