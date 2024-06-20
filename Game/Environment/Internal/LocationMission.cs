using MyBox;
using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий случайно сгенерированную миссию локации. У миссии есть длительность, угроза и событие.
    /// </summary>
    public sealed class LocationMission
    {
        public readonly Location location;
        public readonly DurationLevel durationLevel;
        public readonly ThreatLevel threatLevel;
        public readonly LocationEvent @event;

        /// <summary>
        /// Абстрактный класс, представляющий модифицируемый уровень какого-либо аспекта локации.
        /// </summary>
        public abstract class ModifierLevel
        {
            public readonly string name;
            public readonly string richName;
            public readonly Color color;
            public readonly int value;

            protected ModifierLevel(int value, string name, Color color)
            {
                this.value = value;
                this.name = name;
                this.richName = name.Colored(color);
                this.color = color;
            }
        }
        /// <summary>
        /// Класс, представляющий модификатор длительности путешествия по локации.
        /// </summary>
        public sealed class DurationLevel : ModifierLevel
        {
            public static readonly DurationLevel[] levels;
            static readonly float[] _chances = new float[5] { 20, 35, 20, 15, 10, };

            static DurationLevel()
            {
                // prefix: Длительность...
                levels = new DurationLevel[5]
                {
                    new(3,  "Короткая", Color.gray),
                    new(6,  "Обычная", Color.white),
                    new(9,  "Долгая", Color.red),
                    new(12, "Эпическая", Color.magenta),
                    new(15, "Нескончаемая", Color.cyan),
                };
            }
            DurationLevel(int value, string name, Color color) : base(value, name, color) { }
            public static DurationLevel GetRandom()
            {
                int randIndex = _chances.GetWeightedRandomIndex(v => v);
                return levels[randIndex];
            }
        }
        /// <summary>
        /// Класс, представляющий модификатор угрозы путешествия по локации (определяет сложность миссии).
        /// </summary>
        public sealed class ThreatLevel : ModifierLevel
        {
            public static readonly ThreatLevel[] levels;
            static readonly float[] _chances = new float[5] { 20, 40, 20, 12, 8, };

            static ThreatLevel()
            {
                // prefix: Угроза...
                levels = new ThreatLevel[5]
                {
                    new(1, "Незаметная", Color.gray),
                    new(2, "Обычная", Color.white),
                    new(3, "Опасная", Color.red),
                    new(4, "Смертельная", Color.magenta),
                    new(5, "Кошмарная", Color.cyan),
                };
            }
            ThreatLevel(int value, string name, Color color) : base(value, name, color) { }
            public static ThreatLevel GetRandom()
            {
                int randIndex = _chances.GetWeightedRandomIndex(v => v);
                return levels[randIndex];
            }
        }

        public LocationMission(Location location) : this(location, Random.Range(1, 6)) { }
        public LocationMission(Location location, int threatLvl)
        {
            this.location = location;
            durationLevel = DurationLevel.GetRandom();
            threatLevel = ThreatLevel.levels[threatLvl - 1];
            @event = EnvironmentBrowser.GetLocationEvent(threatLvl);
        }
    }
}
