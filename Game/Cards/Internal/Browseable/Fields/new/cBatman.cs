namespace Game.Cards
{
    public class cBatman : FieldCard
    {
        public cBatman() : base("batman", "double_attack", "stealth")
        {
            name = Translator.GetString("card_batman_1");
            desc = Translator.GetString("card_batman_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cBatman(cBatman other) : base(other) { }
        public override object Clone() => new cBatman(this);
    }
}
