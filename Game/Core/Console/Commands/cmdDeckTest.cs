using Game.Cards;
using GreenOne;
using GreenOne.Console;
using System;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdDeckTest : Command
    {
        const string ID = "decktest";
        static readonly string DESC = Translator.GetString("command_deck_test_1");

        class StageArg : CommandArg
        {
            const string ID = "stage";
            static readonly string DESC = Translator.GetString("command_deck_test_2");

            public StageArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
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

        public cmdDeckTest() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            int stage = args["stage"].ValueAs<int>();
            int pointsPerCardForPlayer = GetPointsPerCardForPlayer(stage);
            int pointsPerCardForEnemy = GetPointsPerCardForEnemy(stage);
            CardDeck playerDeck = new(stage, pointsPerCardForPlayer);
            CardDeck enemyDeck = new(stage, pointsPerCardForEnemy);
            TableConsole.Log(Translator.GetString("command_deck_test_3", stage), LogType.Log);
            TableConsole.Log(Translator.GetString("command_deck_test_4", playerDeck.fieldCards.Count, playerDeck.fieldCards.Count), LogType.Log);
            TableConsole.Log(Translator.GetString("command_deck_test_5", pointsPerCardForPlayer, playerDeck.Points, playerDeck.fieldCards.Sum(c => c.PointsWithoutTraits())), LogType.Log);
            TableConsole.Log(Translator.GetString("command_deck_test_6", enemyDeck.fieldCards.Count, enemyDeck.fieldCards.Count), LogType.Log);
            TableConsole.Log(Translator.GetString("command_deck_test_7", pointsPerCardForEnemy, enemyDeck.Points, enemyDeck.fieldCards.Sum(c => c.PointsWithoutTraits())), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new StageArg(this),
        };

        static int GetPointsPerCardForPlayer(int stage)
        {
            return demo_StageToLocStage(stage);
        }
        static int GetPointsPerCardForEnemy(int stage)
        {
            return (demo_StageToLocStage(stage) * demo_StageToLocStageScale(stage)).Ceiling();
        }

        // update with BattlePlaceMenu
        static float demo_StageToLocStageScale(int stage) => stage switch
        {
            1 => 0.67f,
            2 => 1.00f,
            3 => 1.20f,
            4 => 1.40f,
            5 => 1.60f,
            6 => 1.80f,
            7 => 2.00f,
            _ => throw new NotSupportedException(),
        };
        static int demo_StageToLocStage(int stage) => stage switch
        {
            1 => 8,
            2 => 16,
            3 => 24,
            4 => 32,
            5 => 40,
            6 => 48,
            7 => 56,
            _ => 0,
        };
    }
}
