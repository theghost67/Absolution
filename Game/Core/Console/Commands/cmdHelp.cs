using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class cmdHelp : Command
    {
        const string ID = "help";
        const string DESC = "выводит все существующие команды";

        class CmdArg : CommandArg
        {
            const string ID = "cmd";
            const string DESC = "существующая в консоли команда";
            public CmdArg(Command command) : base(command, ValueType.Optional, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                return Commands.List.Any(c => c.id == str);
            }
        }
        public cmdHelp() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (args.ContainsKey("cmd"))
                TableConsole.WriteLine(Commands.List.First(c => c.id == args["cmd"].input).ToFullString(), LogType.Log);
            else foreach (Command cmd in Commands.List)
                TableConsole.WriteLine(cmd.ToString(), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new CmdArg(this) };
    }
}
