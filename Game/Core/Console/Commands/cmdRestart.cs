using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdRestart : Command
    {
        const string ID = "restart";
        const string DESC = "перезапускает сражение";
        const int SKIP_CUR = -1;
        const int SKIP_FORCE = -2;

        public cmdRestart() : base(ID, DESC) { }
        class ForceArg : CommandArg
        {
            public const string ID = "f";
            public const string DESC = "принудительно перезапускает сражение, не ожидая событий, может вызвать ошибки";
            public ForceArg(Command command) : base(command, ValueType.Flag, ID, DESC) { }
        }

        protected override void Execute(CommandArgInputDict args)
        {
            bool forceSkip = args.ContainsKey(ForceArg.ID);
            if (!forceSkip && TableEventManager.CountAll() != 0)
            {
                TableConsole.Log("Невозможно выполнить команду из-за выполняемых в данный момент событий.", LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not BattlePlaceMenu menu)
            {
                TableConsole.Log("Текущее меню не является местом сражения.", LogType.Error);
                return;
            }

            menu.Restart(forceSkip);
            string output = forceSkip ? $"Сражение перезапущено принудительно (может вызвать ошибки)." : "Сражение перезапущено.";
            TableConsole.Log(output, LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new ForceArg(this) };
    }
}
