namespace Game.Territories
{
    /// <summary>
    /// Интерфейс, реализующий ограничитель использования объекта в виде порога дельты веса,<br/>
    /// по достижению которого сущность типа <typeparamref name="T"/> будет использована во время сражения.
    /// </summary>
    public interface IBattleThresholdUsable<T> where T : IBattleObject
    {
        public abstract BattleWeight WeightDeltaUseThreshold(T entity);

        public bool WeightIsEnough(BattleWeightResult<T> result)
        {
            return WeightIsEnough(result.Entity, result.WeightDeltaAbs, result.WeightDeltaRel);
        }
        public bool WeightIsEnough(T entity, float weightDeltaAbs, float weightDeltaRel)
        {
            BattleWeight weightThreshold = WeightDeltaUseThreshold(entity);
            if (weightThreshold.Equals(BattleWeight.none))
                return true;
            if (weightThreshold.relative > 0 && weightDeltaRel >= weightThreshold.relative)
                return true;
            if (weightThreshold.absolute > 0 && weightDeltaAbs >= weightThreshold.absolute)
                return true;
            return false;
        }
    }
}
