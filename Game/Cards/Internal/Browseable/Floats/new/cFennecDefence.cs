using Cysharp.Threading.Tasks;
using Game.Territories;
using Game.Traits;
using System.Linq;
using UnityEngine;

namespace Game.Cards
{
    public class cFennecDefence : FloatCard
    {
        const string ID = "fennec_defence";
        const string TRAIT_ID = "fennec_soul";

        public cFennecDefence() : base(ID)
        {
            name = Translator.GetString("card_fennec_defence_1");
            desc = Translator.GetString("card_fennec_defence_2");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 1);
        }
        protected cFennecDefence(cFennecDefence other) : base(other) { }
        public override object Clone() => new cFennecDefence(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("card_fennec_defence_3", traitName);
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
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
            BattleFieldCard[] cards = side.Fields().WithCard().Select(f => f.Card).ToArray();
            int health = (int)Mathf.Ceil(side.HealthAtStart * 0.33f);
            await side.Health.AdjustValue(health, card);
            foreach (BattleFieldCard c in cards)
                await c.Traits.Passives.AdjustStacks(TRAIT_ID, 1, card);
        }
    }
}
