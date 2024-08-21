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

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"<color>После смерти владельца (П{PRIORITY})</color>\n" +
                   $"Создаст на месте владельца карту <u>{cardName}</u>. Не сработает, если владелец уже является данной картой.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats } };
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (owner.Data.id == CARD_ID) return;

            BattleField field = e.field;
            if (field.Card != null) return;

            FieldCard card = CardBrowser.NewField(CARD_ID);
            await trait.AnimActivation();
            await owner.Territory.PlaceFieldCard(card, field, trait);
        }
    }
}
