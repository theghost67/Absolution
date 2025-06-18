using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfDefence : ActiveTrait
    {
        const string ID = "order_of_defence";
        const string TRAIT_ID_TO_GIVE = "order_of_defence_wait";
        static readonly TraitStatFormula _healthDecF = new(true, 0.33f, 0.00f);
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tOrderOfDefence() : base(ID)
        {
            name = Translator.GetString("trait_order_of_defence_1");
            desc = Translator.GetString("trait_order_of_defence_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tOrderOfDefence(tOrderOfDefence other) : base(other) { }
        public override object Clone() => new tOrderOfDefence(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID_TO_GIVE).name;
            return Translator.GetString("trait_order_of_defence_3", traitName, _healthDecF.Format(args.stacks, true));

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID_TO_GIVE) { linkFormat = true, stacks = 2 } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.14f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = (BattleFieldCard)e.target.Card;
            BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, _range).WithCard().Select(f => f.Card).ToArray();

            int stacks = 2;
            foreach (BattleFieldCard card in cards)
                await card.Traits.AdjustStacks(TRAIT_ID_TO_GIVE, stacks, trait);

            int health = (owner.Side.HealthAtStart * _healthDecF.Value(stacks)).Ceiling();
            await owner.Side.Health.AdjustValue(-health, trait);

            await owner.Traits.SetStacks(ID, 0, trait.Side);
        }
    }
}
