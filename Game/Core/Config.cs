using UnityEngine;

namespace Game
{
    /// <summary>
    /// Статический класс, содержащий данные конфигурации игры.
    /// </summary>
    public static class Config
    {
        public static int frameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        public static bool writeConsoleLogs = true;
        public static bool writeAllAiResultsLogs = true; // false
        public static bool shufflePrice = false;
    }
}
