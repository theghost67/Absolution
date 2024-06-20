using System.Collections.Generic;

namespace Game.Environment
{
    /// <summary>
    /// Класс, используемый как параметр события при создании колоды во время путешествия (см. <see cref="Traveler"/>).
    /// </summary>
    public class TravelerDeckPreEventArgs
    {
        public int CardsCount => fieldCardsCount + floatCardsCount;
        public readonly Traveler.EntityType deckType;

        public bool handled;
        public int fieldCardsCount;
        public int floatCardsCount;
        public float statPointsPerCard;
        public readonly List<float> statPointsRatios;

        public TravelerDeckPreEventArgs(Traveler.EntityType type, int fieldsCount, int floatsCount, int statPointsPerCard, List<float> statPointsRatios)
        {
            this.deckType = type;
            this.fieldCardsCount = fieldsCount;
            this.floatCardsCount = floatsCount;
            this.statPointsPerCard = statPointsPerCard;
            this.statPointsRatios = statPointsRatios;
            handled = false;
        }
    }
}
