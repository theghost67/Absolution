namespace Game.Cards
{
    public class cMyersMirror : FieldCard
    {
        public cMyersMirror() : base("myers_mirror", "reflection")
        {
            name = "Зеркало Майерса";
            desc = "Величайшая реликвия семьи Майерсов - зеркало. Этот предмет источает только чистое зло. Перед тем, как сбежать из дома, Майкл не забыл взять его с собой. " +
                   "Зеркало не только защищало его от повреждений, но и давало свой дар - видеть каждый следующий шаг своей жертвы.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
            frequency = 1.00f;
        }
        protected cMyersMirror(cMyersMirror other) : base(other) { }
        public override object Clone() => new cMyersMirror(this);
    }
}
