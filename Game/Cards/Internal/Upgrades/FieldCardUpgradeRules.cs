using Game.Traits;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Структура, представляющая правила улучшения (распределения очков) карты поля (см. <see cref="FieldCard"/>).
    /// </summary>
    public struct FieldCardUpgradeRules
    {
        public float points;
        public int traitsCount;
        public IReadOnlyDictionary<string, float> possiblePassivesFreqs;
        public IReadOnlyDictionary<string, float> possibleActivesFreqs;
        static readonly IReadOnlyDictionary<string, float> _empty = new Dictionary<string, float>(capacity: 0);

        public FieldCardUpgradeRules(float points, bool addTraits) : this(points, addTraits ? TraitsCount(points) : 0) { }
        public FieldCardUpgradeRules(float points, int traitsCount) 
        {
            this.points = points;
            this.traitsCount = traitsCount;

            possiblePassivesFreqs = _empty;
            possibleActivesFreqs = _empty;
        }

        public static int TraitsCount(float points)
        {
            return System.Convert.ToInt32(TraitsCountRaw(points));
        }
        public static float TraitsCountRaw(float points)
        {
            if (points < 16)
                return 0;
            else return Mathf.Log((points - 10) / 6);
        }

        public void Upgrade(FieldCard card)
        {
            if (points <= 0)
                return;
            if (points > Card.POINTS_MAX)
                points = Card.POINTS_MAX;

            TableConsole.LogToFile("upsys", $"{card.id}: upgrade started: available points: {points}");

            #region stats/ratios initializing
            int cardTraitsCount = card.traits.Count;
            int allTraitsCount = traitsCount + cardTraitsCount;

            float healthFreq = Random.value;
            float strengthFreq = Random.value * 2;

            float statsPointsShare = points * Random.value;
            float statsPointsShareCurrent = 0;

            float traitsPointsShare = points - statsPointsShare;
            float traitsPointsShareCurrent = 0;

            if (allTraitsCount == 0)
            {
                statsPointsShare = points;
                goto SkipTraitsAll;
            }
            #endregion

            #region possible traits initializing
            // sets possible traits
            Dictionary<string, float> possibleTraits = new(16); // id, frequency
            if (cardTraitsCount >= allTraitsCount)
                goto SkipPossibleTraitsAmplifying;

            foreach (KeyValuePair<string, float> pair in possiblePassivesFreqs)
            {
                string traitId = pair.Key;
                float traitFrequency = pair.Value;
                Trait traitSrc = TraitBrowser.GetPassive(traitId);

                if (traitFrequency != 0) // no need to add trait that has 0 chance
                    possibleTraits.Add(traitId, traitFrequency);
            }
            foreach (KeyValuePair<string, float> pair in possibleActivesFreqs)
            {
                string traitId = pair.Key;
                float traitFrequency = pair.Value;
                Trait traitSrc = TraitBrowser.GetActive(traitId);

                if (traitFrequency != 0) // no need to add trait that has 0 chance
                    possibleTraits.Add(traitId, traitFrequency);
            }
            #endregion

            #region stackable traits creation
            SkipPossibleTraitsAmplifying:
            Dictionary<string, float> cardTraitsStackable = new(capacity: allTraitsCount); // traitId, frequency
            foreach (Trait trait in card.traits.Select(e => e.Trait).Where(t => !t.tags.HasFlag(TraitTag.Static)))
                cardTraitsStackable.Add(trait.id, Random.value);

            for (int i = cardTraitsStackable.Count; i < allTraitsCount; i++)
            {
                if (possibleTraits.Count == 0) break;
                KeyValuePair<string, float> weightedPair = possibleTraits.GetWeightedRandom(pair => pair.Value);
                possibleTraits.Remove(weightedPair.Key);

                string traitId = weightedPair.Key;
                Trait traitSrc = TraitBrowser.GetTrait(traitId);
                float traitFrequency = weightedPair.Value;

                float traitAddPointsDelta = card.PointsDeltaForTrait(traitSrc, 1);
                if (traitAddPointsDelta + traitsPointsShareCurrent < traitsPointsShare)
                {
                    traitsPointsShareCurrent += traitAddPointsDelta;
                    if (traitSrc.isPassive)
                         card.traits.Passives.AdjustStacks(traitId, 1);
                    else card.traits.Actives.AdjustStacks(traitId, 1);
                }
                else
                {
                    possibleTraits.Remove(traitId);
                    continue; // cannot afford another trait add
                }

                if (!traitSrc.tags.HasFlag(TraitTag.Static)) // if is stackable
                    cardTraitsStackable.Add(traitId, Random.value);
            }
            #endregion

            #region stackable traits upgrading           
            float traitsFreqSum = cardTraitsStackable.Values.Sum();
            foreach (KeyValuePair<string, float> pair in cardTraitsStackable)
            {
                string traitId = pair.Key;
                Trait traitSrc = TraitBrowser.GetTrait(traitId);
                float traitPointsShare = traitsPointsShare * pair.Value / traitsFreqSum;
                float traitPointsShareCurrent = 0;
                int traitUpStep = 1000;
                int traitUpIterations = 0;
                int traitStacks = traitSrc.isPassive ? card.traits.Passives[traitId].Stacks : card.traits.Actives[traitId].Stacks;
                while (traitPointsShareCurrent < traitPointsShare)
                {
                    TryAddStacks:
                    float stacksAddPointsDelta = card.PointsDeltaForTrait(traitSrc, traitUpStep);
                    if (stacksAddPointsDelta <= 0) return;
                    if (stacksAddPointsDelta + traitPointsShareCurrent < traitPointsShare)
                    {
                        traitPointsShareCurrent += stacksAddPointsDelta;
                        traitStacks += traitUpStep;
                        if (traitSrc.isPassive)
                            card.traits.Passives.AdjustStacks(traitId, traitUpStep);
                        else card.traits.Actives.AdjustStacks(traitId, traitUpStep);
                    }
                    else if (traitUpStep > 1)
                    {
                        traitUpStep /= 2;
                        goto TryAddStacks;
                    }
                    else break;
                    if (traitUpIterations++ > 1000)
                        throw new FieldCardTraitUpgradeException(traitId, points, statsPointsShare, traitsPointsShare);
                }
                traitsPointsShareCurrent += traitPointsShareCurrent;
                TableConsole.LogToFile("upsys", $"{card.id}: upgrade: trait {traitId}: iterations: {traitUpIterations}, value: {traitStacks}, share_max: {traitPointsShare}, share_cur: {traitPointsShareCurrent}");
            }
            statsPointsShare -= traitsPointsShareCurrent - traitsPointsShare; // adjusts statPointsShare using traitPointsShare not spent points
            #endregion

            #region stats upgrading
            SkipTraitsAll: // upgrades stats using stat frequencies
            bool isHealthStat = true;
            float healthSparePoints = 0;
            float[] statsFrequencies = new float[] { healthFreq, strengthFreq };
            float statsFreqSum = statsFrequencies.Sum();
            foreach (float freq in statsFrequencies)
            {
                float statPointsShare = statsPointsShare * freq / statsFreqSum;
                float statPointsShareCurrent = 0;
                int statUpStep = 1000;
                int statUpIterations = 0;
                int statValue = isHealthStat ? card.health : card.strength;
                if (!isHealthStat) statPointsShare += healthSparePoints;
                while (statPointsShareCurrent < statPointsShare)
                {
                    TryAddStat:
                    float statAddPointsDelta = isHealthStat ? card.PointsDeltaForHealth(statUpStep) : card.PointsDeltaForStrength(statUpStep);
                    if (statAddPointsDelta + statPointsShareCurrent < statPointsShare)
                    {
                        statPointsShareCurrent += statAddPointsDelta;
                        statValue += statUpStep;
                        if (isHealthStat)
                             card.health += statUpStep;
                        else card.strength += statUpStep;
                    }
                    else if (statUpStep > 1)
                    {
                        statUpStep /= 2;
                        goto TryAddStat;
                    }
                    else break;
                    if (statUpIterations++ > 1000)
                        throw new FieldCardStatUpgradeException(isHealthStat ? "health" : "strength", points, statsPointsShare, traitsPointsShare);
                }
                statsPointsShareCurrent += statPointsShareCurrent;
                TableConsole.LogToFile("upsys", $"{card.id}: upgrade: stat {(isHealthStat ? "health" : "strength")}: iterations: {statUpIterations}, value: {statValue}, share_max: {statPointsShare}, share_cur: {statPointsShareCurrent}");
                isHealthStat = false;
                healthSparePoints = statPointsShare - statPointsShareCurrent;
            }
            #endregion

            TableConsole.LogToFile("upsys", $"{card.id}: upgrade finished: spare points: {points - statsPointsShareCurrent - traitsPointsShareCurrent}");
        }
    }
}
