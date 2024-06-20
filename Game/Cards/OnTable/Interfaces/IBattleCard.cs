namespace Game.Cards
{
    /// <summary>
    /// Реализует карту стола как карту сражения, принадлежащей одной из сторон и поддерживающий виртуальное существование (без отрисовщиков).
    /// </summary>
    public interface IBattleCard : ITableCard, IBattleEntity { }
}
