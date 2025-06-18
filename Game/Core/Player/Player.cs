using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Menus;
using Game.Palette;
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
        public static DateTime GameStartTime => _gameStartTime;
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

        static DateTime _gameStartTime;
        static CardDeck _deck;
        static int _locationLevel;

        static int _travelsFinished;
        static int _travelsFailed;

        static int _gold;
        static int _health;
        static int _healthCurrent;

        static Player()
        {
            Name = Translator.GetString("player_1");
            _deck = new();
        }

        public static async UniTask StartTheGame(Menu? from)
        {
            int rerollsModifier;
            if (PlayerConfig.psychoMode && PlayerConfig.chaosMode)
                rerollsModifier = 4;
            else if (PlayerConfig.psychoMode || PlayerConfig.chaosMode)
                rerollsModifier = 2;
            else rerollsModifier = 1;

            CardChooseMenu menu = new(8, 4, 0, 3, 5 * rerollsModifier);
            menu.MenuWhenClosed = () => new BattlePlaceMenu();
            menu.OnClosed += menu.TryDestroy;
            if (from is not MainMenu)
                from.OnClosed += from.TryDestroy;

            await UniTask.Delay(1000);

            _gameStartTime = DateTime.Now;
            AudioBrowser.Shuffle();

            Action action = () =>
            {
                for (int i = 0; i < ColorPalette.All.Length; i++)
                    ColorPalette.Current = Global.Palette;
            };

            await MenuTransit.Between(from, menu, action);
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

        public static void RefreshHealth()
        {
            _health = _deck.fieldCards.Sum(c => c.health);
            _healthCurrent = _health;
        }
    }
}
