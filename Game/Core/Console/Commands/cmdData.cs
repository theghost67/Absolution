using GreenOne.Console;
using System.Diagnostics;
using UnityEngine;

namespace Game
{
    public class cmdData : Command
    {
        const string ID = "data";
        const string DESC = "открывает папку с файлами игры";

        public cmdData() : base(ID, DESC) { }
        protected override void Execute(CommandArgInputDict args)
        {
            Process.Start(Application.persistentDataPath);
        }
    }
}
