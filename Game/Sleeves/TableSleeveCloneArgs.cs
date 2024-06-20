using Game.Cards;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования рукава на столе.
    /// </summary>
    public class TableSleeveCloneArgs : CloneArgs
    {
        public readonly CardDeck srcSleeveDeckClone;
        public TableSleeveCloneArgs(CardDeck srcSleeveDeckClone)
        {
            this.srcSleeveDeckClone = srcSleeveDeckClone;
        }
    }
}
