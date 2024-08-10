﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTactician : PassiveTrait
    {
        const string ID = "tactician";
        const int PRIORITY = 4;
        static readonly TraitStatFormula _moxieF = new(false, 0, 1);

        public tTactician() : base(ID)
        {
            name = "Тактик";
            desc = "Мастерски владеет тактическими приёмами.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tTactician(tTactician other) : base(other) { }
        public override object Clone() => new tTactician(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале хода на территории (П{PRIORITY})",
                    $"увеличивает инициативу владельца на {_moxieF.Format(trait)}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(32, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.WasAdded(e))
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;

            await trait.AnimActivation();
            await trait.Owner.Moxie.AdjustValue(_moxieF.Value(trait), trait);
        }
    }
}
