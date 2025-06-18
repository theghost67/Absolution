using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tScorchingFlame : PassiveTrait
    {
        const string ID = "scorching_flame";
        static readonly TraitStatFormula _strengthF = new(true, 0.25f, 0.25f);

        public tScorchingFlame() : base(ID)
        {
            name = Translator.GetString("trait_scorching_flame_1");
            desc = Translator.GetString("trait_scorching_flame_2");

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tScorchingFlame(tScorchingFlame other) : base(other) { }
        public override object Clone() => new tScorchingFlame(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_scorching_flame_3", _strengthF.Format(args.stacks, true));

        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(12, stacks, 1, 1.5f);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived);
                trait.Owner.OnPostKilled.Add(trait.GuidStr, OnOwnerPostKilled);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
                trait.Owner.OnPostKilled.Remove(trait.GuidStr);
            }
        }

        async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs e)
        {
            await OnOwnerHitReceived((BattleFieldCard)sender, e.Sender, e.Strength);
        }
        async UniTask OnOwnerPostKilled(object sender, BattleKillAttemptArgs e)
        {
            await OnOwnerHitReceived((BattleFieldCard)sender, e.source.AsBattleFieldCard(), e.damage);
        }

        async UniTask OnOwnerHitReceived(BattleFieldCard owner, BattleFieldCard sender, int strength)
        {
            if (owner == null || sender == null || strength < 1) return;
            ITableTrait trait = owner.Traits.Any(ID);
            if (trait == null || sender.IsKilled) return;
            int damage = (int)Mathf.Ceil(strength * _strengthF.Value(trait.GetStacks()));
            await trait.AnimActivationShort();
            sender.Drawer?.CreateTextAsDamage(damage, false);
            await sender.Health.AdjustValue(-damage, trait);
        }
    }
}
