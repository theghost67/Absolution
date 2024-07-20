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
        const int HEALTH_PER_STACK = 2;
        static readonly TerritoryRange cardsSpawn = TerritoryRange.ownerDouble;

        public tSearchInArchive() : base(ID)
        {
            name = "Порыться в архивах";
            desc = "";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tSearchInArchive(tSearchInArchive other) : base(other) { }
        public override object Clone() => new tSearchInArchive(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            int effect = HEALTH_PER_STACK * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании",
                    $"Создаёт рядом с собой карты {cardName} с <u>{effect}</u> ед. здоровья. Тратит все заряды."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 10 * (stacks - 1);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            BattleField[] fields = trait.Territory.Fields(trait.Field.pos, cardsSpawn).WithoutCard().ToArray();
            int health = HEALTH_PER_STACK * trait.GetStacks();

            foreach (BattleField field in fields)
            {
                FieldCard card = CardBrowser.NewField(CARD_ID);
                card.health = health;
                await trait.Territory.PlaceFieldCard(card, field, trait);
            }
            await trait.SetStacks(0, trait.Side);
        }
    }
}
