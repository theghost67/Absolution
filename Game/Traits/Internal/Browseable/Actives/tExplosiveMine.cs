using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tExplosiveMine : ActiveTrait
    {
        const string ID = "explosive_mine";
        const string TRAIT_ID = "explosive";
        const string CARD_ID = "mine";
        static readonly TraitStatFormula _stacksF = new(false, 0, 1);

        public tExplosiveMine() : base(ID)
        {
            name = "Фугасная мина";
            desc = "Бесишься?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tExplosiveMine(tExplosiveMine other) : base(other) { }
        public override object Clone() => new tExplosiveMine(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"<color>При активации на пустом союзном поле</color>\nСоздаёт карту <nobr><color><u>{cardName}</u></color></nobr> на указанном поле. Тратит все заряды.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            TraitStacksPair[] traits = new TraitStacksPair[]
            { new(TRAIT_ID, _stacksF.ValueInt(args.stacks)) };

            return new DescLinkCollection()
            {
                new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats, linkTraits = traits },
                new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = traits[0].stacks }
            };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, _stacksF.Value(result.Entity.GetStacks()) * 2);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(14, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card == null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            FieldCard card = CardBrowser.NewField(CARD_ID);

            card.traits.AdjustStacks(TRAIT_ID, _stacksF.ValueInt(e.traitStacks));
            await trait.Territory.PlaceFieldCard(card, target, trait);
            await trait.SetStacks(0, trait.Side);
        }
    }
}
