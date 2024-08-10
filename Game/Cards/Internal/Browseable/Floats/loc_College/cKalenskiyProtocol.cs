using Cysharp.Threading.Tasks;
using Game.Territories;
using GreenOne;
using System.Collections.Generic;

namespace Game.Cards
{
    public class cKalenskiyProtocol : FloatCard
    {
        const float MOXIE_TO_STRENGTH_REL = 0.20f;
        const int MAX_MOXIE_TO_STRENGTH = 5;

        public cKalenskiyProtocol() : base("kalenskiy_protocol")
        {
            name = "Протокол Каленский";
            desc = "Можно ли обернуть коллизию себе на пользу?";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 2);
            frequency = 1.00f;
        }
        protected cKalenskiyProtocol(cKalenskiyProtocol other) : base(other) { }
        public override object Clone() => new cKalenskiyProtocol(this);

        public override string DescRich(ITableCard card)
        {
            return DescRichBase(card, $"Переносит инициативу всех карт на своей территории в их силу: -1 ед. инициативы в +{MOXIE_TO_STRENGTH_REL * 100}% силы. " +
                                      $"Перенос более {MAX_MOXIE_TO_STRENGTH} инициативы не даёт бонус к силе.");
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

                await fieldCard.Moxie.AdjustValue(-fieldCard.Moxie, card, guid);
                float moxieDelta = fieldCard.Moxie.EntryValue(guid);
                float strengthRel = (-moxieDelta).ClampedMax(MAX_MOXIE_TO_STRENGTH) * MOXIE_TO_STRENGTH_REL;
                await fieldCard.Strength.AdjustValueScale(strengthRel, card);
            }
        }
    }
}
