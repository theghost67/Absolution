using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tRunner : PassiveTrait
    {
        const string ID = "runner";
        static readonly TraitStatFormula _healthF = new(false, 0, 2);

        public tRunner() : base(ID)
        {
            name = "Бегущая";
            desc = "Эй, бегущая! У меня есть кое-какое задание для тебя! ...Только не это.";

            rarity = Rarity.None;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tRunner(tRunner other) : base(other) { }
        public override object Clone() => new tRunner(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После перемещения владельца на новое поле</color>\nВосстанавливает {_healthF.Format(args.stacks)} здоровья владельцу.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsLinear(6, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnFieldPostAttached.Add(trait.GuidStr, OnFieldPostAttached);
            else if (trait.WasRemoved(e))
                trait.Owner.OnFieldPostAttached.Remove(trait.GuidStr);
        }
        static async UniTask OnFieldPostAttached(object sender, TableFieldAttachArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || owner.FieldsAttachments < 2) return;

            int health = _healthF.ValueInt(trait.GetStacks());
            await trait.AnimActivation();
            await owner.Health.AdjustValue(health, trait);
        }
    }
}
