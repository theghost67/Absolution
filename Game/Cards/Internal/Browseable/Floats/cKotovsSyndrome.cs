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
            name = Translator.GetString("card_kotovs_syndrome_1");
            desc = Translator.GetString("card_kotovs_syndrome_2");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cKotovsSyndrome(cKotovsSyndrome other) : base(other) { }
        public override object Clone() => new cKotovsSyndrome(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return Translator.GetString("card_kotovs_syndrome_3", MOXIE_DECREASE);
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            

            BattleFloatCard card = (BattleFloatCard)e.card;
            IEnumerable<BattleField> fields = card.Side.Opposite.Fields().WithCard();

            foreach (BattleField field in fields)
                await field.Card.Moxie.AdjustValue(-MOXIE_DECREASE, card);
        }
    }
}
