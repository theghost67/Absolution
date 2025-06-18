namespace Game.Cards
{
    public class cScp106 : FieldCard
    {
        public cScp106() : base("scp_106", "tea", "pocket_dimension 2", "play_with_victim", "old_authority")
        {
            name = Translator.GetString("card_scp106_1");
            desc = Translator.GetString("card_scp106_2");


            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cScp106(cScp106 other) : base(other) { }
        public override object Clone() => new cScp106(this);
    }
}
