using MyBox;
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

        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            _renderer.sortingOrder = value + 1;
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            _renderer.color = value;
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            _renderer.SetAlpha(value);
        }

        public abstract void RedrawValueAsNull();
        public abstract void RedrawValue();
        public abstract void RedrawValue(int value);

        public abstract void RedrawColor();
        public abstract void RedrawColor(Color color);

        public void RedrawSprite(Sprite sprite)
        {
            _renderer.sprite = sprite;
        }
    }
}
