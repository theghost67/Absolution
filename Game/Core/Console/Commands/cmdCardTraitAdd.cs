using Game.Cards;
using Game.Traits;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class cmdCardTraitAdd : Command
    {
        const string ID = "card_trait_add";
        const string DESC = "добавляет значение к зарядам навыка наведённой карты";

        class IdArg : CommandArg
        {
            const string ID = "id";
            const string DESC = "ID навыка";

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

        public cmdCardTraitAdd() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            TableCardDrawer drawer = (TableCardDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is TableCardDrawer);
            TableCard card = drawer.attached;

            if (drawer == null)
            {
                TableConsole.WriteLine("Наведите курсор на карту, значение зарядов навыка которой нужно изменить.", LogType.Error);
                return;
            }

            string id = args["id"].input;
            int value = args["value"].ValueAs<int>();

            if (!card.Data.isField)
            {
                TableConsole.WriteLine($"Карты способностей не могут иметь заряды навыков.", LogType.Error);
                return;
            }

            TableFieldCard fieldCard = (TableFieldCard)card;
            Trait trait = TraitBrowser.GetTrait(id);
            if (trait.isPassive)
                 fieldCard.Traits.Passives.AdjustStacks(id, value, null);
            else fieldCard.Traits.Actives.AdjustStacks(id, value, null);
            TableConsole.WriteLine($"Заряды навыка ({id}) карты были изменена на {value} (от: null).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new ValueArg(this),
        };
    }
}
