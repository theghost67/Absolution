namespace Game.Cards
{
    public class cOverseer : FieldCard
    {
        public cOverseer() : base("overseer", "look")
        {
            name = "Смотритель";
            desc = "Он будет за вами присматривать. Но не в том смысле, в котором вы подумали. Он будет следить за каждым вашим шагом, прикрываясь добряком и " +
                   "подмечивая самые незначительные на первый взгляд особенности вашей жизни. И как только настанет удобный момент, он продаст вас с потрохами, " +
                   "заработав лишние пару тысяч себе в карман. Он, случаем, не дружит с Карлом?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cOverseer(cOverseer other) : base(other) { }
        public override object Clone() => new cOverseer(this);
    }
}
