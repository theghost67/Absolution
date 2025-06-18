using Cysharp.Threading.Tasks;
using Game.Effects;
using Game.Territories;
using Game.Traits;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cards
{
    public class cDeleteDueToUselessness : FloatCard
    {
        public cDeleteDueToUselessness() : base("delete_due_to_uselessness")
        {
            name = Translator.GetString("card_delete_due_to_uselessness_1");
            desc = Translator.GetString("card_delete_due_to_uselessness_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cDeleteDueToUselessness(cDeleteDueToUselessness other) : base(other) { }
        public override object Clone() => new cDeleteDueToUselessness(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return Translator.GetString("card_delete_due_to_uselessness_3");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            IEnumerable<BattleField> fields = territory.Fields().WithCard();

            int killedCardsCount = 0;
            foreach (BattleField field in fields)
            {
                BattleFieldCard fieldCard = field.Card;
                if (fieldCard.Price > 0 && fieldCard.Strength > 0)
                    continue;

                await fieldCard.TryKill(BattleKillMode.Default, card);
                if (!fieldCard.IsKilled) continue;

                killedCardsCount++;
                fieldCard.Drawer.CreateTextAsSpeech(Translator.GetString("card_delete_due_to_uselessness_4"), Color.red);
            }
            await card.Side.Gold.AdjustValue(killedCardsCount, card);
        }
    }
}
