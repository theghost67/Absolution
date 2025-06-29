using Game.Cards;
using Game.Menus;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdCardStat : Command
    {
        const string ID = "cardstat";
        static readonly string DESC = Translator.GetString("command_card_stat_1");

        class IdArg : CommandArg
        {
            const string ID = "id";
            static readonly string DESC = Translator.GetString("command_card_stat_2");

            public IdArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("price", Translator.GetString("command_card_stat_3")),
                new("moxie", Translator.GetString("command_card_stat_4")),
                new("health", Translator.GetString("command_card_stat_5")),
                new("strength", Translator.GetString("command_card_stat_6")),
            };
        }
        class ValueArg : CommandArg
        {
            const string ID = "value";
            static readonly string DESC = Translator.GetString("command_card_stat_7");

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

        public cmdCardStat() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log(Translator.GetString("command_card_stat_8"), LogType.Error);
                return;
            }
            TableCardDrawer drawer = (TableCardDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is TableCardDrawer);
            if (drawer == null)
            {
                TableConsole.Log(Translator.GetString("command_card_stat_9"), LogType.Error);
                return;
            }

            string id = args["id"].input;
            int value = args["value"].ValueAs<int>();
            TableCard card = drawer.attached;

            if (!card.Data.isField)
            {
                if (id != "price")
                {
                    TableConsole.Log(Translator.GetString("command_card_stat_10"), LogType.Error);
                    return;
                }
                card.Price.AdjustValue(value, Menu.GetCurrent());
                TableConsole.Log(Translator.GetString("command_card_stat_11", id, value), LogType.Log);
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

            stat.AdjustValue(value, Menu.GetCurrent());
            TableConsole.Log(Translator.GetString("command_card_stat_12", id, value), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new ValueArg(this),
        };
    }
}
