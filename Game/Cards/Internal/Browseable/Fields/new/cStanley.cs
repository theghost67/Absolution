namespace Game.Cards
{
    public class cStanley : FieldCard
    {
        public cStanley() : base("stanley", "my_story", "sprinter")
        {
            name = "Стенли";
            desc = "Работник компании, чья обязанность заключается в нажимании появляющихся на экране кнопок. И Стенли был доволен этой работой. Можно сказать, " +
                   "он был счастлив. Но в один день, все его коллеги внезапно исчезли...";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cStanley(cStanley other) : base(other) { }
        public override object Clone() => new cStanley(this);
    }
}
