using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Menus;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Game.Environment
{
    // TODO: destroy location menu here? (in StopTravel method)
    // TODO[?]: implement "unknown" locations
    /// <summary>
    /// Статический класс, представляющий возможность путешествовать по случайно сгенерированной локации и её местам.
    /// </summary>
    public static class Traveler
    {
        const int FORK_LENGTH = 3;

        public static event Action OnTravelStart;
        public static event Action OnTravelEnd;

        public static event Action<TravelerDeckPreEventArgs> OnDeckPreCreated;
        public static event Action<TravelerDeckPostEventArgs> OnDeckPostCreated;

        public static event Action<TravelerCardPreEventArgs> OnCardPreCreated; // field cards
        public static event Action<TravelerCardPostEventArgs> OnCardPostCreated;

        public static Location Location => _location;
        public static LocationMission Mission => _mission;
        public static LocationPlace[] CurrentPlaces => _currentPlaces;

        public static bool IsTraveling => _isTraveling;
        public static bool LastTravelWasCompleted => _lastTravelWasCompleted;

        public static int CurrentProgress => _currentProgress;
        public static int RequiredProgress => _requiredProgress;

        static Location _location;
        static LocationMission _mission;
        static LocationPlace[] _currentPlaces;
        static PlaceNode[,] _placesNodeMap;

        // should be changed only in LocationEvent events
        public static readonly Dictionary<string, float> fieldsFrequencies = new();
        public static readonly Dictionary<string, float> floatsFrequencies = new();
        public static readonly Dictionary<string, float> passivesFrequencies = new();
        public static readonly Dictionary<string, float> activesFrequencies = new();
        public static readonly Dictionary<string, float> placesFrequencies = new();

        static bool _isTraveling;
        static bool _lastTravelWasCompleted;

        static int _currentProgress;
        static int _requiredProgress;

        /// <summary>
        /// Класс, представляющий узёл из <see cref="LocationPlace"/>, с возможностью связывать его с предыдущими/следующими узлами.
        /// </summary>
        class PlaceNode
        {
            public readonly LocationPlace value;
            readonly HashSet<PlaceNode> _next;
            readonly HashSet<PlaceNode> _previous;

            public PlaceNode(LocationPlace value)
            {
                this.value = value;
                _next = new();
                _previous = new();
            }
            public void LinkToPrev(PlaceNode node)
            {
                _previous.Add(node);
                node._next.Add(this);
            }
            public void LinkToNext(PlaceNode node)
            {
                _next.Add(node);
                node._previous.Add(this);
            }
        }
        /// <summary>
        /// Перечисление, представляющее тип сущности, находящейся в путешествуемой местности.
        /// </summary>
        public enum EntityType
        {
            Neutral,
            Player,
            Enemy,
        }

        public async static UniTask TryStartTravel(LocationMission mission)
        {
            if (_isTraveling) return;

            _isTraveling = true;
            _currentProgress = 0;
            _requiredProgress = mission.durationLevel.value;

            _location = mission.location;
            _mission = mission;
            RefreshEntityFrequencies();

            //_placesNodeMap = GenerateNodeMap(yLength: _requiredProgress);
            //OnTravelStart?.Invoke();

            //MusicPack.Get("World").StopFading();
            //SpriteRenderer bg = VFX.CreateScreenBG(Color.black.WithAlpha(0));
            //await bg.DOFade(1, 1).AsyncWaitForCompletion();

            //foreach (Menu menu in Menu.GetAllOpened())
            //    menu.CloseInstantly();

            //await bg.DOFade(0, 1).AsyncWaitForCompletion();
            //new LocationMenu(_location).OpenInstantly();
        }
        public static void TryStopTravel(bool complete)
        {
            if (!_isTraveling) return;
            _isTraveling = false;

            if (complete) _location.stage += _location.level;
            _lastTravelWasCompleted = complete;
            OnTravelEnd?.Invoke();

            SaveSystem.SaveAll();
        }
        public static void TryContinueTravel()
        {
            if (!_isTraveling) return;
            if (_currentProgress == _requiredProgress)
            {
                TryStopTravel(complete: true);
                return;
            }

            _currentPlaces = new LocationPlace[FORK_LENGTH].FillBy(i => _placesNodeMap[i, _currentProgress]?.value ?? null);
            _currentProgress++;
        }

        public static void DeckCardsCount(int pointsPerCard, out int fieldCardsCount, out int floatCardsCount)
        {
            fieldCardsCount = pointsPerCard < 6 ? 3 : Convert.ToInt32(3 + 2 * Mathf.Log(pointsPerCard - 5));
            floatCardsCount = pointsPerCard < 28 ? 0 : Convert.ToInt32(Mathf.Log((pointsPerCard - 12) / 16f));
        }
        public static void DeckCardsCountRandom(int pointsPerCard, out int fieldCardsCount, out int floatCardsCount)
        {
            fieldCardsCount = pointsPerCard < 6 ? 3 : Convert.ToInt32(3 + 2 * Mathf.Log(pointsPerCard - 5));
            floatCardsCount = pointsPerCard < 28 ? 0 : Convert.ToInt32(Mathf.Log((pointsPerCard - 12) / 16f));
            fieldCardsCount = UnityEngine.Random.Range(fieldCardsCount / 2, fieldCardsCount);
        }

        public static CardDeck NewDeck(EntityType type = default, int statPointsPerCard = -1)
        {
            if (!_isTraveling)
                throw new InvalidOperationException("Travel is not in progress.");
            if (statPointsPerCard == -1)
                statPointsPerCard = _location.stage;
            DeckCardsCountRandom(statPointsPerCard, out int fieldCardsCount, out int floatCardsCount);

            CardDeck deck = new();
            int pointsSum = statPointsPerCard * fieldCardsCount;
            int traitsCountSum = Convert.ToInt32(FieldCardUpgradeRules.TraitsCountRaw(statPointsPerCard) * fieldCardsCount);
            List<float> statPointsRatios = new(capacity: fieldCardsCount);
            List<float> traitsCountRatios = new(capacity: fieldCardsCount);

            // generate ratios
            for (int i = 0; i < fieldCardsCount; i++)
            {
                statPointsRatios.Add(UnityEngine.Random.Range(0.25f, 4f));
                traitsCountRatios.Add(UnityEngine.Random.Range(0.25f, 4f));
            }

            // normalize
            float statPointsRatiosSum = statPointsRatios.Sum();
            float traitsCountRatiosSum = traitsCountRatios.Sum();
            for (int i = 0; i < fieldCardsCount; i++)
            {
                statPointsRatios[i] /= statPointsRatiosSum;
                traitsCountRatios[i] /= traitsCountRatiosSum;
            }

            TravelerDeckPreEventArgs preArgs = new(type, fieldCardsCount, floatCardsCount, statPointsPerCard, statPointsRatios);
            OnDeckPreCreated?.Invoke(preArgs);
            if (preArgs.handled) return deck;

            // generate cards using ratios (to make more diversed deck)
            for (int i = 0; i < fieldCardsCount; i++)
            {
                if (deck.LimitReached) break;
                int points = Convert.ToInt32(statPointsRatios[i] * pointsSum);
                int traitsCount = Convert.ToInt32(traitsCountRatios[i] * traitsCountSum);
                deck.fieldCards.Add(NewField(type, points, traitsCount));
            }

            for (int i = 0; i < floatCardsCount; i++)
            {
                if (deck.LimitReached) break;
                deck.floatCards.Add(NewFloat());
            }

            OnDeckPostCreated?.Invoke(new TravelerDeckPostEventArgs(deck, preArgs));
            return deck;
        }
        public static FieldCard NewField(EntityType type = default)
        {
            return NewField(type);
        }
        public static FieldCard NewField(EntityType type = default, int statPoints = -1, int traitsCount = -1)
        {
            if (!_isTraveling)
                throw new InvalidOperationException("Travel is not in progress.");
            if (statPoints == -1)
                statPoints = _location.stage;
            if (traitsCount == -1)
                traitsCount = FieldCardUpgradeRules.TraitsCount(statPoints);

            TravelerCardPreEventArgs preArgs = new(type, statPoints, traitsCount);
            OnCardPreCreated?.Invoke(preArgs);

            string randomId = fieldsFrequencies.GetWeightedRandom(pair => pair.Value).Key;
            FieldCardUpgradeRules rules = new(statPoints, traitsCount)
            {
                possiblePassivesFreqs = passivesFrequencies,
                possibleActivesFreqs = activesFrequencies
            };

            FieldCard card = CardBrowser.NewField(randomId).ShuffleMainStats();
            rules.Upgrade(card);
            OnCardPostCreated?.Invoke(new TravelerCardPostEventArgs(card, preArgs));
            return card;
        }
        public static FloatCard NewFloat()
        {
            if (!_isTraveling)
                throw new InvalidOperationException("Travel is not in progress.");
            string randomId = floatsFrequencies.GetWeightedRandom(pair => pair.Value).Key;
            return CardBrowser.NewFloat(randomId);
        }

        static void RefreshEntityFrequencies()
        {
            fieldsFrequencies.Clear();
            floatsFrequencies.Clear();
            passivesFrequencies.Clear();
            activesFrequencies.Clear();
            placesFrequencies.Clear();

            foreach (string fieldCardId in _location.fieldCards)
            {
                FieldCard card = CardBrowser.GetField(fieldCardId);
                fieldsFrequencies.Add(fieldCardId, card.frequency);
            }
            foreach (string floatCardId in _location.floatCards)
            {
                FloatCard card = CardBrowser.GetFloat(floatCardId);
                floatsFrequencies.Add(floatCardId, card.frequency);
            }
            foreach (string placeId in _location.places) // TODO: rewrite
                placesFrequencies.Add(placeId, EnvironmentBrowser.LocationPlaces[placeId].frequency);

            return; // TODO: implement events
            if (_mission.@event == null)
                return;

            // field
            foreach (LocationFieldCardMod fieldCardMod in _mission.@event.fieldCardsMods)
                fieldCardMod.ModifyCollection(fieldsFrequencies);

            // float
            foreach (LocationFloatCardMod floatCardMod in _mission.@event.floatCardsMods)
                floatCardMod.ModifyCollection(floatsFrequencies);

            // passives
            foreach (LocationPassiveTraitMod passiveTraitMod in _mission.@event.passiveTraitsMods)
                passiveTraitMod.ModifyCollection(passivesFrequencies);

            // actives
            foreach (LocationActiveTraitMod activeTraitMod in _mission.@event.activeTraitsMods)
                activeTraitMod.ModifyCollection(activesFrequencies);

            // places
            foreach (LocationPlaceMod placeMod in _mission.@event.placesMods)
                placeMod.ModifyCollection(placesFrequencies);
        }

        // must be invoked after RefreshEntityFrequencies()
        static PlaceNode[,] GenerateNodeMap(int yLength)
        {
            PlaceNode[,] map = new PlaceNode[FORK_LENGTH, yLength];
            PlaceNode[] prevNodes = GenerateNodeFork();
            PlaceNode[] nextNodes;

            void CopyPrevNodesToMap(int y)
            {
                for (int x = 0; x < FORK_LENGTH; x++)
                {
                    if (x == prevNodes.Length) break;
                    map[x, y] = prevNodes[x];
                }
            }
            CopyPrevNodesToMap(y: 0);

            for (int y = 1; y < yLength; y++)
            {
                nextNodes = GenerateNodeFork();
                if (prevNodes.Length < nextNodes.Length) // link nodes from bigger array to smaller one 
                {
                    foreach (PlaceNode node in nextNodes)
                        node.LinkToPrev(prevNodes.GetRandom());
                }
                else
                {
                    foreach (PlaceNode node in prevNodes)
                        node.LinkToNext(nextNodes.GetRandom());
                }

                prevNodes = nextNodes;
                CopyPrevNodesToMap(y);
            }

            return map;
        }
        static PlaceNode[] GenerateNodeFork()
        {
            PlaceNode[] array = new PlaceNode[UnityEngine.Random.Range(1, FORK_LENGTH + 1)];

            for (int x = 0; x < array.Length; x++)
            {
                string randomId = placesFrequencies.GetWeightedRandom(pair => pair.Value).Key;
                array[x] = new PlaceNode(EnvironmentBrowser.LocationPlaces[randomId]);
            }

            return array;
        }
    }
}
