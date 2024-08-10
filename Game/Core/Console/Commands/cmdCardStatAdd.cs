using Game.Cards;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class cmdCardStatAdd : Command
    {
        const string ID = "card_stat_add";
        const string DESC = "добавляет значение к характеристике наведённой карты";

        class IdArg : CommandArg
        {
            const string ID = "id";
            const string DESC = "ID характеристики";

            public IdArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("price", "цена"),
                new("moxie", "инициатива"),
                new("health", "здоровье"),
                new("strength", "сила"),
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

        public cmdCardStatAdd() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CanAwaitAnyEvents())
            {
                TableConsole.Log("Невозможно выполнить команду из-за выполняемых в данный момент событий.", LogType.Error);
                return;
            }
            TableCardDrawer drawer = (TableCardDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is TableCardDrawer);
            if (drawer == null)
            {
                TableConsole.Log("Наведите курсор на карту, значение характеристики которой нужно изменить.", LogType.Error);
                return;
            }

            string id = args["id"].input;
            int value = args["value"].ValueAs<int>();
            TableCard card = drawer.attached;

            if (!card.Data.isField)
            {
                if (id != "price")
                {
                    TableConsole.Log("Карта способности не имеет данной характеристики.", LogType.Error);
                    return;
                }
                card.Price.AdjustValue(value, null);
                TableConsole.Log($"Характеристика ({id}) карты была изменена на {value} (от: null).", LogType.Log);
                return;
            }

            TableFieldCard fieldCard = (TableFieldCard)card;
            TableStat stat = id switch
            {
                "price" => fieldCard.Price,
                "moxie" => fieldCard.Moxie,
                "health" => fieldCard.Health,
                "strength" => fieldCard.Strength,
                _ => throw new System.NotSupportedException(),
            };

            stat.AdjustValue(value, null);
            TableConsole.Log($"Характеристика ({id}) карты была изменена на {value} (от: null).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new ValueArg(this),
        };
    }
}
