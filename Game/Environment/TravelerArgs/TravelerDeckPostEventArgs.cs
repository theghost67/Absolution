using Game.Cards;
using System.Collections.Generic;

namespace Game.Environment
{
    /// <summary>
    /// Класс, используемый как параметр события после создания колоды во время путешествия (см. <see cref="Traveler"/>).
    /// </summary>
    public class TravelerDeckPostEventArgs
    {
        public int CardsCount => fieldCardsCount + floatCardsCount;
        public readonly CardDeck deck;
        public readonly Traveler.EntityType deckType;
        public readonly int fieldCardsCount;
        public readonly int floatCardsCount;
        public readonly float statPointsPerCard;
        public readonly IReadOnlyList<float> statPointsRatios;

        public TravelerDeckPostEventArgs(CardDeck deck, TravelerDeckPreEventArgs e)
        {
            this.deck = deck;
            deckType = e.deckType;
            fieldCardsCount = e.fieldCardsCount;
            floatCardsCount = e.floatCardsCount;
            statPointsPerCard = e.statPointsPerCard;
            statPointsRatios = e.statPointsRatios;
        }
    }
}
