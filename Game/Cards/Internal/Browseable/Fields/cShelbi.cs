namespace Game.Cards
{
    public class cShelbi : FieldCard
    {
        public cShelbi() : base("shelbi", "origami_victim", "origami_killer")
        {
            name = Translator.GetString("card_shelbi_1");
            desc = Translator.GetString("card_shelbi_2") + 
                   Translator.GetString("card_shelbi_3");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cShelbi(cShelbi other) : base(other) { }
        public override object Clone() => new cShelbi(this);
    }
}
