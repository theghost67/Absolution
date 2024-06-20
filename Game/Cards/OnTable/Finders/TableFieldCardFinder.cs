using Game.Sleeves;
using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Класс, предоставляющий возможность нахождения объекта типа <see cref="TableFieldCard"/>.
    /// </summary>
    public class TableFieldCardFinder : TableFinder
    {
        readonly int _guid;

        public TableFieldCardFinder(TableFieldCard card) : base(card) { _guid = card.Guid; }

        public override object FindInTerritory(TableTerritory territory)
        {
            foreach (TableField field in territory.Fields().WithCard())
            {
                if (field.Card.Guid == _guid)
                    return field.Card;
            }
            return null;
        }
        public override object FindInSleeve(TableSleeve sleeve)
        {
            foreach (ITableCard card in sleeve)
            {
                if (card.Guid == _guid)
                    return card;
            }
            return null;
        }
    }
}