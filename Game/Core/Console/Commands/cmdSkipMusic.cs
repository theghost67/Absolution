using Game.Effects;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdSkipMusic : Command
    {
        const string ID = "skipmusic";
        const string DESC = "пропускает играющую в данный момент музыку";

        public cmdSkipMusic() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (SFX.SkipMusic())
                 TableConsole.Log($"Музыка пропущена.", LogType.Log);
            else TableConsole.Log($"Не удалось пропустить музыку. Подождите некоторое время.", LogType.Error);
        }
    }
}
