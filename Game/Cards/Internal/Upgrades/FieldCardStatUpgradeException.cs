using System;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий исключение во время улучшения одной из характеристик карты (см. <see cref="FieldCardUpgradeRules"/>).
    /// </summary>
    public class FieldCardStatUpgradeException : Exception
	{
		public FieldCardStatUpgradeException(string statName, float upgradePoints, float statsPointsShare, float traitsPointsShare)
			: base($"Stat \'{statName}\' caused thread block during card upgrade.\n" +
				   $"upgradePoints: {upgradePoints}, statsPointsShare: {statsPointsShare}, traitsPointsShare: {traitsPointsShare}") { }
	}
}
