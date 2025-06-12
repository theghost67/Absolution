namespace Game.Cards
{
    public class cMaxwell : FieldCard
    {
        public cMaxwell() : base("maxwell", "mind_split", "dont_starve")
        {
            name = "Максвелл";
            desc = "Некогда выступавший фокусником, сейчас Максвелл побирается объедками у пеньков, запивая это желейкой Гломмера. Как до этого дошло? Ну... когда-то" +
                   "он заинтересовался тёмной магией, и она его полностью поглотила. И поглотила всех тех, кто был ему дорог. Но не до конца, теперь они хотят" +
                   "убить бедного Максвелла за его проступки. Благо, что он теперь не один - у него есть чёрные друзья. Ну, теневые друзья.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cMaxwell(cMaxwell other) : base(other) { }
        public override object Clone() => new cMaxwell(this);
    }
}
