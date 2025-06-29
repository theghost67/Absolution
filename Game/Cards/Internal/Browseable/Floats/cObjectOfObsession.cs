using Cysharp.Threading.Tasks;
using Game.Territories;
using Game.Traits;
using System;
using System.Linq;

namespace Game.Cards
{
    public class cObjectOfObsession : FloatCard
    {
        const string ID = "object_of_obsession";
        const string TRAIT_ID = "obsessed";
        static readonly TerritoryRange _range = TerritoryRange.ownerAll;
        string _eventGuid;
        TableFinder _sideFinder;

        public cObjectOfObsession() : base(ID)
        {
            name = Translator.GetString("card_object_of_obsession_1");
            desc = Translator.GetString("card_object_of_obsession_2");

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cObjectOfObsession(cObjectOfObsession other) : base(other) { }
        public override object Clone() => new cObjectOfObsession(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("card_object_of_obsession_3", traitName);
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            
            if (!e.isInBattle) return;

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleSide side = card.Side;
            BattleTerritory terr = side.Territory;

            _eventGuid = card.GuidStr;
            _sideFinder = side.Finder;

            await terr.ContinuousAttachHandler_Add(_eventGuid, ContinuousAttach_Add);
            terr.OnNextPhase.Add(_eventGuid, OnNextPhase);
        }

        async UniTask OnNextPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (terr.IsStartPhase())
                await terr.ContinuousAttachHandler_Remove(_eventGuid, ContinuousAttach_Remove);
        }
        async UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            bool isInRange = _range.OverlapFromPlayerPos().Contains(e.field.pos);
            if (isInRange && (e.card.FirstFieldAttachment || e.source == null))
                 await e.card.Traits.AdjustStacks(TRAIT_ID, 1, side);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            bool isInRange = _range.OverlapFromPlayerPos().Contains(e.field.pos);
            if (isInRange && (e.card.FirstFieldAttachment || e.source == null))
                await e.card.Traits.AdjustStacks(TRAIT_ID, -1, side);
        }
    }
}
