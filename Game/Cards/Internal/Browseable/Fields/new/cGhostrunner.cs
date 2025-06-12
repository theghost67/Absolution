namespace Game.Cards
{
    public class cGhostrunner : FieldCard
    {
        public cGhostrunner() : base("ghostrunner", "hyper_reflex")
        {
            name = "Бегущая тень";
            desc = "Неуловимый и сверхтехнологичный робот-охотник, который обрёл самосознание и решил перестать разрубать людей на куски, " +
                   "переключившись на салаты. Но если его вывести из себя, то он может вернуться за старое.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cGhostrunner(cGhostrunner other) : base(other) { }
        public override object Clone() => new cGhostrunner(this);
    }
}
