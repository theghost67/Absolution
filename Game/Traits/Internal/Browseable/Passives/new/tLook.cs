using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLook : PassiveTrait
    {
        const string ID = "look";
        static readonly TraitStatFormula _reduceF = new(true, 0.75f, 0.25f);
        int _activationTurn = -1;

        public tLook() : base(ID)
        {
            name = "Я присмотрю за тобой";
            desc = "Большой брат присмотрит за тобой, малыш.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tLook(tLook other) : base(other) { }
        public override object Clone() => new tLook(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед совершением атаки на союзную карту рядом</color>\n" +
                   $"Переведёт атаку на владельца и уменьшит силу атаки на {_reduceF.Format(args.stacks, true)}. Активируется один раз за ход.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.75f);
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            BattleFieldCard owner = trait.Owner;

            if (e.canSeeTarget)
                e.target.OnInitiationPreReceived.Add(trait.GuidStr, OnInitiationPreReceived);
            else e.target.OnInitiationPreReceived.Remove(trait.GuidStr);
        }
        async UniTask OnInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard victim = (BattleFieldCard)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(victim.Territory);
            BattleFieldCard owner = trait.Owner;
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || victim.IsKilled || owner.IsKilled) return;

            if (_activationTurn == trait.TurnAge) return;
            _activationTurn = trait.TurnAge;

            await trait.AnimActivation();
            await e.Strength.AdjustValueScale(-_reduceF.Value(trait.GetStacks()), trait);
            e.ReceiverField = owner.Field;
        }
    }
}
