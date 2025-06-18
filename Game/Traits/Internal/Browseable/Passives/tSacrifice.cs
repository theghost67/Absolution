using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSacrifice : PassiveTrait
    {
        const string ID = "sacrifice";
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tSacrifice() : base(ID)
        {
            name = Translator.GetString("trait_sacrifice_1");
            desc = Translator.GetString("trait_sacrifice_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tSacrifice(tSacrifice other) : base(other) { }
        public override object Clone() => new tSacrifice(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_sacrifice_3");

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

            BattleField[] fields = territory.Fields(owner.Field.pos, _range).WithCard().ToArray();
            if (fields.Length == 0) return;

            await trait.AnimActivation();
            foreach (BattleField field in fields)
            {
                int health = field.Card.Health;
                int strength = field.Card.Strength;

                await field.Card.TryKill(BattleKillMode.Default, trait);
                if (!(field.Card?.IsKilled ?? true))
                {
                    owner.Drawer?.RedrawHeaderTypingWithReset(Translator.GetString("trait_sacrifice_4"));
                    continue;
                }

                await owner.Health.AdjustValue(health, trait);
                await owner.Strength.AdjustValue(strength, trait);
            }
        }
    }
}
