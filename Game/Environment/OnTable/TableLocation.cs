using System;
using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий локацию на столе (см. <see cref="Location"/>).
    /// </summary>
    public sealed class TableLocation : TableObject
    {
        [Obsolete("Move to Location with RefreshMissions()")] public LocationMission[] Missions => _missions;
        public bool IsUnlocked => Player.LocationLevel >= _data.level - 1;

        public Location Data => _data;
        public new TableLocationDrawer Drawer => ((TableObject)this).Drawer as TableLocationDrawer;

        readonly Location _data;
        LocationMission[] _missions;

        public TableLocation(Location data, Transform parent)
        {
            _data = data;
            RefreshMissions();
            Traveler.OnTravelEnd += RefreshMissions;
            TryOnInstantiatedAction(GetType(), typeof(TableLocation));
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

        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableLocationDrawer(this, parent);
        }
    }
}
