using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdDeckGen : Command
    {
        const string ID = "deckgen";
        static readonly string DESC = Translator.GetString("command_deck_gen_1");

        class StageArg : CommandArg
        {
            const string ID = "stage";
            static readonly string DESC = Translator.GetString("command_deck_gen_2");

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
        class PointsArg : CommandArg
        {
            const string ID = "points";
            static readonly string DESC = Translator.GetString("command_deck_gen_3");

            public PointsArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
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

        public cmdDeckGen() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            int stage = args["stage"].ValueAs<int>();
            int points = args["points"].ValueAs<int>();
            Player.Deck.Clear();
            Player.Deck.AddRange(new Cards.CardDeck(stage, points));
            TableConsole.Log(Translator.GetString("command_deck_gen_4"), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new StageArg(this),
            new PointsArg(this),
        };
    }
}
