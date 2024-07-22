using Cysharp.Threading.Tasks;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tOrderOfAttackWait : PassiveTrait
    {
        const string ID = "order_of_attack_wait";
        const int PRIORITY = 4;

        public tOrderOfAttackWait() : base(ID)
        {
            name = "Приказ об атаке (ждёт)";
            desc = "Приказ получен, выдвигаюсь на позицию.";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tOrderOfAttackWait(tOrderOfAttackWait other) : base(other) { }
        public override object Clone() => new tOrderOfAttackWait(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале следующего хода (П{PRIORITY})",
                    $"Совершит свою атакующую инициацию <u>{effect}</u> раз(-а) подряд. Тратит все заряды."),
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
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;
            if (trait.Owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();

            for (int i = 0; i < stacks; i++)
                trait.Territory.Initiations.Enqueue(trait.Owner.CreateInitiation());

            trait.Territory.Initiations.Run();
            TableEventManager.Remove(); // do not await this event anymore (TableEventManager.AwaitAny will still work because of initiations queue)
            await trait.Territory.Initiations.Await();
            TableEventManager.Add(); // restore event await condition
            await trait.SetStacks(0, trait);
        }
    }
}
