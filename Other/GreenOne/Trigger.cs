using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GreenOne
{
    /// <summary>
    /// Используется для упрощения работы с <see cref="EventTrigger"/>.
    /// </summary>
    public class Trigger
    {
        #region Variables
        public readonly EventTriggerType type;
        public readonly UnityAction<BaseEventData> action;
        #endregion

        #region Конструкторы
        public Trigger(EventTriggerType type, UnityAction<BaseEventData> action)
        {
            this.type = type;
            this.action = action;
        }
        #endregion
    }
}