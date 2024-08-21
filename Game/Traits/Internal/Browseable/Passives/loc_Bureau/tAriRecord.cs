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
        const int PRIORITY = 5;
        const string TRAIT_ID = "evasion";
        static readonly TraitStatFormula _traitsF = new(false, 0, 1);

        public tAriRecord() : base(ID)
        {
            name = "УРС запись";
            desc = "Это не первая жертва убийцы. Значит, будут ещё.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tAriRecord(tAriRecord other) : base(other) { }
        public override object Clone() => new tAriRecord(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>После смерти союзной карты рядом с владельцем (П{PRIORITY})</color>\n" +
                   $"Даёт владельцу навык <u>{traitName}</u> с {_traitsF.Format(args.stacks)} зарядами.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = _traitsF.ValueInt(args.stacks) } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(40, stacks);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;

            if (e.canSeeTarget)
                 e.target.OnPostKilled.Add(trait.GuidStr, OnTargetPostKilled, PRIORITY);
            else e.target.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard target = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(target.Territory);

            if (trait == null) return;
            if (trait.Owner.Field == null) return;

            int stacks = _traitsF.ValueInt(trait.GetStacks());
            await trait.AnimActivation();
            await trait.Owner.Traits.AdjustStacks(TRAIT_ID, stacks, trait);
        }
    }
}
