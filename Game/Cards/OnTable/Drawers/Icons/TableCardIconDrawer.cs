using System;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный класс, представляющий взаимодействие пользователя с одной из иконок типа <see cref="TableCard"/>.
    /// </summary>
    public abstract class TableCardIconDrawer : Drawer
    {
        public readonly TableCardDrawer owner;
        protected readonly Func<int> cardDisplayValue;
        readonly SpriteRenderer _renderer;

        public TableCardIconDrawer(TableCardDrawer card, Func<int> displayValue, Transform worldTransform) : this(card, displayValue, worldTransform.gameObject) { }
        public TableCardIconDrawer(TableCardDrawer card, Func<int> displayValue, GameObject worldObject) : base(card, worldObject)
        {
            owner = card;
            cardDisplayValue = displayValue;
            _renderer = gameObject.GetComponent<SpriteRenderer>();
            BlocksSelection = false;
            card.OnMouseEnter += OnOwnerMouseEnter;
        }

        public void RedrawSprite(Sprite sprite)
        {
            _renderer.sprite = sprite;
        }

        public abstract void RedrawValueAsNull();
        public abstract void RedrawValue();
        public abstract void RedrawValue(int value);

        public abstract void RedrawColor();
        public abstract void RedrawColor(Color color);

        protected override void SetSortingOrder(int value)
        {
            base.SetSortingOrder(value);
            _renderer.sortingOrder = value + 1;
        }
        protected override void SetColor(Color value)
        {
            base.SetColor(value);
            _renderer.color = value;
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            e.handled |= !owner.IsSelected;
            if (!e.handled)
                base.OnMouseEnterBase(sender, e);
        }
        protected override void OnDestroyBase(object sender, EventArgs e)
        {
            base.OnDestroyBase(sender, e);
            owner.OnMouseEnter -= OnOwnerMouseEnter;
        }

        void OnOwnerMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (IsSelected)
                OnMouseEnterBase(this, e);
        }
    }
}
