using Game.Territories;
using System;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий параметр для событий, вызываемых при попытке убийства карты поля во время сражения.
    /// </summary>
    public class BattleKillAttemptArgs : EventArgs
    {
        public readonly BattleField field; // ONLY FOR CARDS: field on which the card was placed ('Field' will be null in PostKilledEvent, so use this field instead)
        public readonly int damage;
        public readonly BattleKillMode mode;
        public readonly ITableEntrySource source;
        public bool handled;

        public BattleKillAttemptArgs(IBattleKillable target, int damage, BattleField field, BattleKillMode mode, ITableEntrySource source) 
        {
            this.mode = mode;
            this.damage = damage;
            this.source = source;
            this.field = field;
            this.handled = (!mode.HasFlag(BattleKillMode.IgnoreCanBeKilled) && !target.CanBeKilled) ||
                           (!mode.HasFlag(BattleKillMode.IgnoreHealthRestore) && target.Health > 0);
        }
    }
}
