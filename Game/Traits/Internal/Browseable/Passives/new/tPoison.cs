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
    public class tPoison : PassiveTrait
    {
        const string ID = "poison";
        private static TraitStatFormula _damageF = new(false, 0, 1);

        public tPoison() : base(ID)
        {
            name = "Яд";
            desc = "Выпей яду.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tPoison(tPoison other) : base(other) { }
        public override object Clone() => new tPoison(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>В конце хода на территории</color>\nНаносит {_damageF.Format(args.stacks)} урона владельцу.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.Territory.OnEndPhase.Add(trait.GuidStr, OnTerritoryEndPhase);
            else if (trait.WasRemoved(e))
                trait.Owner.Territory.OnEndPhase.Remove(trait.GuidStr);
        }
        private async UniTask OnTerritoryEndPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || trait.Owner.Field == null) return;

            int damage = _damageF.ValueInt(trait.GetStacks());
            trait.Owner.Drawer.CreateTextAsSpeech($"{name}\n<size=50%>-{damage}", Color.red);
            await trait.Owner.Health.AdjustValue(-damage, trait);
        }
    }
}
