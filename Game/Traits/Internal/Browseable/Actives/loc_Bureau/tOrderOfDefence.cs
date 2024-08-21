using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfDefence : ActiveTrait
    {
        const string ID = "order_of_defence";
        const string TRAIT_ID_TO_GIVE = "order_of_defence_wait";
        const string TRAIT_ID_TO_REMOVE = "order_of_attack";
        static readonly TraitStatFormula _healthDecF = new(true, 0.50f, 0.50f);
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tOrderOfDefence() : base(ID)
        {
            name = "Приказ о защите";
            desc = "Стоять насмерть!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.self;
        }
        protected tOrderOfDefence(tOrderOfDefence other) : base(other) { }
        public override object Clone() => new tOrderOfDefence(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID_TO_GIVE).name;
            return $"<color>При использовании</color>\n" +
                   $"Все карты рядом с владельцем получают навык <u>{traitName}</u>. " +
                   $"Уменьшает здоровье у стороны-владельца на {_healthDecF.Format(args.stacks, true)}. Тратит все заряды всех видов приказов у владельца.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID_TO_GIVE) { linkFormat = true } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.16f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = (BattleFieldCard)e.target.Card;
            BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, _range).WithCard().Select(f => f.Card).ToArray();

            int stacks = e.traitStacks;
            foreach (BattleFieldCard card in cards)
                await card.Traits.AdjustStacks(TRAIT_ID_TO_GIVE, stacks, trait);

            float health = -_healthDecF.Value(stacks);
            await owner.Side.Health.AdjustValueScale(health, trait);

            await owner.Traits.SetStacks(ID, 0, trait.Side);
            await owner.Traits.SetStacks(TRAIT_ID_TO_REMOVE, 0, trait.Side);
        }
    }
}
