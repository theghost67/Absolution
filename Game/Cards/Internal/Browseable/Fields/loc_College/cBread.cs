namespace Game.Cards
{
    public class cBread : FieldCard
    {
        public cBread() : base("bread", "bricky_taste", "food_poisoning")
        {
            name = "Протухший хлеб";
            desc = "Эта буханка была найдена в углу самого дальнего шкафчика на кухне столовой. " +
                   "Его состав неизвестен, как и последствия после его употребления.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBread(cBread other) : base(other) { }
        public override object Clone() => new cBread(this);
    }
}
