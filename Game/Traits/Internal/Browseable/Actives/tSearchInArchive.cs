using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSearchInArchive : ActiveTrait
    {
        const string ID = "search_in_archive";
        const string CARD_ID = "clues";
        static readonly TraitStatFormula _healthF = new(false, 0, 2);
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tSearchInArchive() : base(ID)
        {
            name = Translator.GetString("trait_search_in_archive_1");
            desc = Translator.GetString("trait_search_in_archive_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tSearchInArchive(tSearchInArchive other) : base(other) { }
        public override object Clone() => new tSearchInArchive(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_search_in_archive_3", cardName);
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            int[] stats = new int[] { 0, 0, _healthF.ValueInt(args.stacks), 0 };
            return new DescLinkCollection()
            { new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = stats } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.1f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField[] fields = trait.Territory.Fields(trait.Field.pos, _range).WithoutCard().ToArray();
            int health = _healthF.ValueInt(e.traitStacks);

            foreach (BattleField field in fields)
            {
                FieldCard card = CardBrowser.NewField(CARD_ID);
                card.traits.Clear();
                card.health = health;
                await trait.Territory.PlaceFieldCard(card, field, trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
