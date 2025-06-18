namespace Game.Cards
{
    public class cMyersMirror : FieldCard
    {
        public cMyersMirror() : base("myers_mirror", "reflection")
        {
            name = Translator.GetString("card_myers_mirror_1");
            desc = Translator.GetString("card_myers_mirror_2");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cMyersMirror(cMyersMirror other) : base(other) { }
        public override object Clone() => new cMyersMirror(this);
    }
}
