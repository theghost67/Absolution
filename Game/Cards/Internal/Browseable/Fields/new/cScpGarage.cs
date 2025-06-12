namespace Game.Cards
{
    public class cScpGarage : FieldCard
    {
        public cScpGarage() : base("scp_garage", "shift")
        {
            name = "SCP гараж";
            desc = "Поговаривают, в лесу завёлся гараж, необъяснимым образом перемещающийся с места на место, будто скачащий <i>зайчик</i>. " +
                   "И никто не может угнаться за этим гаражом, или всем попросту всё равно на эту легенду. Но всё же, те, кто смогут найти и попасть в него, " +
                   "откроют для себя <b>незабываемые</b> впечатления...";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cScpGarage(cScpGarage other) : base(other) { }
        public override object Clone() => new cScpGarage(this);
    }
}
