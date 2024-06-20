using System;

namespace Game.Cards
{
	/// <summary>
	/// Класс, представляющий исключение во время улучшения одного из трейтов карты (см. <see cref="FieldCardUpgradeRules"/>).
	/// </summary>
    public class FieldCardTraitUpgradeException : Exception
	{
		public FieldCardTraitUpgradeException(string traitId, int upgradePoints, float statPointsShare, float traitPointsShare)
			: base($"Trait \'{traitId}\' caused thread block during card upgrade.\n" +
				   $"upgradePoints: {upgradePoints}, statPointsShare: {statPointsShare}, traitPointsShare: {traitPointsShare}") { }
	}
}
