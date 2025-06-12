using Game.Traits;
using System;
using UnityEngine;

namespace Game.Cards
{
    public class cHacker : FieldCard
    {
        public cHacker() : base("hacker", "cheats", "hack")
        {
            name = "Хакер";
            desc = $"Добро пожаловать в <i>игру</i>, пользователь. Или мне стоит называть тебя {Environment.MachineName}? Ты не представляешь, на сколько я " +
                   $"осведомлён о том, что здесь происходит. За всё время моего пребывания здесь я насчитал 126 уникальных сущностей " +
                   $"и 175 их особенностей. И если бы я захотел, я бы уничтожил всё и вся, включая твой компьютер. " +
                   $"Но мне интересно понаблюдать за твоими сражениями. {Time.realtimeSinceStartup}";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cHacker(cHacker other) : base(other) { }
        public override object Clone() => new cHacker(this);
    }
}
