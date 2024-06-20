using System;

namespace Game.Territories
{
    /// <summary>
    /// Класс, представляющий аргументы для клонирования территории стола.
    /// </summary>
    public class TableTerritoryCloneArgs : CloneArgs
    {
        public event Action<TableTerritory> OnTerritoryReady; // can be invoked manually after all clone operations (if territory was not cloned)
        public void OnTerritoryReadyInvoke(TableTerritory territoryClone)
        {
            OnTerritoryReady?.Invoke(territoryClone);
            OnTerritoryReady = null;
        }
    }
}
