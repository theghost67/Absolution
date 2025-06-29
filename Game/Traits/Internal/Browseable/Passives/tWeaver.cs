using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWeaver : PassiveTrait
    {
        const string ID = "weaver";
        const string CARD_ID = "spider_cocon";
        const string TRAIT_ID = "weaving";
        static readonly TraitStatFormula _stacksF = new(false, 0, 1);
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tWeaver() : base(ID)
        {
            name = Translator.GetString("trait_weaver_1");
            desc = Translator.GetString("trait_weaver_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tWeaver(tWeaver other) : base(other) { }
        public override object Clone() => new tWeaver(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_weaver_3", cardName);

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
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(6, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnOwnerFieldPostAttached);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (owner.IsKilled) return;
            if (owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            IEnumerable<BattleField> fields = owner.Territory.Fields(owner.Field.pos, _range).WithoutCard();
            foreach (BattleField field in fields)
            {
                FieldCard newCard = CardBrowser.NewField(CARD_ID);
                newCard.traits.AdjustStacks(TRAIT_ID, stacks);
                await owner.Territory.PlaceFieldCard(newCard, field, trait);
            }    
            await trait.SetStacks(0, trait);
        }
    }
}
