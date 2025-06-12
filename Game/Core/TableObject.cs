using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Абстрактный класс, представляющий объект стола (т.е. объект с отрисовщиком, см. <see cref="ITableObject"/>).
    /// </summary>
    public abstract class TableObject : Unique, ITableObject
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public virtual string TableName => "";
        public virtual string TableNameDebug => GetType().ToString();

        public Drawer Drawer => _drawer;
        public bool IsDisposed => _isDisposed;

        Drawer _drawer;
        Transform _parent;
        bool _isDisposed;

        // invokes only in the most derived constructor
        // call TryOnInstantiatedAction (or AddOnInstantiatedAction) as the last instruction in every derived class
        // pass instance type to 'instanceType', class type to 'srcType'
        Action _onInstantiated;

        // used when creating instance with (no) graphics
        public TableObject(Transform parent) : this()
        {
            _parent = parent;
        }
        public TableObject() : base()
        {
            OnDrawerCreated += OnDrawerCreatedBase;
            OnDrawerDestroyed += OnDrawerDestroyedBase;
        }

        // used when cloning instance
        protected TableObject(TableObject src, Transform parent = null) : base(src.Guid)
        {
            _parent = parent;
            OnDrawerCreated = (EventHandler)src.OnDrawerCreated?.Clone();
            OnDrawerDestroyed = (EventHandler)src.OnDrawerDestroyed?.Clone();
        }

        public virtual void Dispose()
        {
            _drawer?.Dispose();
            _drawer = null;
            _isDisposed = true;
        }
        public override string ToString()
        {
            return $"{TableNameDebug} ({GetType()})";
        }

        public bool CreateDrawer(Transform parent)
        {
            if (_isDisposed)
                throw new NotSupportedException("Impossible to create Drawer of a disposed object.");

            if (parent == null || _drawer != null) 
                return false;

            _drawer = DrawerCreator(parent);
            if (_drawer == null) return false;

            OnDrawerCreated?.Invoke(this, EventArgs.Empty);
            return true;
        }
        public bool DestroyDrawer(bool instantly)
        {
            if (_isDisposed)
                throw new NotSupportedException("Impossible to destroy Drawer of a disposed object.");

            if (_drawer == null)
                return false;

            _drawer.TryDestroy(instantly);
            if (!_drawer.IsDestroying) return false;

            OnDrawerDestroyed?.Invoke(this, EventArgs.Empty);
            _drawer = null;
            return true;
        }

        // gives an ability to call more derived constructor first to set instance variables, and call virtual functions later 
        protected void AddOnInstantiatedAction(Type instanceType, Type srcType, Action action)
        {
            _onInstantiated += action;
            TryOnInstantiatedAction(srcType, instanceType);
        }
        protected void TryOnInstantiatedAction(Type instanceType, Type srcType)
        {
            if (srcType != instanceType) return;
            _onInstantiated?.Invoke();
            _onInstantiated = null;
            AfterInstantiated();
        }

        protected virtual void OnDrawerCreatedBase(object sender, EventArgs e) { }
        protected virtual void OnDrawerDestroyedBase(object sender, EventArgs e) { }
        protected abstract Drawer DrawerCreator(Transform parent);

        void AfterInstantiated()
        {
            CreateDrawer(_parent);
        }
    }
}
