using Game.Territories;
using System;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий параметр для событий, вызываемых при попытке убийства карты поля во время сражения.
    /// </summary>
    public class BattleKillAttemptArgs : EventArgs
    {
        public readonly ITableEntrySource source;
        public readonly BattleField field; // field on which the card was placed (BattleFieldCard.Field will be null in PostKilledEvent, so use this field instead)
        public BattleKillAttemptArgs(ITableEntrySource source, BattleField field) 
        {
            this.source = source;
            this.field = field;
        }
    }
}
