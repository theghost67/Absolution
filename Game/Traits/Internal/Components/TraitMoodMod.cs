namespace Game.Traits
{
    /// <summary>
    /// Структура, представляющая модификатор "настроения" улучшения (см. <see cref="TraitMood"/>) одной из характеристик карты поля (здоровья, силы или трейта).
    /// </summary>
    public readonly struct TraitMoodMod
    {
        public static readonly TraitMoodMod none = new(0, 1, 0, 1);

        public readonly float staticAbs;
        public readonly float staticRel;
        public readonly float stackAbs;
        public readonly float stackRel;

        public TraitMoodMod(float staticAbs, float staticRel, float stackAbs = 0, float stackRel = 1)
        {
            this.staticAbs = staticAbs;
            this.staticRel = staticRel;
            this.stackAbs = stackAbs;
            this.stackRel = stackRel;
        }
    }
}
