using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSentence : ActiveTrait
    {
        const string ID = "sentence";
        const int CD = 99;
        static readonly TraitStatFormula _strengthF = new(true, 0.00f, 0.25f);

        public tSentence() : base(ID)
        {
            name = "Приговор";
            desc = "Вынесение приговора!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeSingle);
        }
        protected tSentence(tSentence other) : base(other) { }
        public override object Clone() => new tSentence(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>При активации на территории</color>\nУвеличивает силу следующей инициации на {_strengthF.Format(args.stacks, true)}. После усиления, тратит все заряды. Перезарядка: {CD} х.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, result.Entity.Owner.Strength / 1.5f * _strengthF.Value(result.Entity.GetStacks()), 0);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent);
            trait.SetCooldown(CD);
        }

        private async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivation();
            float strength = _strengthF.Value(trait.GetStacks());
            await e.Strength.AdjustValueScale(strength, trait);
            await trait.SetStacks(0, trait);
        }
    }
}
