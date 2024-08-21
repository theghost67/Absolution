using GreenOne.Console;
using System.Diagnostics;
using UnityEngine;

namespace Game.Console
{
    public class cmdData : Command
    {
        const string ID = "data";
        const string DESC = "открывает папку с файлами игры";

        public cmdData() : base(ID, DESC) { }
        protected override void Execute(CommandArgInputDict args)
        {
            try
            {
                ProcessStartInfo info = new(Application.persistentDataPath) { UseShellExecute = true };
                Process.Start(info);
            }
            catch { TableConsole.Log($"Не удалось запустить папку с данными (путь: {Application.persistentDataPath})", LogType.Error); }
        }
    }
}
