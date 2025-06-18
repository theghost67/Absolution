using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tGrannyAlliance : PassiveTrait
    {
        const string ID = "granny_alliance";
        const string CARD_ID = "granny";
        static readonly TraitStatFormula _strengthF = new(true, 0.00f, 0.50f);

        public tGrannyAlliance() : base(ID)
        {
            name = Translator.GetString("trait_granny_alliance_1");
            desc = Translator.GetString("trait_granny_alliance_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tGrannyAlliance(tGrannyAlliance other) : base(other) { }
        public override object Clone() => new tGrannyAlliance(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_granny_alliance_3", cardName, _strengthF.Format(args.stacks));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(20, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);

            IBattleTrait trait = e.trait;
            string entryId = trait.GuidGen(e.target.Guid);

            if (e.target.Data.id != ID) return;
            if (e.canSeeTarget)
            {
                e.target.OnPostKilled.Add(trait.GuidStr, OnTargetPostKilled);
                await trait.AnimDetectionOnSeen(e.target);
                await trait.Owner.Strength.AdjustValueScale(_strengthF.Value(e.traitStacks), trait, entryId);
            }
            else
            {
                e.target.OnPostKilled.Remove(trait.GuidStr);
                await trait.AnimDetectionOnUnseen(e.target);
                await trait.Owner.Strength.RevertValueScale(entryId);
            }
        }

        async UniTask OnTargetPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard observingCard = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(observingCard.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            await trait.AnimActivation();
            await trait.Owner.TryKill(BattleKillMode.Default, e.source);
        }
    }
}
