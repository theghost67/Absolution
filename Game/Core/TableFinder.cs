using Game.Sleeves;
using Game.Territories;

namespace Game
{
    /// <summary>
    /// Абстрактный класс, предоставляющий возможность нахождения объекта на столе (на территории и в рукаве).
    /// </summary>
    public abstract class TableFinder
    {
        public readonly ITableFindable findable;
        public TableFinder(ITableFindable findable) { this.findable = findable; }

        public abstract object FindInTerritory(TableTerritory territory);
        public abstract object FindInSleeve(TableSleeve sleeve);

        public object FindInBattle(BattleTerritory territory)
        {
            object result = FindInTerritory(territory);
            if (result != null) return result;

            result = FindInSleeve(territory.player.Sleeve);
            if (result != null) return result;

            result = FindInSleeve(territory.enemy.Sleeve);
            return result;
        }
        public object FindOnTable(TableTerritory territory, params TableSleeve[] sleeves)
        {
            object result;
            if (territory != null)
            {
                result = FindInTerritory(territory);
                if (result != null) return result;
            }
            foreach (TableSleeve sleeve in sleeves)
            {
                result = FindInSleeve(sleeve);
                if (result != null) return result;
            }
            return null;
        }
    }
}
