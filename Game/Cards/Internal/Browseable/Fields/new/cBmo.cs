namespace Game.Cards
{
    public class cBmo : FieldCard
    {
        public cBmo() : base("bmo", "load", "ooo")
        {
            name = "БиМо";
            desc = $"БИо-МОдифицированный электронный механизм, который готов о вас позаботиться. Поверьте, он не заставит вас скучать." +
                   $"В этой коробчке есть тысячи игр, миллионы терабайтов информации, и немыслимое количество утилит, " +
                   $"способных адаптироваться к любой ситуации и к любому пользователю. И к любому противнику.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cBmo(cBmo other) : base(other) { }
        public override object Clone() => new cBmo(this);
    }
}
