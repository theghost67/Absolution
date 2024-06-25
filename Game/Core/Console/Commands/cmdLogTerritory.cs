using Game.Menus;
using Game.Territories;
using GreenOne.Console;
using System.IO;
using UnityEngine;

namespace Game
{
    public class cmdLogTerritory : Command
    {
        const string ID = "log_territory";
        const string DESC = "выводит лог территории текущего меню в файл";

        class AiArg : CommandArg
        {
            const string ID = "ai";
            const string DESC = "выведет лог последней территории, используемой ИИ";
            public AiArg(Command command) : base(command, ValueType.Flag, ID, DESC) { }
        }
        public cmdLogTerritory() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (Menu.GetCurrent() is not IMenuWithTerritory menu)
            {
                TableConsole.WriteLine("Текущее меню не содержит территорию.", LogType.Error);
                return;
            }

            string log = args.ContainsKey("ai") ? BattleAI.LastTerritoryLog : menu.Territory.Log;
            string path = Path.Combine(Application.persistentDataPath, "terrLog.txt");
            using StreamWriter stream = new(path);
            stream.Write(log);
            stream.Flush();
            stream.Close();

            TableConsole.WriteLine($"Лог выведен в файл по пути {path}", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[] { new AiArg(this) };
    }
}
