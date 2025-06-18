using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFlame : PassiveTrait
    {
        const string ID = "flame";
        private static TraitStatFormula _propagationF = new(true, 0.50f, 0.00f);
        private static TraitStatFormula _damageF = new(false, 0, 1);

        public tFlame() : base(ID)
        {
            name = Translator.GetString("trait_flame_1");
            desc = Translator.GetString("trait_flame_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tFlame(tFlame other) : base(other) { }
        public override object Clone() => new tFlame(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_flame_3", _damageF.Format(args.stacks), _propagationF.Format(args.stacks), name);

        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.Territory.OnEndPhase.Add(trait.GuidStr, OnTerritoryEndPhase);
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnPostKilled);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.Territory.OnEndPhase.Remove(trait.GuidStr);
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
            }
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
        private async UniTask OnPostKilled(object sender, EventArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            int stacks = trait.GetStacks();
            if (stacks == 1) return;

            await trait.AnimActivation();

            int newTraitStacks = (int)Math.Ceiling(stacks * _propagationF.Value(stacks));
            IEnumerable<BattleFieldCard> nearCards = trait.Territory.Fields(trait.Field.pos, range.potential).WithCard().Select(f => f.Card);
            foreach (BattleFieldCard card in nearCards)
                await card.Traits.Passives.AdjustStacks(ID, stacks, trait);
        }
    }
}
