using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tVaccianide : ActiveTrait
    {
        const string ID = "vaccianide";
        const int CD = 1;
        const string TRAIT_ID_1 = "vaccinated";
        const string TRAIT_ID_2 = "cianided";
        static readonly TraitStatFormula _vaccineF = new(false, 0, 3);
        static readonly TraitStatFormula _cianideF = new(false, 0, 2);

        public tVaccianide() : base(ID)
        {
            name = "Вакцианид";
            desc = "Жизнь - это причина умереть.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.bothTriple);
        }
        protected tVaccianide(tVaccianide other) : base(other) { }
        public override object Clone() => new tVaccianide(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName1 = TraitBrowser.GetTrait(TRAIT_ID_1).name;
            string traitName2 = TraitBrowser.GetTrait(TRAIT_ID_2).name;
            return $"<color>При активации на союзной карте рядом, включая владельца</color>\n" +
                   $"Восстанавливает {_vaccineF.Format(args.stacks)} здоровья цели, даёт ей столько же зарядов навыка <nobr><u>{traitName1}</u></nobr>. Перезарядка: {CD} х.\n\n" +
                   $"<color>При активации на вражеской карте рядом</color>\n" +
                   $"Наносит {_cianideF.Format(args.stacks)} урона цели, даёт ей столько же зарядов навыка <nobr><u>{traitName2}</u></nobr>. Перезарядка: {CD} х.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new()
            {
                new TraitDescriptiveArgs(TRAIT_ID_1) { linkFormat = true, stacks = _vaccineF.ValueInt(args.stacks) },
                new TraitDescriptiveArgs(TRAIT_ID_2) { linkFormat = true, stacks = _cianideF.ValueInt(args.stacks) },
            };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            if (result.Field.Card.Side == result.Entity.Side)
                 return new(result.Entity, _vaccineF.Value(result.Entity.GetStacks()));
            else return new(result.Entity, _cianideF.Value(result.Entity.GetStacks()));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;

            int value;
            if (target.Side.isMe)
            {
                value = _vaccineF.ValueInt(e.traitStacks);
                await target.Health.AdjustValue(value, trait);
                if (!target.IsKilled)
                    await target.Traits.Passives.AdjustStacks(TRAIT_ID_1, value, trait);
            }
            else
            {
                value = _cianideF.ValueInt(e.traitStacks);
                await target.Health.AdjustValue(-value, trait);
                if (!target.IsKilled)
                    await target.Traits.Passives.AdjustStacks(TRAIT_ID_2, value, trait);
            }
            trait.SetCooldown(CD);
        }
    }
}
