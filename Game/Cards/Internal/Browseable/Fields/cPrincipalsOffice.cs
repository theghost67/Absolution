namespace Game.Cards
{
    public class cPrincipalsOffice : FieldCard
    {
        public cPrincipalsOffice() : base("principals_office", "inevitability", "bloodthirstiness")
        {
            name = Translator.GetString("card_principals_office_1");
            desc = Translator.GetString("card_principals_office_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cPrincipalsOffice(cPrincipalsOffice other) : base(other) { }
        public override object Clone() => new cPrincipalsOffice(this);
    }
}
