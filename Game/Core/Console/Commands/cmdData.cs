using GreenOne.Console;
using System.Diagnostics;
using UnityEngine;

namespace Game.Console
{
    public class cmdData : Command
    {
        const string ID = "data";
        static readonly string DESC = Translator.GetString("command_data_1");

        public cmdData() : base(ID, DESC) { }
        protected override void Execute(CommandArgInputDict args)
        {
            try
            {
                ProcessStartInfo info = new(Application.persistentDataPath) { UseShellExecute = true };
                Process.Start(info);
            }
            catch { TableConsole.Log(Translator.GetString("command_data_2", Application.persistentDataPath), LogType.Error); }
        }
    }
}
