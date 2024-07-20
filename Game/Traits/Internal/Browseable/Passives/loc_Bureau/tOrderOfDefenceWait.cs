using Cysharp.Threading.Tasks;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfDefenceWait : PassiveTrait
    {
        const string ID = "order_of_defence_wait";
        const int PRIORITY = 4;
        const string TRAIT_ID = "block";

        public tOrderOfDefenceWait() : base(ID)
        {
            name = "Приказ о защите (ждёт)";
            desc = "Приказ получен, выдвигаюсь на позицию.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tOrderOfDefenceWait(tOrderOfDefenceWait other) : base(other) { }
        public override object Clone() => new tOrderOfDefenceWait(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            float traitStacks = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале следующего хода (П{PRIORITY})",
                    $"Наложит на себя <i>{traitName}</i> с {traitStacks} зарядами."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            await trait.Owner.Traits.Passives.AdjustStacks(TRAIT_ID, stacks, trait);
            trait.SetStacks(0, trait);
        }
    }
}
