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
            name = Translator.GetString("trait_shock_1");
            desc = Translator.GetString("trait_shock_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tShock(tShock other) : base(other) { }
        public override object Clone() => new tShock(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_shock_3", _damageF.Format(args.stacks));

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
