using Cysharp.Threading.Tasks;
using Game.Effects;
using Game.Territories;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cards
{
    public class cDeleteDueToUselessness : FloatCard
    {
        public cDeleteDueToUselessness() : base("delete_due_to_uselessness")
        {
            name = "Удалить из-за ненадобности";
            desc = "Описание удалили из-за ненадобности.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
            frequency = 1f;
        }
        protected cDeleteDueToUselessness(cDeleteDueToUselessness other) : base(other) { }
        public override object Clone() => new cDeleteDueToUselessness(this);

        public override string DescRich(ITableCard card)
        {
            return DescRichBase(card, "Убивает все карты со стоимостью ≤ 0 ед. или атакой ≤ 0 ед., возвращая по 1 ед. золота за каждую убитую карту владельцу.");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            await base.OnUse(e);

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            IEnumerable<BattleField> fields = territory.Fields().WithCard();

            int killedCardsCount = 0;
            foreach (BattleField field in fields)
            {
                BattleFieldCard fieldCard = field.Card;
                if (fieldCard.price > 0 && fieldCard.strength > 0)
                    continue;

                await fieldCard.Kill(card);
                if (!fieldCard.IsKilled) continue;

                killedCardsCount++;
                BattleFieldCardDrawer drawer = fieldCard.Drawer;
                drawer.CreateTextAsSpeech("НЕ НУЖЕН", Color.red);
            }
            await card.Side.gold.AdjustValueAbs(killedCardsCount, card);
        }
    }
}
