﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTillDawn : PassiveTrait
    {
        const string ID = "till_dawn";
        static readonly TraitStatFormula _healthF = new(true, 0, 0.33f);
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

        public tTillDawn() : base(ID)
        {
            name = "Жить до рассвета";
            desc = "Как долго ты будешь продолжать эту игру, Джошуа?";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tTillDawn(tTillDawn other) : base(other) { }
        public override object Clone() => new tTillDawn(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>В начале следующего хода на территории</color>\n" +
                   $"Восстанавливает {_healthF.Format(args.stacks)} здоровья и {_moxieF.Format(args.stacks)} инициативы. Тратит все заряды.";
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            await owner.Health.AdjustValue(owner.Data.health * _healthF.Value(stacks), trait);
            await owner.Moxie.AdjustValue(_moxieF.Value(stacks), trait);
            await trait.SetStacks(0, trait);
        }
    }
}
