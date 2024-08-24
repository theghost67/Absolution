using Cysharp.Threading.Tasks;
using DG.Tweening;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Класс, позволяющий пользователю взаимодействовать с объектом, используя его <see cref="BoxCollider2D"/>.
    /// </summary>
    public class Drawer : IEquatable<Drawer>, IDisposable
    {
        public static IEnumerable<Drawer> SelectedDrawers => Behaviour.SelectedDrawers;
        public static bool InAnySelected => Behaviour.IsAnySelected;

        public bool IsDestroying => _isDestroying;
        public bool IsDestroyed => _isDestroyed;

        public bool IsSelected 
        {
            get => _behaviour.IsSelected;
            set => _behaviour.IsSelected = value;
        }
        public bool BlocksSelection 
        {
            get => _blocksSelection;
            set => _blocksSelection = value;
        }

        public bool ChangePointer
        {
            get => _changePointer;
            set
            {
                _changePointer = value;
                if (IsSelected)
                    Pointer.Redraw();
            }
        }
        public bool ColliderEnabled
        {
            get => _colliderEnabled;
            set { if (ColliderEnabled != value) SetCollider(value); }
        }
        public Vector2 ColliderSize
        {
            get => _colliderSize;
            set { if (ColliderSize != value) SetColliderSize(value); }
        }

        public int SortingOrder
        {
            get => _sortingOrder;
            set { if (SortingOrder != value) SetSortingOrder(value); }
        }
        public int SortingOrderDefault
        {
            get => _sortingDefault;
            set 
            {
                if (SortingOrderDefault != value) 
                    SetSortingOrderDefault(value);
                SortingOrder = value;
            }
        }

        public float Alpha
        {
            get => _color.a;
            set { if (Alpha != value) SetAlpha(value); }
        }
        public Color Color
        {
            get => _color;
            set { if (Color != value) SetColor(value); }
        }

        public event DrawerMouseEventHandler OnMouseEnter;
        public event DrawerMouseEventHandler OnMouseHover;
        public event DrawerMouseEventHandler OnMouseLeave;
        public event DrawerMouseEventHandler OnMouseClick;
        public event DrawerMouseEventHandler OnMouseScroll;

        public event EventHandler OnEnable;
        public event EventHandler OnDisable;
        public event EventHandler OnDestroy;

        public readonly object attached; // object attached to this drawer (drawer displays it's data)
        public readonly GameObject gameObject;
        public readonly Transform transform;

        readonly BoxCollider2D _collider;
        readonly Behaviour _behaviour;

        HorizontalAlignmentOptions _tooltipAlign;
        Func<string> _tooltipFunc;

        Vector2 _colliderSize;
        Color _color;
        int _sortingOrder;
        int _sortingDefault;

        bool _blocksSelection;
        bool _changePointer;
        bool _colliderEnabled;
        bool _isDestroying;
        bool _isDestroyed;

        // this class implements colliders' mouse events logic
        class Behaviour : MonoBehaviour, IComparable<Behaviour>
        {
            public static IEnumerable<Behaviour> SelectedBehaviours => _selectedBehaviours;
            public static IEnumerable<Drawer> SelectedDrawers => _selectedBehaviours.Select(b => b._drawer);

            public static bool IsAnySelected => _selectedBehaviours.Count != 0;
            public bool IsSelected
            {
                get => _selected;
                set
                {
                    if (value)
                         Reselect();
                    else Deselect();
                }
            }

            static readonly HashSet<Behaviour> _aliveBehaviours;
            static readonly HashSet<Behaviour> _overlappedBehaviours;
            static readonly HashSet<Behaviour> _overlappingBehaviours;
            static readonly List<Behaviour> _selectedBehaviours; // sorted by sortingOrder

            static bool _isQuitting;
            static Vector2 _ptrLastPos;
            static bool _mouseClickHandled;
            static bool _mouseScrollHandled;

            Drawer _drawer;
            bool _selected;
            bool _active;
            bool _destroyed;

            bool _overlapsNow;      // used only in 
            bool _overlappedBefore; // UpdateSelections()
            bool _blockedBefore;

            bool _mouseEntered;
            bool _mouseHovered;
            bool _mouseLeft;

            static Behaviour()
            {
                _aliveBehaviours = new HashSet<Behaviour>(32);
                _overlappedBehaviours = new HashSet<Behaviour>(16);
                _overlappingBehaviours = new HashSet<Behaviour>(16);
                _selectedBehaviours = new List<Behaviour>(8);
            }
            public static void Initialize()
            {
                Global.OnUpdate += OnUpdate;
                Application.wantsToQuit += OnWantToQuit;
            }

            public int CompareTo(Behaviour other)
            {
                return -_drawer._sortingOrder.CompareTo(other._drawer._sortingOrder);
            }
            public void Init(Drawer drawer)
            {
                _drawer = drawer;
                _aliveBehaviours.Add(this);
            }

            static void OnUpdate()
            {
                if (_isQuitting) return;
                _mouseClickHandled = false;
                _mouseScrollHandled = false;

                Vector2 pos = Pointer.Position;
                DrawerMouseEventArgs args = new(pos, pos - _ptrLastPos, Input.GetMouseButtonDown(0), Input.GetMouseButtonDown(1), Input.mouseScrollDelta.y);
                UpdateSelections(args);
            }
            static bool OnWantToQuit()
            {
                _isQuitting = true;
                return true;
            }

            static void UpdateSelections(DrawerMouseEventArgs e)
            {
                int blockingSorting = int.MinValue;

                _overlappedBehaviours.Clear();
                foreach (Behaviour b in _overlappingBehaviours)
                    _overlappedBehaviours.Add(b);
                _overlappingBehaviours.Clear();

                foreach (Behaviour b in _aliveBehaviours)
                {
                    Drawer drawer = b._drawer;
                    if (drawer._collider == null) continue;
                    if (b._destroyed) continue;

                    bool colliderEnabled = b._active && drawer.ColliderEnabled;
                    if (!colliderEnabled && !b._overlappedBefore) continue;

                    bool overlapsNow = colliderEnabled && drawer._collider.OverlapPoint(e.position);
                    bool overlappedBefore = _overlappedBehaviours.Contains(b);

                    b._overlapsNow = overlapsNow;
                    b._overlappedBefore = overlappedBefore;
                    if (!overlapsNow)
                    {
                        if (!b._selected) continue;
                        b.TryRemoveFromSelected();
                        b.TryHandleMouseEvents(e);
                        continue;
                    }

                    _overlappingBehaviours.Add(b);
                    if (!drawer._blocksSelection) continue;
                    int sorting = drawer._sortingOrder;
                    if (blockingSorting < sorting)
                        blockingSorting = sorting;
                }

                foreach (Behaviour b in _overlappingBehaviours)
                {
                    bool blocked = b._drawer._sortingOrder < blockingSorting;
                    bool overlapsNow = b._overlapsNow && !blocked;
                    bool overlappedBefore = b._overlappedBefore && !b._blockedBefore;

                    bool mEntered = overlapsNow && !overlappedBefore;
                    bool mHovered = overlapsNow && overlapsNow == overlappedBefore;
                    bool mLeft = !overlapsNow && overlappedBefore;

                    b._overlapsNow = overlapsNow;
                    b._overlappedBefore = overlappedBefore;
                    b._blockedBefore = blocked;
                    b._mouseEntered = mEntered;
                    b._mouseHovered = mHovered;
                    b._mouseLeft = mLeft;

                    if (mEntered)
                        b.TryAddToSelected();
                    else if (mLeft)
                    {
                        b.TryRemoveFromSelected();
                        b.TryHandleMouseEvents(e);
                    }
                }

                _selectedBehaviours.Sort(BehaviourComparer);
                for (int i = 0; i < _selectedBehaviours.Count; i++)
                {
                    Behaviour b = _selectedBehaviours[i];
                    b.TryHandleMouseEvents(e);
                    if (!b._selected) i--; // has been removed from the list
                }
                _ptrLastPos = e.position;
            }
            static int BehaviourComparer(Behaviour x, Behaviour y) => x.CompareTo(y);

            void TryAddToSelected()
            {
                if (!_drawer.CanBeSelected()) return;
                _selected = true;
                _selectedBehaviours.Add(this);
                // events handled in foreach loop (for _selectedBehaviours)
            }
            void TryRemoveFromSelected()
            {
                if (_selected)
                {
                    _mouseEntered = false;
                    _mouseHovered = false;
                    _mouseLeft = true;
                }
                _selected = false;
                _selectedBehaviours.Remove(this);
            }
            void TryHandleMouseEvents(DrawerMouseEventArgs e)
            {
                if (_drawer == null || _destroyed || _drawer._isDestroyed) return;
                if (_selected && !_drawer._collider.OverlapPoint(e.position)) // will sometimes occur (why?)
                    TryRemoveFromSelected();

                if (_mouseEntered)
                    _drawer.OnMouseEnter.Invoke(_drawer, e);
                if (_mouseHovered)
                    _drawer.OnMouseHover.Invoke(_drawer, e);
                if (_mouseLeft)
                    _drawer.OnMouseLeave.Invoke(_drawer, e);

                if (!_selected || !_drawer.ColliderEnabled) return;
                if (!_mouseClickHandled && e.isAnyDown)
                {
                    e.handled = false;
                    _drawer.OnMouseClick.Invoke(_drawer, e);
                    _mouseClickHandled |= e.handled;
                    _mouseClickHandled |= _drawer.HandleMouseEventsAfterClick();
                }
                if (!_mouseScrollHandled && e.scrollDeltaY != 0)
                {
                    e.handled = false;
                    _drawer.OnMouseScroll.Invoke(_drawer, e);
                    _mouseScrollHandled |= e.handled;
                }
            }

            // can be used to force mouse leave & mouse enter invoke
            void Reselect()
            {
                _overlappingBehaviours.Remove(this);
                TryRemoveFromSelected();
            }
            void Deselect()
            {
                if (!_selected) return;
                Vector2 pos = Pointer.Position;
                DrawerMouseEventArgs args = new(pos, Vector2.zero, Input.GetMouseButtonDown(0), Input.GetMouseButtonDown(1), Input.mouseScrollDelta.y);
                _drawer.OnMouseLeave.Invoke(_drawer, args);
                TryRemoveFromSelected();
            }

            void Start()
            {
                OnEnable();
            }
            void OnEnable()
            {
                if (_isQuitting) return;
                _active = true;
                _drawer?.OnEnable?.Invoke(_drawer, EventArgs.Empty);
            }
            void OnDisable()
            {
                if (_isQuitting) return;
                _active = false;
                _drawer?.OnDisable?.Invoke(_drawer, EventArgs.Empty);
            }
            void OnDestroy()
            {
                if (_isQuitting) return;

                _destroyed = true;
                _aliveBehaviours.Remove(this);
                _selectedBehaviours.Remove(this);

                _drawer.TryDestroyInstantly();
                _drawer = null;
            }
        }
        static Drawer()
        {
            Behaviour.Initialize();
        }

        // if attached set to null, it will be set to gameObject
        public Drawer(object attached, GameObject worldObject) : this(attached, worldObject.transform) { }
        public Drawer(object attached, Component worldComponent) : this(attached, worldComponent.transform) { }
        public Drawer(object attached, GameObject prefab, Transform parent) : this(attached, GameObject.Instantiate(prefab, parent != null ? parent : Global.Root)) { }
        public Drawer(object attached, Transform worldTransform)
        {
            transform = worldTransform;
            gameObject = transform.gameObject;
            this.attached = attached ?? gameObject;

            _tooltipAlign = HorizontalAlignmentOptions.Center;
            _collider = transform.GetComponent<BoxCollider2D>();
            _colliderSize = _collider == null ? default : _collider.size;
            _color = Color.white;

            _changePointer = false;
            _blocksSelection = true;
            _colliderEnabled = _collider == null || _collider.enabled;

            OnMouseScroll += OnMouseScrollBase;
            OnMouseEnter += OnMouseEnterBase;
            OnMouseHover += OnMouseHoverBase;
            OnMouseLeave += OnMouseLeaveBase;
            OnMouseClick += OnMouseClickBase;

            OnEnable += OnEnableBase;
            OnDisable += OnDisableBase;
            OnDestroy += OnDestroyBase;

            _behaviour = gameObject.AddComponent<Behaviour>();
            _behaviour.Init(this);
        }

        public void Dispose() => TryDestroyInstantly();
        public bool Equals(Drawer other)
        {
            return _behaviour.Equals(other._behaviour);
        }

        public void TryDestroy(bool instantly)
        {
            if (instantly)
                 TryDestroyInstantly();
            else TryDestroyAnimated();
        }
        public void TryDestroyInstantly()
        {
            if (_isDestroyed) return;
            _isDestroying = true;
            _isDestroyed = true;
            DestroyInstantly();
        }
        public void TryDestroyAnimated()
        {
            if (_isDestroying) return;
            _isDestroying = true;
            DestroyAnimated();
        }

        public void SetTooltipAlign(HorizontalAlignmentOptions align)
        {
            _tooltipAlign = align;
        }
        public void SetTooltip(string str)
        {
            _tooltipFunc = () => str;
        }
        public void SetTooltip(Func<string> func)
        {
            _tooltipFunc = func;
        }

        public void SetSortingAsTop()
        {
            SetSortingOrder(512);
        }
        public void SetSortingAsDefault()
        {
            SetSortingOrder(_sortingDefault);
        }

        protected virtual void SetColliderSize(Vector2 size)
        {
            _colliderSize = size;
            if (_collider != null)
                _collider.size = size;
        }
        protected virtual void SetCollider(bool value)
        {
            _colliderEnabled = value;
            if (_collider != null)
                _collider.enabled = value;
        }
        protected virtual void SetSortingOrder(int value)
        {
            _sortingOrder = value;
        }
        protected virtual void SetColor(Color value)
        {
            // ignores components' alpha levels
            _color = value;
            if (SetActiveStateOnAlphaSet())
                gameObject.SetActive(value.a != 0);
        }

        protected virtual void DestroyInstantly()
        {
            OnDestroy?.Invoke(this, EventArgs.Empty);
            gameObject.Destroy();

            OnMouseEnter = null;
            OnMouseLeave = null;
            OnMouseClick = null;
            OnMouseScroll = null;
            OnDestroy = null;
        }
        protected virtual UniTask DestroyAnimated()
        {
            SetCollider(false);
            Tweener tween = this.DOColor(Color.clear, 0.5f).OnComplete(DestroyInstantly);
            return tween.AsyncWaitForCompletion();
        }

        protected virtual bool SetActiveStateOnAlphaSet() => true;
        protected virtual bool HandleMouseEventsAfterClick() => _blocksSelection;
        protected virtual bool CanBeSelected() => true;

        protected virtual void OnMouseScrollBase(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnMouseEnterBase(object sender, DrawerMouseEventArgs e) 
        {
            if (e.handled) return;
            if (_tooltipFunc == null) return;
            Tooltip.ShowAligned(_tooltipAlign, _tooltipFunc());
        }
        protected virtual void OnMouseHoverBase(object sender, DrawerMouseEventArgs e) { }
        protected virtual void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e) 
        {
            if (e.handled) return;
            if (_tooltipFunc != null)
                Tooltip.Hide();
        }
        protected virtual void OnMouseClickBase(object sender, DrawerMouseEventArgs e) { }

        protected virtual void OnEnableBase(object sender, EventArgs e) { }
        protected virtual void OnDisableBase(object sender, EventArgs e) { }
        protected virtual void OnDestroyBase(object sender, EventArgs e) { }

        private void SetSortingOrderDefault(int value)
        {
            SetSortingOrder(value);
            _sortingDefault = value;
        }
        private void SetAlpha(float value)
        {
            // keeps components' colors
            SetColor(_color.WithAlpha(value));
        }
    }
}
