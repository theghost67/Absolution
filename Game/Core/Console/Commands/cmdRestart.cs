using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdRestart : Command
    {
        const string ID = "restart";
        static readonly string DESC = Translator.GetString("command_restart_1");
        const int SKIP_CUR = -1;
        const int SKIP_FORCE = -2;

        public cmdRestart() : base(ID, DESC) { }
        class ForceArg : CommandArg
        {
            public const string ID = "f";
            public static readonly string DESC = Translator.GetString("command_restart_2");
            public ForceArg(Command command) : base(command, ValueType.Flag, ID, DESC) { }
        }

        protected override void Execute(CommandArgInputDict args)
        {
            bool forceSkip = args.ContainsKey(ForceArg.ID);
            if (!forceSkip && TableEventManager.CountAll() != 0)
            {
                TableConsole.Log(Translator.GetString("command_restart_3"), LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not BattlePlaceMenu menu)
            {
                TableConsole.Log(Translator.GetString("command_restart_4"), LogType.Error);
                return;
            }

            menu.Restart(forceSkip);
            string output = forceSkip ? Translator.GetString("command_restart_5") : Translator.GetString("command_restart_6");
            TableConsole.Log(output, LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new ForceArg(this) };
    }
}
