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
            name = Translator.GetString("card_kalenskiy_protocol_1");
            desc = Translator.GetString("card_kalenskiy_protocol_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 1);
        }
        protected cKalenskiyProtocol(cKalenskiyProtocol other) : base(other) { }
        public override object Clone() => new cKalenskiyProtocol(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return Translator.GetString("card_kalenskiy_protocol_3", MOXIE_TO_STRENGTH_REL * 100, MAX_MOXIE_TO_STRENGTH);

        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            

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
