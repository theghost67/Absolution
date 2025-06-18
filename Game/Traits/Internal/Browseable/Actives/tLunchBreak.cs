using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLunchBreak : ActiveTrait
    {
        const string ID = "lunch_break";
        static readonly TraitStatFormula _healthF = new(false, 0, 2);
        const int CD = 1;

        public tLunchBreak() : base(ID)
        {
            name = Translator.GetString("trait_lunch_break_1");
            desc = Translator.GetString("trait_lunch_break_2");

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tLunchBreak(tLunchBreak other) : base(other) { }
        public override object Clone() => new tLunchBreak(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string healthF = _healthF.Format(args.stacks, true);
            return Translator.GetString("trait_lunch_break_3", healthF, CD, healthF, CD);

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(8, stacks);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;

            trait.SetCooldown(CD);
            int health = (int)_healthF.Value(e.traitStacks);

            if (target.Card != null)
                 await target.Card.Health.AdjustValue(health, trait);
            else await target.Health.AdjustValue(health, trait);
        }
    }
}
