using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Реализует объект как объект стола, у которого может быть отрисовщик.
    /// </summary>
    public interface ITableDrawable
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;
        public Drawer Drawer { get; }

        public void CreateDrawer(Transform parent);
        // CloneDrawer(Drawer other); ?
        public void DestroyDrawer(bool instantly);
    }
}
