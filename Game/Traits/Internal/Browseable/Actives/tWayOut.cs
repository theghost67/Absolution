using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWayOut : ActiveTrait
    {
        const string ID = "way_out";

        public tWayOut() : base(ID)
        {
            name = "Побег";
            desc = "С тобой или без тебя - я это сделаю.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tWayOut(tWayOut other) : base(other) { }
        public override object Clone() => new tWayOut(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на территории</color>\nВозвращает владельца в рукав.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.1f);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            BattleSide ownerSide = owner.Side;

            await owner.TryAttachToSideSleeve(ownerSide, trait);
        }
    }
}
