namespace Game.Cards
{
    public class cVinsent : FieldCard
    {
        public cVinsent() : base("vinsent")
        {
            name = "Винсент";
            desc = "Уважаемый полицейский и главный участник операции Выход Наружу, заключавшаяся в маскировке под заключённого с целью найти убийцу его брата. " +
                   "Операция показала, что Винсент может втереться в доверие к кому угодно... и нанести смертельный удар в спину.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1;
        }
        protected cVinsent(cVinsent other) : base(other) { }
        public override object Clone() => new cVinsent(this);
    }
}
