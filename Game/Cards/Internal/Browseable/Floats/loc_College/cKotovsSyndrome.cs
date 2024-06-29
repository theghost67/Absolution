using Cysharp.Threading.Tasks;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Cards
{
    public class cKotovsSyndrome : FloatCard
    {
        const string ID = "kotovs_syndrome";
        const int MOXIE_DECREASE = 5;

        public cKotovsSyndrome() : base(ID)
        {
            name = "Синдром Котова";
            desc = "Психологическое расстройство, при котором появляется желание оскорблять и непристойно шутить тогда, когда это совсем не к месту.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
            frequency = 1f;
        }
        protected cKotovsSyndrome(cKotovsSyndrome other) : base(other) { }
        public override object Clone() => new cKotovsSyndrome(this);

        public override string DescRich(ITableCard card)
        {
            return DescRichBase(card, $"Уменьшает инициативу всех противников на {MOXIE_DECREASE} ед.");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            await base.OnUse(e);

            BattleFloatCard card = (BattleFloatCard)e.card;
            IEnumerable<BattleField> fields = card.Side.Opposite.Fields().WithCard();

            foreach (BattleField field in fields)
                await field.Card.moxie.AdjustValue(-MOXIE_DECREASE, card);
        }
    }
}
