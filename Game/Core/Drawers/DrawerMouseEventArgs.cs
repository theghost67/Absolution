using UnityEngine;

namespace Game
{
    /// <summary>
    /// Класс, представляющий параметр для обработчиков событий мыши у отрисовщика (см. <see cref="Drawer"/>).
    /// </summary>
    public class DrawerMouseEventArgs
    {
        public readonly Vector2 position;
        public readonly Vector2 delta;
        public bool handled;

        public DrawerMouseEventArgs(Vector2 position, Vector2 delta)
        {
            this.position = position;
            this.delta = delta;
        }
    }
}
