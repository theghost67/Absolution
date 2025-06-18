using Game.Effects;
using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdSkipMusic : Command
    {
        const string ID = "skipmusic";
        static readonly string DESC = Translator.GetString("command_skip_music_1");

        public cmdSkipMusic() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (SFX.SkipMusic())
                 TableConsole.Log(Translator.GetString("command_skip_music_2"), LogType.Log);
            else TableConsole.Log(Translator.GetString("command_skip_music_3"), LogType.Error);
        }
    }
}
