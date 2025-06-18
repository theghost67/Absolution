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
        const string CARD_ID = "origami";

        public tOrigamiMark() : base(ID)
        {
            name = Translator.GetString("trait_origami_mark_1");
            desc = Translator.GetString("trait_origami_mark_2");

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tOrigamiMark(tOrigamiMark other) : base(other) { }
        public override object Clone() => new tOrigamiMark(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_origami_mark_3", cardName);

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
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (owner.Data.id == CARD_ID) return;

            BattleField field = e.field;
            if (field.Card != null) return;

            FieldCard card = CardBrowser.NewField(CARD_ID);
            await trait.AnimActivation();
            await owner.Territory.PlaceFieldCard(card, field, trait);
        }
    }
}
