using UnityEngine;

namespace Game.Environment
{
    public sealed class TableLocationMission : TableObject
    {
        public const int HEIGHT = 64;
        public const int WIDTH = 457;
        public LocationMission Data => _data;
        public new TableLocationMissionDrawer Drawer => ((TableObject)this).Drawer as TableLocationMissionDrawer;

        readonly LocationMission _data;

        public TableLocationMission(LocationMission data, Transform parent)
        {
            _data = data;
            TryOnInstantiatedAction(GetType(), typeof(TableLocationMission));
        }
        public void TryStartTravel()
        {
            Traveler.TryStartTravel(_data);
        }
        protected override Drawer DrawerCreator(Transform parent)
        {
            return new TableLocationMissionDrawer(this, parent);
        }
    }
}
