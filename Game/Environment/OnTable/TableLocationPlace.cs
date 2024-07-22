using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий место локации на столе (см. <see cref="LocationPlace"/>).
    /// </summary>
    public sealed class TableLocationPlace : TableObject
    {
        public const int WIDTH = 64; // 32 * scale = 64
        public const int HEIGHT = 64;

        public LocationPlace Data => _data;
        public new TableLocationPlaceDrawer Drawer => ((TableObject)this).Drawer as TableLocationPlaceDrawer;

        readonly LocationPlace _data;

        public TableLocationPlace(LocationPlace data, Transform parent) : base(parent)
        {
            _data = data;
            TryOnInstantiatedAction(GetType(), typeof(TableLocationPlace));
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableLocationPlaceDrawer(this, parent);
        }
    }
}
