namespace Game.Cards
{
    public class cMyers : FieldCard
    {
        public cMyers() : base("myers", "p stalker")
        {
            name = "Майкл Майерс";
            desc = "Один из самых жутких убийц, Майкл Майерс, перед тем, как убить свою цель, пристально наблюдал за ней на расстоянии в своей зловещей маске, заставляя её паниковать. " +
                     "Под действием паники, его жертвы всегда действовали нелогично и просто тупо, раскрывая свою уязвимость.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 0.5f;
        }
        protected cMyers(cMyers other) : base(other) { }
        public override object Clone() => new cMyers(this);
    }
}
