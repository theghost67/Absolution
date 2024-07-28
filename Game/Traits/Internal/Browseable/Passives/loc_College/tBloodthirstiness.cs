﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBloodthirstiness : PassiveTrait
    {
        const string ID = "bloodthirstiness";
        const int PRIORITY = 5;
        const float STRENGTH_REL_INCREASE = 0.25f;

        public tBloodthirstiness() : base(ID)
        {
            name = "Кровожадность";
            desc = "Ещё, ЕЩЁ! Мне нужно больше крови!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = new BattleRange(TerritoryRange.ownerSingle, TerritoryRange.oppositeAll);
        }
        protected tBloodthirstiness(tBloodthirstiness other) : base(other) { }
        public override object Clone() => new tBloodthirstiness(this);

        public override string DescRich(ITableTrait trait)
        {
            float effect = STRENGTH_REL_INCREASE * 100 * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После убийства карты владельцем (П{PRIORITY})",
                    $"увеличивает силу владельца на <u>{effect}%</u>."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 6 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await owner.strength.AdjustValueScale(STRENGTH_REL_INCREASE * trait.GetStacks(), trait);
        }
    }
}
