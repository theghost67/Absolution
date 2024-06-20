using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования карты без характеристик на столе.
    /// </summary>
    public class TableFloatCardCloneArgs : TableCardCloneArgs
    {
        public readonly new FloatCard srcCardDataClone;
        public TableFloatCardCloneArgs(FloatCard srcCardDataClone, TableTerritoryCloneArgs terrCArgs)
            : base(srcCardDataClone, terrCArgs)
        {
            this.srcCardDataClone = srcCardDataClone;
        }
    }
}
