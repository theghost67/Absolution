using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBadTime : PassiveTrait
    {
        const string ID = "bad_time";
        const string MOXIE_ID = "moxie";
        const int ENEMY_MOXIE_THRESHOLD = 15;
        const int MOXIE_TO_GIVE = 5;
        static readonly TraitStatFormula _strengthF = new(true, 0.04f, 0.01f);

        public tBadTime() : base(ID)
        {
            name = "<color=red>BAD TIME</color>";
            desc = Translator.GetString("trait_bad_time_1");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0.25f;
        }
        protected tBadTime(tBadTime other) : base(other) { }
        public override object Clone() => new tBadTime(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_bad_time_2", _strengthF.Format(args.stacks), ENEMY_MOXIE_THRESHOLD, MOXIE_TO_GIVE);

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks, 1, 1.8f);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            int moxieSum = 0;
            float strength = 0;
            float strengthPerMoxie = _strengthF.Value(trait.GetStacks());

            IEnumerable<BattleFieldCard> cards = trait.Side.Opposite.Fields().WithCard().Select(f => f.Card);
            foreach (BattleFieldCard card in cards)
            {
                strength += card.Moxie * strengthPerMoxie;
                moxieSum += card.Moxie;
            }
            strength = strength.ClampedMin(0);

            bool hadExtraMoxie = trait.Storage.ContainsKey(MOXIE_ID);
            bool hasExtraMoxie = moxieSum >= ENEMY_MOXIE_THRESHOLD;
            bool extraMoxieActivation = hadExtraMoxie != hasExtraMoxie;
            float moxieToGive = hadExtraMoxie ? -(float)trait.Storage[MOXIE_ID] : MOXIE_TO_GIVE;

            if (strength > 0 || extraMoxieActivation)
                await trait.AnimActivation();

            string entryId = NewGuidStr;
            if (strength > 0)
            {
                await trait.Owner.Strength.RevertValueScale(entryId);
                await trait.Owner.Strength.AdjustValueScale(strength, trait, entryId);
            }
            if (extraMoxieActivation)
            {
                await trait.Owner.Moxie.RevertValue(entryId);
                await trait.Owner.Moxie.AdjustValue(moxieToGive, trait, entryId);
                trait.Storage[MOXIE_ID] = trait.Owner.Moxie.EntryValue(entryId);
            }
        }
    }
}
