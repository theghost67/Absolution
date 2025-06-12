namespace Game.Cards
{
    public class cMoshev : FieldCard
    {
        public cMoshev() : base("moshev", "armored_tank", "meaty", "tactician")
        {
            name = "Мошев";
            desc = "То время, когда он хотел захватить человечество путём вавулизации, также известной как проект \"Вавулов-2\", запомнит каждый. " +
                   "После неудачной попытки порабощения, у него отобрали все инструменты и устройства. Странно, однако калаш у него никто не отбирал...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cMoshev(cMoshev other) : base(other) { }
        public override object Clone() => new cMoshev(this);
    }
}
