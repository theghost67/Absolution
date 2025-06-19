using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTeleportationBag : ActiveTrait
    {
        const string ID = "teleportation_bag";
        const string TRAIT_ID = "teleportation_scroll";
        const int CD = 1;

        public tTeleportationBag() : base(ID)
        {
            name = Translator.GetString("trait_teleportation_bag_1");
            desc = Translator.GetString("trait_teleportation_bag_2");

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tTeleportationBag(tTeleportationBag other) : base(other) { }
        public override object Clone() => new tTeleportationBag(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("trait_teleportation_bag_3", traitName, CD);
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return BattleWeight.One(result.Entity);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;

            await target.Traits.AdjustStacks(TRAIT_ID, 1, trait);
            await trait.AdjustStacks(-1, trait.Side);
        }
    }
}
