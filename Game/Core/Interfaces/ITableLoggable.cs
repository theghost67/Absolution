namespace Game
{
    /// <summary>
    /// Интерфейс, реализующий объект как объект с возможностью получения его имени для точной идентификации и последующей записи в логи.
    /// </summary>
    public interface ITableLoggable
    {
        public string TableName { get; }      // used in IN-GAME logs
        public string TableNameDebug { get; } // used in DEBUG logs
    }
}
