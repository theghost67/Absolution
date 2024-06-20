﻿using Game.Cards;
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
            _drawer.OnMouseClickLeft += OnDrawerMouseClick;
        }
        public void Disable() 
        {
            if (!_enabled) return;
            _enabled = false;

            _drawer.OnMouseEnter -= OnDrawerMouseEnter;
            _drawer.OnMouseLeave -= OnDrawerMouseLeave;
            _drawer.OnMouseClickLeft -= OnDrawerMouseClick;
        }

        void OnDrawerMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            TableCardDrawer drawer = (TableCardDrawer)sender;
            ITableSleeveCard drawerCard = (ITableSleeveCard)drawer.attached;
            drawerCard.TryPullOut();
        }
        void OnDrawerMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            TableCardDrawer drawer = (TableCardDrawer)sender;
            ITableSleeveCard drawerCard = (ITableSleeveCard)drawer.attached;
            drawerCard.TryPullIn();
        }
        void OnDrawerMouseClick(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            TableCardDrawer drawer = (TableCardDrawer)sender;
            ITableSleeveCard drawerCard = (ITableSleeveCard)drawer.attached;
            PlayerHand.TryTakeCard(drawerCard);
        }
    }
}