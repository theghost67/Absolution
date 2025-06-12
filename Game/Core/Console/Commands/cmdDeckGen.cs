using GreenOne.Console;
using UnityEngine;

namespace Game.Console
{
    public class cmdDeckGen : Command
    {
        const string ID = "deckgen";
        const string DESC = "создаёт новую колоду игрока";

        class StageArg : CommandArg
        {
            const string ID = "stage";
            const string DESC = "количество очков для улучшения одной карты в колоде";

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

        public cmdDeckGen() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            int stage = args["stage"].ValueAs<int>();
            Player.Deck.Clear();
            Player.Deck.AddRange(new Cards.CardDeck(stage));
            TableConsole.Log($"Колода игрока сгенерирована.", LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new StageArg(this),
        };
    }
}
