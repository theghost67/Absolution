using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game
{
    public class cmdSkip : Command
    {
        const string ID = "skip";
        const string DESC = "пропускает сражение";
        public cmdSkip() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (Menu.GetCurrent() is not BattlePlaceMenu menu)
            {
                TableConsole.Log("Текущее меню не является местом сражения.", LogType.Error);
                return;
            }
            menu.TryFlee();
            TableConsole.Log($"Сражение пропущено.", LogType.Log);
        }
    }
}
