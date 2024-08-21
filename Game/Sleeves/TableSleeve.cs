using Cysharp.Threading.Tasks;
using Game.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Sleeves
{
    // TODO: implement holding cards "history"? (if just added card was the last one removed, insert it to it's previous index)
    /// <summary>
    /// Класс, представляющий возможность подбирания игроком карт рукава типа <see cref="ITableSleeveCard"/> (из привязанной колоды карт).
    /// </summary>
    public class TableSleeve : TableObject, ICloneableWithArgs, IEnumerable<ITableSleeveCard>
    {
        public const int LIMIT = CardDeck.LIMIT + 8;

        public CardDeck Deck => _deck;
        public int Count => _cards.Count;
        public ITableSleeveCardsCollection Cards => _cards;
        public new TableSleeveDrawer Drawer => ((TableObject)this).Drawer as TableSleeveDrawer;

        public readonly bool isForMe;

        readonly CardDeck _deck;
        readonly HashSet<int> _takenCardsGuids;
        readonly ITableSleeveCardsCollection _cards;
        ITableSleeveCard _latestRemovedCard;
        int _latestRemovedCardIndex;

        // NOTE: pass player deck clone to separate sleeve.Deck and Player.Deck card add/remove 
        public TableSleeve(CardDeck deck, bool forMe, Transform parent) : base(parent)
        {
            isForMe = forMe;
            _deck = deck;
            _takenCardsGuids = new HashSet<int>();
            _cards = CollectionCreator();
            TryOnInstantiatedAction(GetType(), typeof(TableSleeve));
        }
        protected TableSleeve(TableSleeve src, TableSleeveCloneArgs args) : base(src)
        {
            isForMe = src.isForMe;
            _deck = args.srcSleeveDeckClone;
            _takenCardsGuids = new HashSet<int>(src._takenCardsGuids);
            _cards = CollectionCreator();
            AddOnInstantiatedAction(GetType(), typeof(TableSleeve), () =>
            {
                foreach (ITableSleeveCard card in src)
                    Add(HoldingCardCloner(card, args));
            });
        }

        public ITableSleeveCard this[int index] => _cards[index];

        public UniTask TakeMissingCards(bool instantly = false)
        {
            if (instantly || (Drawer?.IsMovedOut ?? true))
            {
                TakeMissingCardsInstantly();
                return UniTask.CompletedTask;
            }
            else return TakeMissingCardsAnimated();
        }
        public UniTask TakeCards(int count, bool instantly = false)
        {
            if (instantly || (Drawer?.IsMovedOut ?? true))
            {
                TakeCardsInstantly(count);
                return UniTask.CompletedTask;
            }
            else return TakeCardsAnimated(count);
        }

        public bool Add(Card card)
        {
            if (Count < LIMIT)
                 return Add(HoldingCardCreator(card));
            else return false;
        }
        public bool Add(ITableSleeveCard card)
        {
            if (card == null) return false;
            if (Count >= LIMIT) return false;

            if (_latestRemovedCardIndex != -1 && _latestRemovedCardIndex < _cards.Count && card == _latestRemovedCard)
                _cards.Insert(card, _latestRemovedCardIndex);
            else _cards.Add(card);

            Drawer?.AddCardDrawer(card);
            return true;
        }
        public bool Remove(ITableSleeveCard card)
        {
            if (card == null) return false;
            _latestRemovedCard = card;
            _latestRemovedCardIndex = _cards.IndexOf(card);
            if (!_cards.Remove(card)) return false;
            Drawer?.RemoveCardDrawer(card);
            return true;
        }
        public bool Contains(ITableSleeveCard card)
        {
            foreach (ITableSleeveCard sCard in _cards)
            {
                if (sCard.Equals(card))
                    return true;
            }
            return false;
        }

        public override void Dispose()
        {
            base.Dispose();
            _cards.Clear();
            Drawer?.Dispose();
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is TableSleeveCloneArgs cArgs)
                return new TableSleeve(this, cArgs);
            else return null;
        }

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableSleeveDrawer(this, parent);
        }
        protected virtual ITableSleeveCardsCollection CollectionCreator()
        {
            return new TableSleeveCardsCollection();
        }

        protected virtual ITableSleeveCard HoldingCardCreator(Card data)
        {
            throw new NotImplementedException("TableSleeve.HoldingCardCreator() method does not have it's own implementation. You should implement this method in a derived classes.");
        }
        protected virtual ITableSleeveCard HoldingCardCloner(ITableSleeveCard src, TableSleeveCloneArgs args)
        {
            throw new NotImplementedException("TableSleeve.HoldingCardCloner() method does not have it's own implementation. You should implement this method in a derived classes.");
        }

        async UniTask TakeMissingCardsAnimated()
        {
            await TakeCardsAnimated(_deck.Count);
        }
        async UniTask TakeCardsAnimated(int count)
        {
            if (count <= 0) return;
            Drawer.ColliderEnabled = false;

            int delay = 500;
            for (int i = 0; i < count; i++)
            {
                if (Drawer?.IsDestroyed ?? true) return;
                Card takenCard = TakeCardFromDeck();
                if (takenCard == null) break;

                ITableSleeveCard sCard = HoldingCardCreator(takenCard);
                if (sCard == null) continue;
                if (!Add(sCard))
                    throw new InvalidOperationException("Unable to add taken card to collection.");

                await UniTask.Delay(delay);
                if (delay > 250)
                    delay -= 50;
            }

            await UniTask.Delay(500);
            Drawer.ColliderEnabled = true;
        }

        void TakeMissingCardsInstantly()
        {
            TakeCardsInstantly(count: _deck.Count);
        }
        void TakeCardsInstantly(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Card takenCard = TakeCardFromDeck();
                if (takenCard == null) break;

                ITableSleeveCard sCard = HoldingCardCreator(takenCard);
                if (sCard == null) continue;
                
                Add(sCard);
            }
        }
        Card TakeCardFromDeck()
        {
            Card card = null;
            foreach (Card deckCard in _deck)
            {
                if (_takenCardsGuids.Contains(deckCard.Guid))
                    continue;
                _takenCardsGuids.Add(deckCard.Guid);
                card = deckCard;
                break;
            }
            return card;
        }

        public IEnumerator<ITableSleeveCard> GetEnumerator() => _cards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
