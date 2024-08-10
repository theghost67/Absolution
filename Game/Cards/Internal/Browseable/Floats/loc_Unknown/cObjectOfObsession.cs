using Cysharp.Threading.Tasks;
using Game.Territories;
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
            frequency = 1.00f;
        }
        protected cObjectOfObsession(cObjectOfObsession other) : base(other) { }
        public override object Clone() => new cObjectOfObsession(this);

        public override string DescRich(ITableCard card)
        {
            return DescRichBase(card, $"На следующем ходу все союзные карты будут атаковать вражескую карту с наибольшим количеством здоровья (П{PRIORITY}).");
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

            terr.ContinuousAttachHandler_Add(_eventGuid, ContinuousAttach_Add);
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
            if (_range.OverlapFromPlayerPos().Contains(e.field.pos)) // owner side
                 return e.card.Traits.AdjustStacks(TRAIT_ID, 1, side);
            else return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            if (_range.OverlapFromPlayerPos().Contains(e.field.pos)) // owner side
                 return e.card.Traits.AdjustStacks(TRAIT_ID, -1, side);
            else return UniTask.CompletedTask;
        }
    }
}
