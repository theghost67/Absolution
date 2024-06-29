using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий параметр для функций, вызываемых при изменеии поля карты типа <see cref="TableFieldCard"/>.
    /// </summary>
    public class TableFieldAttachArgs
    {
        public readonly TableField field;
        public readonly ITableEntrySource source;
        public TableFieldAttachArgs(TableField field, ITableEntrySource source)
        {
            this.field = field;
            this.source = source;
        }
    }
}
