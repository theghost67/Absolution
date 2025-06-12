using MyBox;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех существующих данных навыков в исходном состоянии.
    /// </summary>
    public static class TraitBrowser
    {
        public static IReadOnlyDictionary<string, PassiveTrait> PassivesIndexed => _passives;
        public static IReadOnlyDictionary<string, ActiveTrait> ActivesIndexed => _actives;
        public static IReadOnlyDictionary<string, Trait> AllIndexed => _all;

        public static IReadOnlyCollection<PassiveTrait> Passives => _passives.Values;
        public static IReadOnlyCollection<ActiveTrait> Actives => _actives.Values;
        public static IReadOnlyCollection<Trait> All => _all.Values;

        static readonly Dictionary<string, PassiveTrait> _passives = new();
        static readonly Dictionary<string, ActiveTrait> _actives = new();
        static readonly Dictionary<string, Trait> _all = new();

        public static void Initialize()
        {
            AddPassive(new tEvasion());
            AddPassive(new tBlock());
            AddPassive(new tWideSwing());
            AddPassive(new tWideSwingPlus());
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
            AddPassive(new tAdrenaline());

            AddPassive(new tDeadlyCrit());
            AddPassive(new tExplosive());
            AddPassive(new tHeroesNeverDie());
            AddPassive(new tObsessed());
            AddPassive(new tOnLookout());

            AddPassive(new tPlayWithVictim());
            AddPassive(new tPocketDimension());
            AddPassive(new tReflection());
            AddPassive(new tRobbery());
            AddPassive(new tSacrifice());

            AddPassive(new tShootingPassion());
            AddPassive(new tSmellyTrapper());
            AddPassive(new tStalker());
            AddPassive(new tTillDawn());
            AddPassive(new tTurningPoint());

            AddPassive(new tWeaver());
            AddPassive(new tWeaving());
            AddPassive(new tAmazement());
            AddPassive(new tAvenger());
            AddPassive(new tBadTime());

            AddPassive(new tBoom());
            AddPassive(new tCamper());
            AddPassive(new tStun());
            AddPassive(new tCheats());
            AddPassive(new tCreatorsMark());

            AddPassive(new tDarkPlans());
            AddPassive(new tDontStarve());
            AddPassive(new tDoubleAttack());
            AddPassive(new tDownsideBet());
            AddPassive(new tException());

            AddPassive(new tExceptional());
            AddPassive(new tImmortality());
            AddPassive(new tExoskeleton());
            AddPassive(new tFeed());
            AddPassive(new tFinger());

            AddPassive(new tFlame());
            AddPassive(new tFrighteningPresence());
            AddPassive(new tFry());
            AddPassive(new tGrandThief());

            AddPassive(new tHack());
            AddPassive(new tHardened());
            AddPassive(new tInnocence());
            AddPassive(new tInvestment());
            AddPassive(new tBadKarma());

            AddPassive(new tLoad());
            AddPassive(new tLook());
            AddPassive(new tMikelove());
            AddPassive(new tMiracleDrug());
            AddPassive(new tMultipaw());

            AddPassive(new tMutated());
            AddPassive(new tNagaKwista());
            AddPassive(new tNineLives());
            AddPassive(new tNotHere());
            AddPassive(new tOoo());

            AddPassive(new tOpressing());
            AddPassive(new tPlunder());
            AddPassive(new tPoison());
            AddPassive(new tRandom());
            AddPassive(new tRunner());

            AddPassive(new tSadNews());
            AddPassive(new tScorchingFlame());
            AddPassive(new tScreenShield());
            AddPassive(new tShift());
            AddPassive(new tShock());

            AddPassive(new tShocking());
            AddPassive(new tSpecialAttack());
            AddPassive(new tTableManipulations());
            AddPassive(new tVaccinated());
            AddPassive(new tCianided());

            AddPassive(new tMilitaryService());
            AddPassive(new tFennecSoul());
            AddPassive(new tShopping());
            AddPassive(new tDoctor());
            AddPassive(new tItsGuard());

            AddPassive(new tChao());
            AddPassive(new tMiracleAftertaste());
            AddPassive(new tTrauma());
            AddPassive(new tExposed());


            AddActive(new tScope());
            AddActive(new tScopePlus());
            AddActive(new tLunchBreak());
            AddActive(new tTesting());
            AddActive(new tUnscheduledTest());

            AddActive(new tWhiteBombing());
            AddActive(new tZenSchool());
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
            AddActive(new tDoofinator());
            AddActive(new tEmpoweringBeam());

            AddActive(new tExplosiveMine());
            AddActive(new tHealingBeam());
            AddActive(new tNerfTime());
            AddActive(new tSelfDestruction());
            AddActive(new tSprinter());

            AddActive(new tTeleportationBag());
            AddActive(new tTeleportationScroll());
            AddActive(new tBoo());
            AddActive(new tBrawl());
            AddActive(new tBuilder());

            AddActive(new tCane());
            AddActive(new tChess());
            AddActive(new tCult());
            AddActive(new tDarkBall());
            AddActive(new tDarkShield());

            AddActive(new tDeathChord());
            AddActive(new tEgo());
            AddActive(new tFuriousSwing());
            AddActive(new tGottaGo());
            AddActive(new tHammerGo());

            AddActive(new tHammerOut());
            AddActive(new tHastaLaVista());
            AddActive(new tHired());
            AddActive(new tHyperReflex());
            AddActive(new tLightningSpeed());

            AddActive(new tMamaBag());
            AddActive(new tMindSplit());
            AddActive(new tMyStory());
            AddActive(new tParkour());
            AddActive(new tPartyAnimal());

            AddActive(new tPlanned());
            AddActive(new tPoisonGrenade());
            AddActive(new tSpiderSuit());
            AddActive(new tSunrisingFlame());
            AddActive(new tTeleportationBracelet());

            AddActive(new tVaccianide());
            AddActive(new tWeeds());
            AddActive(new tWeNeedYou());
            AddActive(new tExecution());
            AddActive(new tInsight());

            AddActive(new tTea());
            AddActive(new tFurnitureProtection());
        }

        public static PassiveTrait NewPassive(string id) => (PassiveTrait)GetPassive(id).CloneAsNew();
        public static ActiveTrait NewActive(string id) => (ActiveTrait)GetActive(id).CloneAsNew();
        public static Trait NewTrait(string id) => (Trait)GetTrait(id).CloneAsNew();

        public static PassiveTrait NewPassiveRandom()
        {
            return (PassiveTrait)_passives.Values.GetWeightedRandom(c => c.frequency).CloneAsNew();
        }
        public static ActiveTrait NewFloatRandom()
        {
            return (ActiveTrait)_actives.Values.GetWeightedRandom(c => c.frequency).CloneAsNew();
        }
        public static Trait NewTraitRandom()
        {
            return (Trait)_all.Values.GetWeightedRandom(c => c.frequency).CloneAsNew();
        }

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

        public static PassiveTrait GetPassiveRandom()
        {
            return _passives.Values.GetWeightedRandom(c => c.frequency);
        }
        public static ActiveTrait GetFloatRandom()
        {
            return _actives.Values.GetWeightedRandom(c => c.frequency);
        }
        public static Trait GetTraitRandom()
        {
            return _all.Values.GetWeightedRandom(c => c.frequency);
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
