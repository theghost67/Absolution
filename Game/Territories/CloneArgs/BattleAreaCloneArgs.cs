﻿using Game.Cards;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования области действия.
    /// </summary>
    public class BattleAreaCloneArgs : CloneArgs
    {
        public readonly IBattleFighter srcAreaObserverClone;
        public readonly BattleFieldCard srcAreaObservingPointClone;
        public readonly BattleTerritoryCloneArgs terrCArgs;

        public BattleAreaCloneArgs(IBattleFighter srcAreaObserverClone, BattleFieldCard srcAreaObservingPointClone, BattleTerritoryCloneArgs terrCArgs)
        {
            this.srcAreaObserverClone = srcAreaObserverClone;
            this.srcAreaObservingPointClone = srcAreaObservingPointClone;
            this.terrCArgs = terrCArgs;
        }
    }
}
