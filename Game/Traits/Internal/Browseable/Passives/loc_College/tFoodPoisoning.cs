using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFoodPoisoning : PassiveTrait
    {
        const string ID = "food_poisoning";
        const int PRIORITY = 5;
        static readonly TraitStatFormula _healthF = new(true, 0.50f, 0.50f);

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
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После смерти владельца (П{PRIORITY})",
                    $"уменьшает силу и здоровье инициатора на {_healthF.Format(trait)}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(24, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

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

            float value = _healthF.Value(trait);
            await trait.AnimActivation();
            await killer.Strength.AdjustValueScale(value, trait);
            await killer.Health.AdjustValueScale(value, trait);
        }
    }
}
