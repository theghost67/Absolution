using DG.Tweening;
using GreenOne;
using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Абстрактный класс, представляющий отрисовщик с возможностью его выделения/обводки/покрытия, используя <see cref="SpriteRenderer"/>.
    /// </summary>
    public abstract class SelectableDrawer : Drawer
    {
        protected abstract SpriteRenderer SelectableRenderer { get; }

        static readonly GameObject _selectionPrefab;
        static readonly GameObject _outlinePrefab;
        static readonly GameObject _coveringPrefab;

        readonly SpriteRenderer _selectionRenderer;
        readonly SpriteRenderer _outlineRenderer;

        Tweener _selectionTween;
        Tweener _outlineTween;

        static SelectableDrawer()
        {
            _selectionPrefab = Resources.Load<GameObject>("Prefabs/Selectable/Selection");
            _coveringPrefab = Resources.Load<GameObject>("Prefabs/Selectable/Covering");
            _outlinePrefab = Resources.Load<GameObject>("Prefabs/Selectable/Outline");
        }
        public SelectableDrawer(object attached, GameObject worldObject) : this(attached, worldObject.transform) { }
        public SelectableDrawer(object attached, Component worldComponent) : this(attached, worldComponent.transform) { }
        public SelectableDrawer(object attached, GameObject prefab, Transform parent) : this(attached, GameObject.Instantiate(prefab, parent).transform) { }
        public SelectableDrawer(object attached, Transform worldTransform) : base(attached, worldTransform) 
        {
            GameObject outlineGO = GameObject.Instantiate(_outlinePrefab, transform);
            outlineGO.name = $"Outline ({nameof(SelectableDrawer)})";

            GameObject selectionGO = GameObject.Instantiate(_selectionPrefab, transform);
            selectionGO.name = $"Selection ({nameof(SelectableDrawer)})";

            _outlineRenderer = outlineGO.GetComponent<SpriteRenderer>();
            _outlineRenderer.color = Color.clear;

            _selectionRenderer = selectionGO.GetComponent<SpriteRenderer>();
            _selectionRenderer.color = Color.clear;
        }

        public Tween AnimShowSelection()
        {
            SpriteRenderer renderer = SelectableRenderer;
            if (renderer.drawMode == SpriteDrawMode.Sliced)
                throw new NotSupportedException("Cannot create drawer selection on sprites with draw mode set to Sliced.");

            Vector2 inflatedSize = (Vector2.one * 32 + renderer.sprite.rect.size) / Global.NORMAL_SCALE;
            Vector2 normalSize = (Vector2.one * 16 + renderer.sprite.rect.size) / Global.NORMAL_SCALE;

            _selectionTween.Kill();
            _selectionRenderer.transform.position = transform.position;
            _selectionRenderer.size = inflatedSize;
            _selectionRenderer.color = Color.white;
            return _selectionTween = DOTween.To(() => _selectionRenderer.size, v => _selectionRenderer.size = v, normalSize, 0.5f).SetEase(Ease.OutCubic);
        }
        // TODO: make pulsing?
        public Tween AnimShowOutline()
        {
            _outlineTween.Kill();
            _outlineRenderer.transform.position = transform.position;
            _outlineRenderer.color = Color.white;
            _outlineTween = DOVirtual.Float(0, 1, 1, null).SetLoops(-1).OnComplete(() =>
            _outlineRenderer.gameObject.SetActive(!_outlineRenderer.gameObject.activeSelf));
            return _outlineTween;
        }

        public Tween AnimHideSelection()
        {
            SpriteRenderer renderer = SelectableRenderer;
            if (renderer.drawMode == SpriteDrawMode.Sliced)
                throw new NotSupportedException("Cannot create drawer selection on sprites with draw mode set to Sliced.");

            Color startColor = _selectionRenderer.color;
            Color endColor = new(1, 1, 1, 0);
            if (startColor == Color.clear) return null;
            Vector2 inflatedSize = (Vector2.one * 32 + renderer.sprite.rect.size) / Global.NORMAL_SCALE;
            Vector2 normalSize = (Vector2.one * 16 + renderer.sprite.rect.size) / Global.NORMAL_SCALE;

            _selectionTween.Kill();
            _selectionTween = DOVirtual.Float(0, 1, 0.5f, v =>
            {
                _selectionRenderer.size = Vector2.Lerp(normalSize, inflatedSize, v);
                _selectionRenderer.color = Color.Lerp(startColor, endColor, v);
            }).SetEase(Ease.OutCubic);
            return _selectionTween;
        }
        public Tween AnimHideOutline()
        {
            _outlineTween.Kill();
            _outlineRenderer.color = Color.clear;
            return null;
        }
    }
}
