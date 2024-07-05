using UnityEngine;

namespace Game
{
    /// <summary>
    /// Класс, представляющий пустой отрисовщик. Он всё ещё может быть привязан к переменной, создан и уничтожен.
    /// </summary>
    public class DrawerShell : Drawer
    {
        public DrawerShell(object attached, GameObject worldObject) : base(attached, worldObject) { }
        public DrawerShell(object attached, Transform worldTransform) : base(attached, worldTransform) { }
    }
}
