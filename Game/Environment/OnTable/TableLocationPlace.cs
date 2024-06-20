using System;
using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий место локации на столе (см. <see cref="LocationPlace"/>).
    /// </summary>
    public sealed class TableLocationPlace : ITableDrawable
    {
        public const int WIDTH = 64; // 32 * scale = 64
        public const int HEIGHT = 64;

        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public LocationPlace Data => _data;
        public TableLocationPlaceDrawer Drawer => _drawer;

        readonly LocationPlace _data;
        TableLocationPlaceDrawer _drawer;
        Drawer ITableDrawable.Drawer => _drawer;

        public TableLocationPlace(LocationPlace data, Transform parent)
        {
            _data = data;
            CreateDrawer(parent);
        }

        public void CreateDrawer(Transform parent)
        {
            _drawer = new TableLocationPlaceDrawer(this, parent);
        }
        public void DestroyDrawer(bool instantly)
        {
            _drawer?.TryDestroy(instantly);
        }
    }
}
