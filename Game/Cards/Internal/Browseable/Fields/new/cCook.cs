namespace Game.Cards
{
    public class cCook : FieldCard
    {
        public cCook() : base("cook", "sunrising_flame", "scorching_flame")
        {
            name = "Суси Повару";
            desc = "При поступлении на работу, ему было достаточно сказать всего одно предложение: \"Я делать синьк-синьк, получаться вкусный еда, ой вкусный!\", " +
                   "остальную часть слушать не стали, его сразу же посадили за резку рыбы. Теперь он делает высококлассные суши, на незнакомой ему части суши." +
                   "И кто посмеет его отвлекать от работы, станет жертвой его пылкого темперамента. Как он говорит, \"Есё раз меня позовёс, я созгу здесь всё!\".";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cCook(cCook other) : base(other) { }
        public override object Clone() => new cCook(this);
    }
}
