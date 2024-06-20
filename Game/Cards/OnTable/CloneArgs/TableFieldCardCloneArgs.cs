using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования карты поля на столе.
    /// </summary>
    public class TableFieldCardCloneArgs : TableCardCloneArgs
    {
        public readonly new FieldCard srcCardDataClone;
        public readonly TableField srcCardFieldClone;

        public TableFieldCardCloneArgs(FieldCard srcCardDataClone, TableField srcCardFieldClone, TableTerritoryCloneArgs terrCArgs)
            : base(srcCardDataClone, terrCArgs)
        {
            this.srcCardDataClone = srcCardDataClone;
            this.srcCardFieldClone = srcCardFieldClone;
        }
    }
}
