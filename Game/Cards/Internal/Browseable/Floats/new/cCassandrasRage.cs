using Cysharp.Threading.Tasks;
using Game.Territories;
using System.Linq;

namespace Game.Cards
{
    public class cCassandrasRage : FloatCard
    {
        const string ID = "cassandras_rage";

        public cCassandrasRage() : base(ID)
        {
            name = "Кассандра в ярости";
            desc = $"Как я вижу, твоя колония пребывает в безмятежности? Да, приятное зрелище. Даже слишком. Пара ядерных ударов это исправит.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 3);
        }
        protected cCassandrasRage(cCassandrasRage other) : base(other) { }
        public override object Clone() => new cCassandrasRage(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            return $"Понижает здоровье и силу всех вражеских карт на 100%.";
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            BattleFieldCard[] cards = card.Side.Opposite.Fields().WithCard().Select(f => f.Card).ToArray();

            foreach (BattleFieldCard c in cards)
            {
                await c.Health.AdjustValueScale(-1, card);
                if (c.IsKilled) continue;
                await c.Strength.AdjustValueScale(-1, card);
            }
        }
    }
}
