namespace Game.Cards
{
    public class cPlague : FieldCard
    {
        public cPlague() : base("plague", "poison_grenade", "weeds")
        {
            name = "Чумница";
            desc = "Основательница культа травли, терроризирующий города под видом \"освободителей от чумы\". Её методы лечения тяжело назвать научными, " +
                   "как и деятельность её культа. Однако, некоторые совсем отчаявшиеся страждущие, вопреки здравому смыслу, просят докторшу вылечить их недуги. " +
                   "И они вылечиваются! Ну, половина из них.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cPlague(cPlague other) : base(other) { }
        public override object Clone() => new cPlague(this);
    }
}
