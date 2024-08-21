namespace Game.Cards
{
    public class cCrapper : FieldCard
    {
        public cCrapper() : base("crapper", "smelly_trapper 4")
        {
            name = "Крэппер";
            desc = "Младший брат всеми известного убийцы, использующего медвежьи капканы. К сожалению, для него капканов не осталось. " +
                   "Но он придумал свою разновидность ловушек и расставляет их по всему миру. Эти ловушки тяжёло обезвредить... " +
                   "психически, поэтому лучше не пытаться - просто смотрите под ноги.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cCrapper(cCrapper other) : base(other) { }
        public override object Clone() => new cCrapper(this);
    }
}
