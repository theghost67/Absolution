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
        const int COVERING_ORDER = 32;
        const float COVERING_START_Y = -40f / Global.NORMAL_SCALE;
        const float COVERING_END_Y = 56f / Global.NORMAL_SCALE;
        const float COVERING_SPEED = 25f;

        protected abstract SpriteRenderer SelectableRenderer { get; }

        static readonly GameObject _selectionPrefab;
        static readonly GameObject _outlinePrefab;
        static readonly GameObject _coveringPrefab;

        Tweener _selectionTween;
        SpriteRenderer _selectionRenderer;

        Tweener _outlineTween;
        SpriteRenderer _outlineRenderer;

        static int _totalCoverings;
        Tweener _coveringTween;
        Transform _coveringElement;

        bool _isSelected;
        bool _isOutlined;
        bool _isCovered;

        static SelectableDrawer()
        {
            _selectionPrefab = Resources.Load<GameObject>("Prefabs/Selectable/Selection");
            _coveringPrefab = Resources.Load<GameObject>("Prefabs/Selectable/Covering");
            _outlinePrefab = Resources.Load<GameObject>("Prefabs/Selectable/Outline");
        }
        public SelectableDrawer(object attached, GameObject worldObject) : base(attached, worldObject) { }
        public SelectableDrawer(object attached, Component worldComponent) : base(attached, worldComponent) { }
        public SelectableDrawer(object attached, Transform worldTransform) : base(attached, worldTransform) { }
        public SelectableDrawer(object attached, GameObject prefab, Transform parent) : base(attached, prefab, parent) { }

        // DO NOT USE booleans to check state, instead, kill previous show/hide tween and start a new anim
        public Tween AnimShowSelection()
        {
            if (_isSelected) 
                return Utils.emptyTween;

            SpriteRenderer renderer = SelectableRenderer;
            if (renderer.drawMode == SpriteDrawMode.Sliced)
                throw new NotSupportedException("Cannot create drawer selection on sprites with draw mode set to Sliced.");

            Vector2 inflatedSize = (Vector2.one * 32 + renderer.sprite.rect.size) / Global.NORMAL_SCALE;
            Vector2 normalSize = (Vector2.one * 16 + renderer.sprite.rect.size) / Global.NORMAL_SCALE;

            _isSelected = true;
            _selectionRenderer = GameObject.Instantiate(_selectionPrefab, Global.Root).GetComponent<SpriteRenderer>();
            _selectionRenderer.transform.position = transform.position;
            _selectionRenderer.size = inflatedSize;
            return _selectionTween = DOTween.To(() => _selectionRenderer.size, v => _selectionRenderer.size = v, normalSize, 0.5f).SetEase(Ease.OutCubic);
        }
        [Obsolete("Make as outline pulse")] public Tween AnimShowOutline()
        {
            if (_isOutlined) 
                return Utils.emptyTween;

            _isOutlined = true;
            _outlineRenderer = GameObject.Instantiate(_outlinePrefab, Global.Root).GetComponent<SpriteRenderer>();
            _outlineRenderer.transform.position = transform.position;
            _outlineTween = DOVirtual.Float(0, 1, 1, null).OnComplete(() =>
            {
                _outlineRenderer.gameObject.SetActive(!_outlineRenderer.gameObject.activeSelf);
                _outlineTween.Restart();
            });
            return _outlineTween;
        }
        [Obsolete("Implement or remove")] public Tween AnimShowCovering()
        {
            return Utils.emptyTween;
            //if (_isCovered) return;

            //_isCovered = true;
            //_totalCoverings++;

            //var order = COVERING_ORDER + _totalCoverings;
            //var coveringMask = GameObject.Instantiate(_coveringPrefab, Global.Root).GetComponent<SpriteMask>();
            //coveringMask.transform.position = transform.position;
            //coveringMask.frontSortingOrder = order;
            //coveringMask.backSortingOrder = order - 1;

            //_coveringElement = coveringMask.transform.GetChild(0);
            //_coveringElement.GetComponent<SpriteRenderer>().sortingOrder = order;

            //OnDestroyed += DestroyCovering;
            //CoveringMove();
        }

        // TODO: implement hide animations
        public Tween AnimHideSelection()
        {
            if (!_isSelected) 
                return Utils.emptyTween;

            _isSelected = false;
            _selectionTween.Kill();
            _selectionRenderer.gameObject.Destroy();
            return Utils.emptyTween;
        }
        public Tween AnimHideOutline()
        {
            if (!_isOutlined) 
                return Utils.emptyTween;

            _isOutlined = false;
            _outlineTween.Kill();
            _outlineRenderer.gameObject.Destroy();
            return Utils.emptyTween;
        }
        public Tween AnimHideCovering()
        {
            if (!_isCovered) 
                return Utils.emptyTween;

            _isCovered = false;
            _totalCoverings--;
            _coveringElement.parent.gameObject.Destroy();
            return Utils.emptyTween;
            _coveringTween.Kill();
        }

        //void CoveringMove()
        //{
        //    _coveringTween = DOVirtual.Float(0, 0, 30, v =>
        //    {
        //        _coveringElement.transform.position += COVERING_SPEED * Time.fixedDeltaTime * Vector3.up;

        //        if (_coveringElement.transform.localPosition.y >= COVERING_END_Y)
        //            _coveringElement.transform.localPosition += (COVERING_START_Y - COVERING_END_Y) * Vector3.up;
        //    }).SetLoops(-1);
        //}
    }
}
