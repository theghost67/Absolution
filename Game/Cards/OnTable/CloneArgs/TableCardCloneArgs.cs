using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования карты на столе.
    /// </summary>
    public class TableCardCloneArgs : CloneArgs
    {
        public readonly Card srcCardDataClone;
        public readonly TableTerritoryCloneArgs terrCArgs; // can be null

        public TableCardCloneArgs(Card srcCardDataClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.srcCardDataClone = srcCardDataClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
