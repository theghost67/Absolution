namespace Game.Cards
{
    public class cMyers : FieldCard
    {
        public cMyers() : base("michael_myers", "stalker", "camper", "mikelove")
        {
            name = Translator.GetString("card_myers_1");
            desc = Translator.GetString("card_myers_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cMyers(cMyers other) : base(other) { }
        public override object Clone() => new cMyers(this);
    }
}
