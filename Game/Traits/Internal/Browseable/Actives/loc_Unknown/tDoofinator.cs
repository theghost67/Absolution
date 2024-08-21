using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using static UnityEngine.GraphicsBuffer;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDoofinator : ActiveTrait
    {
        const string ID = "doofinator";
        const string CARD_ID = "doof";
        static readonly TraitStatFormula _moxieF = new(false, 0, 0);

        public tDoofinator() : base(ID)
        {
            name = "Фуфел-инатор";
            desc = "Трепещи, Перри-утконос, Фуфел-инатор!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tDoofinator(tDoofinator other) : base(other) { }
        public override object Clone() => new tDoofinator(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"<color>При использовании на вражеской карте рядом</color>\n" +
                   $"Если инициатива цели ≤ {_moxieF.Format(args.stacks)}, она превратится в карту {cardName} с такими же характеристиками, как у владельца. Тратит один заряд.";
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(0, 0.12f);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(16, stacks, 2, 1.6f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null 
                && e.trait.Owner.Field != null && e.target.Card.Moxie <= _moxieF.Value(e.traitStacks);
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField targetField = (BattleField)e.target;
            BattleFieldCard target = targetField.Card;

            await trait.AdjustStacks(-1, trait.Side);
            await target.TryKill(BattleKillMode.Default, trait);
            if (target.IsKilled)
            {
                FieldCard ownerClone = (FieldCard)trait.Owner.Data.CloneAsNew();
                ownerClone.traits.Clear();
                await trait.Territory.PlaceFieldCard(ownerClone, targetField, trait);
            }
        }
    }
}
