using Game.Traits;
using System;
using UnityEngine;

namespace Game.Cards
{
    public class cHacker : FieldCard
    {
        public cHacker() : base("hacker", "cheats", "hack")
        {
            name = Translator.GetString("card_hacker_1");
            desc = Translator.GetString("card_hacker_2", Environment.MachineName, Time.realtimeSinceStartup);


            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cHacker(cHacker other) : base(other) { }
        public override object Clone() => new cHacker(this);
    }
}
