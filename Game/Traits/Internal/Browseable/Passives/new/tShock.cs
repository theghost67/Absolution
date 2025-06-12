using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tShock : PassiveTrait
    {
        const string ID = "shock";
        static readonly TraitStatFormula _damageF = new(true, 0.50f, 0.00f);

        public tShock() : base(ID)
        {
            name = "Электрошок";
            desc = "Ситуация становится электрошокирующе напряжённой.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tShock(tShock other) : base(other) { }
        public override object Clone() => new tShock(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После совершения атаки владельцем</color>\n" +
                   $"Мгновенно наносит урон рядомстоящим союзным картам, равный {_damageF.Format(args.stacks)} от силы атаки.";
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostSent.Add(trait.GuidStr, OnOwnerInitiationPostSent);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostSent.Remove(trait.GuidStr);
        }
        static async UniTask OnOwnerInitiationPostSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null || e.Strength <= 0) return;

            BattleFieldCard[] cards = owner.Territory.Fields(owner.Field.pos, trait.Data.range.potential).WithCard().Select(f => f.Card).ToArray();
            if (cards.Length == 0) return;

            int damage = (int)Mathf.Ceil(e.Strength * _damageF.Value(trait.GetStacks()));
            await trait.AnimActivationShort();
            foreach (BattleFieldCard card in cards)
            {
                card.Drawer?.CreateTextAsDamage(damage, false);
                await card.Health.AdjustValue(-damage, trait);
            }
        }
    }
}
