using Game.Cards;
using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class cmdSideCardPlace : Command
    {
        const string ID = "side_card_place";
        const string DESC = "создаёт и устанавливает карту на наведённое поле сражения";

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
        public cmdSideCardPlace() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CanAwaitAnyEvents())
            {
                TableConsole.Log("Невозможно выполнить команду из-за выполняемых в данный момент событий.", LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.Log("Текущее меню не содержит территорию сражения.", LogType.Error);
                return;
            }
            BattleFieldDrawer drawer = (BattleFieldDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is BattleFieldDrawer);
            if (drawer == null)
            {
                TableConsole.Log("Наведите курсор на поле, на которое нужно установить карту.", LogType.Error);
                return;
            }

            string id = args["id"].input;
            int points = args["points"].ValueAs<int>();
            BattleField field = drawer.attached;

            if (field.Card != null)
            {
                TableConsole.Log($"Указанное поле для установки карты уже занято.", LogType.Error);
                return;
            }

            Card card = CardBrowser.NewCard(id);
            if (card is FieldCard fCard)
                 territory.PlaceFieldCard(fCard.ShuffleMainStats().UpgradeWithTraitAdd(points), field, null);
            else territory.PlaceFloatCard((FloatCard)card, field.Side, null);

            TableConsole.Log($"Карта {id} создана и установлена (от: null, принадлежит владельцу поля).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new PointsArg(this),
        };
    }
}
