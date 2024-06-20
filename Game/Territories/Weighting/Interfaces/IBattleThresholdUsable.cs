namespace Game.Territories
{
    /// <summary>
    /// Интерфейс, реализующий ограничитель использования объекта в виде порога дельты веса,<br/>
    /// по достижению которого сущность типа <typeparamref name="T"/> будет использована во время сражения.
    /// </summary>
    public interface IBattleThresholdUsable<T> where T : IBattleEntity
    {
        public abstract BattleWeight GetWeightDeltaUseThreshold(T entity);

        public bool WeightIsEnough(BattleWeightResult<T> result)
        {
            return WeightIsEnough(result.Entity, result.WeightDeltaAbs, result.WeightDeltaRel);
        }
        public bool WeightIsEnough(T entity, float weightDeltaAbs, float weightDeltaRel)
        {
            BattleWeight weightThreshold = GetWeightDeltaUseThreshold(entity);
            if (weightThreshold.relative > 0 && weightDeltaRel >= weightThreshold.relative)
                return true;
            if (weightThreshold.absolute > 0 && weightDeltaAbs >= weightThreshold.absolute)
                return true;
            return false;
        }
    }
}
