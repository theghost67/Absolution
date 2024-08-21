using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdAiTest : Command
    {
        const string ID = "aitest";
        const string DESC = "выполняет тестирование системы ИИ на виртуальных сражениях";

        public cmdAiTest() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            TableConsole.Log($"В данной версии игры команда не поддерживается.", LogType.Error);
        }
    }
}
