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
        const int PRIORITY = 1;
        static readonly TerritoryRange _range = TerritoryRange.ownerAll;
        string _eventGuid;
        TableFinder _sideFinder;

        public cObjectOfObsession() : base(ID)
        {
            name = "Объект одержимости";
            desc = "Бросить все планы. Игнорировать всех остальных. Сфокусироваться только на одной цели. И УНИЧТОЖИТЬ ЕЁ.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cObjectOfObsession(cObjectOfObsession other) : base(other) { }
        public override object Clone() => new cObjectOfObsession(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"До следующего хода, все союзные карты на территории (П{PRIORITY}) получают навык <u>{traitName}</u>.";
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
            await base.OnUse(e);
            if (!e.isInBattle) return;

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleSide side = card.Side;
            BattleTerritory terr = side.Territory;

            _eventGuid = card.GuidStr;
            _sideFinder = side.Finder;

            terr.ContinuousAttachHandler_Add(_eventGuid, ContinuousAttach_Add, PRIORITY);
            terr.OnNextPhase.Add(_eventGuid, OnNextPhase);
        }

        UniTask OnNextPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (terr.IsStartPhase())
                terr.ContinuousAttachHandler_Remove(_eventGuid, ContinuousAttach_Remove);
            return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            bool isInRange = _range.OverlapFromPlayerPos().Contains(e.field.pos);
            if (isInRange && (e.card.FieldsAttachments == 1 || e.source == null))
                 return e.card.Traits.AdjustStacks(TRAIT_ID, 1, side);
            else return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            bool isInRange = _range.OverlapFromPlayerPos().Contains(e.field.pos);
            if (isInRange && (e.card.FieldsAttachments == 1 || e.source == null))
                return e.card.Traits.AdjustStacks(TRAIT_ID, -1, side);
            else return UniTask.CompletedTask;
        }
    }
}
