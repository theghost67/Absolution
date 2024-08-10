using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tTeleportationBag : ActiveTrait
    {
        const string ID = "teleportation_bag";
        const string TRAIT_ID = "teleportation_scroll";

        public tTeleportationBag() : base(ID)
        {
            name = "Сумка телепортации";
            desc = "Всегда с собой ношу несколько свитков на экстренные случаи.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerAllNotSelf);
        }
        protected tTeleportationBag(tTeleportationBag other) : base(other) { }
        public override object Clone() => new tTeleportationBag(this);

        public override string DescRich(ITableTrait trait)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"При использовании на территории на любой союзной карте",
                    $"Даёт цели навык <i>{traitName}</i>. Тратит один заряд."),
            });
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            BattleFieldCard card = result.Field.Card;
            if (card == null || card.Traits[TRAIT_ID] == null)
                 return BattleWeight.none;
            else return BattleWeight.negative;
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(30, stacks, 2);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.trait.Owner.Field != null;
        }
        public override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            await base.OnUse(e);

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;

            await trait.AdjustStacks(-1, trait.Side);
            await target.Traits.AdjustStacks(TRAIT_ID, 1, trait);
        }
    }
}
