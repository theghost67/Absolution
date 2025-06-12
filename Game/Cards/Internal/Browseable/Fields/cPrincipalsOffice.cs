namespace Game.Cards
{
    public class cPrincipalsOffice : FieldCard
    {
        public cPrincipalsOffice() : base("principals_office", "inevitability", "bloodthirstiness")
        {
            name = "Кабинет директора";
            desc = "Лишь единицы знают, что находится за дверью кабинета директора, большинство оттуда так и не смогло уйти. " +
                   "Все окна в кабинете прикрыты, а свет никогда не загорается.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cPrincipalsOffice(cPrincipalsOffice other) : base(other) { }
        public override object Clone() => new cPrincipalsOffice(this);
    }
}
