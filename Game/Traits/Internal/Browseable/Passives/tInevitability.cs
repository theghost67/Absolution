using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tInevitability : PassiveTrait
    {
        const string ID = "inevitability";
        static readonly TraitStatFormula _healthF = new(true, 0.10f, 0.10f);

        public tInevitability() : base(ID)
        {
            name = "Неотвратимость";
            desc = "Я - сама неотвратимость.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tInevitability(tInevitability other) : base(other) { }
        public override object Clone() => new tInevitability(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После смерти любой карты на территории</color>\nУвеличивает здоровье владельца на {_healthF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(8, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.WasAdded(e))
            {
                foreach (BattleField field in trait.Territory.Fields())
                {
                    field.Card?.OnPostKilled.Add(trait.GuidStr, OnTerritoryCardPostKilled);
                    field.OnCardAttached.Add(trait.GuidStr, OnCardAttachedToAnyField);
                }
            }
            else if (trait.WasRemoved(e))
            {
                foreach (BattleField field in trait.Territory.Fields())
                {
                    field.Card?.OnPostKilled.Remove(trait.GuidStr);
                    field.OnCardAttached.Remove(trait.GuidStr);
                }
            }
        }

        async UniTask OnCardAttachedToAnyField(object sender, TableFieldAttachArgs e)
        {
            BattleField field = (BattleField)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(field.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (field.Card == null) return;
            field.Card.OnPostKilled.Add(trait.GuidStr, OnTerritoryCardPostKilled);
        }
        async UniTask OnTerritoryCardPostKilled(object sender, BattleKillAttemptArgs e)
        {
            BattleFieldCard terrCard = (BattleFieldCard)sender;
            BattlePassiveTrait trait = (BattlePassiveTrait)TraitFinder.FindInBattle(terrCard.Territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            if (trait.Owner.Field == null) return;
            if (terrCard == trait.Owner) return;

            await trait.AnimActivation();
            await trait.Owner.Health.AdjustValueScale(_healthF.Value(trait.GetStacks()), trait);
        }
    }
}
