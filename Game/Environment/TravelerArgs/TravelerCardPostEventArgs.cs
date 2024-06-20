using Game.Cards;

namespace Game.Environment
{
    /// <summary>
    /// Класс, используемый как параметр события после созданиея карты поля во время путешествия (см. <see cref="Traveler"/>).
    /// </summary>
    public class TravelerCardPostEventArgs
    {
        public readonly FieldCard card;
        public readonly Traveler.EntityType cardType;
        public readonly int statPoints;
        public readonly int traitsCount;

        public TravelerCardPostEventArgs(FieldCard card, TravelerCardPreEventArgs e)
        {
            this.card = card;
            cardType = e.cardType;
            statPoints = e.statPoints;
            traitsCount = e.traitsCount;
        }
    }
}
