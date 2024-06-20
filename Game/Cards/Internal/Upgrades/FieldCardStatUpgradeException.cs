using System;

namespace Game.Cards
{
	/// <summary>
	/// Класс, представляющий исключение во время улучшения одной из характеристик карты (см. <see cref="FieldCardUpgradeRules"/>).
	/// </summary>
    public class FieldCardStatUpgradeException : Exception
	{
		public FieldCardStatUpgradeException(string statName, int upgradePoints, float statPointsShare, float traitPointsShare)
			: base($"Stat \'{statName}\' caused thread block during card upgrade.\n" +
				   $"upgradePoints: {upgradePoints}, statPointsShare: {statPointsShare}, traitPointsShare: {traitPointsShare}") { }
	}
}
