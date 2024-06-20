using UnityEngine;

namespace Game.Cards
{
    public class ccGold : CardCurrency
    {
        public ccGold() : base("gold")
        {
            name = "Золото";
            desc = "Распространённая валюта, представляющая материальную ценность самых разных объектов.";
            iconPath = "";
            color = new Color(1, 1, 0);
        }
    }
}
