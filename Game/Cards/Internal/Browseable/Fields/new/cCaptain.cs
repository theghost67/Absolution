namespace Game.Cards
{
    public class cCaptain : FieldCard
    {
        public cCaptain() : base("captain", "hammer_out", "avenger")
        {
            name = "Несмирившийся";
            desc = "Одна из версий всемиизвестного мстителя, которая проиграла в финальной битве. Конечно, он остался за кадром истории, но не" +
                   "стоит игнорировать всё то, через что он прошёл... Теперь он поклялся защищать своих друзей любой ценой. " +
                   "Только вот какие-то бомжи украли его щит, пока он спал, защищать стало тяжелее. Может, по этой причине его история так обернулась?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cCaptain(cCaptain other) : base(other) { }
        public override object Clone() => new cCaptain(this);
    }
}
