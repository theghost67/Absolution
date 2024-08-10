using Cysharp.Threading.Tasks;
using Game.Territories;

namespace Game.Cards
{
    /// <summary>
    /// Реализует карту стола как карту сражения, принадлежащей одной из сторон и поддерживающий виртуальное существование (без отрисовщиков).
    /// </summary>
    public interface IBattleCard : ITableCard, IBattleObject
    {
        // will throw an exception if it is not a ITableSleeveCard
        public UniTask TryAttachToSideSleeve(BattleSide side, ITableEntrySource source);
    }
}
