using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdSideStat : Command
    {
        const string ID = "sidestat";
        static readonly string DESC = Translator.GetString("command_side_stat_1");

        class IdArg : CommandArg
        {
            const string ID = "id";
            static readonly string DESC = Translator.GetString("command_side_stat_2");

            public IdArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("health", Translator.GetString("command_side_stat_3")),
                new("gold", Translator.GetString("command_side_stat_4")),
                new("ether", Translator.GetString("command_side_stat_5")),
            };
        }
        class ValueArg : CommandArg
        {
            const string ID = "value";
            static readonly string DESC = Translator.GetString("command_side_stat_6");

            public ValueArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                if (!int.TryParse(str, out int parse))
                    return false;

                value = parse;
                return true;
            }
        }
        class SideArg : CommandArg
        {
            const string ID = "side";
            static readonly string DESC = Translator.GetString("command_side_stat_7");

            public SideArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("p", Translator.GetString("command_side_stat_8")),
                new("e", Translator.GetString("command_side_stat_9")),
            };
        }

        public cmdSideStat() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log(Translator.GetString("command_side_stat_10"), LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.Log(Translator.GetString("command_side_stat_11"), LogType.Error);
                return;
            }

            string id = args["id"].input;
            int value = args["value"].ValueAs<int>();
            bool isPlayerSide = args["side"].input == "p";

            BattleSide side = isPlayerSide ? territory.Player : territory.Enemy;
            TableStat stat = id switch
            {
                "health" => side.Health,
                "gold" => side.Gold,
                "ether" => side.Ether,
                _ => throw new System.NotSupportedException(),
            };

            stat.AdjustValue(value, menu);
            TableConsole.Log(Translator.GetString("command_side_stat_12", id, value), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new ValueArg(this),
            new SideArg(this),
        };
    }
}
