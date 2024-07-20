namespace Game.Cards
{
    public class cFunKiller : FieldCard
    {
        public cFunKiller() : base("fun_killer")
        {
            name = "Убийца веселья";
            desc = "Человек, разрушивший сотни тысяч матчей, уничтоживший здравый смысл и баланс в собственной игре в погоне за деньгами. " +
                     "Несмотря на всю критику, которую он получает, отвечает он всегда одним и тем же: I THINK WE DID A PRETTY GOOD JOB SO FAR.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 0);
            frequency = 1.00f;
        }
        protected cFunKiller(cFunKiller other) : base(other) { }
        public override object Clone() => new cFunKiller(this);
    }
}
