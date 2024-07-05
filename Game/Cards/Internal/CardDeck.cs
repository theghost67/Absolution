using GreenOne;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий изменяемую колоду из карт типа <see cref="FloatCard"/> и <see cref="FieldCard"/>.
    /// </summary>
    public class CardDeck : IEnumerable<Card>, ISerializable, ICloneable, IDisposable
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
        public CardDeck(SerializationDict dict)
        {
            throw new NotImplementedException();
            //List<SerializationDict> fieldCardsDictList = dict.DeserializeKeyAs<List<SerializationDict>>("field");
            //List<SerializationDict> floatCardsDictList = dict.DeserializeKeyAs<List<SerializationDict>>("float");

            //IEnumerable<FieldCard> loadedFieldCards = floatCardsDictList.Select(sd =>
            //{
            //    string cardId = sd.DeserializeKeyAs<string>("id");
            //    Card cardOriginal = CardBrowser.GetCardSource(cardId);
            //    return new FieldCard(sd, cardOriginal);
            //});
            //IEnumerable<FloatCard> loadedFloatCards = fieldCardsDictList.Select(sd =>
            //{
            //    string cardId = sd.DeserializeKeyAs<string>("id");
            //    FloatCard cardOriginal = CardBrowser.FloatCards[cardId];
            //    return new FloatCard(sd, cardOriginal);
            //});

            //fieldCards = new Collection<FieldCard>(this, loadedFieldCards);
            //floatCards = new Collection<FloatCard>(this, loadedFloatCards);
        }
        protected CardDeck(CardDeck other) : this()
        {
            foreach (FieldCard srcCard in other.fieldCards)
                fieldCards.Add((FieldCard)srcCard.Clone());

            foreach (FloatCard srcCard in other.floatCards)
                floatCards.Add((FloatCard)srcCard.Clone());
        }

        public void Dispose()
        {
            Clear();
        }
        public object Clone()
        {
            return new CardDeck(this);
        }
        public SerializationDict Serialize()
        {
            var dict = new SerializationDict();

            var floatCardsDictList = new List<SerializationDict>(floatCards.Select(c => c.Serialize()));
            var fieldCardsDictList = new List<SerializationDict>(fieldCards.Select(c => c.Serialize()));

            dict.Add("float", floatCardsDictList);
            dict.Add("field", fieldCardsDictList);

            return dict;
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
