using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHealingBeam : ActiveTrait
    {
        const string ID = "healing_beam";
        const int CD = 1;
        static readonly TraitStatFormula _healthF = new(false, 0, 4);

        public tHealingBeam() : base(ID)
        {
            name = Translator.GetString("trait_healing_beam_1");
            desc = Translator.GetString("trait_healing_beam_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tHealingBeam(tHealingBeam other) : base(other) { }
        public override object Clone() => new tHealingBeam(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_healing_beam_3", _healthF.Format(args.stacks, true), CD);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _healthF.Value(result.Entity.GetStacks()) * 0.33f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait1 = (IBattleTrait)e.trait;
            BattleFieldCard card = (BattleFieldCard)e.target.Card;
            trait1.SetCooldown(CD);
            await card.Health.AdjustValue(_healthF.Value(e.traitStacks), trait1);
        }
    }
}
