using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDoofinator : ActiveTrait
    {
        const string ID = "doofinator";
        const string CARD_ID = "doof";
        static readonly TraitStatFormula _healthF = new(false, 0, 4);

        public tDoofinator() : base(ID)
        {
            name = Translator.GetString("trait_doofinator_1");
            desc = Translator.GetString("trait_doofinator_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tDoofinator(tDoofinator other) : base(other) { }
        public override object Clone() => new tDoofinator(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_doofinator_3", _healthF.Format(args.stacks), cardName);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(10, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null 
                && e.trait.Owner.Field != null && e.target.Card.Health <= _healthF.Value(e.traitStacks);
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField targetField = (BattleField)e.target;
            BattleFieldCard target = targetField.Card;

            await target.TryKill(BattleKillMode.Default, trait);
            if (!target.IsKilled) return;
            FieldCard ownerClone = (FieldCard)trait.Owner.Data.CloneAsNew();
            ownerClone.traits.Clear();
            await trait.Territory.PlaceFieldCard(ownerClone, targetField, trait);
            await trait.SetStacks(0, trait.Side);
        }
    }
}
