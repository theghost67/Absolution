using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех существующих данных трейтов в исходном состоянии.
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
            ||          PASSIVE TRAITS           ||
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

            // -----------------------------------


            /* --------------------------------- //
            ||           ACTIVE TRAITS           ||
            // --------------------------------- */

            AddActive(new tLunchBreak());
            AddActive(new tTesting());
            AddActive(new tUnscheduledTest());
            AddActive(new tWhiteBombing());
            AddActive(new tZenSchool());

            // -----------------------------------
        }

        public static PassiveTrait NewPassive(string id) => (PassiveTrait)GetPassive(id).Clone();
        public static ActiveTrait NewActive(string id) => (ActiveTrait)GetActive(id).Clone();
        public static Trait NewTrait(string id) => (Trait)GetTrait(id).Clone();

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
