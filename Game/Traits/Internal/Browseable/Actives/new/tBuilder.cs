using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBuilder : ActiveTrait
    {
        const string ID = "builder";
        const int CD = 1;
        const string CARD_ID = "brick";
        static readonly TraitStatFormula _healthF = new(false, 2, 0);

        public tBuilder() : base(ID)
        {
            name = "Постройка";
            desc = "Знаете, Москва не за день строилась, с чего-то нужно начинать.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerRadiusSmall);
        }
        protected tBuilder(tBuilder other) : base(other) { }
        public override object Clone() => new tBuilder(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return $"<color>При активации на поле рядом с владельцем</color>\n" +
                   $"Создаёт карту <nobr><u>{cardName}</u></nobr> на указанном поле. Перезарядка: {CD} х.\n\n" +
                   $"<color>При активации на карте рядом с владельцем</color>\n" +
                   $"Если у цели меньше {_healthF.Format(args.stacks)} здоровья, убивает её. Не вызывает перезарядку навыка.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new CardDescriptiveArgs(CARD_ID) { linkStats = CardDescriptiveArgs.normalStats } };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.08f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && (e.target.Card == null || e.target.Card.Health < _healthF.ValueInt(e.traitStacks));
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;

            if (target.Card == null)
            {
                await owner.Territory.PlaceFieldCard(CardBrowser.NewField(CARD_ID), target, trait);
                trait.SetCooldown(CD);
            }
            else await target.Card.TryKill(BattleKillMode.Default, trait);
        }
    }
}
