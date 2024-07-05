using Game.Cards;
using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class cmdSideCardAdd : Command
    {
        const string ID = "side_card_add";
        const string DESC = "создаёт и добавляет карту в рукав";

        class IdArg : CommandArg
        {
            const string ID = "id";
            const string DESC = "ID для создания карты";

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
            const string DESC = "количество очков для улучшения карты";

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
            const string DESC = "сторона, которой будет выдана карта (p/e)";

            public SideArg(Command command) : base(command, ValueType.Required | ValueType.Fixed, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[]
            {
                new("p", "сторона игрока"),
                new("e", "сторона противника"),
            };
        }

        public cmdSideCardAdd() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.Log("Текущее меню не содержит территорию сражения.", LogType.Error);
                return;
            }

            string id = args["id"].input;
            int points = args["points"].ValueAs<int>();
            bool isPlayerSide = args["side"].input == "p";

            Card card = CardBrowser.NewCard(id);
            if (card.isField)
                ((FieldCard)card).UpgradeWithTraitAdd(points);

            if (isPlayerSide)
                 territory.Player.Sleeve.Add(card);
            else territory.Enemy.Sleeve.Add(card);

            TableConsole.Log($"Карта {id} создана и выдана в рукав.", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new PointsArg(this),
            new SideArg(this),
        };
    }
}
