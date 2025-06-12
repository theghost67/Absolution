namespace Game.Cards
{
    public class cCat : FieldCard
    {
        public cCat() : base("cat", "nine_lives", "innocence")
        {
            name = "Кот Борис";
            desc = "В чём секрет кота Бориса? Этим вопросом задались учёные, которые решили похитить этого бедного котика и устроить ему допрос. " +
                   "Кот оказался немногословным, поэтому учёные захотели допросить самых близких ему существ. К сожалению, таких у него не оказалось. По этой " +
                   "причине они создали 8 клонов кота Бориса, чтобы попытаться выяснить секрет у них. Странно, но они тоже ничего не смогли сказать...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
        }
        protected cCat(cCat other) : base(other) { }
        public override object Clone() => new cCat(this);
    }
}
