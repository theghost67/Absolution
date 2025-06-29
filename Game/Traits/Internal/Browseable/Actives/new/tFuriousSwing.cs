using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFuriousSwing : ActiveTrait
    {
        const string ID = "furious_swing";
        const int CD = 1;
        static readonly TraitStatFormula _verticalF = new(false, 0, 3);
        static readonly TraitStatFormula _horizontalF = new(false, 0, 1);

        public tFuriousSwing() : base(ID)
        {
            name = Translator.GetString("trait_furious_swing_1");
            desc = Translator.GetString("trait_furious_swing_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tFuriousSwing(tFuriousSwing other) : base(other) { }
        public override object Clone() => new tFuriousSwing(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_furious_swing_3", _verticalF.Format(args.stacks), CD, _horizontalF.Format(args.stacks), CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _verticalF.Value(result.Entity.GetStacks()));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(6, stacks);
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
            trait.SetCooldown(CD);
            bool isOpposite = owner.Field.Opposite == target;
            if (isOpposite)
            {
                int damage = _verticalF.ValueInt(e.traitStacks);
                target.Drawer.CreateTextAsDamage(damage, false);
                await target.Card.Health.AdjustValue(-damage, trait);
            }
            else
            {
                int damage = _horizontalF.ValueInt(e.traitStacks);
                BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, TerritoryRange.oppositeTriple).WithCard().Select(f => f.Card).ToArray();
                foreach (BattleFieldCard card in cards)
                {
                    card.Drawer.CreateTextAsDamage(damage, false);
                    if (!card.IsKilled)
                        await card.Health.AdjustValue(-damage, trait);
                }
            }
        }
    }
}
