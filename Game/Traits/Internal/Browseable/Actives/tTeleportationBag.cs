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

        public tTeleportationBag() : base(ID)
        {
            name = "Сумка телепортации";
            desc = "Всегда с собой ношу несколько свитков на экстренные случаи.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tTeleportationBag(tTeleportationBag other) : base(other) { }
        public override object Clone() => new tTeleportationBag(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При активации на любой союзной карте</color>\nДаёт цели навык <nobr><u>{traitName}</u></nobr>. Тратит один заряд.";
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
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(18, stacks, 2, 1.8f);
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
