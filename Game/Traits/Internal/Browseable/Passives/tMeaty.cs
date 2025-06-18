using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMeaty : PassiveTrait
    {
        const string ID = "meaty";
        static readonly TraitStatFormula _healthF = new(true, 0.00f, 0.25f);

        public tMeaty() : base(ID)
        {
            name = Translator.GetString("trait_meaty_1");
            desc = Translator.GetString("trait_meaty_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tMeaty(tMeaty other) : base(other) { }
        public override object Clone() => new tMeaty(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_meaty_3", name, _healthF.Format(args.stacks, true));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
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

            if (e.target.Traits.Passive(ID) == null) return;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await trait.Owner.Health.AdjustValueScale(_healthF.Value(e.traitStacks), trait, entryId);
            }
        }
    }
}
