using Cysharp.Threading.Tasks;
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
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.25f);

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
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После убийства карты владельцем (П{PRIORITY})",
                    $"увеличивает силу владельца на {_strengthF.Format(trait)}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(20, stacks);
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
            await owner.Strength.AdjustValueScale(_strengthF.Value(trait), trait);
        }
    }
}
