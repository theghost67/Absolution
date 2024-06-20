using Game.Cards;
using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using UnityEngine;

namespace Game
{
    public class cmdSideCardPlace : Command
    {
        const string ID = "side_card_place";
        const string DESC = "создаёт и устанавливает карту на территорию";

        class PosArg : CommandArg
        {
            const string ID = "pos";
            const string DESC = "позиция поля, на которую будет установлена карта";

            public PosArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                if (str.Length < 2)
                    return false;

                char posChar = str[0];
                char sideChar = str[1];

                if (!int.TryParse(posChar.ToString(), out int posParse))
                    return false;
                if (posParse < 1 || posParse > 5)
                    return false;
                if (sideChar != 'p' && sideChar != 'e')
                    return false;

                value = new Pos(posParse - 1, sideChar == 'p');
                return true;
            }
        }
        class IdArg : CommandArg
        {
            const string ID = "id";
            const string DESC = "ID для создания карты";

            public IdArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
        }
        class PointsArg : CommandArg
        {
            const string ID = "points";
            const string DESC = "количество очков для улучшения карты";

            public PointsArg(Command command) : base(command, ValueType.Default, ID, DESC) { }
            protected override FixedValue[] FixedValuesCreator() => new FixedValue[] { new("0", "") };
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

        readonly struct Pos
        {
            public readonly int x;
            public readonly bool isPlayer;
            public Pos(int x, bool isPlayer)
            {
                this.x = x;
                this.isPlayer = isPlayer;
            }
        }

        public cmdSideCardPlace() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.WriteLine("Текущее меню не содержит территорию сражения.", LogType.Error);
                return;
            }

            Pos pos = args["pos"].ValueAs<Pos>();
            string id = args["id"].input;
            int points = args["points"].ValueAs<int>();

            Card card;
            try { card = CardBrowser.NewCard(id); } 
            catch { TableConsole.WriteLine($"Карта с ID {id} не найдена.", LogType.Error); return; }

            BattleField field;
            if (pos.isPlayer)
                 field = territory.Field(pos.x, BattleTerritory.PLAYER_FIELDS_Y);
            else field = territory.Field(pos.x, BattleTerritory.ENEMY_FIELDS_Y);

            if (card is FieldCard fCard)
                 territory.PlaceFieldCard(fCard.UpgradeWithTraitAdd(points), field, null);
            else territory.PlaceFloatCard((FloatCard)card, field.Side, null);

            TableConsole.WriteLine($"Карта {id} создана и установлена (принадлежит владельцу поля).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new PosArg(this),
            new IdArg(this),
            new PointsArg(this),
        };
    }
}
