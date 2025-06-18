namespace Game.Cards
{
    public class cGeneralP : FieldCard
    {
        public cGeneralP() : base("general_p", "order_of_attack", "order_of_defence", "tactician")
        {
            name = Translator.GetString("card_general_p_1");
            desc = Translator.GetString("card_general_p_2") + 
                   Translator.GetString("card_general_p_3");


            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cGeneralP(cGeneralP other) : base(other) { }
        public override object Clone() => new cGeneralP(this);
    }
}
