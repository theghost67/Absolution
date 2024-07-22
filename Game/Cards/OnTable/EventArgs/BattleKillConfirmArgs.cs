using System;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий параметр для событий, вызываемых при подтверждении убийства одной картой поля во время сражения другой.
    /// </summary>
    public class BattleKillConfirmArgs : EventArgs
    {
        public readonly BattleFieldCard victim;
        public readonly BattleKillAttemptArgs attempt;
        public BattleKillConfirmArgs(BattleFieldCard victim, BattleKillAttemptArgs args)
        {
            this.victim = victim;
            this.attempt = args;
        }
    }
}
