//#define SHUFFLE_PRICE

using System.Collections.Generic;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех существующих данных карт в исходном состоянии.
    /// </summary>
    public static class CardBrowser
    {
        public static IReadOnlyCollection<CardCurrency> Currencies => _currencies.Values;
        public static IReadOnlyCollection<FieldCard> Fields => _fields.Values;
        public static IReadOnlyCollection<FloatCard> Floats => _floats.Values;
        public static IReadOnlyCollection<Card> All => _all.Values;

        static readonly Dictionary<string, CardCurrency> _currencies = new();
        static readonly Dictionary<string, FieldCard> _fields = new();
        static readonly Dictionary<string, FloatCard> _floats = new();
        static readonly Dictionary<string, Card> _all = new();

        public static void Initialize()
        {
            #region (remove) NOTES
            //new FloatCard("work")
            //{
            //    name   = "Работаем, работаем",
            //    alias  = "Мотивационная речь",
            //    advice = "Прибавляет к текущему здоровью союзных карт их здоровье по умолчанию.",
            //    desc   = "Эта пламенная речь, придуманная самим Гавенко, может замотивировать даже мёртвого воскреснуть на некоторое время, не говоря уже о живых. " +
            //             "Он произносит её ежедневно в своём офисе, чтобы сотрудники не забывали, что нужно работать, работать усерднее и эффективнее. " +
            //             "Не произношение этой фразы может вызвать смерть сотрудников, так как им уже давным давно было пора отправиться на тот свет."

            //    threat = 0,
            //    price = new CardPrice(CardPriceType.Gold, 2),

            //    onUsed = card =>
            //    {
            //        if (card is not IBattleSideCard sideCard)
            //            throw new InvalidOperationException();

            //        foreach (var field in sideCard.Side.fields.Where(f => f.Card != null))
            //            field.Card.HealthCurrent += field.Card.Health;
            //    }
            //};

            //new FloatCard("allah_akbar")
            //{
            //    name = "Аллах акбар",
            //    origin = new CardOrigin
            //    (
            //        alias: "Исповедь разрушения",
            //        advice: "Убивает все карты на территории. Обращаться с осторожностью.",
            //        desc: "Имя Ибрагим вам о чём-нибудь говорит? " +
            //        "Если нет, то лучше не подходите к этой женщине, иначе рискуйте полностью дезинтригрироваться в пространстве за считанные секунды. " +
            //        "Одной своей фразой Кандибобер может взрывать целые города, что позволяет перевернуть ход множества войн, не говоря уже о простых поединках..."
            //    ),

            //    price = new CardPrice(CardPriceType.Gold, 0),
            //    canBeUsed = card => true,
            //    onUsed = card =>
            //    {
            //        if (card is not IBattleSideCard sideCard)
            //            throw new InvalidOperationException();

            //        foreach (var field in sideCard.Side.territory.GetAllFields().Where(f => f.Card != null))
            //            ((BattleCard)field.Card).Kill(null);
            //    }
            //};
            #endregion

            // > --------- CURRENCIES --------- <
            AddCurrency(new ccGold());
            AddCurrency(new ccEther());
            // > ------------------------------ <

            /* --------------------------------- //
            ||            LOC: COLLEGE           ||
            // --------------------------------- */

            AddField(new cBarbarian());
            AddField(new cBread());
            AddField(new cCanteen());
            AddField(new cCyberCutlet());
            AddField(new cGavenko());

            AddField(new cGermanSausages());
            AddField(new cGranny());
            AddField(new cHysteric());
            AddField(new cMichael());
            AddField(new cMoshev());

            AddField(new cPigeon());
            AddField(new cPigeonLitter());
            AddField(new cPrincipalsOffice());
            AddField(new cRussian());
            AddField(new cStudent());

            AddField(new cVavulov());

            AddFloat(new cDeleteDueToUselessness());
            AddFloat(new cKalenskiyProtocol());
            AddFloat(new cKotovsSyndrome());
            AddFloat(new cVavulization());
            return;

            /* --------------------------------- //
            ||            LOC: BUREAU            ||
            // --------------------------------- */

            AddField(new cHitman());
            AddField(new cVanga());
            AddField(new cAnderson());
            AddField(new cConnor());
            AddField(new cVinsent());

            AddField(new c007());
            AddField(new cBeholder());
            AddField(new cNorman());
            AddField(new cShelbi());
            AddField(new cOrigami());

            AddField(new cHarry());
            AddField(new cClues());
            AddField(new cCarl());
            AddField(new cArchivist());
            AddField(new cGeneral());

            /* --------------------------------- //
            ||           LOC: MEGAPOLIS          ||
            // --------------------------------- */

            AddField(new cChief());
            AddField(new cOguzok());
            AddField(new cClaudeMonet());
            AddField(new cViktorovich());
            AddField(new cPeter());

            AddField(new cJeweller());
            AddField(new cSecretary());
            AddField(new cHacker());
            AddField(new cBusinessman());
            AddField(new cMayor());

            AddField(new cInvestor());
            AddField(new cConsultant());
            AddField(new cBoris());
            AddField(new cHobo());
            AddField(new cIncredible());

            //===================================//
            //     WITH NO SPECIFIC LOCATION     //
            //===================================//

            AddField(new cDoof());
            AddField(new cDj());
            AddField(new cMyers());
            AddField(new cMyersMirror());

            AddField(new cSpiderling());
            AddField(new cSpiderlingCocon());
            AddField(new cCrapper());
            AddField(new cCrap());
            AddField(new cSatanist());

            AddField(new cScp173());
            AddField(new cScp106());
            AddField(new cTerrorist());
            AddField(new cOtzdarva());
            AddField(new cFunKiller());

            AddField(new cPhantom());
            AddField(new cWidow());
        }

        public static CardCurrency NewCurrency(string id)
        {
            throw new System.NotSupportedException($"Card currency creation is not supported.\nUse {nameof(GetCurrency)} instead.");
        }
        public static FieldCard NewField(string id) => (FieldCard)GetField(id).Clone();
        public static FloatCard NewFloat(string id) => (FloatCard)GetFloat(id).Clone();
        public static Card NewCard(string id) => (Card)GetCard(id).Clone();

        public static CardCurrency GetCurrency(string id)
        {
            if (_currencies.TryGetValue(id, out CardCurrency currency))
                return currency;
            else throw new System.NullReferenceException($"Card currency with specified id was not found: {id}.");
        }
        public static FieldCard GetField(string id)
        {
            if (_fields.TryGetValue(id, out FieldCard card))
                return card;
            else throw new System.NullReferenceException($"Field card with specified id was not found: {id}.");
        }
        public static FloatCard GetFloat(string id)
        {
            if (_floats.TryGetValue(id, out FloatCard card))
                return card;
            else throw new System.NullReferenceException($"Float card with specified id was not found: {id}.");
        }
        public static Card GetCard(string id)
        {
            if (_all.TryGetValue(id, out Card card))
                return card;
            else throw new System.NullReferenceException($"Card with specified id was not found: {id}.");
        }

        public static FieldCard ShuffleMainStats(this FieldCard card)
        {
            #if SHUFFLE_PRICE
            card.price.value = Random.Range(0, 6);
            #endif
            card.moxie = Random.Range(0, 6);
            return card;
        }
        public static FieldCard ShuffleAllStats(this FieldCard card)
        {
            int sum = card.strength + card.health;
            int rand = Random.Range(0, sum);
            card.health = rand;
            card.strength = sum - rand;
            #if SHUFFLE_PRICE
            card.price.value = Random.Range(0, 6);
            #endif
            card.moxie = Random.Range(0, 6);
            return card;
        }

        public static FieldCard ResetMainStats(this FieldCard card)
        {
            card.moxie = 0;
            card.price.value = 0;
            return card;
        }
        public static FieldCard ResetAllStats(this FieldCard card, bool resetTraits = false)
        {
            card.health = 1;
            card.strength = 0;
            card.moxie = 0;
            card.price.value = 0;
            if (resetTraits)
                card.traits.Clear();
            return card;
        }

        public static FieldCard UpgradeWithTraitAdd(this FieldCard card, in float statPoints, in int traitsCount)
        {
            FieldCardUpgradeRules rules = new(statPoints - card.Points(), traitsCount);
            rules.Upgrade(card);
            return card;
        }
        public static FieldCard UpgradeWithTraitAdd(this FieldCard card, in float statPoints)
        {
            FieldCardUpgradeRules rules = new(statPoints - card.Points(), true);
            rules.Upgrade(card);
            return card;
        }
        public static FieldCard UpgradeWithoutTraitAdd(this FieldCard card, in float statPoints)
        {
            FieldCardUpgradeRules rules = new(statPoints - card.Points(), false);
            rules.Upgrade(card);
            return card;
        }

        static void AddCurrency(CardCurrency currency)
        {
            _currencies.Add(currency.id, currency);
        }
        static void AddField(FieldCard srcCard)
        {
            _fields.Add(srcCard.id, srcCard);
            _all.Add(srcCard.id, srcCard);
        }
        static void AddFloat(FloatCard srcCard)
        {
            _floats.Add(srcCard.id, srcCard);
            _all.Add(srcCard.id, srcCard);
        }
    }
}
