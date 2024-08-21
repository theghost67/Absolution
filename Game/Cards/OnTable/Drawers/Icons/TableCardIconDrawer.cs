using System;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Абстрактный класс, представляющий взаимодействие пользователя с одной из иконок типа <see cref="TableCard"/>.
    /// </summary>
    public abstract class TableCardIconDrawer : Drawer
    {
        public readonly TableCardDrawer card;
        protected readonly Func<int> cardDisplayValue;
        readonly SpriteRenderer _renderer;

        public TableCardIconDrawer(TableCardDrawer card, Func<int> cardDisplayValue, Transform worldTransform) : this(card, cardDisplayValue, worldTransform.gameObject) { }
        public TableCardIconDrawer(TableCardDrawer card, Func<int> cardDisplayValue, GameObject worldObject) : base(card, worldObject)
        {
            this.card = card;
            this.cardDisplayValue = cardDisplayValue;
            _renderer = gameObject.GetComponent<SpriteRenderer>();
            BlocksSelection = false;
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

        public void RedrawSprite(Sprite sprite)
        {
            _renderer.sprite = sprite;
        }
    }
}
