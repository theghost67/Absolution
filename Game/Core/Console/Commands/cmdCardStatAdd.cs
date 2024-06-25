﻿using Game.Cards;
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
                if (parse <= 0)
                    return false;

                value = parse;
                return true;
            }
        }

        public cmdCardStatAdd() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            TableCardDrawer drawer = (TableCardDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is TableCardDrawer);
            TableCard card = drawer.attached;

            if (drawer == null)
            {
                TableConsole.WriteLine("Наведите курсор на карту, значение характеристики которой нужно изменить.", LogType.Error);
                return;
            }

            string id = args["id"].input;
            int value = args["value"].ValueAs<int>();

            if (!card.Data.isField)
            {
                if (id != "price")
                {
                    TableConsole.WriteLine("Карта способности не имеет данной характеристики.", LogType.Error);
                    return;
                }
                card.price.AdjustValueAbs(value, null);
                TableConsole.WriteLine($"Характеристика ({id}) карты была изменена на {value} (от: null).", LogType.Log);
                return;
            }

            TableFieldCard fieldCard = (TableFieldCard)card;
            TableStat stat = id switch
            {
                "price" => fieldCard.price,
                "moxie" => fieldCard.moxie,
                "health" => fieldCard.health,
                "strength" => fieldCard.strength,
                _ => throw new System.NotSupportedException(),
            };

            stat.AdjustValueAbs(value, null);
            TableConsole.WriteLine($"Характеристика ({id}) карты была изменена на {value} (от: null).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new ValueArg(this),
        };
    }
}