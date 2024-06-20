﻿using DG.Tweening;
using Game.Palette;
using GreenOne;
using TMPro;
using UnityEngine;
using MyBox;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с одной из иконок типа <see cref="TableCard"/>.
    /// </summary>
    public class TableCardIconDrawer : Drawer
    {
        public readonly TableCardDrawer card;
        public readonly TableCardIconType type; // can be removed (with 'switch') by creating derived classes..

        readonly SpriteRenderer _renderer;
        readonly TextMeshPro _textMesh;     // depends on type
        readonly SpriteRenderer[] _chunks;  // depends on type
        Tween _chunksTween;

        public TableCardIconDrawer(TableCardDrawer card, Transform worldTransform, TableCardIconType type) : this(card, worldTransform.gameObject, type) { }
        public TableCardIconDrawer(TableCardDrawer card, GameObject worldObject, TableCardIconType type) : base(card, worldObject)
        {
            this.card = card;
            this.type = type;

            _renderer = gameObject.GetComponent<SpriteRenderer>();
            _textMesh = type == TableCardIconType.Texts ? transform.Find<TextMeshPro>("Text") : null;
            _chunks = type == TableCardIconType.Chunks ? ChunksArrayFilledWithChildren(transform.Find("Chunks")) : null;
            _chunksTween = Utils.emptyTween;

            BlocksSelection = false;
        }

        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            int overlapOrder = value + 1;

            _renderer.sortingOrder = overlapOrder;
            switch (type)
            {
                case TableCardIconType.Chunks:
                    foreach (SpriteRenderer chunk in _chunks)
                        chunk.sortingOrder = overlapOrder;
                    break;

                case TableCardIconType.Texts:
                    _textMesh.sortingOrder = overlapOrder;
                    break;
            }
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            _renderer.SetAlpha(value);
            switch (type)
            {
                case TableCardIconType.Chunks:
                    foreach (SpriteRenderer chunk in _chunks)
                        chunk.SetAlpha(value);
                    break;

                case TableCardIconType.Texts:
                    _textMesh.SetAlpha(value);
                    break;
            }
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            _renderer.color = value;
            switch (type)
            {
                case TableCardIconType.Chunks:
                    foreach (SpriteRenderer chunk in _chunks)
                        chunk.color = value;
                    break;

                case TableCardIconType.Texts:
                    _textMesh.color = value;
                    break;
            }
        }

        public void RedrawSprite(Sprite sprite)
        {
            _renderer.sprite = sprite;
        }
        public void RedrawText(int value)
        {
            if (type != TableCardIconType.Texts) return;
            _textMesh.text = value.ToString();
        }
        public void RedrawChunks(int count)
        {
            if (type != TableCardIconType.Chunks) return;
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i].gameObject.SetActive(i < count);
        }

        public void RedrawChunksColor()
        {
            RedrawChunksColor(ColorPalette.GetColor(0));
        }
        public void RedrawChunksColor(Color color)
        {
            foreach (SpriteRenderer chunk in _chunks)
                chunk.color = color;
        }
        public void HighlightChunks(Color color)
        {
            _chunksTween.Kill();
            _chunksTween = DOVirtual.DelayedCall(1, () => RedrawChunksColor());
            RedrawChunksColor(color);
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