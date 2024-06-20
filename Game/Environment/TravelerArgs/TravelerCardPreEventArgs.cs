namespace Game.Environment
{
    /// <summary>
    /// Класс, используемый как параметр события перед созданием карты поля во время путешествия (см. <see cref="Traveler"/>).
    /// </summary>
    public class TravelerCardPreEventArgs
    {
        public readonly Traveler.EntityType cardType;
        public int statPoints;
        public int traitsCount;

        public TravelerCardPreEventArgs(Traveler.EntityType type, int statPoints, int traitsCount)
        {
            this.cardType = type;
            this.statPoints = statPoints;
            this.traitsCount = traitsCount;
        }
    }
}
