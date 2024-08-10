namespace Game.Cards
{
    public class cFengMina : FieldCard
    {
        public cFengMina() : base("feng_mine", "explosive_mine", "adrenaline")
        {
            name = "Фэнг Мина";
            desc = "Всемилюбимая жертва каждого убийцы. Возможно, что её вызывающий наряд привлекает этих маньяков. Но Фэнг и не против лишний раз побегать от них, " +
                   "ведь она живёт от адреналина, получаемого во время погонь. Уже долгое время никто не может её поймать, а всё из-за фугасных мин, " +
                   "разрывающих тех самых убийц на куски. И кто тут настоящий убийца? Нет, точно не Фэнг.";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cFengMina(cFengMina other) : base(other) { }
        public override object Clone() => new cFengMina(this);
    }
}
