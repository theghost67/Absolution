﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tReporting : ActiveTrait
    {
        const string ID = "reporting";
        const string CARD_ID = "clues";
        const int STACKS_DECREASE = 1;

        public tReporting() : base(ID)
        {
            name = "Составление рапорта";
            desc = "Да у него непробиваемый слой защиты! Что это может быть?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeAll);
        }
        protected tReporting(tReporting other) : base(other) { }
        public override object Clone() => new tReporting(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на любой вражеской карте",
                    $"Устанавливает карту <i>{cardName}</i> на поле напротив цели, если поле напротив свободно. Тратит {STACKS_DECREASE} ед. зарядов."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 20 * Mathf.Pow(stacks - 1, 2);
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Opposite.Card == null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            BattleActiveTrait trait = (BattleActiveTrait)e.trait;
            BattleField newCardField = (BattleField)e.target.Opposite;
            FieldCard newCard = CardBrowser.NewField(CARD_ID);

            await trait.Territory.PlaceFieldCard(newCard, newCardField, trait);
            await trait.AdjustStacks(-STACKS_DECREASE, trait.Side);
        }
    }
}