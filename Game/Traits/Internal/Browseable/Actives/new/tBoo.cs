using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBoo : ActiveTrait
    {
        const string ID = "boo";
        const string TRAIT_ID = "stun";

        public tBoo() : base(ID)
        {
            name = "Бу!";
            desc = "Испугался?";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tBoo(tBoo other) : base(other) { }
        public override object Clone() => new tBoo(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При активации на любой вражеской карте</color>\nДаёт цели навык <nobr><u>{traitName}</u></nobr>, тратит все заряды.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
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
            await target.Card.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
            await trait.SetStacks(0, owner.Side);
        }
    }
}
