using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tAdrenaline : PassiveTrait
    {
        const string ID = "adrenaline";
        const int PRIORITY = 6;

        public tAdrenaline() : base(ID)
        {
            name = "Адреналин";
            desc = "Последний рывок до финиша.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tAdrenaline(tAdrenaline other) : base(other) { }
        public override object Clone() => new tAdrenaline(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед смертью владельца (П{PRIORITY})",
                    $"Восстанавливает здоровье владельца до 1 единицы. Тратит все заряды."),
            });
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPreKilled.Add(trait.GuidStr, OnOwnerPreKilled, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPreKilled.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerPreKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await trait.SetStacks(0, trait);
            await owner.Health.SetValue(1, trait);
        }
    }
}
