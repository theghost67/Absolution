namespace Game.Territories
{
    /// <summary>
    /// Интерфейс, реализующий ограничитель использования объекта в виде порога дельты веса,<br/>
    /// по достижению которого сущность типа <typeparamref name="T"/> будет использована во время сражения.
    /// </summary>
    public interface IBattleThresholdUsable<T> where T : IBattleObject
    {
        public abstract BattleWeight WeightDeltaUseThreshold(BattleWeightResult<T> result);
        public bool WeightIsEnough(BattleWeightResult<T> result)
        {
            BattleWeight weightThreshold = WeightDeltaUseThreshold(result);
            if (weightThreshold.relative == 0 && weightThreshold.absolute == 0)
                return true;
            if (weightThreshold.relative > 0 && result.WeightDeltaRel >= weightThreshold.relative)
                return true;
            if (weightThreshold.absolute > 0 && result.WeightDeltaAbs >= weightThreshold.absolute)
                return true;
            return false;
        }
    }
}
