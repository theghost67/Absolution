using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования поля стола.
    /// </summary>
    public class TableFieldCloneArgs : CloneArgs
    {
        public readonly TableTerritoryCloneArgs terrCArgs;
        public TableFieldCloneArgs(FieldCard srcFieldCardDataClone, TableTerritoryCloneArgs terrCArgs)
        {
            this.terrCArgs = terrCArgs;
        }
    }
}
