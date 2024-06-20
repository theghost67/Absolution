using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Интерфейс, реализующий ограничитель использования карты без характеристик в виде порога дельты веса,<br/>
    /// по достижению которого карта без характеристик (см. <see cref="BattleFloatCard"/>) будет использована во время сражения.
    /// </summary>
    public interface IBattleFloatCardThresholdUsable : IBattleThresholdUsable<BattleFloatCard> { }
}
