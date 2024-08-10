﻿using MyBox;
using UnityEngine;

namespace Game.Environment
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableLocationPlace"/>.
    /// </summary>
    public sealed class TableLocationPlaceDrawer : Drawer
    {
        static readonly GameObject _prefab;
        public readonly new TableLocationPlace attached;
        readonly SpriteRenderer _spriteRenderer;

        static TableLocationPlaceDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Place icon");
        }
        public TableLocationPlaceDrawer(TableLocationPlace place, Transform parent) : base(place, _prefab, parent)
        {
            attached = place;

            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = Resources.Load<Sprite>(place.Data.iconPath);

            ChangePointer = true;
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            TableLocationPlaceDrawer drawer = (TableLocationPlaceDrawer)sender;
            drawer.transform.StartShake(-1, 1f);
            //Tooltip.Show(drawer._spriteRenderer, drawer.attached.Data.name);
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            TableLocationPlaceDrawer drawer = (TableLocationPlaceDrawer)sender;
            drawer.transform.StopShake();
            //Tooltip.Hide();
        }
        protected override void OnMouseClickBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseClickBase(sender, e);
            if (!e.isLmbDown) return;
            TableLocationPlaceDrawer drawer = (TableLocationPlaceDrawer)sender;
            drawer.attached.Data.menuCreator().TransitToThis();
        }
    }
}
