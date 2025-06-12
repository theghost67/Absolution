using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tEmpoweringBeam : ActiveTrait
    {
        const string ID = "empowering_beam";
        const int CD = 2;
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tEmpoweringBeam() : base(ID)
        {
            name = "Усиливающий луч";
            desc = "Покажи им!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tEmpoweringBeam(tEmpoweringBeam other) : base(other) { }
        public override object Clone() => new tEmpoweringBeam(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на союзной карте рядом</color>\nУвеличивает силу цели на {_strengthF.Format(args.stacks, true)}. Перезарядка: {CD} х.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _strengthF.Value(result.Entity.GetStacks()) * 0.33f);
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
            await card.Strength.AdjustValue(_strengthF.Value(e.traitStacks), trait1);
        }
    }
}
