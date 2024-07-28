using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех существующих данных навыков в исходном состоянии.
    /// </summary>
    public static class TraitBrowser
    {
        public static IReadOnlyCollection<PassiveTrait> Passives => _passives.Values;
        public static IReadOnlyCollection<ActiveTrait> Actives => _actives.Values;
        public static IReadOnlyCollection<Trait> All => _all.Values;

        static readonly Dictionary<string, PassiveTrait> _passives = new();
        static readonly Dictionary<string, ActiveTrait> _actives = new();
        static readonly Dictionary<string, Trait> _all = new();

        public static void Initialize()
        {
            /* --------------------------------- //
            ||            ANY LOCATION           ||
            // --------------------------------- */

            AddPassive(new tEvasion());
            AddPassive(new tBlock());

            AddActive(new tScope());
            AddActive(new tScopePlus());

            // -----------------------------------

            /* --------------------------------- //
            ||            LOC: COLLEGE           ||
            // --------------------------------- */

            AddPassive(new tArmoredTank());
            AddPassive(new tBloodthirstiness());
            AddPassive(new tBoilingAnger());
            AddPassive(new tBrickyTaste());
            AddPassive(new tCompetitiveObsession());

            AddPassive(new tFoodPoisoning());
            AddPassive(new tGrannyAlliance());
            AddPassive(new tInevitability());
            AddPassive(new tLookOfDespair());
            AddPassive(new tMeaty());

            AddPassive(new tOldAuthority());
            AddPassive(new tPigeonFright());
            AddPassive(new tScholar());
            AddPassive(new tTactician());
            AddPassive(new tUltrasonicScream());
            AddPassive(new tUnpleasantScent());

            AddActive(new tLunchBreak());
            AddActive(new tTesting());
            AddActive(new tUnscheduledTest());
            AddActive(new tWhiteBombing());
            AddActive(new tZenSchool());

            // -----------------------------------

            /* --------------------------------- //
            ||             LOC: BUREAU           ||
            // --------------------------------- */

            AddPassive(new tAlcoRage());
            AddPassive(new tAriRecord());
            AddPassive(new tBecomeHuman());
            AddPassive(new tBecomeMachine());
            AddPassive(new tCrosseyedShooter());

            AddPassive(new tMinistryRat());
            AddPassive(new tOrderOfAttackWait());
            AddPassive(new tOrderOfDefenceWait());
            AddPassive(new tOrigamiKiller());
            AddPassive(new tOrigamiMark());

            AddPassive(new tPrediction());
            AddPassive(new tStealth());
            AddPassive(new tSummarizing());
            AddPassive(new tTimeToDecideMachine());

            AddActive(new tAlcoHeal());
            AddActive(new tOrderOfAttack());
            AddActive(new tOrderOfDefence());
            AddActive(new tOrigamiVictim());
            AddActive(new tRecruitment());

            AddActive(new tReporting());
            AddActive(new tSearchInArchive());
  
            AddActive(new tTimeToDecide());
            AddActive(new tTriptocainum());
            AddActive(new tSentence());
            AddActive(new tWayOut());

            // -----------------------------------
        }

        public static PassiveTrait NewPassive(string id) => (PassiveTrait)GetPassive(id).CloneAsNew();
        public static ActiveTrait NewActive(string id) => (ActiveTrait)GetActive(id).CloneAsNew();
        public static Trait NewTrait(string id) => (Trait)GetTrait(id).CloneAsNew();

        public static PassiveTrait GetPassive(string id)
        {
            if (_passives.TryGetValue(id, out PassiveTrait trait))
                 return trait;
            else throw new System.NullReferenceException($"Passive trait with specified id was not found: {id}.");
        }
        public static ActiveTrait GetActive(string id)
        {
            if (_actives.TryGetValue(id, out ActiveTrait trait))
                 return trait;
            else throw new System.NullReferenceException($"Active trait with specified id was not found: {id}.");
        }
        public static Trait GetTrait(string id)
        {
            if (_all.TryGetValue(id, out Trait trait))
                 return trait;
            else throw new System.NullReferenceException($"Trait with specified id was not found: {id}.");
        }

        static void AddPassive(PassiveTrait srcTrait)
        {
            _passives.Add(srcTrait.id, srcTrait);
            _all.Add(srcTrait.id, srcTrait);
        }
        static void AddActive(ActiveTrait srcTrait)
        {
            _actives.Add(srcTrait.id, srcTrait);
            _all.Add(srcTrait.id, srcTrait);
        }
    }
}
