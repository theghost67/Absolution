using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tArmoredTank : PassiveTrait
    {
        const string ID = "armored_tank";
        static readonly TraitStatFormula _strengthF = new(true, 0.25f, 0.25f);

        public tArmoredTank() : base(ID)
        {
            name = "Бронетанк";
            desc = "Да у него непробиваемый слой защиты! Что это может быть?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tArmoredTank(tArmoredTank other) : base(other) { }
        public override object Clone() => new tArmoredTank(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед совершением атаки на владельца</color>\nУменьшает силу атаки на {_strengthF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(24, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnOwnerInitiationPreReceived);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (e.Strength < 0) return;

            await trait.AnimActivation();
            await e.Strength.AdjustValueScale(-_strengthF.Value(trait.GetStacks()), trait);
        }
    }
}
