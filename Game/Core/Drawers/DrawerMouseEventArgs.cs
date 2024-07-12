using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Класс, представляющий параметр для обработчиков событий мыши у отрисовщика (см. <see cref="Drawer"/>).
    /// </summary>
    public class DrawerMouseEventArgs : EventArgs
    {
        public readonly Vector2 position;
        public readonly Vector2 delta;
        public readonly bool isLmbDown;
        public readonly bool isRmbDown;
        public readonly bool isAnyDown;
        public readonly float scrollDeltaY;
        public bool handled; // set to true to ignore this type of event on next selected drawers

        public DrawerMouseEventArgs(Vector2 position, Vector2 delta, bool isLmbDown, bool isRmbDown, float scrollDeltaY)
        {
            this.position = position;
            this.delta = delta;
            this.isLmbDown = isLmbDown;
            this.isRmbDown = isRmbDown;
            this.isAnyDown = isLmbDown || isRmbDown;
            this.scrollDeltaY = scrollDeltaY;
        }
    }
}
