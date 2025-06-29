using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTesting : ActiveTrait
    {
        const string ID = "testing";
        static readonly TraitStatFormula _moxieF = new(false, 0, 0);
        static readonly TraitStatFormula _strengthF = new(false, 9999, 0);
        static readonly TerritoryRange targets = TerritoryRange.oppositeAll;

        public tTesting() : base(ID)
        {
            name = Translator.GetString("trait_testing_1");
            desc = Translator.GetString("trait_testing_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tTesting(tTesting other) : base(other) { }
        public override object Clone() => new tTesting(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_testing_3", _moxieF.Format(args.stacks), _strengthF.Format(args.stacks));

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.16f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            int moxie = _moxieF.ValueInt(e.traitStacks);
            int strength = _strengthF.ValueInt(e.traitStacks);

            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, targets).WithCard();
            foreach (BattleFieldCard card in fields.Select(f => f.Card))
            {
                if (card.Moxie > moxie) continue;
                card.Drawer.CreateTextAsDamage(strength, false);
                await card.Health.AdjustValue(-strength, owner);
            }
            await trait.SetStacks(0, owner.Side);
        }
    }
}
