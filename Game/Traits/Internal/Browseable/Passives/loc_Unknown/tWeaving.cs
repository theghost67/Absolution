using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tWeaving : PassiveTrait
    {
        const string ID = "weaving";
        const string CARD_ID = "spider";
        const int CARD_PRICE = 0;
        const int CARD_MOXIE = 3;
        const int PRIORITY = 3;
        static readonly TraitStatFormula _statsF = new(false, 0, 1);

        public tWeaving() : base(ID)
        {
            name = "Пряжение";
            desc = "Со временем коконы подарили мне ещё больше друзей.";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tWeaving(tWeaving other) : base(other) { }
        public override object Clone() => new tWeaving(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            string formula = _statsF.Format(trait);
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале каждого хода, пока владелец жив (П{PRIORITY})",
                    $"Выдаёт карту <i>{cardName}</i>. Карта будет иметь {CARD_PRICE} ед. стоимости, {CARD_MOXIE} ед. инициативы, <u>{formula}</u> здоровья и <u>{formula}</u> силы."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsLinear(12, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;
            await trait.AnimActivation();

            FieldCard newCard = CardBrowser.NewField(CARD_ID);
            int formula = (int)_statsF.Value(trait);

            newCard.price = new CardPrice(CardBrowser.GetCurrency("gold"), CARD_PRICE);
            newCard.moxie = CARD_MOXIE;
            newCard.health = formula;
            newCard.strength = formula;

            if (!trait.Side.Sleeve.Add(newCard))
                owner.Drawer?.CreateTextAsSpeech("ПОЛНАЯ РУКА", UnityEngine.Color.red);
        }
    }
}
