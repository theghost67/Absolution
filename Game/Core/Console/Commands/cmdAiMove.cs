using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdAiMove : Command
    {
        const string ID = "aimove";
        static readonly string DESC = Translator.GetString("command_ai_move_1");

        public cmdAiMove() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log(Translator.GetString("command_ai_move_2"), LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not BattlePlaceMenu menu)
            {
                TableConsole.Log(Translator.GetString("command_ai_move_3"), LogType.Error);
                return;
            }
            _ = menu.Territory.Player.ai.MakeTurn();
            menu.SetPlayerControls(false);
            TableConsole.Log(Translator.GetString("command_ai_move_4"), LogType.Log);
        }
    }
}
