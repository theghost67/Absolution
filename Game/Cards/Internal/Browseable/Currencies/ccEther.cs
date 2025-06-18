using UnityEngine;

namespace Game.Cards
{
    public class ccEther : CardCurrency
    {
        public ccEther() : base("ether")
        {
            name = Translator.GetString("currency_ether_1");
            desc = Translator.GetString("currency_ether_2");
            iconPath = "";
            color = new Color(1, 0, 1);
        }
    }
}
