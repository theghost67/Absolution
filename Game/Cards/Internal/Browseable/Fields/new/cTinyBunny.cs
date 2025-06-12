namespace Game.Cards
{
    public class cTinyBunny : FieldCard
    {
        public cTinyBunny() : base("tiny_bunny", "feed", "innocence")
        {
            name = "Зайчик";
            desc = "Когда-то это существо было обыкновенным юношей, посещавшим школу и живущим свою обычную жизнь. " +
                   "Но его любопытство захотело раскрыть тайну пропавшего ребёнка. Главная зацепка заставила его отправиться в лес. " +
                   "И то, что он там увидел, изменило его навсегда.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cTinyBunny(cTinyBunny other) : base(other) { }
        public override object Clone() => new cTinyBunny(this);
    }
}
