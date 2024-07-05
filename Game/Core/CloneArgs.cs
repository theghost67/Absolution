using System;

namespace Game
{
    /// <summary>
    /// Базовый класс для клонирования объекта, используя параметр (см. <see cref="ICloneableWithArgs"/>).<br/>
    /// Примечание: объекты, реализующие <see cref="ITableDrawable"/> изначально будут клонироваться без отрисовщиков.
    /// </summary>
    public class CloneArgs 
    { 
        public static readonly CloneArgs Empty = new();
        Action _onCloned; // invokes only in the most derived constructor (should be the last instruction)

        public CloneArgs() { }

        // USE ONLY IF CLASS IS NOT DERIVED FROM TableObject (see it's implementation)
        public void AddOnClonedAction(Type srcType, Type instanceType, Action action)
        {
            _onCloned += action;
            if (srcType == instanceType)
            {
                _onCloned?.Invoke();
                _onCloned = null;
            }
        }
        public void TryOnClonedAction(Type srcType, Type instanceType)
        {
            if (srcType == instanceType)
            {
                _onCloned?.Invoke();
                _onCloned = null;
            }
        }
    }
}
