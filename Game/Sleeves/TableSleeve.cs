using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Traits;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Sleeves
{
    /// <summary>
    /// Класс, представляющий возможность подбирания игроком карт рукава типа <see cref="ITableSleeveCard"/> (из привязанной колоды карт).
    /// </summary>
    public class TableSleeve : ITableDrawable, ICloneableWithArgs, IEnumerable<ITableSleeveCard>, IDisposable
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public CardDeck Deck => _deck;
        public int Count => _cards.Count;
        public ITableSleeveCardsCollection Cards => _cards;

        public TableSleeveDrawer Drawer => _drawer;

        public readonly bool isForMe;
        readonly CardDeck _deck;

        ITableSleeveCardsCollection _cards;
        Drawer ITableDrawable.Drawer => _drawer;
        TableSleeveDrawer _drawer;

        // NOTE: pass player deck clone to separate sleeve.Deck and Player.Deck card add/remove 
        public TableSleeve(CardDeck deck, bool isForMe, Transform parent, bool withDrawer = true)
        {
            _cards = CollectionCreator();
            _deck = deck; 
            this.isForMe = isForMe;

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected TableSleeve(TableSleeve src, TableSleeveCloneArgs args)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

            _cards = CollectionCreator();
            _deck = args.srcSleeveDeckClone;
            isForMe = src.isForMe;

            args.AddOnClonedAction(src.GetType(), typeof(TableSleeve), () =>
            {
                foreach (ITableSleeveCard card in src)
                    Add(HoldingCardCloner(card, args));
            });
        }

        public ITableSleeveCard this[int index] => _cards[index];

        public UniTask TakeMissingCards(bool instantly = false)
        {
            if (_drawer == null || _drawer.IsMovedOut || instantly)
            {
                TakeMissingCardsInstantly();
                return UniTask.CompletedTask;
            }
            else return TakeMissingCardsAnimated();
        }
        public UniTask TakeCards(int count, bool instantly = false)
        {
            if (_drawer == null || _drawer.IsMovedOut || instantly)
            {
                TakeCardsInstantly(count);
                return UniTask.CompletedTask;
            }
            else return TakeCardsAnimated(count);
        }

        public void CreateDrawer(Transform parent)
        {
            if (_drawer != null) return;
            TableSleeveDrawer drawer = DrawerCreator(parent);
            DrawerSetter(drawer);
            OnDrawerCreated?.Invoke(this, EventArgs.Empty);
        }
        public void DestroyDrawer(bool instantly)
        {
            if (_drawer == null) return;
            _drawer.TryDestroy(instantly);
            DrawerSetter(null);
            OnDrawerDestroyed?.Invoke(this, EventArgs.Empty);
        }

        public void Add(ITableSleeveCard card)
        {
            _cards.Add(card);
            _drawer?.AddCardDrawer(card);
        }
        public void Remove(ITableSleeveCard card)
        {
            _cards.Remove(card);
            _drawer?.RemoveCardDrawer(card);
        }

        public virtual void Dispose()
        {
            _cards.Clear();
            _deck.Dispose();
            _drawer?.Dispose();
            _drawer = null;
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is TableSleeveCloneArgs cArgs)
                return new TableSleeve(this, cArgs);
            else return null;
        }

        protected virtual void DrawerSetter(TableSleeveDrawer value)
        {
            _drawer = value;
        }
        protected virtual TableSleeveDrawer DrawerCreator(Transform parent)
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
            _drawer.SetCollider(false);

            List<ITableSleeveCard> cards = new(count);
            int delay = 500;
            for (int i = 0; i < count; i++)
            {
                Card takenCard = _deck.Take();
                if (takenCard == null) break;

                ITableSleeveCard sCard = HoldingCardCreator(takenCard);
                if (sCard == null) continue;
                
                Add(sCard);
                sCard.PullOutBase();
                cards.Add(sCard);

                await UniTask.Delay(delay);
                if (delay > 250)
                    delay -= 50;
            }

            await UniTask.Delay(500);
            foreach (ITableSleeveCard card in cards)
                card.PullInBase();
            _drawer.SetCollider(true);
        }

        void TakeMissingCardsInstantly()
        {
            TakeCardsInstantly(count: _deck.Count);
        }
        void TakeCardsInstantly(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Card takenCard = _deck.Take();
                if (takenCard == null) break;

                ITableSleeveCard sCard = HoldingCardCreator(takenCard);
                if (sCard == null) continue;
                
                Add(sCard);
            }
        }

        public IEnumerator<ITableSleeveCard> GetEnumerator() => _cards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
