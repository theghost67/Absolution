namespace Game.Cards
{
    public class cAnderson : FieldCard
    {
        public cAnderson() : base("anderson", "become_human", "become_machine", "old_authority")
        {
            name = Translator.GetString("card_anderson_1");
            desc = Translator.GetString("card_anderson_2");


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cAnderson(cAnderson other) : base(other) { }
        public override object Clone() => new cAnderson(this);
    }
}
