namespace Game.Cards
{
    public class cCanteen : FieldCard
    {
        public cCanteen() : base("canteen", "lunch_break")
        {
            name = "Школьная столовая";
            desc = "Ассортимент в этой столовой довольно скудный, но большинство людей не против перекусить здесь. " +
                   "Цены дешёвы, а чем-то заправиться хочется. Однако мало кто знает, из каких ингредиентов приготавливается здешняя еда.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cCanteen(cCanteen other) : base(other) { }
        public override object Clone() => new cCanteen(this);
    }
}
