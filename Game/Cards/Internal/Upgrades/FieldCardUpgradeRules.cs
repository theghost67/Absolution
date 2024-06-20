using Game.Traits;
using GreenOne;
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
        public int statPoints;
        public int traitsCount;
        public IReadOnlyDictionary<string, float> possiblePassivesFreqs;
        public IReadOnlyDictionary<string, float> possibleActivesFreqs;

        public FieldCardUpgradeRules(int statPoints, bool addTraits) : this(statPoints, addTraits ? TraitsCount(statPoints) : 0) { }
        public FieldCardUpgradeRules(int statPoints, int traitsCount) 
        {
            this.statPoints = statPoints;
            this.traitsCount = traitsCount;

            possiblePassivesFreqs = null;
            possibleActivesFreqs = null;
        }

        public static int TraitsCount(int cardStatPoints)
        {
            return System.Convert.ToInt32(TraitsCountRaw(cardStatPoints));
        }
        public static float TraitsCountRaw(int cardStatPoints)
        {
            if (cardStatPoints < 16)
                return 0;
            else return Mathf.Log((cardStatPoints - 10) / 6);
        }

        public readonly void Reset(FieldCard card)
        {
            card.health = 1;
            card.strength = 0;
            card.moxie = Random.Range(1, 6);
        }
        public readonly void Upgrade(FieldCard card)
        {
            if (statPoints <= 0)
                return;

            #region stats/ratios initializing
            int cardTraitsCount = card.traits.Count;
            int allTraitsCount = traitsCount + cardTraitsCount;

            float healthFreq = Random.Range(0.5f, 2f);
            float strengthFreq = Random.Range(0.5f, 2f);

            float statPointsShare = 0;
            float statPointsShareCurrent = 0;

            float traitPointsShare = 0;
            float traitPointsShareCurrent = 0;

            if (allTraitsCount == 0)
            {
                statPointsShare = statPoints;
                goto SkipTraitsAll;
            }
            UpdatePointsShares(ref statPointsShare, ref traitPointsShare, statPoints, healthFreq, strengthFreq);
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

                AmplifyTraitFreqByMoodAndCardStats(ref traitFrequency, traitSrc.mood, card);
                if (traitFrequency != 0) // no need to add trait that has 0 chance
                    possibleTraits.Add(traitId, traitFrequency);
            }
            foreach (KeyValuePair<string, float> pair in possibleActivesFreqs)
            {
                string traitId = pair.Key;
                float traitFrequency = pair.Value;
                Trait traitSrc = TraitBrowser.GetActive(traitId);

                AmplifyTraitFreqByMoodAndCardStats(ref traitFrequency, traitSrc.mood, card);
                if (traitFrequency != 0) // no need to add trait that has 0 chance
                    possibleTraits.Add(traitId, traitFrequency);
            }
            #endregion

            #region traits creation
            SkipPossibleTraitsAmplifying: // fills new dict using card.traits and possibleTraits, also scales stats and traits upgrade frequency
            Dictionary<string, float> cardTraitsStackable = new(capacity: allTraitsCount); // traitId, frequency
            foreach (Trait trait in card.traits.Select(e => e.Trait).Where(t => !t.tags.HasFlag(TraitTag.Static)))
                cardTraitsStackable.Add(trait.id, 1f - trait.frequency);

            for (int i = cardTraitsStackable.Count; i < allTraitsCount; i++)
            {
                if (possibleTraits.Count == 0) break;
                KeyValuePair<string, float> weightedPair = possibleTraits.GetWeightedRandom(pair => pair.Value);
                possibleTraits.Remove(weightedPair.Key);

                string traitId = weightedPair.Key;
                Trait traitSrc = TraitBrowser.GetTrait(traitId);
                float traitFrequency = weightedPair.Value;
                TraitMood traitMood = traitSrc.mood;

                int traitAddPointsDelta = card.PointsDeltaForTrait(traitSrc, 1);
                if (traitAddPointsDelta + traitPointsShareCurrent < traitPointsShare)
                {
                    traitPointsShareCurrent += traitAddPointsDelta;
                    if (traitSrc.isPassive)
                         card.traits.Passives.Adjust(traitId, 1);
                    else card.traits.Actives.Adjust(traitId, 1);
                }
                else
                {
                    possibleTraits.Remove(traitId);
                    continue; // cannot afford another trait add
                }

                if (!traitSrc.tags.HasFlag(TraitTag.Static)) // if is stackable
                    cardTraitsStackable.Add(traitId, 1f - traitFrequency);

                UpdateStatFreqs(ref healthFreq, ref strengthFreq, traitMood, possibleTraits, 0);
            }
            #endregion

            #region traits upgrading
            // upgrades generated traits (stackable ones)
            int traitUpgrIterations = 0;
            while (traitPointsShareCurrent < traitPointsShare)
            {
                if (cardTraitsStackable.Count == 0) break;
                KeyValuePair<string, float> weightedPair = cardTraitsStackable.GetWeightedRandom(pair => pair.Value);

                string traitId = weightedPair.Key;
                Trait traitSrc = TraitBrowser.GetTrait(traitId);
                TraitMood traitMood = traitSrc.mood;

                int stackAddPointsDelta = card.PointsDeltaForTrait(traitSrc, 1);
                if (stackAddPointsDelta + traitPointsShareCurrent < traitPointsShare)
                {
                    traitPointsShareCurrent += stackAddPointsDelta;
                    if (traitSrc.isPassive)
                        card.traits.Passives.Adjust(traitId, 1);
                    else card.traits.Actives.Adjust(traitId, 1);
                }
                else
                {
                    cardTraitsStackable.Remove(traitId);
                    goto Continue; // cannot afford another stack add
                }

                UpdateStatFreqs(ref healthFreq, ref strengthFreq, traitMood, possibleTraits, 0);
                UpdatePointsShares(ref statPointsShare, ref traitPointsShare, statPoints, healthFreq, strengthFreq);

                Continue:
                if (traitUpgrIterations++ > 1000)
                    throw new FieldCardTraitUpgradeException(traitId, statPoints, statPointsShare, traitPointsShare);
            }
            statPointsShare -= traitPointsShareCurrent - traitPointsShare; // adjusts statPointsShare using traitPointsShare not spent points
            #endregion

            #region stats upgrading
            SkipTraitsAll: // upgrades stats using stat frequencies
            int statUpIterations = 0;
            int statUpStep = (int)(statPointsShare * 0.1f).ClampedMin(1);
            bool healthUps = true;
            bool strengthUps = true;

            strengthFreq = strengthFreq.ClampedMin(0);
            healthFreq = healthFreq.ClampedMin(0);
            while (statPointsShareCurrent < statPointsShare)
            {
                bool healthUp;
                if (!healthUps)
                     healthUp = false;
                else if (!strengthUps)
                     healthUp = true;
                else healthUp = Random.Range(0, strengthFreq + healthFreq) < healthFreq;

                int statAddPointsDelta;
                if (healthUp)
                     statAddPointsDelta = card.PointsDeltaForHealth(statUpStep);
                else statAddPointsDelta = card.PointsDeltaForStrength(statUpStep);

                if (statAddPointsDelta + statPointsShareCurrent < statPointsShare)
                {
                    statPointsShareCurrent += statAddPointsDelta;
                    if (healthUp)
                         card.strength += statUpStep;
                    else card.health += statUpStep;
                }
                else
                {
                    if (healthUp)
                         healthUps = false;
                    else strengthUps = false;

                    if (!healthUps && !strengthUps)
                         return;        // cannot afford any stat add
                    else goto Continue; // cannot afford one of stat add
                }

                Continue:
                if (statUpIterations++ > 1000)
                    throw new FieldCardStatUpgradeException(healthUp ? "health" : "strength", statPoints, statPointsShare, traitPointsShare);
            }
            #endregion
        }

        static void AmplifyTraitFreqByMoodAndCardStats(ref float traitFrequency, in TraitMood traitMood, in FieldCard card)
        {
            if (traitMood.moxieReq != TraitMoxieReq.Any)
                traitFrequency *= GetFreqMod(card.moxie, traitMood.moxieReq);
        }
        static float GetFreqMod(int aspectValue, TraitMoxieReq aspectReq)
        {
            // aspect should be from 1 to 5
            float aspectValueAbs = Mathf.Abs(aspectValue - 3);
            switch (aspectReq)
            {
                case TraitMoxieReq.Low: return 2f / Mathf.Pow(2, aspectValue - 1);

                case TraitMoxieReq.Medium:
                    if (aspectValueAbs >= 2)
                        return 0.5f;
                    else if (aspectValueAbs >= 1)
                        return 1f;
                    else return 2f;

                case TraitMoxieReq.High: return 2f / Mathf.Pow(2, 5 - aspectValue);

                case TraitMoxieReq.HigherIsBetter: return (0.75f + 0.25f * Mathf.Pow(2, aspectValue - 1)).ClampedMax(3f);
                case TraitMoxieReq.LowerIsBetter: return (0.75f + 0.25f * Mathf.Pow(2, 5 - aspectValue)).ClampedMax(3f);
                default: throw new System.ArgumentOutOfRangeException();
            }
        }
        static float ModifyValue(in float oldValue, in TraitMoodMod mod, in int stacks)
        {
            if (stacks <= 0)
                return (oldValue + mod.staticAbs) * mod.staticRel;
            else return (oldValue + mod.stackAbs) * mod.stackRel;
            // old version for stacks: (oldValue + mod.stackAbs / stack) * (1 + (mod.stackRel - 1) / stack)
        }

        static void UpdatePointsShares(ref float statPointsShare, ref float traitPointsShare, in float statPoints, in float healthFreq, in float strengthFreq)
        {
            /* 
             * avg 0.25  = 0  % from total share
             * avg 0.50  = 25 % from total share
             * avg 1.00x = 50 % from total share
             * avg 2.00x = 75 % from total share
             * avg 4.00x = 100% from total share
             */

            float avgStatMood = (healthFreq.ClampedMin(0) + strengthFreq.ClampedMin(0)) / 2;
            float statsShareAmplifier = avgStatMood switch
            {
                < 0.25f => 0,
                < 1.00f => Mathf.Log(avgStatMood + 1, 3) - 0.13093f,
                < 4.00f => Mathf.Sqrt(avgStatMood) / 2,
                _ => 1,
            };

            statPointsShare = statPoints * statsShareAmplifier;
            traitPointsShare = statPoints - statPointsShare;
        }
        static void UpdateStatFreqs(ref float healthFreq, ref float strengthFreq, in TraitMood tMood, Dictionary<string, float> tDict, in int stacks)
        {
            healthFreq = ModifyValue(healthFreq, tMood.healthMod, stacks);
            strengthFreq = ModifyValue(strengthFreq, tMood.strengthMod, stacks);

            foreach (KeyValuePair<string, TraitMoodMod> moodPair in tMood.traitsMods)
            {
                if (!tDict.ContainsKey(moodPair.Key)) continue;
                float oldPossibleTraitFreq = tDict[moodPair.Key];
                float newPossibleTraitFreq = ModifyValue(oldPossibleTraitFreq, moodPair.Value, stacks);

                if (newPossibleTraitFreq != 0)
                     tDict[moodPair.Key] = newPossibleTraitFreq;
                else tDict.Remove(moodPair.Key);
            }
        }
    }
}
