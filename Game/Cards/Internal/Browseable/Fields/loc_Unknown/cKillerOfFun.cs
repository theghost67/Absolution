namespace Game.Cards
{
    public class cKillerOfFun : FieldCard
    {
        public cKillerOfFun() : base("killer_of_fun", "nerf_time")
        {
            name = "Убийца веселья";
            desc = "Человек, разрушивший сотни тысяч матчей, уничтоживший здравый смысл и баланс в собственной игре в погоне за деньгами. " +
                   "Несмотря на всю критику, которую он получает, отвечает он всегда одним и тем же: I THINK WE DID A PRETTY GOOD JOB SO FAR.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
            frequency = 1.00f;
        }
        protected cKillerOfFun(cKillerOfFun other) : base(other) { }
        public override object Clone() => new cKillerOfFun(this);
    }
}
