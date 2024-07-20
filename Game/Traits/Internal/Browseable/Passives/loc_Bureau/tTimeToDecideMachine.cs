using Cysharp.Threading.Tasks;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTimeToDecideMachine : PassiveTrait
    {
        const string ID = "time_to_decide_machine";
        const int PRIORITY = 7;

        public tTimeToDecideMachine() : base(ID)
        {
            name = "Время решать (машина)";
            desc = "Простите, лейтенант.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tTimeToDecideMachine(tTimeToDecideMachine other) : base(other) { }
        public override object Clone() => new tTimeToDecideMachine(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В конце хода (П{PRIORITY})",
                    $"переходит на вражеское поле напротив, если оно не занято. Тратит все заряды."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Territory.OnEndPhase.Add(trait.GuidStr, OnTerritoryEndPhase, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Territory.OnEndPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryEndPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;
            if (trait.Owner.Field.Opposite.Card != null) return;

            await trait.AnimActivation();
            await trait.Owner.AttachToField(trait.Owner.Field.Opposite, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
