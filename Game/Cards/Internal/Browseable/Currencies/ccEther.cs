using UnityEngine;

namespace Game.Cards
{
    public class ccEther : CardCurrency
    {
        public ccEther() : base("ether")
        {
            name = "Эфир";
            desc = "Загадочная эссенция, наделённая силой духов, заключённых внутри неё.";
            iconPath = "";
            color = new Color(1, 0, 1);
        }
    }
}
