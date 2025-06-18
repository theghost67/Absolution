using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBadKarma : PassiveTrait
    {
        const string ID = "bad_karma";
        const string CARD_ID = "death";
        const string CARD_TRAIT_ID = "execution";
        const int TRAIT_STACKS = 3;

        public tBadKarma() : base(ID)
        {
            name = Translator.GetString("trait_bad_karma_1");
            desc = "Listen, mark my words, one daaay; you will paaay, you will paaaaaay! Karma's gonna come collect your debt!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tBadKarma(tBadKarma other) : base(other) { }
        public override object Clone() => new tBadKarma(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_bad_karma_2", TRAIT_STACKS, cardName);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            TraitStacksPair[] traits = new TraitStacksPair[] { new(CARD_TRAIT_ID, 1) };
            return new() 
            { 
                new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkTraits = traits }, 
                new TraitDescriptiveArgs(CARD_TRAIT_ID) { linkFormat = true }, 
            };
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryOnStartPhase);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
        }
        async UniTask OnTerritoryOnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.GetStacks() < TRAIT_STACKS) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null || owner.Field.Opposite == null) return;

            await trait.AnimActivation();
            await trait.Territory.PlaceFieldCard(CardBrowser.NewField(CARD_ID), owner.Field.Opposite, trait);
        }
    }
}
