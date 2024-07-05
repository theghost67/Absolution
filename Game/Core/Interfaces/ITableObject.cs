using Game.Core;
using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Реализует объект как уникальный объект, у которого может быть отрисовщик.
    /// </summary>
    public interface ITableObject : ITableLoggable, IUnique, IDisposable
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;
        public Drawer Drawer { get; }

        public bool CreateDrawer(Transform parent);
        // CloneDrawer(Drawer other); ?
        public bool DestroyDrawer(bool instantly);
    }
}
