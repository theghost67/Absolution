using DG.Tweening;
using Game.Palette;
using GreenOne;
using MyBox;
using System;
using TMPro;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с нижними иконками типа <see cref="TableCard"/>.<br/>
    /// У иконок есть текст, на котором можно отобразить значение привязанной переменной.
    /// </summary>
    public class TableCardLowerIconDrawer : TableCardIconDrawer
    {
        readonly TextMeshPro _textMesh;
        Tween _textTween;
        int _textValue;
        bool _hasValue;

        public TableCardLowerIconDrawer(TableCardDrawer card, Func<int> cardDisplayValue, Transform worldTransform) : this(card, cardDisplayValue, worldTransform.gameObject) { }
        public TableCardLowerIconDrawer(TableCardDrawer card, Func<int> cardDisplayValue, GameObject worldObject) : base(card, cardDisplayValue, worldObject)
        {
            _textMesh = transform.Find<TextMeshPro>("Text");
        }

        public override void RedrawValueAsNull()
        {
            _textTween.Kill();
            _textMesh.text = "";
            _hasValue = false;
        }
        public override void RedrawValue() => RedrawText(cardDisplayValue());
        public override void RedrawValue(int value) => RedrawText(value);

        public override void RedrawColor()
        {
            _textMesh.color = ColorPalette.C1.ColorCur;
        }
        public override void RedrawColor(Color color)
        {
            _textMesh.color = color;
        }

        protected override void SetSortingOrder(int value)
        {
            base.SetSortingOrder(value);
            int overlapOrder = value + 1;
            _textMesh.sortingOrder = overlapOrder;
        }
        protected override void SetColor(Color value)
        {
            base.SetColor(value);
            _textMesh.color = value;
        }

        void RedrawText(int value)
        {
            if (!_hasValue)
            {
                _textMesh.text = value.ToString();
                goto End;
            }

            if (value == _textValue) return;
            if (value > _textValue)
                 _textMesh.color = Color.green;
            else _textMesh.color = Color.red;

            _textTween.Kill();
            _textTween = DOVirtual.Int(_textValue, value, 1f, v => _textMesh.text = $"{v}").OnComplete(RedrawColor);

            End:
            _textValue = value;
            _hasValue = true;
        }
    }
}
