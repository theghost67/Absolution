using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tHired : ActiveTrait
    {
        const string ID = "hired";
        const string TRAIT_ID = "stun";

        public tHired() : base(ID)
        {
            name = "Ты нанят!";
            desc = "Я зарабатываю по 1000$ в день. Как? Всё просто...";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tHired(tHired other) : base(other) { }
        public override object Clone() => new tHired(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При активации на вражеской карте рядом</color>\nКоличество золота стороны-владельца уменьшится на значение, " +
                   $"равное стоимости цели. Цель получит <nobr><u>{traitName}</u></nobr>.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.target.Card.Price <= ((IBattleTrait)e.trait).Side.Gold;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            await trait.Side.Gold.AdjustValue(-target.Card.Price, trait);
            await target.Card.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
        }
    }
}
