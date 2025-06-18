using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdHelp : Command
    {
        const string ID = "help";
        static readonly string DESC = Translator.GetString("command_help_1");

        class CmdArg : CommandArg
        {
            const string ID = "cmd";
            static readonly string DESC = Translator.GetString("command_help_2");
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
                TableConsole.Log(Commands.List.First(c => c.id == args["cmd"].input).ToFullString(), LogType.Log);
            else foreach (Command cmd in Commands.List)
                TableConsole.Log(cmd.ToString(), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new CmdArg(this) };
    }
}
