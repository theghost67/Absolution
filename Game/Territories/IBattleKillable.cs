using Cysharp.Threading.Tasks;
using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Реализует объект как объект, который можно уничтожить каким-либо источником (см. <see cref="ITableEntrySource"/>),
    /// <br/>доведя характеристику здоровья до нуля или ниже. Или используя функцию мгновенного убийства.
    /// </summary>
    public interface IBattleKillable
    {
        public TableStat Health { get; }
        public bool IsKilled { get; }
        public bool CanBeKilled { get; set; }
        public UniTask TryKill(BattleKillMode mode, ITableEntrySource source);
    }
}
