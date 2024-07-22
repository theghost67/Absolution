using Game.Sleeves;
using Unity.Mathematics;

namespace Game.Territories
{
    /// <summary>
    /// Класс, предоставляющий возможность нахождения объекта типа <see cref="TableField"/><br/>
    /// путём кэширования необходимых для поиска данных через конструктор.
    /// </summary>
    public class TableFieldFinder : TableFinder
    {
        readonly int2 _pos;

        public TableFieldFinder(TableField field) : base(field) { _pos = field.pos; }

        public override object FindInTerritory(TableTerritory territory)
        {
            return territory.Field(_pos);
        }
        public override object FindInSleeve(TableSleeve sleeve)
        {
            throw new System.InvalidOperationException($"Fields are not stored in {nameof(TableSleeve)} instances. Use this method only for traits and cards.");
        }
    }
}
