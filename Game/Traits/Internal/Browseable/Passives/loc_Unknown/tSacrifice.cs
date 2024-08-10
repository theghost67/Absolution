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
        const int PRIORITY = 3;
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tSacrifice() : base(ID)
        {
            name = "Жертвенный дар";
            desc = "Прими же нашу смиренную жертву!";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tSacrifice(tSacrifice other) : base(other) { }
        public override object Clone() => new tSacrifice(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале хода владельца (П{PRIORITY})",
                    $"Убивает союзные карты рядом и забирает их силу и здоровье."),
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
                    owner.Drawer?.RedrawHeaderTypingWithReset("Не дёргайся!");
                    continue;
                }

                await owner.Health.AdjustValue(health, trait);
                await owner.Strength.AdjustValue(strength, trait);
            }
        }
    }
}
