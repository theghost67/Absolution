namespace Game.Cards
{
    public class cPigeonLitter : FieldCard
    {
        public cPigeonLitter() : base("pigeon_litter")
        {
            name = "Голубиный помёт";
            desc = "Мерзко. Больше нечего сказать.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cPigeonLitter(cPigeonLitter other) : base(other) { }
        public override object Clone() => new cPigeonLitter(this);
    }
}
