namespace Game
{
    /// <summary>
    /// Интерфейс, реализующий объект как источник изменения какого-либо аспекта (переменной) объекта на столе (см. <see cref="TableEntry"/>).
    /// </summary>
    public interface ITableEntrySource : IUnique, ITableFindable 
    { 
        public string TableName { get; } // used in logs
    }

    /*  NOTE: 'pre' events shouldn't apply if source == null (as it's an internal action)

        Most common entry sources:

        1. ITableTrait
            > BattlePassiveTrait
            > BattleActiveTrait
            > TablePassiveTrait
            > TableActiveTrait

        2. ITableCard
            > BattleFieldCard
            > BattleFloatCard
            > TableFieldCard
            > TableFloatCard

        3. BattleSide (player/enemy)
        4. PlaceMenu (mostly usable ones) 

        Try to handle all common source in events in switch expression with pattern-matching
        Also you can use properties Trait.isPassive and Card.isField
    */
}
