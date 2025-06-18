using Game.Cards;
using Game.Traits;
using GreenOne.Console;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Console
{
    public class cmdPointsTest : Command
    {
        const string ID = "pointstest";
        static readonly string DESC = Translator.GetString("command_points_test_1");

        public cmdPointsTest() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            string path = Application.persistentDataPath + "/points.txt";
            StreamWriter stream = new(path);

            FieldCard card = CardBrowser.NewField("vinsent");
            card.price.value = 0;
            card.moxie = 0;
            card.traits.Clear();

            foreach (Trait trait in TraitBrowser.All)
            {
                stream.WriteLine($"-------------------- trait: {trait.id}");
                for (int i = 1; i < 10; i++)
                    stream.WriteLine($"[stacks = {i}] {trait.Points(card, i)}");
            }

            stream.Flush();
            stream.Close();
            TableConsole.Log(Translator.GetString("command_points_test_2", path), LogType.Log);
        }
    }
}
