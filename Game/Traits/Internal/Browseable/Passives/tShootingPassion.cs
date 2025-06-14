﻿using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tShootingPassion : PassiveTrait
    {
        const string ID = "shooting_passion";
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tShootingPassion() : base(ID)
        {
            name = "Стрелковый азарт";
            desc = "Только убивая, они чувствуют, что живут.";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tShootingPassion(tShootingPassion other) : base(other) { }
        public override object Clone() => new tShootingPassion(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После любой убийства карты владельцем</color>\n" +
                   $"увеличивает инициативу владельца на {_moxieF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(20, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed);
            else if (trait.WasRemoved(e))
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            await owner.Moxie.AdjustValue(_moxieF.Value(stacks), trait);
        }
    }
}
