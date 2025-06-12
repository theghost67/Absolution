namespace Game.Cards
{
    public class cMichaelSanta : FieldCard
    {
        public cMichaelSanta() : base("michael_santa", "grand_thief", "plunder", "mikelove")
        {
            name = "Майкл де Санта";
            desc = "Живя припеваючи, Майкл захотел встряхнуть стариной. Затем он зачем-то решил грабануть самый элитный банк страны. И у него это даже получилось!" +
                   "И он даже выжил! Теперь, будучи миллиардером, ему приходится тяжко. Его постоянно терроризируют вечно-злая жена и избалованные до нельзя дети." +
                   "Ему остаётся только пособолезновать...";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 3);
        }
        protected cMichaelSanta(cMichaelSanta other) : base(other) { }
        public override object Clone() => new cMichaelSanta(this);
    }
}
