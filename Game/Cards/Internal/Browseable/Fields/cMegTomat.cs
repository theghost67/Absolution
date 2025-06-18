namespace Game.Cards
{
    public class cMegTomat : FieldCard
    {
        public cMegTomat() : base("meg_tomat", "sprinter", "adrenaline")
        {
            name = Translator.GetString("card_meg_tomat_1");
            desc = Translator.GetString("card_meg_tomat_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cMegTomat(cMegTomat other) : base(other) { }
        public override object Clone() => new cMegTomat(this);
    }
}
