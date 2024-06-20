using Cysharp.Threading.Tasks;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Game.Menus
{
    // TODO[IMPORTANT]: think out animation for each menu transit (implement in Open/Close() methods)
    /// <summary>
    /// Абстрактный класс, представляющий какой-либо графический интерфейс для пользователя как меню.
    /// </summary>
    public abstract class Menu : IMenu
    {
        public static string Log => _logString;

        public event Action OnOpened;
        public event Action OnClosed;

        public GameObject GameObject => _gameObject;
        public Transform Transform => _transform;
        public string Id => _id;

        public bool IsDestroyed => _isDestroyed;
        public bool IsOpened => _isOpened;
        public bool ColliderEnabled => _colliderEnabled;
        public int SortingOrder => _sortingOrder;
        public int OpenDepth => _openDepth; 
        public int FullDepth => _fullDepth;

        static readonly Transform _parent = Global.Root.Find("MENUS").transform;
        static readonly List<Menu> _openList = new();
        static readonly List<Menu> _fullList = new();

        static string _logString;
        static StringBuilder _logBuilder = new();

        readonly string _id;
        readonly GameObject _gameObject;
        readonly Transform _transform;

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

            _openDepth = -1;
            _fullDepth = _fullList.Count;
            _fullList.Add(this);
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
            if (depth < 0 || depth > _openList.Count)
                return null;

            Menu menu = _openList[depth];
            if (menu == null) return null;
            if (menu._isOpened)
                return menu;
            else return null;
        }
        public static Menu GetOpened(string name)
        {
            for (int i = 0; i < _openList.Count; i++)
            {
                Menu menu = _openList[i];
                if (menu._isOpened && menu._gameObject.name == name)
                    return menu;
            }
            return null;
        }

        public static Menu Get(int depth)
        {
            if (depth < 0 || depth > _openList.Count)
                return null;
            else return _openList[depth];
        }
        public static Menu Get(string name)
        {
            for (int i = 0; i < _openList.Count; i++)
            {
                if (_openList[i]._gameObject.name == name)
                    return _openList[i];
            }
            return null;
        }

        public static void WriteLogToCurrent(string text)
        {
            GetCurrent()?.WriteLog(text);
        }
        public static void WriteDescToCurrent(string text)
        {
            GetCurrent()?.WriteDesc(text);
        }

        public Menu GetPrevious()
        {
            if (_fullDepth > 0)
                return _fullList[_fullDepth - 1];
            else return null;
        }
        public Menu GetPreviousOpened()
        {
            if (_openDepth > 0)
                return _openList[_openDepth - 1];
            else return null;
        }

        public Menu GetNext()
        {
            if (_fullDepth + 1 < _fullList.Count)
                return _fullList[_fullDepth + 1];
            else return null;
        }
        public Menu GetNextOpened()
        {
            if (_openDepth + 1 < _openList.Count)
                return _openList[_openDepth + 1];
            else return null;
        }

        public async UniTask DestroyAnimated()
        {
            await CloseAnimated();
            DestroyInstantly();
        }
        public async UniTask ReturnAnimated()
        {
            if (_openDepth == 0) return;
            await CloseAnimated();

            Menu prev = GetPrevious();
            if (prev != null)
                await prev.OpenAnimated();
        }

        public virtual UniTask OpenAnimated()
        {
            OpenInstantly();
            return UniTask.CompletedTask;
        }
        public virtual UniTask CloseAnimated()
        {
            CloseInstantly();
            return UniTask.CompletedTask;
        }

        public virtual void OpenInstantly()
        {
            if (_isOpened) return;
            _isOpened = true;

            _openDepth = _openList.Count;
            _openList.Add(this);
            _gameObject.SetActive(true);

            SetSortingOrder(_openDepth * 32);
            OnOpened?.Invoke();
        }
        public virtual void CloseInstantly()
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
        }
        public virtual void DestroyInstantly()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            if (_fullList.Remove(this))
            {
                for (int i = _fullDepth + 1; i < _fullList.Count; i++)
                    _fullList[i]._fullDepth--;
            }

            CloseInstantly();
            _gameObject.Destroy();
            _fullDepth = -1;
        }

        public virtual void WriteLog(string text)
        {
            _logBuilder.AppendLine(text);
            _logString = _logBuilder.ToString();
        }
        public virtual void WriteDesc(string text) { }

        public virtual void SetColliders(bool value) => _colliderEnabled = value;
        public virtual void SetSortingOrder(int value) => _sortingOrder = value;
    }
}
