using Game.Cards;
using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Console
{
    public class cmdMove : Command
    {
        const string ID = "move";
        static readonly string DESC = Translator.GetString("command_move_1");

        class PosArg : CommandArg
        {
            public const string ID = "pos";
            public static readonly string DESC = Translator.GetString("command_move_2");

            public PosArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                if (str.Length < 2)
                    return false;

                string numChar = str[0].ToString();
                string sideChar = str[1].ToString();

                if (!int.TryParse(numChar, out int x))
                    return false;
                if (x <= 0 || x > BattleTerritory.MAX_WIDTH)
                    return false;
                if (sideChar != "p" && sideChar != "e")
                    return false;

                value = new int2(x - 1, sideChar == "p" ? BattleTerritory.PLAYER_FIELDS_Y : BattleTerritory.ENEMY_FIELDS_Y);
                return true;
            }
        }
        public cmdMove() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log(Translator.GetString("command_move_3"), LogType.Error);
                return;
            }
            if (Menu.GetCurrent() is not IMenuWithTerritory menu || menu.Territory is not BattleTerritory territory)
            {
                TableConsole.Log(Translator.GetString("command_move_4"), LogType.Error);
                return;
            }

            BattleFieldCardDrawer drawer = (BattleFieldCardDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is BattleFieldCardDrawer);
            if (drawer == null)
            {
                TableConsole.Log(Translator.GetString("command_move_5"), LogType.Error);
                return;
            }

            int2 pos = args[PosArg.ID].ValueAs<int2>();
            BattleField field = territory.Field(pos);
            BattleFieldCard fieldCard = drawer.attached;
            if (field.Card == null)
            {
                _ = fieldCard.TryAttachToField(field, menu);
                TableConsole.Log(Translator.GetString("command_move_6"), LogType.Log);
            }
            else TableConsole.Log(Translator.GetString("command_move_7"), LogType.Error);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new PosArg(this),
        };
    }
}
