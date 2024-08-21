using Game.Cards;
using UnityEngine;

namespace Game.Sleeves
{
    public class TableSleeveCardComponent : MonoBehaviour
    {
        public bool Enabled => _enabled;
        TableCardDrawer _drawer;
        bool _enabled;

        public void Attach(TableCardDrawer drawer)
        {
            if (drawer.attached is not ITableSleeveCard sCard)
                throw new System.InvalidCastException($"drawer card must be type of {nameof(ITableSleeveCard)} instance.");

            _drawer = drawer;
            Enable();
        }
        public void Detatch()
        {
            Disable();
            _drawer = null;
        }

        public void Enable()
        {
            if (_enabled) return;
            _enabled = true;

            _drawer.OnMouseEnter += OnDrawerMouseEnter;
            _drawer.OnMouseLeave += OnDrawerMouseLeave;
            _drawer.OnMouseClick += OnDrawerMouseClick;
        }
        public void Disable() 
        {
            if (!_enabled) return;
            _enabled = false;

            _drawer.OnMouseEnter -= OnDrawerMouseEnter;
            _drawer.OnMouseLeave -= OnDrawerMouseLeave;
            _drawer.OnMouseClick -= OnDrawerMouseClick;
        }

        void OnDrawerMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            TableCardDrawer drawer = (TableCardDrawer)sender;
            ITableSleeveCard drawerCard = (ITableSleeveCard)drawer.attached;
            drawerCard.TryPullOut(false);
        }
        void OnDrawerMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            TableCardDrawer drawer = (TableCardDrawer)sender;
            ITableSleeveCard drawerCard = (ITableSleeveCard)drawer.attached;
            drawerCard.TryPullIn(false);
        }
        void OnDrawerMouseClick(object sender, DrawerMouseEventArgs e)
        {
            TableCardDrawer drawer = (TableCardDrawer)sender;
            ITableSleeveCard drawerCard = (ITableSleeveCard)drawer.attached;
            ITableSleeveCard.TryTakeCard(drawerCard);
        }
    }
}
