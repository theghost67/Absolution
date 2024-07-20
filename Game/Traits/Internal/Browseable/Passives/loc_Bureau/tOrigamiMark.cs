using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrigamiMark : PassiveTrait
    {
        const string ID = "origami_mark";
        const int PRIORITY = 6;
        const string CARD_ID = "origami";

        public tOrigamiMark() : base(ID)
        {
            name = "Метка Оригами";
            desc = "Следующая жертва Мастера Оригами не заставит себя долго ждать.";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tOrigamiMark(tOrigamiMark other) : base(other) { }
        public override object Clone() => new tOrigamiMark(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти владельца (П{PRIORITY})",
                    $"создаёт на месте владельца карту <i>{cardName}</i>."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPostKilled(object sender, ITableEntrySource source)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            FieldCard card = CardBrowser.NewField(CARD_ID);
            await trait.AnimActivation();
            await owner.Territory.PlaceFieldCard(card, owner.Field, trait);
        }
    }
}
