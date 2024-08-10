using Cysharp.Threading.Tasks;
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
        const int PRIORITY = 7;
        static readonly TraitStatFormula _healthF = new(true, 0, 0.33f);
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

        public tTillDawn() : base(ID)
        {
            name = "Жить до рассвета";
            desc = "Как долго ты будешь продолжать эту игру, Джошуа?";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tTillDawn(tTillDawn other) : base(other) { }
        public override object Clone() => new tTillDawn(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале следующего хода (П{PRIORITY})",
                    $"Восстанавливает {_healthF.Format(trait)} здоровья и {_moxieF.Format(trait)} инициативы. Тратит все заряды."),
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
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;

            await trait.AnimActivation();
            await trait.SetStacks(0, trait);
            await owner.Health.AdjustValue(owner.Data.health * _healthF.Value(trait), trait);
            await owner.Moxie.AdjustValue(_moxieF.Value(trait), trait);
        }
    }
}
