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
    public class tCianided : PassiveTrait
    {
        const string ID = "cianided";
        private static TraitStatFormula _healF = new(false, 0, 1);

        public tCianided() : base(ID)
        {
            name = Translator.GetString("trait_cianided_1");
            desc = Translator.GetString("trait_cianided_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tCianided(tCianided other) : base(other) { }
        public override object Clone() => new tCianided(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_cianided_3", _healF.Format(args.stacks));
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

            int heal = _healF.ValueInt(trait.GetStacks());
            trait.Owner.Drawer.CreateTextAsSpeech($"{name}\n<size=50%>-{heal}", Color.red);
            await trait.Owner.Health.AdjustValue(heal, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
