using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tChao : PassiveTrait
    {
        const string ID = "chao";

        public tChao() : base(ID)
        {
            name = "Чао, чао, чао!";
            desc = "O partigiano, portami via! O bella ciao, bella ciao, bella ciao, ciao, ciao!";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tChao(tChao other) : base(other) { }
        public override object Clone() => new tChao(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После смерти владельца</color>\nУничтожит своего убийцу.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled);
            else if (trait.WasRemoved(e))
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            BattleFieldCard killer = e.source.AsBattleFieldCard();
            if (trait == null || trait.Owner == null) return;
            if (killer == null) return;

            await trait.AnimActivation();
            await killer.TryKill(BattleKillMode.Default, null);
        }
    }
}
