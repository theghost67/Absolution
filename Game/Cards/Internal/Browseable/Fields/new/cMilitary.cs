namespace Game.Cards
{
    public class cMilitary : FieldCard
    {
        public cMilitary() : base("military", "we_need_you")
        {
            name = "Вояка";
            desc = "Скажем честно - его все ненавидят. В один момент жизни он внезапно появляется и забирает лучших из нас, превращая их в бездушных скотов. " +
                   "Окажите всем услугу - сделайте так, чтобы он больше не существовал на этом свете.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);

            frequency = 0.5f;
        }
        protected cMilitary(cMilitary other) : base(other) { }
        public override object Clone() => new cMilitary(this);
    }
}
