using Cysharp.Threading.Tasks;
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
        const int PRIORITY = 6;
        static readonly TraitStatFormula _moxieF = new(false, 0, 1);

        public tShootingPassion() : base(ID)
        {
            name = "Стрелковый азарт";
            desc = "Только убивая, они чувствуют, что живут.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tShootingPassion(tShootingPassion other) : base(other) { }
        public override object Clone() => new tShootingPassion(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После убийства карты владельцем (П{PRIORITY})",
                    $"увеличивает инициативу владельца на {_moxieF.Format(trait)}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(38, stacks);
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
            await owner.Moxie.AdjustValue(_moxieF.Value(trait), trait);
        }
    }
}
