using Cysharp.Threading.Tasks;
using Game.Territories;
using System;

namespace Game.Cards
{
    public class cObjectOfObsession : FloatCard
    {
        const string ID = "object_of_obsession";
        const string TRAIT_ID = "obsessed";
        const int PRIORITY = 1;
        BattleFloatCard _card;

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

            _card = (BattleFloatCard)e.card;
            BattleTerritory terr = _card.Territory;
            terr.ContinuousAttachHandler_Add(_card.GuidStr, ContinuousAttach_Add);
            terr.OnNextPhase.Add(_card.GuidStr, OnNextPhase);
        }

        UniTask OnNextPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            if (terr.IsStartPhase())
                terr.ContinuousAttachHandler_Remove(_card.GuidStr, ContinuousAttach_Remove);
            return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            e.card.Traits.AdjustStacks(TRAIT_ID, 1, _card);
            return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            e.card.Traits.AdjustStacks(TRAIT_ID, -1, _card);
            return UniTask.CompletedTask;
        }
    }
}
