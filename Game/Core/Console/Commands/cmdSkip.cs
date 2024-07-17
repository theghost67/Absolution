using Game.Menus;
using GreenOne.Console;
using UnityEngine;

namespace Game
{
    public class cmdSkip : Command
    {
        const string ID = "skip";
        const string DESC = "пропускает сражение";
        public cmdSkip() : base(ID, DESC) { }

        class StageArg : CommandArg
        {
            public const string ID = "stage";
            public const string DESC = "этап, до которого нужно пропускать сражения";
            public StageArg(Command command) : base(command, ValueType.Optional, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
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
            if (Menu.GetCurrent() is not BattlePlaceMenu menu)
            {
                TableConsole.Log("Текущее меню не является местом сражения.", LogType.Error);
                return;
            }

            int stage = (int)(args.ContainsKey(StageArg.ID) ? args[StageArg.ID].value : -1);
            if (stage != -1)
            {
                if (stage <= menu.DemoDifficulty)
                {
                    TableConsole.Log("Этап должен быть выше, чем текущий этап сражения.", LogType.Error);
                    return;
                }
                else menu.DemoDifficulty = stage - 1;
            }

            if (!menu.PlayerControlsEnabled)
            {
                TableConsole.Log("В данный момент сражения пропуск невозможен, так как управление игрока заблокировано.", LogType.Error);
                return;
            }

            menu.TryFlee();
            TableConsole.Log($"Сражение пропущено.", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new StageArg(this) };
    }
}
