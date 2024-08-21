using Game.Territories;
namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий параметры для функций, вызываемых при установке карты рукава (см. <see cref="ITableSleeveCard"/>) на игровое поле (см. <see cref="TableField"/>).
    /// </summary>
    public class TableSleeveCardDropArgs
    {
        public TableField field;
        public bool isPreview; // means that the drop is queued in PlayerQueue, but not in action

        public TableSleeveCardDropArgs(TableField field, bool isPreview = false)
        {
            this.field = field;
            this.isPreview = isPreview;
        }
    }
}
