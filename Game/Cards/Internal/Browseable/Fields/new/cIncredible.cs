namespace Game.Cards
{
    public class cIncredible : FieldCard
    {
        public cIncredible() : base("incredible", "exception", "exceptional")
        {
            name = "Мистер Исключительный";
            desc = "Он исключительно хорош в скрытии своей настоящей персоны. По утрам он сидит в офисе за клавиатурой, расправляясь с отчётами, " +
                   "а по вечерам он расправляется с противниками городского масштаба. Да, звучит не так пафосно, как противники планетарного масштаба, " +
                   "но и наш герой не особенно-то и крут...";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cIncredible(cIncredible other) : base(other) { }
        public override object Clone() => new cIncredible(this);
    }
}
