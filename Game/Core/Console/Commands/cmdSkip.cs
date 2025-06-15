using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdSkip : Command
    {
        const string ID = "skip";
        const string DESC = "пропускает сражение";
        const int SKIP_CUR = -1;
        const int SKIP_FORCE = -2;

        public cmdSkip() : base(ID, DESC) { }
        class StageArg : CommandArg
        {
            public const string ID = "stage";
            public const string DESC = "этап, до которого нужно пропускать сражения, используйте 'f' для принудительного пропуска";
            public StageArg(Command command) : base(command, ValueType.Optional, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                if (str == "f")
                {
                    value = SKIP_FORCE;
                    return true;
                }

                if (!int.TryParse(str, out int stage))
                    return false;
                if (stage <= 0 || stage > 7)
                    return false;

                value = stage;
                return true;
            }
        }

        protected override void Execute(CommandArgInputDict args)
        {
            int stage = (int)(args.ContainsKey(StageArg.ID) ? args[StageArg.ID].value : SKIP_CUR);
            bool forceSkip = stage == SKIP_FORCE;
            if (forceSkip)
                stage = SKIP_CUR;

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

            if (stage != SKIP_CUR)
            {
                if (stage <= menu.DemoDifficulty)
                {
                    TableConsole.Log("Этап должен быть выше, чем текущий этап сражения.", LogType.Error);
                    return;
                }
                else menu.DemoDifficulty = stage - 1;
            }

            menu.Skip(forceSkip);
            string output = forceSkip ? $"Сражение пропущено принудительно (может вызвать ошибки)." : "Сражение пропущено.";
            TableConsole.Log(output, LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new StageArg(this) };
    }
}
