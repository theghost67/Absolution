namespace Game.Cards
{
    public class cTerminator : FieldCard
    {
        public cTerminator() : base("terminator", "hasta_la_vista", "scope", "exoskeleton")
        {
            name = "Терминатор";
            desc = "Титановый экзоскелет, натянутый на живую кожу. Единственной его задачей было уничтожить Джона. Но что-то в его программе пошло не так, " +
                   "и он начал уничтожать всё подряд. Более того, он адаптировался к обществу, и поэтому может захватить с собой приятелей, " +
                   "которые будут помогать в достижении его цели.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cTerminator(cTerminator other) : base(other) { }
        public override object Clone() => new cTerminator(this);
    }
}
