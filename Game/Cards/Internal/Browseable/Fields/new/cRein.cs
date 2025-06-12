namespace Game.Cards
{
    public class cRein : FieldCard
    {
        public cRein() : base("rein", "hammer_go", "screen_shield")
        {
            name = "Рейн";
            desc = "Представьте огромный шкаф. Огромный металлический шкаф, с ракетным ускорителем и экранным щитом. Это и есть краткое описание Райнхардта. " +
                   "Этот садист размазал по стенке не одну сотню тел, и вряд ли он остановится на этом. И если каким-то чудом тело пережило столкновение со стеной, " +
                   "боюсь, удар молота с размаху оно пережить не сможет...";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cRein(cRein other) : base(other) { }
        public override object Clone() => new cRein(this);
    }
}
