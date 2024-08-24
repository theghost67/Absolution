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
            name = "Порыться в архивах";
            desc = "Сейчас, где же оно? О, нашёл, тут сказано, что вы...";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tSearchInArchive(tSearchInArchive other) : base(other) { }
        public override object Clone() => new tSearchInArchive(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"<color>При использовании</color>\nСоздаёт рядом с собой карты <nobr><color><u>{cardName}</u></color></nobr>. Тратит все заряды.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            int[] stats = new int[] { 0, 0, _healthF.ValueInt(args.stacks), 0 };
            return new DescLinkCollection()
            { new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = stats } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(10, stacks);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.1f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField[] fields = trait.Territory.Fields(trait.Field.pos, _range).WithoutCard().ToArray();
            int health = _healthF.ValueInt(e.traitStacks);

            await trait.SetStacks(0, trait.Side);
            foreach (BattleField field in fields)
            {
                FieldCard card = CardBrowser.NewField(CARD_ID);
                card.traits.Clear();
                card.health = health;
                await trait.Territory.PlaceFieldCard(card, field, trait);
            }
        }
    }
}
