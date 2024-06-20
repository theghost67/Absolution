using Cysharp.Threading.Tasks;
using Game.Territories;
using System.Collections.Generic;

namespace Game.Cards
{
    public class cVavulization : FloatCard
    {
        public cVavulization() : base("vavulization")
        {
            name = "Вавулизация";
            desc = "Глобальная Вавулизация населения, которую однажды желал инициировать Мошев для поддержания " +
                   "умеренно-деградантского интеллектуального уровня населения и его последующего порабощения.";

            rarity = Rarity.None;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 4);
            frequency = 1f;
        }
        protected cVavulization(cVavulization other) : base(other) { }
        public override object Clone() => new cVavulization(this);

        public override string DescRich(ITableCard card)
        {
            return DescRichBase(card, "Превращает всех противников в карту <i>Вавулов</i>. Карта может защититься от превращения, если только её нельзя убить.");
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
            IEnumerable<BattleField> fields = card.Side.Opposite.Fields().WithCard();

            foreach (BattleField field in fields)
            {
                BattleFieldCard fieldCard = field.Card;
                await fieldCard.Kill(source: null, BattleKillMode.IgnoreHealthRestore);
                if (fieldCard.IsKilled)
                    await territory.PlaceFieldCard(CardBrowser.NewField("vavulov"), field, card);
            }
        }
    }
}
