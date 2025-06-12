using MyBox;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех существующих данных карт в исходном состоянии.
    /// </summary>
    public static class CardBrowser
    {
        public static IReadOnlyDictionary<string, CardCurrency> CurrenciesIndexed => _currencies;
        public static IReadOnlyDictionary<string, FieldCard> FieldsIndexed => _fields;
        public static IReadOnlyDictionary<string, FloatCard> FloatsIndexed => _floats;
        public static IReadOnlyDictionary<string, Card> AllIndexed => _all;

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
            AddCurrency(new ccGold());
            AddCurrency(new ccEther());



            AddField(new cBarbarian());
            AddField(new cBread());
            AddField(new cCanteen());
            AddField(new cCyberCutlet());
            AddField(new cGavenko());

            AddField(new cGermanSausages());
            AddField(new cGranny());
            AddField(new cHysteric());
            AddField(new cMichaelKgk());
            AddField(new cMoshev());

            AddField(new cPigeon());
            AddField(new cPigeonLitter());
            AddField(new cPrincipalsOffice());
            AddField(new cRussian());
            AddField(new cStudent());

            AddField(new cVavulov());
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
            AddField(new c626());
            AddField(new cAlex());

            AddField(new cBatman());
            AddField(new cBerlin());
            AddField(new cBmo());
            AddField(new cBook());
            AddField(new cBrick());

            AddField(new cCadence());
            AddField(new cCaptain());
            AddField(new cCardsharper());
            AddField(new cCarter());
            AddField(new cCat());

            AddField(new cChief());
            AddField(new cCook());
            AddField(new cCreeper());
            AddField(new cCultist());
            AddField(new cDeath());

            AddField(new cEntrepreneur());
            AddField(new cFaith());
            AddField(new cFinn());
            AddField(new cGazikel());
            AddField(new cGhost());

            AddField(new cGhostrunner());
            AddField(new cHacker());
            AddField(new cHouse());
            AddField(new cHouseWilson());
            AddField(new cIncredible());

            AddField(new cInvestor());
            AddField(new cInvisible());
            AddField(new cJake());
            AddField(new cJoy());
            AddField(new cLibrarian());

            AddField(new cLoba());
            AddField(new cMaxwell());
            AddField(new cMaxwellClone());
            AddField(new cMichaelSanta());
            AddField(new cMichaelScott());

            AddField(new cMilitary());
            AddField(new cMongol());
            AddField(new cOppressor());
            AddField(new cOverseer());
            AddField(new cPlague());

            AddField(new cProfessor());
            AddField(new cQueen());
            AddField(new cRandy());
            AddField(new cRein());
            AddField(new cSalad());

            AddField(new cSans());
            AddField(new cScpGarage());
            AddField(new cSenorita());
            AddField(new cShadowheart());
            AddField(new cSonic());

            AddField(new cSpiderMan());
            AddField(new cStanley());
            AddField(new cWilson());
            AddField(new cTerminator());
            AddField(new cTinyBunny());

            AddField(new cWardrobe());
            AddField(new cMarket());



            AddFloat(new cDeleteDueToUselessness());
            AddFloat(new cKalenskiyProtocol());
            AddFloat(new cKotovsSyndrome());
            AddFloat(new cVavulization());
            AddFloat(new cCodeInterception());

            AddFloat(new cSatelliteSurveillance());
            AddFloat(new cObjectOfObsession());
            AddFloat(new cUntilDawn());
            AddFloat(new cBlackFriday());
            AddFloat(new cCassandrasRage());

            AddFloat(new cComeForTea());
            AddFloat(new cFennecDefence());
            AddFloat(new cNoed());
            AddFloat(new cShockTherapy());
        }

        public static FieldCard NewField(string id)
        {
            return (FieldCard)GetField(id).CloneAsNew();
        }
        public static FloatCard NewFloat(string id)
        {
            return (FloatCard)GetFloat(id).CloneAsNew();
        }
        public static Card NewCard(string id)
        {
            return (Card)GetCard(id).CloneAsNew();
        }

        public static FieldCard NewFieldRandom()
        {
            return (FieldCard)_fields.Values.GetWeightedRandom(c => c.frequency).CloneAsNew();
        }
        public static FloatCard NewFloatRandom()
        {
            return (FloatCard)_floats.Values.GetWeightedRandom(c => c.frequency).CloneAsNew();
        }
        public static Card NewCardRandom()
        {
            return (Card)_all.Values.GetWeightedRandom(c => c.frequency).CloneAsNew();
        }

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

        public static CardCurrency GetCurrencyRandom()
        {
            return _currencies.Values.GetRandom();
        }
        public static FieldCard GetFieldRandom()
        {
            return _fields.Values.GetWeightedRandom(c => c.frequency);
        }
        public static FloatCard GetFloatRandom()
        {
            return _floats.Values.GetWeightedRandom(c => c.frequency);
        }
        public static Card GetCardRandom()
        {
            return _all.Values.GetWeightedRandom(c => c.frequency);
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
            FieldCardUpgradeRules rules = new(points - card.Points(), FieldCardUpgradeRules.TraitsCountRandom(points));
            rules.Upgrade(card);
            return card;
        }
        public static FieldCard UpgradeWithoutTraitAdd(this FieldCard card, in float points)
        {
            FieldCardUpgradeRules rules = new(points - card.Points(), 0);
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
