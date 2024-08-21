namespace Game.Cards
{
    public class cFengMine : FieldCard
    {
        public cFengMine() : base("feng_mine", "explosive_mine", "adrenaline")
        {
            name = "Фэнг Мина";
            desc = "Всемилюбимая жертва каждого убийцы. Возможно, что её вызывающий наряд привлекает этих маньяков, хотя Фэнг и не против лишний раз побегать от них. " +
                   "До сих пор её никто не может поймать, а всё из-за фугасных мин, разрывающих тех самых убийц на куски. На самом деле, большинство из них уже не хотят за ней гнаться...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
        }
        protected cFengMine(cFengMine other) : base(other) { }
        public override object Clone() => new cFengMine(this);
    }
}
