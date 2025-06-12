using Cysharp.Threading.Tasks;
using Game.Effects;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Game.Menus
{
    // NOTE: do NOT open multiple menus if they have the same type or derived from the same type (not Menu)
    // example: BattlePlaceMenu and BossPlaceMenu

    /// <summary>
    /// Абстрактный класс, представляющий какой-либо графический интерфейс для пользователя как меню.
    /// </summary>
    public abstract class Menu : Unique, IMenu
    {
        public static string Log => _logString;
        public static event Action OnAnyOpened;
        public static event Action OnAnyClosed;

        public event Action OnOpened;
        public event Action OnClosed;

        public string Id => _id;
        public GameObject GameObject => _gameObject;
        public Transform Transform => _transform;
        public TableFinder Finder => _finder;
        public Func<Menu> MenuWhenClosed { get; set; } // invokes on TransitFromThis (if not overriden)

        public bool IsDestroyed => _isDestroyed;
        public bool IsOpened => _isOpened;
        public bool ColliderEnabled
        {
            get => _colliderEnabled;
            set { if (!_isDestroyed && _colliderEnabled != value) SetCollider(value); }
        }
        public int SortingOrder
        {
            get => _sortingOrder;
            set { if (!_isDestroyed && _sortingOrder != value) SetSortingOrder(value); }
        }
        public int OpenDepth => _openDepth;
        public int FullDepth => _fullDepth;
        public virtual string LinkedMusicMixId => null; // see AudioBrowser

        public string TableName => "Меню";
        public string TableNameDebug => _id;

        static readonly Transform _parent = Global.Root.Find("MENUS").transform;
        static readonly List<Menu> _openList = new();
        static readonly List<Menu> _fullList = new();

        static string _logString;
        static StringBuilder _logBuilder = new();

        readonly string _id;
        readonly GameObject _gameObject;
        readonly Transform _transform;
        readonly MenuFinder _finder;

        bool _isDestroyed;
        bool _isOpened;
        bool _colliderEnabled;
        int _sortingOrder;
        int _openDepth;
        int _fullDepth;

        public Menu(string id, GameObject prefab)
        {
            _id = id;
            _gameObject = GameObject.Instantiate(prefab, _parent);
            _gameObject.name = $"Menu ({id})";
            _gameObject.SetActive(false);
            _transform = _gameObject.transform;
            _finder = new(this);

            _openDepth = -1;
            _fullDepth = _fullList.Count;
            _fullList.Add(this);

            MenuWhenClosed = () => GetCurrent().GetPrevious();
        }

        public static Menu GetCurrent()
        {
            return _openList.LastOrDefault();
        }
        public static Menu[] GetAll()
        {
            return _fullList.ToArray();
        }
        public static Menu[] GetAllOpened()
        {
            return _openList.ToArray();
        }

        public static Menu GetOpened(int depth)
        {
            if (depth < 0 || depth >= _openList.Count)
                return null;

            Menu menu = _openList[depth];
            if (menu == null) return null;
            if (menu._isOpened)
                return menu;
            else return null;
        }
        public static Menu GetOpened(string id)
        {
            for (int i = 0; i < _openList.Count; i++)
            {
                Menu menu = _openList[i];
                if (menu._isOpened && menu._id == id)
                    return menu;
            }
            return null;
        }

        public static Menu GetAny(int depth)
        {
            if (depth < 0 || depth >= _fullList.Count)
                return null;

            Menu menu = _fullList[depth];
            if (menu == null) return null;
            return menu;
        }
        public static Menu GetAny(string id)
        {
            for (int i = 0; i < _fullList.Count; i++)
            {
                Menu menu = _fullList[i];
                if (menu._id == id)
                    return menu;
            }
            return null;
        }

        public Menu GetPrevious()
        {
            if (this == null) return null;
            if (_fullDepth > 0)
                return _fullList[_fullDepth - 1];
            else return null;
        }
        public Menu GetPreviousOpened()
        {
            if (this == null) return null;
            if (_openDepth > 0)
                return _openList[_openDepth - 1];
            else return null;
        }

        public Menu GetNext()
        {
            if (this == null) return null;
            if (_fullDepth + 1 < _fullList.Count)
                return _fullList[_fullDepth + 1];
            else return null;
        }
        public Menu GetNextOpened()
        {
            if (this == null) return null;
            if (_openDepth + 1 < _openList.Count)
                return _openList[_openDepth + 1];
            else return null;
        }

        public UniTask TransitToThis()
        {
            return MenuTransit.Between(GetCurrent(), this);
        }
        public UniTask TransitFromThis()
        {
            return MenuTransit.Between(this, MenuWhenClosed() ?? GetCurrent().GetPrevious());
        }

        public void TryOpen()
        {
            if (_isDestroyed) return;
            if (!_isOpened)
                Open();
        }
        public void TryClose()
        {
            if (_isDestroyed) return;
            if (_isOpened)
                Close();
        }
        public void TryDestroy()
        {
            if (_isDestroyed) return;
            Destroy();
        }

        public virtual void OnTransitStart(bool isFromThis)
        {
            if (!isFromThis)
                SFX.PlayMusicMix(LinkedMusicMixId);
            else ColliderEnabled = false;
        }
        public virtual void OnTransitMiddle(bool isFromThis)
        {
            if (!isFromThis)
                ColliderEnabled = false;
        }
        public virtual void OnTransitEnd(bool isFromThis)
        {
            if (!isFromThis)
                ColliderEnabled = true;
        }
        public virtual void Refresh() { } // use Translator class

        protected virtual void Open()
        {
            _isOpened = true;
            _openDepth = _openList.Count;
            _openList.Add(this);
            _gameObject.SetActive(true);

            SetSortingOrder(_openDepth * 32);
            OnOpened?.Invoke();
            OnAnyOpened?.Invoke();
        }
        protected virtual void Close()
        {
            if (!_isOpened) return;
            _isOpened = false;

            if (_openList.Remove(this))
            {
                for (int i = _openDepth + 1; i < _openList.Count; i++)
                    _openList[i]._openDepth--;
            }

            _openDepth = -1;
            _gameObject.SetActive(false);
            OnClosed?.Invoke();
            OnAnyClosed?.Invoke();
        }
        protected virtual void Destroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            if (_fullList.Remove(this))
            {
                for (int i = _fullDepth + 1; i < _fullList.Count; i++)
                    _fullList[i]._fullDepth--;
            }

            Close();
            _gameObject.Destroy();
            _fullDepth = -1;
        }

        protected virtual void SetCollider(bool value) => _colliderEnabled = value;
        protected virtual void SetSortingOrder(int value) => _sortingOrder = value;
    }
}
