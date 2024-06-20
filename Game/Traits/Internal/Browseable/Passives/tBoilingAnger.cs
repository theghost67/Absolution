﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых трейтов.
    /// </summary>
    public class tBoilingAnger : PassiveTrait
    {
        const string ID = "boiling_anger";
        const int PRIORITY = 5;
        const float STRENGTH_REL_INCREASE = 0.25f;

        public tBoilingAnger() : base(ID)
        {
            name = "Кипящая злость";
            desc = "Кажется, с каждой секундой ей становится только хуже.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tBoilingAnger(tBoilingAnger other) : base(other) { }
        public override object Clone() => new tBoilingAnger(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = STRENGTH_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале хода (П{PRIORITY}/Т)",
                    $"увеличивает силу владельца на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 4 * Mathf.Pow(stacks, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;
            if (trait.WasAdded(e))
                trait.Owner.Territory.OnStartPhase.Add(OnTerritoryStartPhase, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnStartPhase.Remove(OnTerritoryStartPhase);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;

            await trait.AnimActivation();
            await trait.Owner.strength.AdjustValueRel(STRENGTH_REL_INCREASE * trait.GetStacks(), trait);
        }
    }
}