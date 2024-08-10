﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBecomeMachine : PassiveTrait
    {
        const string ID = "become_machine";
        const int PRIORITY = 4;
        const string OBS_CARD_ID = "connor";
        static readonly TraitStatFormula _strengthF = new(true, 0.50f, 0.00f);
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tBecomeMachine() : base(ID)
        {
            name = "Стать машиной";
            desc = "Я думал- Я думал, что...";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeAll, PRIORITY);
        }
        protected tBecomeMachine(tBecomeMachine other) : base(other) { }
        public override object Clone() => new tBecomeMachine(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(OBS_CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При появлении карты <i>{cardName}</i> на вражеской стороне (П{PRIORITY})",
                    $"Уменьшает свою силу на {_strengthF.Format(trait)} и инициативу на {_moxieF.Format(trait)}."),
            });
        }
        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            BattleFieldCard owner = trait.Owner;
            string guid = trait.GuidGen(e.target.Guid);

            if (e.target.Data.id != OBS_CARD_ID) return;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(e.target);
                await owner.Strength.AdjustValueScale(-_strengthF.Value(trait), trait, guid);
                await owner.Moxie.AdjustValue(-_moxieF.Value(trait), trait, guid);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(e.target);
                await owner.Strength.RevertValueScale(guid);
                await owner.Moxie.RevertValue(guid);
            }
        }
    }
}
