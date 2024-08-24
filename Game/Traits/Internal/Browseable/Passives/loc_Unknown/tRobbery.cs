using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tRobbery : PassiveTrait
    {
        const string ID = "robbery";
        const int PRIORITY = 7;
        static readonly TraitStatFormula _goldF = new(false, 0, 1);
        static readonly TraitStatFormula _moxieF = new(false, 3, 0);

        public tRobbery() : base(ID)
        {
            name = "Это ограбление!";
            desc = "Guys, the drill, go get it.";

            rarity = Rarity.Epic;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tRobbery(tRobbery other) : base(other) { }
        public override object Clone() => new tRobbery(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После убийства карты владельцем (П{PRIORITY})</color>\n" +
                   $"Даёт {_goldF.Format(args.stacks)} золота стороне-владельцу и понижает инициативу владельца на {_moxieF.Format(args.stacks, true)}. Тратит все заряды.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return PointsExponential(24, stacks, 1);
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

            int stacks = trait.GetStacks();
            await trait.AnimActivation();
            await trait.SetStacks(0, trait);
            await owner.Side.Gold.AdjustValue(_goldF.Value(stacks), trait);
            await owner.Moxie.AdjustValue(-_moxieF.Value(stacks), trait);
        }
    }
}
