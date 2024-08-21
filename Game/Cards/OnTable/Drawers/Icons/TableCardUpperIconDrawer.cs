using DG.Tweening;
using Game.Palette;
using MyBox;
using System;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с верхними иконками типа <see cref="TableCard"/>.<br/>
    /// У иконок есть шкала с пятью делениями, на которой можно отобразить значение привязанной переменной.
    /// </summary>
    public class TableCardUpperIconDrawer : TableCardIconDrawer
    {
        static readonly Color overflowColor  = new(0.50f, 1.00f, 0.75f);
        static readonly Color underflowColor = new(1.00f, 0.50f, 0.50f);
        readonly SpriteRenderer[] _chunks;
        Tween _chunksTween;
        int _chunksValue;
        bool _hasValue;

        public TableCardUpperIconDrawer(TableCardDrawer card, Func<int> cardDisplayValue, Transform worldTransform) : this(card, cardDisplayValue, worldTransform.gameObject) { }
        public TableCardUpperIconDrawer(TableCardDrawer card, Func<int> cardDisplayValue, GameObject worldObject) : base(card, cardDisplayValue, worldObject)
        {
            _chunks = ChunksArrayFilledWithChildren(transform.Find("Chunks"));
        }

        public override void RedrawValueAsNull()
        {
            RedrawChunks(0);
            _hasValue = false;
        }
        public override void RedrawValue() => RedrawChunks(cardDisplayValue());
        public override void RedrawValue(int value) => RedrawChunks(value);

        public override void RedrawColor()
        {
            RedrawColor(GetChunksColor());
        }
        public override void RedrawColor(Color color)
        {
            foreach (SpriteRenderer chunk in _chunks)
                chunk.color = color;
        }

        protected override void SetSortingOrder(int value)
        {
            base.SetSortingOrder(value);
            int overlapOrder = value + 1;
            foreach (SpriteRenderer chunk in _chunks)
                chunk.sortingOrder = overlapOrder;
        }
        protected override void SetColor(Color value)
        {
            base.SetColor(value);
            foreach (SpriteRenderer chunk in _chunks)
                chunk.color = value;
        }

        void RedrawChunks(int value)
        {
            if (!_hasValue)
            {
                SetChunksActive(value);
                goto End;
            }

            if (value == _chunksValue) return;
            if (value > _chunksValue)
                 RedrawColor(Color.green);
            else RedrawColor(Color.red);

            _chunksTween.Kill();
            if (value < 0 && _chunksValue < 0)
                 _chunksTween = DOVirtual.DelayedCall(1f, RedrawColor);
            else _chunksTween = DOVirtual.Int(_chunksValue, value, 1f, SetChunksActive).OnComplete(() =>
            {
                RedrawColor();
                if (value < 0)
                    SetChunksActive(5);
            });

            End:
            _chunksValue = value;
            _hasValue = true;
        }
        void SetChunksActive(int value)
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i].gameObject.SetActive(i < value);
        }

        Color GetChunksColor()
        {
            if (_chunksValue > 5)
                return overflowColor;
            else if (_chunksValue < 0)
                return underflowColor;
            else return ColorPalette.C1.ColorCur;
        }
        SpriteRenderer[] ChunksArrayFilledWithChildren(Transform chunksParent)
        {
            SpriteRenderer[] array = new SpriteRenderer[5]; // displays 0 to 5 chunks
            int index = 0;
            foreach (Transform child in chunksParent)
                array[index++] = child.GetComponent<SpriteRenderer>();
            return array;
        }
    }
}
