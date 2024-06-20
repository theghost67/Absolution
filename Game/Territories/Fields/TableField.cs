using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using GreenOne;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий игровое поле на столе с возможностью хранения карты типа <see cref="TableFieldCard"/>.
    /// </summary>
    public class TableField : Unique, ITableDrawable, ITableFindable, ICloneableWithArgs, IDisposable
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public IIdEventVoidAsync OnCardAttached => _onCardAttached;
        public IIdEventVoidAsync OnCardDetatched => _onCardDetatched;

        public TableTerritory Territory => _territory;
        public TableFieldCard Card => _card;
        public TableFieldDrawer Drawer => _drawer;
        public TableFinder Finder => _finder;
        Drawer ITableDrawable.Drawer => _drawer;

        public readonly int2 pos;
        public readonly TableStat health;

        readonly TableTerritory _territory;
        readonly TableFieldFinder _finder;
        readonly TableEventVoid _onCardAttached;
        readonly TableEventVoid _onCardDetatched; // invokes before setting card to null

        TableFieldDrawer _drawer;
        TableFieldCard _card;

        // territory can be null
        public TableField(TableTerritory territory, int2 pos, Transform parent, bool withDrawer = true) : base()
        {
            OnDrawerCreated += OnDrawerCreatedBase;
            OnDrawerDestroyed += OnDrawerDestroyedBase;

            _territory = territory;
            _finder = new TableFieldFinder(this);
            _onCardAttached = new TableEventVoid();
            _onCardDetatched = new TableEventVoid();

            this.pos = pos;
            health = new TableStat(this);
            health.OnPostSet.Add(OnHealthPostSet);

            if (withDrawer)
                CreateDrawer(parent);
        }
        protected TableField(TableField src, TableFieldCloneArgs args) : base(src.Guid)
        {
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();

            pos = src.pos;
            _finder = new TableFieldFinder(this);
            _onCardAttached = (TableEventVoid)src._onCardAttached.Clone();
            _onCardDetatched = (TableEventVoid)src._onCardDetatched.Clone();

            TableStatCloneArgs healthCArgs = new(this, args.terrCArgs);
            health = (TableStat)src.health.Clone(healthCArgs);

            args.AddOnClonedAction(src.GetType(), typeof(TableField), () => CardBaseSetter(CardCloner(src.Card, args)));
        }

        public virtual void Dispose()
        {
            if (_card != null)
            {
                _card.Dispose();
                CardBaseSetter(null);
            }

            if (_drawer != null)
                _drawer.Dispose();
        }
        public virtual object Clone(CloneArgs args)
        {
            if (args is TableFieldCloneArgs cArgs)
                 return new TableField(this, cArgs);
            else return null;
        }

        public async UniTask AttachCard(TableFieldCard card)
        {
            if (_card != null) return;
            if (card == null)
                throw new InvalidOperationException("Cannot attach null card. Use DetatchCard() instead.");

            CardBaseSetter(card);
            if (_drawer != null)
                await _drawer.AnimAttachCard(card.Drawer).AsyncWaitForCompletion();
            await _onCardAttached.Invoke(this, EventArgs.Empty);
            await card.AttachToAnotherField(this);
        }
        public async UniTask DetatchCard()
        {
            if (_card == null) return;
            TableFieldCard card = _card;

            CardBaseSetter(null);
            await _onCardDetatched.Invoke(this, EventArgs.Empty);
            if (_drawer != null)
                await _drawer.AnimDetatchCard(card.Drawer).AsyncWaitForCompletion();
            await card.AttachToAnotherField(null);
        }

        public void CreateDrawer(Transform parent)
        {
            if (_drawer != null) return;
            TableFieldDrawer drawer = DrawerCreator(parent);
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

        protected virtual void DrawerSetter(TableFieldDrawer value)
        {
            _drawer = value;
        }
        protected virtual TableFieldDrawer DrawerCreator(Transform parent)
        {
            return new TableFieldDrawer(this, parent);
        }

        protected virtual void CardPropSetter(TableFieldCard value)
        {
            if (value == _card) return;
            if (value != null)
                 AttachCard(value);
            else DetatchCard();
        }
        protected virtual void CardBaseSetter(TableFieldCard value)
        {
            _card = value;
        }
        protected virtual TableFieldCard CardCloner(TableFieldCard src, TableFieldCloneArgs args)
        {
            if (src == null) return null;
            TableFieldCardCloneArgs cardCArgs = new((FieldCard)src.Data.Clone(), this, args.terrCArgs);
            return (TableFieldCard)src.Clone(cardCArgs);
        }

        protected virtual void OnDrawerCreatedBase(object sender, EventArgs e)
        {
            TableField field = (TableField)sender;
            if (field.Card != null) field._card.CreateDrawer(_drawer.transform);
        }
        protected virtual void OnDrawerDestroyedBase(object sender, EventArgs e)
        {
            TableField field = (TableField)sender;
            if (field.Card != null) field._card.DestroyDrawer(true);
        }

        UniTask OnHealthPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableField field = (TableField)stat.Owner;
            field.Drawer.CreateDamageTextSplash(e.totalDeltaValue);
            return UniTask.CompletedTask;
        }
    }
}
