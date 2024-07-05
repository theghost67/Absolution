using Game.Sleeves;

namespace Game.Territories
{
    /// <summary>
    /// Класс, предоставляющий возможность нахождения объекта типа <see cref="BattleSide"/><br/>
    /// путём кэширования необходимых для поиска данных через конструктор.
    /// </summary>
    public class BattleSideFinder : TableFinder
    {
        readonly bool _isMe;

        public BattleSideFinder(BattleSide side) : base(side) { _isMe = side.isMe; }

        public override object FindInTerritory(TableTerritory territory)
        {
            if (territory is BattleTerritory bTerr)
                 return bTerr.Player.isMe == _isMe ? bTerr.Player : bTerr.Enemy;
            else return null;
        }
        public override object FindInSleeve(TableSleeve sleeve)
        {
            return null;
        }
    }
}
