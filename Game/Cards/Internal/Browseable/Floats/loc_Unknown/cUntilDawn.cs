using Cysharp.Threading.Tasks;
using Game.Territories;
using GreenOne;
using System;

namespace Game.Cards
{
    public class cUntilDawn : FloatCard
    {
        const string ID = "until_dawn";
        const string TRAIT_ID = "till_dawn";
        const float HEALTH_RESTORE_REL = 0.33f;
        const int MOXIE_INCREASE_ABS = 2;
        BattleFloatCard _card;

        public cUntilDawn() : base(ID)
        {
            name = "Дожить до рассвета";
            desc = "Итак... ты хочешь приступить к этой \"игре\"?";

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
            frequency = 1.00f;
        }
        protected cUntilDawn(cUntilDawn other) : base(other) { }
        public override object Clone() => new cUntilDawn(this);

        public override string DescRich(ITableCard card)
        {
            float healthEffect = HEALTH_RESTORE_REL * 100f;
            int moxieEffect = MOXIE_INCREASE_ABS;
            return DescRichBase(card, $"В начале следующего хода, все карты на территории, которые пережили прошлый ход, " +
                                      $"восстанавливают {healthEffect.ToSignedString()}% здоровья и получают {moxieEffect.ToSignedString()} ед. инициативы.");
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
