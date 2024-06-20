using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game
{
    public class cmdLogTerritory : Command
    {
        const string ID = "log_territory";
        const string DESC = "выводит лог территории текущего меню";

        public cmdLogTerritory() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (Menu.GetCurrent() is not IMenuWithTerritory menu)
            {
                TableConsole.WriteLine("Текущее меню не содержит территорию.", LogType.Error);
                return;
            }

            TableConsole.WriteLine("Лог территории текущего меню:", LogType.Log);
            TableConsole.WriteLine(menu.Territory.Log, LogType.Log);
        }
    }
}
