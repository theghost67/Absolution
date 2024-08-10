﻿using Cysharp.Threading.Tasks;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfAttackWait : PassiveTrait
    {
        const string ID = "order_of_attack_wait";
        const int PRIORITY = 4;

        public tOrderOfAttackWait() : base(ID)
        {
            name = "Приказ об атаке (ждёт)";
            desc = "Приказ получен, выдвигаюсь на позицию.";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tOrderOfAttackWait(tOrderOfAttackWait other) : base(other) { }
        public override object Clone() => new tOrderOfAttackWait(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале следующего хода (П{PRIORITY})",
                    $"Совершит свою атаку <u>{effect}</u> раз(-а) подряд. Тратит все заряды."),
            });
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
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();

            BattleInitiationSendArgs[] initiations = new BattleInitiationSendArgs[stacks];
            for (int i = 0; i < stacks; i++)
                initiations[i] = trait.Owner.CreateInitiation();

            territory.Initiations.EnqueueAndRun(initiations);
            await trait.SetStacks(0, trait);
        }
    }
}
