﻿using Cysharp.Threading.Tasks;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;

namespace Game.Cards
{
    public class cVavulization : FloatCard
    {
        const string CARD_ID = "vavulov";

        public cVavulization() : base("vavulization")
        {
            name = "Вавулизация";
            desc = "Глобальная Вавулизация населения, которую однажды желал инициировать Мошев для поддержания " +
                   "умеренно-деградантского интеллектуального уровня населения и его последующего порабощения.";

            rarity = Rarity.Epic;
            price = new CardPrice(CardBrowser.GetCurrency("gold"), 5);
        }
        protected cVavulization(cVavulization other) : base(other) { }
        public override object Clone() => new cVavulization(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"Для активации требуется карта {cardName} на территории стороны-владельца. " +
                   $"Превращает всех противников в карту <nobr><u>{cardName}</u><nobr> (без навыков). " +
                   $"Карта может защититься от превращения, если только её нельзя убить.";
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new CardDescriptiveArgs(CARD_ID) { linkFormat = true, linkStats = CardDescriptiveArgs.normalStats } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle && ((BattleFloatCard)e.card).Side.Fields().WithCard().Select(f => f.Card).Any(c => c.Data.id == CARD_ID);
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleTerritory territory = (BattleTerritory)e.territory;
            IEnumerable<BattleField> fields = card.Side.Opposite.Fields().WithCard();

            foreach (BattleField field in fields)
            {
                BattleFieldCard fieldCard = field.Card;
                await fieldCard.TryKill(BattleKillMode.IgnoreHealthRestore, card);
                if (!fieldCard.IsKilled) continue;
                FieldCard cardData = CardBrowser.NewField(CARD_ID);
                cardData.traits.Clear();
                await territory.PlaceFieldCard(cardData, field, card);
            }
        }
    }
}
