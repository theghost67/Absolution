using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFoodPoisoning : PassiveTrait
    {
        const string ID = "food_poisoning";
        const int PRIORITY = 5;
        const float HP_STR_DECREASE_SCALE = 1f;

        public tFoodPoisoning() : base(ID)
        {
            name = "Пищевое отравление";
            desc = "Может всё же не будем это есть?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tFoodPoisoning(tFoodPoisoning other) : base(other) { }
        public override object Clone() => new tFoodPoisoning(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = HP_STR_DECREASE_SCALE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти владельца (П{PRIORITY})",
                    $"уменьшает силу и здоровье инициатора на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 20 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }

        async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            BattleFieldCard killer = e.source.AsBattleFieldCard();
            if (killer == null) return;
            float relDecrease = -HP_STR_DECREASE_SCALE * trait.GetStacks();

            await trait.AnimActivation();
            await killer.strength.AdjustValueScale(relDecrease, trait);
            await killer.health.AdjustValueScale(relDecrease, trait);
        }
    }
}
