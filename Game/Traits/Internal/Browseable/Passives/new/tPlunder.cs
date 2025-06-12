using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;
using MyBox;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPlunder : PassiveTrait
    {
        const string ID = "plunder";
        const int MAX_EFFECT_STACKS = 5;
        static readonly TraitStatFormula _strengthF = new(true, 0.20f, 0.00f);

        public tPlunder() : base(ID)
        {
            name = "Разграбление";
            desc = "Мы войдём в историю.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tPlunder(tPlunder other) : base(other) { }
        public override object Clone() => new tPlunder(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед совершением атаки на карту владельцем</color>\nУвеличивает урон атаки на {_strengthF.Format(args.stacks)} за каждую " +
                   $"пустую ячейку стоимости цели (макс. {MAX_EFFECT_STACKS}). Если целей несколько, будет учитываться минимальная стоимость среди целей.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreSent.Add(trait.GuidStr, OnOwnerInitiationPreSent);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreSent.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.handled || e.Strength < 0) return;

            BattleField[] fields = e.Receivers.WithCard().ToArray();
            if (fields.Length == 0) return;
            BattleFieldCard target = fields.Select(f => f.Card).MinBy(c => c.Price.Value);
            if (target == null) return;
            float strength = _strengthF.Value(trait.GetStacks()) * (MAX_EFFECT_STACKS - target.Price).Clamped(0, MAX_EFFECT_STACKS);
            if (strength <= 0) return;

            await trait.AnimActivation();
            await e.Strength.AdjustValueScale(strength, trait);
        }
    }
}
