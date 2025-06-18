using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAlcoHeal : ActiveTrait
    {
        const string ID = "alco_heal";
        static readonly TraitStatFormula _healthIncF = new(false, 0, 6);
        static readonly TraitStatFormula _moxieDecF = new(false, 1, 0);
        const int CD = 2;

        public tAlcoHeal() : base(ID)
        {
            name = Translator.GetString("trait_alco_heal_1");
            desc = Translator.GetString("trait_alco_heal_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tAlcoHeal(tAlcoHeal other) : base(other) { }
        public override object Clone() => new tAlcoHeal(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_alco_heal_3", _healthIncF.Format(args.stacks), _moxieDecF.Format(args.stacks, true), CD);

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _healthIncF.Value(result.Entity.GetStacks()) * 0.8f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard card = (BattleFieldCard)e.target.Card;

            trait.SetCooldown(CD);
            await card.Health.AdjustValue(_healthIncF.Value(e.traitStacks), trait);
            await card.Moxie.AdjustValue(-_moxieDecF.Value(e.traitStacks), trait);
        }
    }
}
