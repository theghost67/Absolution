namespace Game.Cards
{
    public class cMegTomat : FieldCard
    {
        public cMegTomat() : base("meg_tomat", "sprinter", "adrenaline")
        {
            name = "Мэг Томат";
            desc = "Атлетическая спортсменка, любящая взрывать всё на своём пути и несущаяся проявить себя с первых секунд. " +
                   "По этой же причине она не раз получала по своей кислой мине. Но, как говорится, быстрые ноги-";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cMegTomat(cMegTomat other) : base(other) { }
        public override object Clone() => new cMegTomat(this);
    }
}
