using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий параметр для функций, вызываемых при изменеии поля карты типа <see cref="TableFieldCard"/>.
    /// </summary>
    public class TableFieldAttachArgs
    {
        public readonly TableFieldCard card;
        public readonly TableField field;
        public readonly ITableEntrySource source;
        public TableFieldAttachArgs(TableFieldCard card, TableField field, ITableEntrySource source)
        {
            this.card = card;
            this.field = field;
            this.source = source;
        }
    }
}
