using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using UnityEngine;

namespace Game
{
    public class cmdSideStatAdd : Command
    {
        const string ID = "side_stat_add";
        const string DESC = "добавляет значение к характеристике стороны";

        class IdArg : CommandArg
        {
            const string ID = "id";
            const string DESC = "ID характеристики";

            public IdArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("health", "здоровье"),
                new("gold", "золото"),
                new("ether", "эфир"),
            };
        }
        class ValueArg : CommandArg
        {
            const string ID = "value";
            const string DESC = "прибавляемое значение к характеристике";

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
            const string DESC = "сторона, которой будет прибавлена характеристика (p/e)";

            public SideArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("p", "сторона игрока"),
                new("e", "сторона противника"),
            };
        }

        public cmdSideStatAdd() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.Log("Текущее меню не содержит территорию сражения.", LogType.Error);
                return;
            }

            string id = args["id"].input;
            int value = args["value"].ValueAs<int>();
            bool isPlayerSide = args["side"].input == "p";

            BattleSide side = isPlayerSide ? territory.Player : territory.Enemy;
            TableStat stat = id switch
            {
                "health" => side.health,
                "gold" => side.gold,
                "ether" => side.ether,
                _ => throw new System.NotSupportedException(),
            };

            stat.AdjustValue(value, null);
            TableConsole.Log($"Значение характеристики ({id}) стороны было изменено на {value} (от: null).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new ValueArg(this),
            new SideArg(this),
        };
    }
}
