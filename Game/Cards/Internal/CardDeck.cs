using GreenOne;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий изменяемую колоду из карт типа <see cref="FloatCard"/> и <see cref="FieldCard"/>.
    /// </summary>
    public class CardDeck : IEnumerable<Card>, ICloneable, IDisposable
    {
        public const int LIMIT = 16;

        public bool LimitReached => _allCards.Count >= LIMIT;
        public int Count => _allCards.Count;

        public int Traits => fieldCards.Sum(c => c.traits.Count);
        public float Points => fieldCards.Sum(c => c.Points());

        public readonly Collection<FieldCard> fieldCards;
        public readonly Collection<FloatCard> floatCards;

        readonly List<Card> _allCards;

        /// <summary>
        /// Класс, представляющий коллекцию карт колоды одного типа.
        /// </summary>
        public class Collection<T> : IEnumerable<T> where T : Card
        {
            public int Count => _list.Count;
            readonly CardDeck _deck;
            readonly List<T> _list;

            public Collection(CardDeck deck)
            {
                _deck = deck;
                _list = new List<T>();
            }
            public Collection(CardDeck deck, IEnumerable<T> cards)
            {
                _deck = deck;
                _list = new List<T>(cards);
            }
            public T this[int guid] => _list.FirstOrDefault(c => c.Guid == guid);

            public bool AddRange(IEnumerable<T> cards)
            {
                foreach (T card in cards)
                {
                    if (!Add(card))
                        return false;
                }

                return true;
            }
            public bool Add(T card, bool ignoreLimit = false)
            {
                if (!ignoreLimit && _deck.Count >= LIMIT)
                    return false;

                _deck._allCards.Add(card);
                _list.Add(card);

                return true;
            }
            public bool Remove(T card)
            {
                if (!_deck._allCards.Remove(card)) return false;
                return _list.Remove(card);
            }
            public bool Contains(T card)
            {
                return _list.Contains(card);
            }
            public void Clear()
            {
                for (int i = _list.Count - 1; i >= 0; i--)
                    Remove(_list[i]);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public CardDeck()
        {
            fieldCards = new Collection<FieldCard>(this);
            floatCards = new Collection<FloatCard>(this);
            _allCards = new List<Card>();
        }
        public CardDeck(int stage, int statPointsPerCard) : this()
        {
            GetCardsCount(stage, out int fieldCardsCount, out int floatCardsCount);
            int fieldCardsCountRandom = Utils.RandomIntSafe(4, (fieldCardsCount * 1.4f).Ceiling()).ClampedMax(LIMIT - floatCardsCount);
            float randomToActualCountRatio = ((float)fieldCardsCountRandom / fieldCardsCount).ClampedMin(1);
            int pointsSum = statPointsPerCard * fieldCardsCount;
            fieldCardsCount = fieldCardsCountRandom;
            List<float> pointsRatios = new(capacity: fieldCardsCount);

            // generate ratios
            for (int i = 0; i < fieldCardsCount; i++)
                pointsRatios.Add(Utils.RandomIntSafe(25, 400) / 100f);

            // normalize
            float statPointsRatiosSum = pointsRatios.Sum();
            for (int i = 0; i < fieldCardsCount; i++)
                pointsRatios[i] /= statPointsRatiosSum;

            // generate cards using ratios (to make more diverse deck)
            for (int i = 0; i < fieldCardsCount; i++)
            {
                if (LimitReached) break;
                int points = Convert.ToInt32(pointsRatios[i] * pointsSum);
                FieldCard card = CardBrowser.NewFieldRandom().ShuffleMainStats().UpgradeWithTraitAdd(points);
                fieldCards.Add(card);
            }

            for (int i = 0; i < floatCardsCount; i++)
            {
                if (LimitReached) break;
                floatCards.Add(CardBrowser.NewFloatRandom());
            }
        }
        protected CardDeck(CardDeck other) : this()
        {
            foreach (FieldCard srcCard in other.fieldCards)
                fieldCards.Add((FieldCard)srcCard.Clone());

            foreach (FloatCard srcCard in other.floatCards)
                floatCards.Add((FloatCard)srcCard.Clone());
        }

        public static void GetCardsCount(int stage, out int fieldCardsCount, out int floatCardsCount)
        {
            fieldCardsCount = stage switch
            {
                7 => 12,
                6 => 11,
                5 => 10,
                4 => 9,
                3 => 8,
                2 => 6,
                1 => 4,
                _ => 4,
            };
            floatCardsCount = stage switch
            {
                7 => 4,
                6 => 3,
                5 => 2,
                4 => 1,
                3 => 0,
                2 => 0,
                1 => 0,
                _ => 0,
            };
        }

        public void Dispose()
        {
            Clear();
        }
        public object Clone()
        {
            return new CardDeck(this);
        }

        public void AddRange(CardDeck deck)
        {
            foreach (FieldCard card in deck.fieldCards)
                fieldCards.Add(card);

            foreach (FloatCard card in deck.floatCards)
                floatCards.Add(card);
        }
        public void Clear()
        {
            fieldCards.Clear();
            floatCards.Clear();
        }

        public IEnumerator<Card> GetEnumerator()
        {
            foreach (var card in fieldCards)
                yield return card;

            foreach (var card in floatCards)
                yield return card;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
