using Cysharp.Threading.Tasks;
using Game.Effects;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Cards
{
    public class cComeForTea : FloatCard
    {
        const string ID = "come_for_tea";

        public cComeForTea() : base(ID)
        {
            name = Translator.GetString("card_come_for_tea_1");
            desc = Translator.GetString("card_come_for_tea_2");

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 5);
        }
        protected cComeForTea(cComeForTea other) : base(other) { }
        public override object Clone() => new cComeForTea(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return Translator.GetString("card_come_for_tea_3");
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            BattleField[] enemyfields = card.Side.Opposite.Fields().ToArray();
            BattleField[] myfields = card.Side.Fields().ToArray();
            int damage = (int)Mathf.Ceil(card.Side.HealthAtStart * 0.5f);

            // TODO: implement anim (from left to right)
            foreach (BattleField field in enemyfields)
            {
                if (field.Card == null) continue;
                field.Card.Drawer.CreateTextAsDamage(damage, false);
                await field.Card.Health.AdjustValue(-damage, card);
            }

            // TODO: implement anim (from right to left)
            foreach (BattleField field in myfields)
            {
                if (field.Card == null) continue;
                field.Card.Drawer.CreateTextAsDamage(damage, false);
                await field.Card.Health.AdjustValue(-damage, card);
            }
        }
    }
}
