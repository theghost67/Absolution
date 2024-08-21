using Game.Cards;
using Game.Environment;
using GreenOne;
using System;
using System.Linq;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий изменяемые данные игрока.
    /// </summary>
    public static class Player
    {
        public static event Action<int> OnGoldSet;
        public static event Action<int> OnHealthSet;

        public static string Name { get; set; }
        public static CardDeck Deck => _deck;
        public static int LocationLevel => _locationLevel;

        public static int TravelsFinished => _travelsFinished;
        public static int TravelsFailed => _travelsFailed;

        public static int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
                OnGoldSet?.Invoke(value);
            }
        }
        public static int HealthMax => _health;
        public static int HealthCurrent
        {
            get => _healthCurrent;
            set
            {
                _healthCurrent = value;
                OnHealthSet?.Invoke(value);
            }
        }

        public static int startGold;
        public static int startEther;

        static CardDeck _deck;
        static int _locationLevel;

        static int _travelsFinished;
        static int _travelsFailed;

        static int _gold;
        static int _health;
        static int _healthCurrent;

        static Player()
        {
            Traveler.OnTravelStart += OnTravelStart;
            Traveler.OnTravelEnd += OnTravelEnd;
            Name = "Игрок";
        }

        public static bool Owns(TableFieldCard card)
        {
            return _deck.fieldCards.Contains(card.Data);
        }
        public static bool Owns(FieldCard card)
        {
            return _deck.fieldCards.Contains(card);
        }

        public static bool Owns(TableFloatCard card)
        {
            return _deck.floatCards.Contains(card.Data);
        }
        public static bool Owns(FloatCard card)
        {
            return _deck.floatCards.Contains(card);
        }

        static void OnTravelStart()
        {
            _health = _deck.fieldCards.Sum(c => c.health);
            _healthCurrent = _health;
        }
        static void OnTravelEnd()
        {
            if (Traveler.LastTravelWasCompleted)
            {
                _locationLevel = Traveler.Location.level;
                _travelsFinished++;
            }
            else _travelsFailed++;
        }

        public static void Save()
        {
            SerializationDict dict = new()
            {
                { "deck", _deck.Serialize() },
                { "sgold", startGold },
                { "sether", startEther },
                { "gold", _gold },
                { "location", _locationLevel },
                { "finished", _travelsFinished },
                { "failed", _travelsFailed },
            };
            SaveSystem.Save(dict, "player");
        }
        public static void Load()
        {
            SerializationDict dict = SaveSystem.LoadDict("player");
            if (dict == null)
            {
                startGold = 0;
                startEther = 0;

                _locationLevel = 0;
                _travelsFinished = 0;
                _travelsFailed = 0;

                _deck = new CardDeck(); // TODO[IMPORTANT]: player should choose first 5 cards
                _gold = 0;
                _health = 100;

                return;
            }

            startGold = dict.DeserializeKeyAs<int>("sgold");
            startEther = dict.DeserializeKeyAs<int>("sether");

            _locationLevel = dict.DeserializeKeyAs<int>("location");
            _travelsFinished = dict.DeserializeKeyAs<int>("finished");
            _travelsFailed = dict.DeserializeKeyAs<int>("failed");

            _deck = new CardDeck(dict.DeserializeKeyAsDict("deck"));
            _gold = dict.DeserializeKeyAs<int>("savings");
        }
    }
}
