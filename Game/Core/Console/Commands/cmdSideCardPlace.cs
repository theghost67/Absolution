using Game.Cards;
using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdSideCardPlace : Command
    {
        const string ID = "sidecardplace";
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
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log("Невозможно выполнить команду из-за выполняемых в данный момент событий.", LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.Log("Текущее меню не содержит территорию сражения.", LogType.Error);
                return;
            }

            string id = args["id"].input;
            int points = args["points"].ValueAs<int>();
            BattleFieldDrawer drawer = (BattleFieldDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is BattleFieldDrawer);
            BattleField field = drawer?.attached;
            bool isFieldCard = CardBrowser.FieldsIndexed.ContainsKey(id);

            if ((field == null && isFieldCard) || (field != null && field.Card != null))
            {
                TableConsole.Log($"Невозможно установить карту на данную позицию.", LogType.Error);
                return;
            }

            Card card = CardBrowser.NewCard(id);
            if (card is FieldCard fCard)
                 territory.PlaceFieldCard(fCard.ShuffleMainStats().UpgradeWithTraitAdd(points), field, null);
            else territory.PlaceFloatCard((FloatCard)card, field?.Side ?? territory.Player, null);

            TableConsole.Log($"Карта {id} создана и установлена (от: null, принадлежит владельцу поля).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new PointsArg(this),
        };
    }
}
