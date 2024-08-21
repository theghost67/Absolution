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
            // > --------- CURRENCIES --------- <
            AddCurrency(new ccGold());
            AddCurrency(new ccEther());

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

            /* --------------------------------- //
            ||            LOC: BUREAU            ||
            // --------------------------------- */

            AddField(new cAgent007());
            AddField(new cAgentOfBeholder());
            AddField(new cAnderson()); 
            AddField(new cArchivist());
            AddField(new cCarl());

            AddField(new cClues());
            AddField(new cCodeLoaf());
            AddField(new cConnor());
            AddField(new cGeneralP());
            AddField(new cHarry());

            AddField(new cHitman());
            AddField(new cNorman());
            AddField(new cOrigami());
            AddField(new cShelbi());
            AddField(new cVanga());

            AddField(new cVinsent());

            AddFloat(new cCodeInterception());
            AddFloat(new cSatelliteSurveillance());

            /* --------------------------------- //
            ||           LOC: MEGAPOLIS          ||
            // --------------------------------- */

            //AddField(new cChief());
            //AddField(new cOguzok());
            //AddField(new cClaudeMonet());
            //AddField(new cViktorovich());
            //AddField(new cPeter());

            //AddField(new cJeweller());
            //AddField(new cSecretary());
            //AddField(new cHacker());
            //AddField(new cBusinessman());
            //AddField(new cMayor());

            //AddField(new cInvestor());
            //AddField(new cConsultant());
            //AddField(new cBoris());
            //AddField(new cHobo());
            //AddField(new cIncredible());

            //===================================//
            //     WITH NO SPECIFIC LOCATION     //
            //===================================//

            AddField(new cCrap());
            AddField(new cCrapper());
            AddField(new cDallas());
            AddField(new cDj());
            AddField(new cDoof());

            AddField(new cFengMine());
            AddField(new cKillerOfFun());
            AddField(new cMegTomat());
            AddField(new cMercy());
            AddField(new cMine());

            AddField(new cMyers());
            AddField(new cMyersMirror());
            AddField(new cPhantom());
            AddField(new cSatanist());
            AddField(new cScp106());

            AddField(new cScp173());
            AddField(new cSpider());
            AddField(new cSpiderling());
            AddField(new cSpiderCocon());
            AddField(new cTerrorist());

            AddField(new cWidow());

            AddFloat(new cObjectOfObsession());
            AddFloat(new cUntilDawn());
        }

        public static CardCurrency NewCurrency(string id)
        {
            throw new System.NotSupportedException($"Card currency creation is not supported.\nUse {nameof(GetCurrency)} instead.");
        }
        public static FieldCard NewField(string id) => (FieldCard)GetField(id).CloneAsNew();
        public static FloatCard NewFloat(string id) => (FloatCard)GetFloat(id).CloneAsNew();
        public static Card NewCard(string id) => (Card)GetCard(id).CloneAsNew();

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
            if (PlayerConfig.shufflePrice)
                card.price.value = Random.Range(0, 6);
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

        public static FieldCard UpgradeWithTraitAdd(this FieldCard card, in float points, in int traitsCount)
        {
            FieldCardUpgradeRules rules = new(points - card.Points(), traitsCount);
            rules.Upgrade(card);
            return card;
        }
        public static FieldCard UpgradeWithTraitAdd(this FieldCard card, in float points)
        {
            FieldCardUpgradeRules rules = new(points - card.Points(), true);
            rules.Upgrade(card);
            return card;
        }
        public static FieldCard UpgradeWithoutTraitAdd(this FieldCard card, in float points)
        {
            FieldCardUpgradeRules rules = new(points - card.Points(), false);
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
