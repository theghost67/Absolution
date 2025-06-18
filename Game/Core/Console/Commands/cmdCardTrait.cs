using Game.Cards;
using Game.Menus;
using Game.Traits;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdCardTrait : Command
    {
        const string ID = "cardtrait";
        static readonly string DESC = Translator.GetString("command_card_trait_1");

        class IdArg : CommandArg
        {
            const string ID = "id";
            static readonly string DESC = Translator.GetString("command_card_trait_2");

            public IdArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                return TraitBrowser.All.Any(t => t.id == str);
            }
        }
        class ValueArg : CommandArg
        {
            const string ID = "value";
            static readonly string DESC = Translator.GetString("command_card_trait_3");

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

        public cmdCardTrait() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log(Translator.GetString("command_card_trait_4"), LogType.Error);
                return;
            }
            TableCardDrawer drawer = (TableCardDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is TableCardDrawer);
            if (drawer == null)
            {
                TableConsole.Log(Translator.GetString("command_card_trait_5"), LogType.Error);
                return;
            }

            TableCard card = drawer.attached;
            string id = args["id"].input;
            int value = args["value"].ValueAs<int>();

            if (!card.Data.isField)
            {
                TableConsole.Log(Translator.GetString("command_card_trait_6"), LogType.Error);
                return;
            }

            TableFieldCard fieldCard = (TableFieldCard)card;
            Trait trait = TraitBrowser.GetTrait(id);
            if (trait.isPassive)
                 _ = fieldCard.Traits.Passives.AdjustStacks(id, value, Menu.GetCurrent());
            else _ = fieldCard.Traits.Actives.AdjustStacks(id, value, Menu.GetCurrent());
            TableConsole.Log(Translator.GetString("command_card_trait_7", id, value), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new ValueArg(this),
        };
    }
}
