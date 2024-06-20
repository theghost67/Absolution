using Game.Traits;

namespace Game.Territories
{
    /// <summary>
    /// Интерфейс, реализующий ограничитель использования активного трейта в виде порога дельты веса,<br/>
    /// по достижению которого активный трейт (см. <see cref="BattleActiveTrait"/>) будет использован во время сражения.
    /// </summary>
    public interface IBattleActiveTraitThresholdUsable : IBattleThresholdUsable<BattleActiveTrait> { }
}
