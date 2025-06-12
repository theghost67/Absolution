using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdAiMove : Command
    {
        const string ID = "aimove";
        const string DESC = "выполняет ход игрока, используя ИИ";

        public cmdAiMove() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log("Невозможно выполнить команду из-за выполняемых в данный момент событий.", LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not BattlePlaceMenu menu)
            {
                TableConsole.Log("Текущее меню не содержит территорию сражения.", LogType.Error);
                return;
            }
            _ = menu.Territory.Player.ai.MakeTurn();
            menu.SetPlayerControls(false);
            TableConsole.Log($"Выполнение хода с помощью ИИ...", LogType.Log);
        }
    }
}
