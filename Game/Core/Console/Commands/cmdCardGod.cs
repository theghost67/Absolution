using Game.Cards;
using Game.Menus;
using Game.Traits;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdCardGod : Command
    {
        const string ID = "cardgod";
        const string DESC = "выдаёт все игровые навыки наведённой карте поля";
        const string IGNORE_TRAIT_ID = "military_service";

        public cmdCardGod() : base(ID, DESC) { }
        class StageArg : CommandArg
        {
            public const string ID = ";";
            public const string DESC = "игнорирует навык " + IGNORE_TRAIT_ID;
            public StageArg(Command command) : base(command, ValueType.Flag, ID, DESC) { }
        }

        protected override void Execute(CommandArgInputDict args)
        {
            if (TableEventManager.CountAll() != 0)
            {
                TableConsole.Log("Невозможно выполнить команду из-за выполняемых в данный момент событий.", LogType.Error);
                return;
            }
            TableFieldCardDrawer drawer = (TableFieldCardDrawer)Drawer.SelectedDrawers.FirstOrDefault(d => d is TableFieldCardDrawer);
            if (drawer == null)
            {
                TableConsole.Log("Наведите курсор на карту, значение характеристики которой нужно изменить.", LogType.Error);
                return;
            }

            bool ignoreTrait = args.ContainsKey(StageArg.ID);
            Menu menu = Menu.GetCurrent();
            TableFieldCard card = drawer.attached;
            FieldCard data = card.Data;
            foreach (Trait trait in TraitBrowser.All)
            {
                if (ignoreTrait && trait.id == IGNORE_TRAIT_ID) continue;
                data.traits.AdjustStacks(trait.id, 1);
                card.Traits.AdjustStacks(trait.id, 1, menu);
            }
            TableConsole.Log($"Карте выданы все навыки (от: меню).", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new StageArg(this) };
    }
}
