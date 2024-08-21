using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdAiStage : Command
    {
        const string ID = "aistage";
        const string DESC = "переключает режим ИИ для игрока до конца этапа";

        public cmdAiStage() : base(ID, DESC) { }

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
            if (menu.Territory.Player.ai.IsEnabled)
            {
                TableConsole.Log($"Режим ИИ уже включён. Его не остановить.", LogType.Error);
                return;
            }
            menu.Territory.Player.ai.IsEnabled = true;
            menu.SetPlayerControls(false);
            TableConsole.Log($"Режим ИИ был переключён. НЕ выполняйте каких-либо действий, связанных со сражением.", LogType.Log);
        }
    }
}
