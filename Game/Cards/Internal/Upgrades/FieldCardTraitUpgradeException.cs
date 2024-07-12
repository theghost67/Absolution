using System;

namespace Game.Cards
{
	/// <summary>
	/// Класс, представляющий исключение во время улучшения одного из навыков карты (см. <see cref="FieldCardUpgradeRules"/>).
	/// </summary>
    public class FieldCardTraitUpgradeException : Exception
	{
		public FieldCardTraitUpgradeException(string traitId, float upgradePoints, float statsPointsShare, float traitsPointsShare)
			: base($"Trait \'{traitId}\' caused thread block during card upgrade.\n" +
				   $"upgradePoints: {upgradePoints}, statsPointsShare: {statsPointsShare}, traitsPointsShare: {traitsPointsShare}") { }
	}
}
