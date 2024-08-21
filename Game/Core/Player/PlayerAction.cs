using System;

namespace Game
{
    /// <summary>
    /// Класс, представляющий одно из запрашиваемых игроком действий. Зачастую, действия являются асинхронными.
    /// </summary>
    public class PlayerAction
    {
        public Func<bool> conditionFunc;
        public Action successFunc;
        public Action failFunc;
        public Action abortFunc; // invoked when action is canceled because of a fail of an PlayerQueue element
        public int msDelay;

        public PlayerAction() { }
    }
}
