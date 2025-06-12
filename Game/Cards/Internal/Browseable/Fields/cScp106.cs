﻿namespace Game.Cards
{
    public class cScp106 : FieldCard
    {
        public cScp106() : base("scp_106", "tea", "pocket_dimension 2", "play_with_victim", "old_authority")
        {
            name = "Дед";
            desc = "Игривый дед, любящий поиграть со своей жертвой перед её смертью. Появляется в самых неожиданных местах - за углом, в стене, в штанах. " +
                     "Изначально кажется, что этот дед дружелюбен и безвреден, но как только он достанет свою гнилую [ДАННЫЕ УДАЛЕНЫ] из кармана, вам будет не до смеха.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cScp106(cScp106 other) : base(other) { }
        public override object Clone() => new cScp106(this);
    }
}
