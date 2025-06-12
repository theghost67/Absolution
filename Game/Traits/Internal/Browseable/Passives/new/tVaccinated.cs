using Cysharp.Threading.Tasks;
using Game.Effects;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tVaccinated : PassiveTrait
    {
        const string ID = "vaccinated";
        private static TraitStatFormula _damageF = new(false, 0, 1);

        public tVaccinated() : base(ID)
        {
            name = "Вакцинирован";
            desc = "Ты вакцинирован, Макс.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tVaccinated(tVaccinated other) : base(other) { }
        public override object Clone() => new tVaccinated(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>В начале хода на территории</color>\nНаносит {_damageF.Format(args.stacks)} урона владельцу, тратит все заряды.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnStartPhase.Remove(trait.GuidStr);
        }
        private async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null) return;

            int damage = _damageF.ValueInt(trait.GetStacks());
            trait.Owner.Drawer.CreateTextAsSpeech($"{name}\n<size=50%>-{damage}", Color.red);
            await trait.Owner.Health.AdjustValue(-damage, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
