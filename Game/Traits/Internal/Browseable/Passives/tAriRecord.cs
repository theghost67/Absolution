using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAriRecord : PassiveTrait
    {
        const string ID = "ari_record";
        const string TRAIT_ID = "evasion";
        static readonly TraitStatFormula _traitsF = new(false, 0, 1);

        public tAriRecord() : base(ID)
        {
            name = Translator.GetString("trait_ari_record_1");
            desc = Translator.GetString("trait_ari_record_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tAriRecord(tAriRecord other) : base(other) { }
        public override object Clone() => new tAriRecord(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_ari_record_3", traitName, _traitsF.Format(args.stacks));

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = _traitsF.ValueInt(args.stacks) } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(40, stacks);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;

            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(trait.GuidStr, OnTargetPostKilled);
            else e.target.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard target = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(target.Territory);

            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (trait.Owner.Field == null) return;

            int stacks = _traitsF.ValueInt(trait.GetStacks());
            await trait.AnimActivation();
            await trait.Owner.Traits.AdjustStacks(TRAIT_ID, stacks, trait);
        }
    }
}
