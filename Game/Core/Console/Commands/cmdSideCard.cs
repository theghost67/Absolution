using Game.Cards;
using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdSideCard : Command
    {
        const string ID = "sidecard";
        static readonly string DESC = Translator.GetString("command_side_card_1");

        class IdArg : CommandArg
        {
            const string ID = "id";
            static readonly string DESC = Translator.GetString("command_side_card_2");

            public IdArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                return CardBrowser.All.Any(c => c.id == str);
            }
        }
        class PointsArg : CommandArg
        {
            const string ID = "points";
            static readonly string DESC = Translator.GetString("command_side_card_3");

            public PointsArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
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
        class SideArg : CommandArg
        {
            const string ID = "side";
            static readonly string DESC = Translator.GetString("command_side_card_4");

            public SideArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("p", Translator.GetString("command_side_card_5")),
                new("e", Translator.GetString("command_side_card_6")),
            };
        }

        public cmdSideCard() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log(Translator.GetString("command_side_card_7"), LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.Log(Translator.GetString("command_side_card_8"), LogType.Error);
                return;
            }

            string id = args["id"].input;
            int points = args["points"].ValueAs<int>();
            bool isPlayerSide = args["side"].input == "p";

            Card card = CardBrowser.NewCard(id);
            if (card.isField)
                ((FieldCard)card).UpgradeWithTraitAdd(points);

            bool result;
            if (isPlayerSide)
                 result = territory.Player.Sleeve.Add(card);
            else result = territory.Enemy.Sleeve.Add(card);
            if (result)
                TableConsole.Log(Translator.GetString("command_side_card_9", id), LogType.Log);
            else TableConsole.Log(Translator.GetString("command_side_card_10"), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new PointsArg(this),
            new SideArg(this),
        };
    }
}
