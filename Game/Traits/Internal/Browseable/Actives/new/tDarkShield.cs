using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tDarkShield : ActiveTrait
    {
        const string ID = "dark_shield";
        const string KEY = "dark_stacks";
        static readonly TraitStatFormula _chargesPerKillF = new(false, 1, 0);
        static readonly TraitStatFormula _healthBuffF = new(true, 0.25f, 0.25f);
        static readonly TerritoryRange _healRange = TerritoryRange.ownerDouble;

        public tDarkShield() : base(ID)
        {
            name = "Тёмный щит";
            desc = "Вы будете служить мне вечно, мои воины.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.self;
        }
        protected tDarkShield(tDarkShield other) : base(other) { }
        public override object Clone() => new tDarkShield(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            object darkStacks = null;
            args.table?.Storage.TryGetValue(KEY, out darkStacks);
            string str = $"<color>При активации на территории</color>\nДарует союзным картам рядом {_healthBuffF.Format(args.stacks)} от здоровья владельца. " +
                         $"Получает {_chargesPerKillF.Format(args.stacks)} зарядов тьмы после убийства карты владельцем. " +
                         $"Этот навык можно использовать только при наличии одного или более зарядов тьмы.";
            if (darkStacks != null)
                str += $" Зарядов тьмы: <color=#FF00FF>{(int)darkStacks}</color>.";
            return str;
        }
        public override BattleWeight Weight(IBattleTrait trait)
        {
            int darkStacks = (int)trait.Storage[KEY];
            float ratio = _healthBuffF.Value(trait.GetStacks());
            return new(trait, darkStacks * trait.Owner.Health * ratio);
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            float health = result.Entity.Owner.Health * _healthBuffF.Value(result.Entity.GetStacks());
            return new(result.Entity, health * 1.5f, 0);
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(16, stacks);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            trait.Storage[KEY] = 0;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && (int)e.trait.Storage[KEY] > 0;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            int health = (int)Mathf.Ceil(owner.Health * _healthBuffF.Value(e.traitStacks));
            BattleFieldCard[] cards = owner.Territory.Fields(target.pos, _healRange).WithCard().Select(f => f.Card).ToArray();
            foreach (BattleFieldCard card in cards)
                await card.Health.AdjustValue(health, trait);
            await trait.AdjustStacks(-1, owner.Side);
        }

        private async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || owner.IsKilled) return;

            int charges = _chargesPerKillF.ValueInt(trait.GetStacks());
            trait.Storage[KEY] = (int)trait.Storage[KEY] + charges;
            await trait.AnimActivationShort();
        }
    }
}
