using System;
using UnityEngine;

namespace Game.Environment
{
    public sealed class TableLocationMission : ITableDrawable
    {
        public const int HEIGHT = 64;
        public const int WIDTH = 457;

        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        public LocationMission Data => _data;
        public TableLocationMissionDrawer Drawer => _drawer;

        readonly LocationMission _data;
        TableLocationMissionDrawer _drawer;
        Drawer ITableDrawable.Drawer => _drawer;

        public TableLocationMission(LocationMission data, Transform parent)
        {
            _data = data;
            CreateDrawer(parent);
        }
        public void TryStartTravel()
        {
            Traveler.TryStartTravel(_data);
        }

        public void CreateDrawer(Transform parent)
        {
            _drawer = new TableLocationMissionDrawer(this, parent);
        }
        public void DestroyDrawer(bool instantly)
        {
            _drawer.TryDestroy(instantly);
            _drawer = null;
        }
    }
}
