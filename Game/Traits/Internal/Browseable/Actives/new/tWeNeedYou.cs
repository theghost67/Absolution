using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWeNeedYou : ActiveTrait
    {
        const string ID = "we_need_you";
        const string TRAIT_ID = "military_service";
        const string COUNTER_TRAIT_ID = "creators_mark";

        public tWeNeedYou() : base(ID)
        {
            name = Translator.GetString("trait_we_need_you_1");
            desc = Translator.GetString("trait_we_need_you_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerRadiusSmall);
        }
        protected tWeNeedYou(tWeNeedYou other) : base(other) { }
        public override object Clone() => new tWeNeedYou(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            string counterTraitName = TraitBrowser.GetTrait(COUNTER_TRAIT_ID).name;
            return Translator.GetString("trait_we_need_you_3", traitName, counterTraitName);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new()
            { 
                new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true },
                new TraitDescriptiveArgs(COUNTER_TRAIT_ID) { linkFormat = true }
            };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.16f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.target.Card.Traits.Passive(COUNTER_TRAIT_ID) == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            await target.Card.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
            await trait.AdjustStacks(-1, trait.Owner.Side);
        }
    }
}
