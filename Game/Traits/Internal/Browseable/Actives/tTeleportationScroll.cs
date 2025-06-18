using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTeleportationScroll : ActiveTrait
    {
        const string ID = "teleportation_scroll";

        public tTeleportationScroll() : base(ID)
        {
            name = Translator.GetString("trait_teleportation_scroll_1");
            desc = Translator.GetString("trait_teleportation_scroll_2");

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tTeleportationScroll(tTeleportationScroll other) : base(other) { }
        public override object Clone() => new tTeleportationScroll(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_teleportation_scroll_3");
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.1f);
        }
        public override BattleWeight Weight(IBattleTrait trait)
        {
            return new(trait, trait.Owner.CalculateWeight(trait.Guid).Total * 0.25f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            await owner.TryAttachToField(target, trait);
            await trait.AdjustStacks(-1, owner.Side);
        }
    }
}
