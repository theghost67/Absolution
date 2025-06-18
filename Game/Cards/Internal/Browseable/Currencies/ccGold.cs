using UnityEngine;

namespace Game.Cards
{
    public class ccGold : CardCurrency
    {
        public ccGold() : base("gold")
        {
            name = Translator.GetString("currency_gold_1");
            desc = Translator.GetString("currency_gold_2");
            iconPath = "";
            color = new Color(1, 1, 0);
        }
    }
}
