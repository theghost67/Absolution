namespace Game.Cards
{
    public class cCultist : FieldCard
    {
        public cCultist() : base("cultist", "cult")
        {
            name = "Культист";
            desc = "Неофит, недавно обращённый в веру Точка с Запятой. Последователи этого культа непрестанно ищут пятерых избранных с этим знаком, " +
                   "искренно веря в то, что они принесут сказочные богатства, никогда ранее не виданных за всю историю цивилизаций. После обряда инициации," +
                   "культисты начинают тщательно изучать древние скрежали на таких вымерших языках, как КОБОЛ, БЕЙСИК, ФОРТРАН. Да уж, совсем сумасшедшие...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cCultist(cCultist other) : base(other) { }
        public override object Clone() => new cCultist(this);
    }
}
