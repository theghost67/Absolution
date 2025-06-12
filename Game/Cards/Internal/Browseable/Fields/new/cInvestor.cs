namespace Game.Cards
{
    public class cInvestor : FieldCard
    {
        public cInvestor() : base("investor", "investment", "downside_bet")
        {
            name = "Инвестор";
            desc = "Он сделал не лучшее вложение своих средств. Теперь он вынужден ждать и надеяться, что навоз подоражает в цене. Как только " +
                   "ему удастся поднакопить немного денег, он планирует сделать более мудрое инвестирование. Подписка на ИИ-инвестора - а что, звучит неплохо!";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cInvestor(cInvestor other) : base(other) { }
        public override object Clone() => new cInvestor(this);
    }
}
