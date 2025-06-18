using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCane : ActiveTrait
    {
        const string ID = "cane";
        const int CD = 2;
        static readonly TraitStatFormula _strengthF = new(false, 0, 2);

        public tCane() : base(ID)
        {
            name = Translator.GetString("trait_cane_1");
            desc = Translator.GetString("trait_cane_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerRadiusSmall);
        }
        protected tCane(tCane other) : base(other) { }
        public override object Clone() => new tCane(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_cane_3", _strengthF.Format(args.stacks, true), CD);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _strengthF.Value(result.Entity.GetStacks()) * 0.75f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            int strength = _strengthF.ValueInt(trait.GetStacks());
            BattleInitiationSendArgs initiation = new(owner, strength, true, false, target);
            trait.SetCooldown(CD);
            await owner.Territory.Initiations.EnqueueAndAwait(initiation);
        }
    }
}
