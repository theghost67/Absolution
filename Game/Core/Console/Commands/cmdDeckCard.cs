using Game.Cards;
using GreenOne.Console;
using System.Linq;
using UnityEngine;

namespace Game.Console
{
    public class cmdDeckCard : Command
    {
        const string ID = "deckcard";
        static readonly string DESC = Translator.GetString("command_deck_card_1");

        class IdArg : CommandArg
        {
            const string ID = "id";
            static readonly string DESC = Translator.GetString("command_deck_card_2");

            public IdArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                return CardBrowser.All.Any(c => c.id == str);
            }
        }
        class PointsArg : CommandArg
        {
            const string ID = "points";
            static readonly string DESC = Translator.GetString("command_deck_card_3");

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

        public cmdDeckCard() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            string id = args["id"].input;
            int points = args["points"].ValueAs<int>();
            Card card = CardBrowser.NewCard(id);

            if (card.isField)
            {
                FieldCard fieldCard = (FieldCard)card;
                Player.Deck.fieldCards.Add(fieldCard.UpgradeWithTraitAdd(points));
            }
            else
            {
                FloatCard floatCard = (FloatCard)card;
                Player.Deck.floatCards.Add(floatCard);
            }

            TableConsole.Log(Translator.GetString("command_deck_card_4", id), LogType.Log);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new IdArg(this),
            new PointsArg(this),
        };
    }
}
