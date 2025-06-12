namespace Game.Cards
{
    public class cCreeper : FieldCard
    {
        public cCreeper() : base("creeper", "boom")
        {
            name = "Крипер";
            desc = "Особый вид враждебных кубоголовых мутантов, который любит подходить со спины и нежно шептать на ушко, " +
                   "убивая цель своих злодеяний от испуга ещё до самого нападения. Часто их так смешат испуганные физиономии своих жертв, что " +
                   "они буквально разрываются от смеха. Разрываются с силой, эквивалентной пачке динамита. Да уж, такое пережить ещё тяжелее.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cCreeper(cCreeper other) : base(other) { }
        public override object Clone() => new cCreeper(this);
    }
}
