using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTriptocainum : ActiveTrait
    {
        const string ID = "triptocainum";
        const int CD = 3;
        static readonly TraitStatFormula _strengthF = new(true, 0.00f, 0.50f);

        public tTriptocainum() : base(ID)
        {
            name = Translator.GetString("trait_triptocainum_1");
            desc = Translator.GetString("trait_triptocainum_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tTriptocainum(tTriptocainum other) : base(other) { }
        public override object Clone() => new tTriptocainum(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_triptocainum_3", _strengthF.Format(args.stacks, true), CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.1f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, 1, 1, 1.75f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;

            float strength = _strengthF.Value(e.traitStacks);
            await trait.Owner.Strength.AdjustValueScale(strength, trait);
            trait.Owner.OnInitiationPostSent.Add(trait.GuidStr, OnInitiationPostSent);
            trait.SetCooldown(CD);
        }

        static async UniTask OnInitiationPostSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivation();
            await owner.TryKill(BattleKillMode.IgnoreHealthRestore, trait);
        }
    }
}
