using Cysharp.Threading.Tasks;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Cards
{
    public class cKalenskiyProtocol : FloatCard
    {
        const float MOXIE_TO_STRENGTH_REL = 0.2f;

        public cKalenskiyProtocol() : base("kalenskiy_protocol")
        {
            name = "Протокол Каленский";
            desc = "Можно ли обернуть коллизию себе на пользу?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1f;
        }
        protected cKalenskiyProtocol(cKalenskiyProtocol other) : base(other) { }
        public override object Clone() => new cKalenskiyProtocol(this);

        public override string DescRich(ITableCard card)
        {
            const float EFFECT = MOXIE_TO_STRENGTH_REL * 100;
            return DescRichBase(card, $"Переносит инициативу всех карт на своей территории в их силу: -1 ед. инициативы в +{EFFECT}% силы");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            await base.OnUse(e);

            BattleFloatCard card = (BattleFloatCard)e.card;
            IEnumerable<BattleField> fields = card.Side.Fields().WithCard();

            foreach (BattleField field in fields)
            {
                BattleFieldCard fieldCard = field.Card;
                string guid = Unique.NewGuidStr;

                await fieldCard.moxie.AdjustValueAbs(-fieldCard.moxie, card, guid);
                float moxieDelta = fieldCard.moxie.EntryValueAbs(guid);
                float strengthRel = -moxieDelta * MOXIE_TO_STRENGTH_REL;
                await fieldCard.strength.AdjustValueAbs(strengthRel, card);
            }
        }
    }
}
