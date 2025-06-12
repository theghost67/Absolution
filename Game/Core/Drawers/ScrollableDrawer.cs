using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Traits;
using GreenOne;
using MyBox;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Абстрактный класс, представляющий область прокрутки, внутри которой могут быть отрисовщики.<br/>
    /// Отрисовщики могут маскироваться в области прокрутки, используя порядок сортировки.
    /// </summary>
    public abstract class ScrollableDrawer : Drawer
    {
        const float SCROLL_DISTANCE = 0.1f;
        const float VIEWPORT_EDGE_OFFSET = 0.02f;
        const float VIEWPORT_ELEMENTS_OFFSET = 0.00f;

        readonly SpriteMask _mask;
        readonly List<Drawer> _drawers;
        readonly Transform _viewport;

        float _scrollViewSize;
        float _viewportSize;
        Tween _scrollTween;

        public ScrollableDrawer(object attached, GameObject worldObject) : this(attached, worldObject.transform) { }
        public ScrollableDrawer(object attached, Component worldComponent) : this(attached, worldComponent.transform) { }
        public ScrollableDrawer(object attached, GameObject prefab, Transform parent) : this(attached, GameObject.Instantiate(prefab, parent).transform) { }
        public ScrollableDrawer(object attached, Transform worldTransform) : base(attached, worldTransform)
        {
            gameObject.name = $"Scroll view ({nameof(ScrollableDrawer)})";
            BlocksSelection = false;

            _drawers = new List<Drawer>();
            _viewport = transform.CreateEmptyObject("Viewport");

            if (!gameObject.TryGetComponent(out _mask))
                throw new System.NullReferenceException($"gameObject of a {nameof(ScrollableDrawer)} must have a {nameof(SpriteMask)} component.");
            if (gameObject.GetComponent<BoxCollider2D>() == null)
                throw new System.NullReferenceException($"gameObject of a {nameof(ScrollableDrawer)} must have a {nameof(BoxCollider2D)} component.");

            SetSortingOrder(0);
        }

        public void AddToViewport(Drawer drawer, bool update = true)
        {
            if (drawer?.IsDestroyed ?? true) return;
            _drawers.Add(drawer);
            drawer.transform.SetParent(_viewport, true);
            drawer.SortingOrder = SortingOrder;
            if (update) UpdateViewport();
        }
        public void RemoveFromViewport(Drawer drawer, bool update = true)
        {
            if (drawer?.IsDestroyed ?? true) return;
            _drawers.Remove(drawer);
            if (update) UpdateViewport();
        }
        public void UpdateViewport()
        {
            _scrollViewSize = ColliderSize.y;
            _viewportSize = 0;
            float nextDrawerPosY = _scrollViewSize / 2f - VIEWPORT_EDGE_OFFSET;
            foreach (Drawer drawer in _drawers)
            {
                float drawerSizeY = drawer.ColliderSize.y;
                drawer.transform.localPosition = Vector3.up * (nextDrawerPosY - (drawerSizeY / 2));
                nextDrawerPosY -= drawerSizeY + VIEWPORT_ELEMENTS_OFFSET;
                _viewportSize += drawerSizeY + VIEWPORT_ELEMENTS_OFFSET;
            }
            _viewportSize += VIEWPORT_EDGE_OFFSET;
        }

        public UniTask AddToViewportAsync(Drawer drawer)
        {
            _drawers.Add(drawer);
            drawer.transform.SetParent(_viewport, true);
            drawer.SortingOrder = SortingOrder;
            return UpdateViewportAsync();
        }
        public UniTask RemoveFromViewportAsync(Drawer drawer)
        {
            _drawers.Remove(drawer);
            return UpdateViewportAsync();
        }
        public UniTask UpdateViewportAsync()
        {
            _scrollViewSize = ColliderSize.y;
            _viewportSize = 0;
            float nextDrawerPosY = _scrollViewSize / 2f - VIEWPORT_EDGE_OFFSET;
            UniTask lastTask = UniTask.CompletedTask;
            for (int i = 0; i < _drawers.Count; i++)
            {
                Drawer drawer = _drawers[i];
                float drawerSizeY = drawer.ColliderSize.y;
                float targetLocalPosY = nextDrawerPosY - (drawerSizeY / 2);
                if (drawer.transform.localPosition.y != targetLocalPosY)
                    lastTask = drawer.transform.DOLocalMoveY(targetLocalPosY, 0.5f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
                nextDrawerPosY -= drawerSizeY + VIEWPORT_ELEMENTS_OFFSET;
                _viewportSize += drawerSizeY + VIEWPORT_ELEMENTS_OFFSET;
            }
            _viewportSize += VIEWPORT_EDGE_OFFSET;
            return lastTask;
        }

        public Tween ScrollTo(Drawer to)
        {
            if (IsDestroyed) return null;
            _scrollTween.Kill();

            float targetUpperPosY = 0;
            float targetLowerPosY = 0;
            foreach (Drawer drawer in _drawers)
            {
                if (drawer != to) continue;
                float drawerHalfSizeY = (drawer.ColliderSize.y + VIEWPORT_EDGE_OFFSET) * drawer.transform.lossyScale.y / 2f;
                targetUpperPosY = drawer.transform.position.y + drawerHalfSizeY;
                targetLowerPosY = drawer.transform.position.y - drawerHalfSizeY;
                break;
            }

            float scrollViewHalfSizeY = _scrollViewSize * transform.lossyScale.y / 2f;
            float scrollViewUpperPosY = transform.position.y + scrollViewHalfSizeY;
            float scrollViewLowerPosY = transform.position.y - scrollViewHalfSizeY;
            if (scrollViewUpperPosY > targetUpperPosY && scrollViewLowerPosY < targetLowerPosY)
                return null; // is visible

            float posYDelta = targetLowerPosY < scrollViewLowerPosY ? scrollViewLowerPosY - targetLowerPosY : scrollViewUpperPosY - targetUpperPosY;
            return _scrollTween = _viewport.DOMoveY(_viewport.position.y + posYDelta, 0.5f).SetEase(Ease.InOutQuad);
        }
        public bool ScrollIsPlaying()
        {
            return _scrollTween != null && _scrollTween.IsPlaying();
        }
        public void ScrollStop()
        {
            _scrollTween.Kill();
        }

        protected override void SetCollider(bool value)
        {
            base.SetCollider(value);
            foreach (Drawer drawer in _drawers)
            {
                if (drawer != null)
                    drawer.ColliderEnabled = value;
            }
        }
        protected override void SetSortingOrder(int value)
        {
            base.SetSortingOrder(value);
            _mask.backSortingOrder = value - 1;
            _mask.frontSortingOrder = value + 1;
            foreach (Drawer drawer in _drawers)
            {
                if (drawer != null)
                    drawer.SortingOrder = value;
            }
        }
        protected override void SetColor(Color value)
        {
            base.SetColor(value);
            foreach (Drawer drawer in _drawers)
            {
                if (drawer != null)
                    drawer.Color = drawer.Color.WithAlpha(value.a);
            }
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            Global.OnFixedUpdate += OnFixedUpdateWhileMouseEntered;
            foreach (Drawer drawer in _drawers)
                drawer.IsSelected = true;
        }
        protected override void OnMouseScrollBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseScrollBase(sender, e);
            if (IgnoreMouseScroll()) return;
            Scroll(e.scrollDeltaY);
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            Global.OnFixedUpdate -= OnFixedUpdateWhileMouseEntered;
        }

        protected virtual bool IgnoreMouseScroll()
        {
            return ScrollIsPlaying();
        }
        private void OnFixedUpdateWhileMouseEntered()
        {
            float value = Input.GetAxis("Vertical");
            if (value != 0)
                Scroll(value);
        }
        private void Scroll(float delta)
        {
            Vector3 pos = _viewport.localPosition;
            float maxY = (_viewportSize - _scrollViewSize).ClampedMin(0);
            pos.y = (pos.y - delta * SCROLL_DISTANCE).Clamped(0, maxY);
            _viewport.localPosition = pos;
        }
    }
}
