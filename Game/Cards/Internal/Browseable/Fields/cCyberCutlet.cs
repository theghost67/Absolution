namespace Game.Cards
{
    public class cCyberCutlet : FieldCard
    {
        public cCyberCutlet() : base("cyber_cutlet", "meaty", "competitive_obsession")
        {
            name = Translator.GetString("card_cyber_cutlet_1");
            desc = Translator.GetString("card_cyber_cutlet_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cCyberCutlet(cCyberCutlet other) : base(other) { }
        public override object Clone() => new cCyberCutlet(this);
    }
}
