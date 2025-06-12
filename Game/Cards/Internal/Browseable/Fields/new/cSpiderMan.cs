namespace Game.Cards
{
    public class cSpiderMan : FieldCard
    {
        public cSpiderMan() : base("spider_man", "spider_suit", "amazement")
        {
            name = "Человек-анекдот";
            desc = "Супергерой, доводящий своих противников до смерти от смеха. Вероятно, один из самых жестоких супергероев, когда-либо существовавших. " +
                   "К тому же, у него есть паутина, которой он может обездвижить своих противников, чтобы заставить слушать их анекдоты до остановки дыхания.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cSpiderMan(cSpiderMan other) : base(other) { }
        public override object Clone() => new cSpiderMan(this);
    }
}
