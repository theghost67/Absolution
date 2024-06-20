using System;
using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий локацию на столе (см. <see cref="Location"/>).
    /// </summary>
    public sealed class TableLocation : ITableDrawable
    {
        public event EventHandler OnDrawerCreated;
        public event EventHandler OnDrawerDestroyed;

        [Obsolete("Move to Location with RefreshMissions()")] public LocationMission[] Missions => _missions;
        public bool IsUnlocked => Player.LocationLevel >= _data.level - 1;

        public Location Data => _data;
        public TableLocationDrawer Drawer => _drawer;

        readonly Location _data;
        TableLocationDrawer _drawer;
        LocationMission[] _missions;

        Drawer ITableDrawable.Drawer => _drawer;

        public TableLocation(Location data, Transform parent)
        {
            _data = data;
            CreateDrawer(parent);
            RefreshMissions();
            Traveler.OnTravelEnd += RefreshMissions;
        }
        void RefreshMissions()
        {
            _missions = new LocationMission[5]
            {
                new(_data, threatLvl: 1),
                new(_data, threatLvl: 2),
                new(_data, threatLvl: 3),
                new(_data, threatLvl: 4),
                new(_data, threatLvl: 5),
            };
        }

        public void CreateDrawer(Transform parent)
        {
            _drawer = new TableLocationDrawer(this, parent);
        }
        public void DestroyDrawer(bool instantly)
        {
            _drawer?.TryDestroy(instantly);
        }
    }
}
