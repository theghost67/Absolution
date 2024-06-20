using GreenOne;
using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Статический класс, представляющий игровой мир (меню, где выбираются локации).
    /// </summary>
    public static class World
    {
        public static double PlayTime => _playTime;
        public static int Days => _days;

        static double _playTime;
        static int _days;

        static World()
        {
            Traveler.OnTravelEnd += NextDay;
        }
        static void NextDay()
        {
            _days++;
        }

        public static void Save()
        {
            double time = _playTime + Time.realtimeSinceStartup;
            SerializationDict dict = new()
            {
                { "play_time", time.ToDotString() },
                { "days", _days },
            };
            SaveSystem.Save(dict, "world");
        }
        public static void Load()
        {
            SerializationDict dict = SaveSystem.LoadDict("world");
            if (dict == null)
            {
                _playTime = 0;
                _days = 1;
                return;
            }

            _playTime = dict.DeserializeKeyAs<double>("play_time");
            _days = dict.DeserializeKeyAs<int>("days");
        }
    }
}
